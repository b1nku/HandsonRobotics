using System.Collections.Generic;
using UnityEngine;

namespace HandsOnRobotics.Robot
{
    /* Direction B: TF Frame Visualisation.

    Place on any persistent scene GameObject. On Start() reads NiryoOneJointMap
    and creates one TFFrameVisualiser per joint link, wiring each frame's parent
    connection to the previous link in the kinematic chain (mirroring the RViz
    TF display). Call Toggle() or set Visible in the Inspector to show/hide all
    frames at once. */
    public class TFFrameDisplay : MonoBehaviour
    {
        [Header("Axes")]
        [Tooltip("Length of each axis line in metres.")]
        [SerializeField] float _axisLength = 0.05f;
        [Tooltip("Base width of each axis line in metres.")]
        [SerializeField] float _axisWidth  = 0.003f;

        [Header("Labels")]
        [SerializeField] bool  _showLabels    = true;
        [Tooltip("TextMeshPro font size for frame name labels.")]
        [SerializeField] float _labelFontSize = 0.025f;

        [Header("Tree lines")]
        [SerializeField] bool _showParentLines = true;

        [Header("Visibility")]
        [SerializeField] bool _visibleOnStart  = true;
        [Tooltip("Render axes and parent lines on top of all geometry (ZTest Always).")]
        [SerializeField] bool _overlayGeometry = true;

        readonly List<TFFrameVisualiser> _frames = new();
        bool _visible;

        void Start()
        {
            var map = NiryoOneJointMap.Instance;
            if (map == null)
            {
                Debug.LogError("[TFFrameDisplay] NiryoOneJointMap not found.");
                return;
            }

            var joints = map.AllJoints;
            for (int i = 0; i < joints.Count; i++)
            {
                var entry = joints[i];
                if (entry.link == null) continue;

                // Parent link is the previous joint's link, or null for the first joint.
                Transform parentLink = i > 0 ? joints[i - 1].link : null;

                var go = new GameObject($"TF_{entry.link.name}");
                go.transform.SetParent(entry.link);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;

                var vis = go.AddComponent<TFFrameVisualiser>();
                vis.Initialise(
                    entry.link.name,
                    parentLink,
                    _axisLength,
                    _axisWidth,
                    _labelFontSize,
                    _showLabels,
                    _showParentLines,
                    _overlayGeometry);

                _frames.Add(vis);
            }

            SetVisible(_visibleOnStart);
        }

        public void Toggle() => SetVisible(!_visible);

        public void SetVisible(bool visible)
        {
            _visible = visible;
            foreach (var frame in _frames)
                frame.SetVisible(visible);
        }
    }
}
