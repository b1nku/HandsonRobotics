FROM ros:noetic

SHELL ["/bin/bash", "-c"]

RUN apt-get update && apt-get install -y \
    python3-catkin-tools \
    python-is-python3 \
    ros-noetic-moveit-msgs \
    ros-noetic-moveit-ros-planning-interface \
    ros-noetic-xacro \
    git \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /catkin_ws/src

# ROS <-> Unity TCP bridge (ROS 1 branch)
RUN git clone -b main --depth 1 \
    https://github.com/Unity-Technologies/ROS-TCP-Endpoint.git

# Niryo One message definitions (shallow clone; catkin builds msgs only)
RUN git clone --depth 1 \
    https://github.com/NiryoRobotics/niryo_one_ros.git

# niryo_moveit custom messages (pick-and-place tutorial package)
# Provides niryo_moveit/NiryoMoveitJoints and niryo_moveit/MoverService
RUN git clone --depth 1 \
    https://github.com/Unity-Technologies/Unity-Robotics-Hub.git /tmp/urh && \
    cp -r /tmp/urh/tutorials/pick_and_place/ROS/src/niryo_moveit . && \
    rm -rf /tmp/urh

WORKDIR /catkin_ws

# apt lists were cleaned above; refresh before rosdep.
# moveit_fake_controller_manager was renamed in Noetic final snapshot — skip it.
RUN apt-get update && \
    source /opt/ros/noetic/setup.bash && \
    rosdep install \
        --from-paths src/ROS-TCP-Endpoint src/niryo_one_ros/niryo_one_msgs src/niryo_moveit \
        --ignore-src -r -y \
        --skip-keys="moveit_fake_controller_manager" && \
    catkin build ros_tcp_endpoint niryo_one_msgs niryo_moveit && \
    rm -rf /var/lib/apt/lists/*

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
CMD ["bash"]
