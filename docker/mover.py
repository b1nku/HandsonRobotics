#!/usr/bin/env python3
"""
Simplified MoverService server - plans a single trajectory from the current
joint state to the requested pick_pose. Ignores place_pose entirely (not
needed for trajectory preview). Returns one RobotTrajectory or an empty list.
"""
import sys
import rospy
import moveit_commander
from moveit_msgs.msg import RobotState
from sensor_msgs.msg import JointState
from niryo_moveit.srv import MoverService, MoverServiceResponse

PLANNING_GROUP = "arm"
PLANNING_TIME  = 10.0  # seconds


def plan(req):
    response = MoverServiceResponse()
    group = moveit_commander.MoveGroupCommander(PLANNING_GROUP)
    group.set_planning_time(PLANNING_TIME)

    # Set start state from joints sent by Unity
    active_joints = group.get_active_joints()
    js = JointState()
    js.name     = list(active_joints)
    js.position = [
        req.joints_input.joints[i] if i < len(req.joints_input.joints) else 0.0
        for i in range(len(active_joints))
    ]
    start = RobotState()
    start.joint_state = js
    group.set_start_state(start)

    p = req.pick_pose.position
    group.set_position_target([p.x, p.y, p.z])
    success, trajectory, _, _ = group.plan()

    if success and trajectory.joint_trajectory.points:
        rospy.loginfo(f"[niryo_moveit] Plan succeeded: {len(trajectory.joint_trajectory.points)} waypoints.")
        response.trajectories = [trajectory]
    else:
        rospy.logwarn(f"[niryo_moveit] Planning failed for pos=({p.x:.3f},{p.y:.3f},{p.z:.3f}).")

    group.clear_pose_targets()
    return response


if __name__ == '__main__':
    moveit_commander.roscpp_initialize(sys.argv)
    rospy.init_node('niryo_moveit_server')
    rospy.Service('niryo_moveit', MoverService, plan)
    rospy.loginfo('[niryo_moveit] Service ready.')
    rospy.spin()
