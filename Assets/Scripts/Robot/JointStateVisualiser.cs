using System.Collections.Generic;
using HandsOnRobotics.ROS;
using RosMessageTypes.NiryoOne;
using RosMessageTypes.Sensor;
using UnityEngine;

namespace HandsOnRobotics.Robot
{
    /* Orchestrates Direction A: Joint State and Motor Health Visualisation.

    Place on any persistent scene GameObject (e.g. the niryo_one root).
    Requires NiryoOneJointMap to be present in the scene.

    On Start(), finds a JointAnnotation component on or under each mapped link.
    When ROS data arrives it routes joint state and hardware status to the
    correct annotation by joint name. */
    public class JointStateVisualiser : MonoBehaviour
    {
        // Built from NiryoOneJointMap on Start(): joint name -> annotation.
        readonly Dictionary<string, JointAnnotation> _annotations = new();

        // Hardware status indexes by motor order, which matches joint order
        // on the Niryo One (motor 0 = joint_1, motor 1 = joint_2, etc.).
        readonly List<string> _motorIndexToJoint = new();

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

                var annotation = entry.link.GetComponentInChildren<JointAnnotation>();
                if (annotation == null)
                {
                    Debug.LogWarning($"[JointStateVisualiser] No JointAnnotation found under {entry.link.name} ({entry.rosName}).");
                    continue;
                }

                _annotations[entry.rosName] = annotation;
                _motorIndexToJoint.Add(entry.rosName);
            }
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

        void HandleJointState(JointStateMsg msg)
        {
            for (int i = 0; i < msg.name.Length; i++)
            {
                if (!_annotations.TryGetValue(msg.name[i], out var annotation)) continue;

                float position = i < msg.position.Length ? (float)msg.position[i] : 0f;
                float velocity = i < msg.velocity.Length ? (float)msg.velocity[i] : 0f;
                float effort   = i < msg.effort.Length   ? (float)msg.effort[i]   : 0f;

                annotation.SetJointState(msg.name[i], position, velocity, effort);
            }
        }

        void HandleHardwareStatus(HardwareStatusMsg msg)
        {
            for (int i = 0; i < _motorIndexToJoint.Count; i++)
            {
                if (i >= msg.temperatures.Length) break;
                if (!_annotations.TryGetValue(_motorIndexToJoint[i], out var annotation)) continue;

                annotation.SetHardwareStatus(
                    msg.temperatures[i],
                    i < msg.voltages.Length        ? msg.voltages[i]        : 0.0,
                    i < msg.hardware_errors.Length ? msg.hardware_errors[i] : 0);
            }
        }
    }
}
