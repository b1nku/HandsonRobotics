using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandsOnRobotics.Robot
{
    [Serializable]
    public struct JointEntry
    {
        [Tooltip("Joint name as published on /joint_states (e.g. joint_1)")]
        public string rosName;
        [Tooltip("The link Transform in the URDF hierarchy for this joint")]
        public Transform link;
    }

    /* Attach to the root GameObject of the imported Niryo One URDF.
    Maps ROS joint names to their corresponding link Transforms so that
    visualisation components (joint state overlays, trajectory preview)
    can look up the right object without searching the hierarchy at runtime.
    
    Default entries match the Niryo One URDF joint names. Assign the
    link Transforms in the Inspector after URDF import. */

    public class NiryoOneJointMap : MonoBehaviour
    {
        public static NiryoOneJointMap Instance { get; private set; }

        [SerializeField]
        JointEntry[] _joints = new JointEntry[]
        {
            new() { rosName = "joint_1" },
            new() { rosName = "joint_2" },
            new() { rosName = "joint_3" },
            new() { rosName = "joint_4" },
            new() { rosName = "joint_5" },
            new() { rosName = "joint_6" },
        };

        readonly Dictionary<string, Transform> _map = new();

        public IReadOnlyList<JointEntry> AllJoints => _joints;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            foreach (var entry in _joints)
                if (entry.link != null)
                    _map[entry.rosName] = entry.link;
        }

        public bool TryGetLink(string jointName, out Transform link) => _map.TryGetValue(jointName, out link);

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
