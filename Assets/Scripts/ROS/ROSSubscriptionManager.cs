using System;
using HandsOnRobotics.UI;
using RosMessageTypes.Sensor;
using RosMessageTypes.NiryoOne;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace HandsOnRobotics.ROS
{
    /* Central ROS subscription hub. Place on a persistent scene GameObject.
    Visualisation components subscribe to the static events : they never touch ROSConnection directly. */

    public class ROSSubscriptionManager : MonoBehaviour
    {
        public static ROSSubscriptionManager Instance { get; private set; }

        [Header("Topics")]
        [SerializeField] string _jointStateTopic     = "/joint_states";
        [SerializeField] string _hardwareStatusTopic = "/niryo_one/hardware_status";
        [SerializeField] string _robotStateTopic     = "/niryo_one/robot_state";

        // Visualisation components subscribe to these; they fire on the Unity
        // main thread via the ROS TCP Connector's message dispatch.
        public static event Action<JointStateMsg>     OnJointState;
        public static event Action<HardwareStatusMsg> OnHardwareStatus;
        public static event Action<RobotStateMsg>     OnRobotState;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            var ros = ROSConnection.GetOrCreateInstance();

            TopicMonitorPanel.Register(_jointStateTopic,     "sensor_msgs/JointState");
            TopicMonitorPanel.Register(_hardwareStatusTopic, "niryo_one_msgs/HardwareStatus");
            TopicMonitorPanel.Register(_robotStateTopic,     "niryo_one_msgs/RobotState");

            ros.Subscribe<JointStateMsg>(_jointStateTopic, msg =>
            {
                TopicMonitorPanel.RecordMessage(_jointStateTopic);
                OnJointState?.Invoke(msg);
            });
            ros.Subscribe<HardwareStatusMsg>(_hardwareStatusTopic, msg =>
            {
                TopicMonitorPanel.RecordMessage(_hardwareStatusTopic);
                OnHardwareStatus?.Invoke(msg);
            });
            ros.Subscribe<RobotStateMsg>(_robotStateTopic, msg =>
            {
                TopicMonitorPanel.RecordMessage(_robotStateTopic);
                OnRobotState?.Invoke(msg);
            });
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
