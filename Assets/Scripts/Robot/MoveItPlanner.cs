using System;
using RosMessageTypes.Geometry;
using RosMessageTypes.Moveit;
using RosMessageTypes.NiryoMoveit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

namespace HandsOnRobotics.Robot
{
    /* Wraps the niryo_moveit MoverService ROS service call.

    Call Plan() with a target world-space pose and current joint angles.
    The response is returned via callback on the Unity main thread.

    NOTE: pick_pose is sent in Unity world space converted to ROS FLU frame.
    If the robot origin in ROS differs from Unity world origin, set
    _robotRosOrigin to the robot root's world position so the offset is
    applied before sending. */
    public class MoveItPlanner : MonoBehaviour
    {
        public static MoveItPlanner Instance { get; private set; }

        [SerializeField] string _serviceName = "niryo_moveit";
        [Tooltip("World position of the robot base in Unity, used to offset target pose into robot frame.")]
        [SerializeField] Transform _robotBase;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            ROSConnection.GetOrCreateInstance()
                .RegisterRosService<MoverServiceRequest, MoverServiceResponse>(_serviceName);
        }

        public void Plan(Vector3 targetWorldPos, Quaternion targetWorldRot,
                         double[] currentJoints, Action<RobotTrajectoryMsg[]> onResponse)
        {
            // Convert target from Unity world space to robot-local space before FLU conversion.
            var localPos = _robotBase != null
                ? _robotBase.InverseTransformPoint(targetWorldPos)
                : targetWorldPos;
            var localRot = _robotBase != null
                ? Quaternion.Inverse(_robotBase.rotation) * targetWorldRot
                : targetWorldRot;

            var rosPos = localPos.To<FLU>();
            var rosRot = localRot.To<FLU>();
            Debug.Log($"[MoveItPlanner] Sending plan request to '{_serviceName}'. " +
                      $"Target (ROS): pos=({rosPos.x:F3},{rosPos.y:F3},{rosPos.z:F3}) " +
                      $"joints=[{string.Join(", ", Array.ConvertAll(currentJoints, j => j.ToString("F3")))}]");

            var request = new MoverServiceRequest
            {
                joints_input = new NiryoMoveitJointsMsg { joints = currentJoints },
                pick_pose    = new PoseMsg { position = rosPos, orientation = rosRot },
                place_pose   = new PoseMsg()
            };

            ROSConnection.GetOrCreateInstance().SendServiceMessage<MoverServiceResponse>(
                _serviceName, request,
                response =>
                {
                    int count = response.trajectories?.Length ?? 0;
                    Debug.Log($"[MoveItPlanner] Response received: {count} trajectory/trajectories.");
                    onResponse?.Invoke(response.trajectories);
                });
        }
    }
}
