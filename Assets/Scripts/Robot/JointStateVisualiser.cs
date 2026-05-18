using System.Collections.Generic;
using HandsOnRobotics.ROS;
using RosMessageTypes.NiryoOne;
using RosMessageTypes.Sensor;
using UnityEngine;

namespace HandsOnRobotics.Robot
{
    /* Orchestrates Direction A: Joint State and Motor Health Visualisation.

    Place on any persistent scene GameObject (e.g. the niryo_one root).
    Requires NiryoOneJointMap in the scene. Wire _detailPanel in the Inspector.

    On Start() discovers a JointRing under each mapped link. Each frame it
    head-gazes (camera-forward raycast) against JointRing SphereColliders;
    after _gazeDwell seconds the shared JointDetailPanel binds to that ring. */
    public class JointStateVisualiser : MonoBehaviour
    {
        [SerializeField] JointDetailPanel _detailPanel;
        [SerializeField] float _gazeDwell = 0.3f;
        [SerializeField] float _gazeDistance = 5f;
        [SerializeField] LayerMask _ringLayer = ~0;

        readonly Dictionary<string, JointRing> _rings = new();
        readonly List<string> _motorIndexToJoint = new();

        JointRing _gazedRing;
        JointRing _activeRing;
        float _dwellTimer;

        void Start()
        {
            var map = NiryoOneJointMap.Instance;
            if (map == null)
            {
                Debug.LogError("[JointStateVisualiser] NiryoOneJointMap not found in scene.");
                return;
            }

            foreach (var entry in map.AllJoints)
            {
                if (entry.link == null) continue;

                var ring = entry.link.GetComponentInChildren<JointRing>();
                if (ring == null)
                {
                    Debug.LogWarning($"[JointStateVisualiser] No JointRing found under {entry.link.name} ({entry.rosName}).");
                    continue;
                }

                _rings[entry.rosName] = ring;
                _motorIndexToJoint.Add(entry.rosName);
            }

            if (_detailPanel) _detailPanel.Unbind();
        }

        void OnEnable()
        {
            ROSSubscriptionManager.OnJointState     += HandleJointState;
            ROSSubscriptionManager.OnHardwareStatus += HandleHardwareStatus;
        }

        void OnDisable()
        {
            ROSSubscriptionManager.OnJointState     -= HandleJointState;
            ROSSubscriptionManager.OnHardwareStatus -= HandleHardwareStatus;
        }

        void Update()
        {
            if (_detailPanel == null || Camera.main == null) return;

            var ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            JointRing hit = null;
            if (Physics.Raycast(ray, out var hitInfo, _gazeDistance, _ringLayer))
                hit = hitInfo.collider.GetComponentInParent<JointRing>();

            if (hit != _gazedRing)
            {
                _gazedRing  = hit;
                _dwellTimer = 0f;
            }

            if (_gazedRing != null)
            {
                _dwellTimer += Time.deltaTime;
                if (_dwellTimer >= _gazeDwell && _gazedRing != _activeRing)
                {
                    _activeRing = _gazedRing;
                    _detailPanel.Bind(_activeRing);
                }
            }
            else if (_activeRing != null)
            {
                _activeRing = null;
                _detailPanel.Unbind();
            }
        }

        void HandleJointState(JointStateMsg msg)
        {
            for (int i = 0; i < msg.name.Length; i++)
            {
                if (!_rings.TryGetValue(msg.name[i], out var ring)) continue;

                float pos = i < msg.position.Length ? (float)msg.position[i] : 0f;
                float vel = i < msg.velocity.Length ? (float)msg.velocity[i] : 0f;
                float eff = i < msg.effort.Length   ? (float)msg.effort[i]   : 0f;
                ring.SetJointState(msg.name[i], pos, vel, eff);
            }

            if (_activeRing != null) _detailPanel.Refresh();
        }

        void HandleHardwareStatus(HardwareStatusMsg msg)
        {
            for (int i = 0; i < _motorIndexToJoint.Count; i++)
            {
                if (i >= msg.temperatures.Length) break;
                if (!_rings.TryGetValue(_motorIndexToJoint[i], out var ring)) continue;

                ring.SetHardwareStatus(
                    msg.temperatures[i],
                    i < msg.voltages.Length        ? msg.voltages[i]        : 0.0,
                    i < msg.hardware_errors.Length ? msg.hardware_errors[i] : 0);
            }

            if (_activeRing != null) _detailPanel.Refresh();
        }
    }
}
