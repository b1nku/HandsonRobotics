# Hands-on Robotics

A VR developer tooling project for operating and programming a ROS-enabled 6 DOF robotic arm (Niryo One). The core research question is whether making **hidden robot state visible in VR** can lower the cognitive load of robot operators and developers, drawing on NASA research supporting VR's potential in this domain.

The design constraint throughout: every visualisation must reduce cognitive load, not add to it. Interaction design principles govern how information is surfaced. We want to explore leveraging skeuomorphic design for this purpose.

**Unity:** 6000.4.0f1 | **Target:** Android (Meta Quest Pro) | **Pipeline:** URP 17.4.0


---

## Research Context

Existing literature indicates VR has considerable potential for lowering operator cognitive load in robotics. The specific research direction explored here is making **hidden variables** (internal robot state not directly visible to a human observer) explicit through VR visualisation.

A 6 DOF arm (versus a mobile platform such as TurtleBot) is chosen because it has a richer set of hidden variables: joint torques and efforts, inverse kinematics solutions, planned trajectories, motor thermal state, reachability envelopes, and collision margins are all quantities the operator cannot perceive directly from watching the physical robot.

---

## Current Project State

| Area | Status |
|---|---|
| Unity project & URP configured | Done |
| Meta XR Core + Interaction SDK 85.0.0 | Installed |
| OpenXR runtime (Quest Pro target) | Configured |
| ROS-TCP-Connector (Unity-Robotics-Hub) | Installed (git) |
| URDF Importer | Installed (git) |
| Niryo One ROS messages generated | Done (`Assets/RosMessages/NiryoOne/`) |
| MoveIt ROS messages generated | Done (`Assets/RosMessages/Moveit/`, `NiryoMoveit/`) |
| Main scene | Exists (`Assets/Scenes/MainScene.unity`) |
| In-scene robot model / prefabs | Imported + working (with jank) |
| Floating tablet UI | Done (`FloatingTablet`, `TabletViewController`) |
| Visualisation features | See direction table below |

| Direction | Status |
|---|---|
| A | Done |
| B | Done |
| C | In progress |
| D | Done |

---

## Research Directions

### A: Joint State & Motor Health Visualisation

**Objective:** Overlay real-time per-joint data directly on the 3D robot model in VR. Each joint shows its current effort (torque), temperature, voltage, and any hardware error state as a spatial annotation attached to that link in the URDF hierarchy.

**Hidden variables surfaced:**
- Joint effort / torque (`sensor_msgs/JointState.effort`)
- Joint velocity and position error
- Per-motor temperature and voltage (`HardwareStatusMsg.temperatures`, `.voltages`)
- Per-motor hardware error codes (`HardwareStatusMsg.hardware_errors`)
- Calibration state (`HardwareStatusMsg.calibration_needed`, `.calibration_in_progress`)

**What we stand to gain:**  
An operator watching the physical arm sees motion but has no access to the forces and thermal state driving it. Joints under excessive load, motors running hot, or link positions diverging from commanded values are invisible until something breaks. Attaching live readouts spatially to each joint gives operators an immediate sense of robot health without requiring them to consult a terminal or RViz. The spatial anchoring also means no visual search cost; the annotation is always at the thing it describes.

**Research value:** Direct test of whether spatially-registered hidden-variable disclosure lowers workload compared to a traditional panel or external display.

**Implementation:**
- `JointRing`: compact radial-fill health ring manually aligned to each joint plane. Color codes health state (green/yellow/red) from temperature, voltage, and hardware error thresholds. The ring does not billboard; it sits in the joint's plane as a physical-feeling indicator. Optional `Overlay Geometry` flag renders it in front of all geometry (ZTest Always via `unity_GUIZTestMode`).
- `JointDetailPanel`: a single shared floating panel that appears on head-gaze dwell (0.3 s default). Shows joint name, position (degrees), velocity (rad/s), effort (Nm), temperature, voltage, and error code. Positions itself between the gazed ring and the camera to avoid clipping through the arm. Renders with TMP Distance Field Overlay shader.
- `JointStateVisualiser`: discovers `JointRing` components via `NiryoOneJointMap`, routes `JointStateMsg` by joint name and `HardwareStatusMsg` by motor index order. Head-gaze raycast drives the detail panel via `SphereCollider` on each ring.

---

### B: Live ROS Topic Inspector

**Objective:** A floating, hand-tracked VR panel that surfaces live values from a configurable set of ROS topics (equivalent to `rostopic echo`), but integrated into the operator's workspace and readable at a glance without leaving VR.

**Hidden variables surfaced:**
- Any subscribed topic; initial targets:
  - `HardwareStatusMsg`: connection state, error messages, calibration flags
  - `ProcessStateMsg`: which ROS process is active and its state
  - `LogStatusMsg`: recent log output
  - `DigitalIOStateMsg`: current I/O pin states
  - `ConveyorFeedbackMsg`: conveyor speed and direction (if attached)
  - `RobotStateMsg`: end-effector position (XYZ) and orientation (RPY)

**What we stand to gain:**  
Having live topic values surfaced directly within the VR experience enables the operator to view relevant variables without the need for external displays or taking the headset off.

**Research value:** Allows comparison of a "dashboard in VR" interaction model against external monitor workflows, and exploration of how information density in the panel affects cognitive load.

**Implementation:**
- `TopicMonitorPanel`: live topic monitor displayed on the floating tablet. Each registered topic gets a row showing topic name, message type, publish rate (Hz calculated over a 3 s sliding window), and a color-coded status dot (grey = never received, green = healthy, yellow = stale, red = dropped). Extensible: any new subscriber calls `TopicMonitorPanel.Register()` and `TopicMonitorPanel.RecordMessage()` and a row appears automatically.
- `TFFrameDisplay` + `TFFrameVisualiser`: replicates RViz's TF display in VR. At runtime, creates colored XYZ axis lines (red/green/blue, tapered to indicate direction) at each joint link, grey connection lines tracing the kinematic chain, and billboarded frame name labels. Configurable axis length, width, and label size. Toggle all frames via `TFFrameDisplay.Toggle()`.
- RViz feature parity goal: robot operators should not feel they are missing information available in RViz. Both the topic monitor and TF display were designed with this in mind.

---

### C: Trajectory Preview & Motion Intent

**Objective:** Before the robot executes a planned move, render its intended trajectory as a ghost/preview in VR, as a translucent shadow of the arm stepping through each waypoint, or a ribbon tracing the end-effector path. The operator sees what the robot is *about to do* before it does it. Extends to VR-native trajectory authoring: the operator places a target in 3D space, MoveIt plans to it, and the result is previewed before anything moves on real hardware.

**Hidden variables surfaced:**
- Full planned joint trajectory (`TrajectoryPlanMsg.trajectory` → `RobotTrajectoryMsg.joint_trajectory`)
- Start state (`TrajectoryPlanMsg.trajectory_start`)
- Waypoint timing (durations between trajectory points)
- End-effector path derived from FK over the trajectory waypoints
- Planning group (`TrajectoryPlanMsg.group_name`)

**What we stand to gain:**  
MoveIt may move the arm in an inappropriate way, so giving the operator a preview before execution may prevent accidents. More importantly, letting the operator author trajectories by placing a spatial target in VR (rather than typing joint angles or using a 2D interface) is the kind of interaction RViz cannot offer. If it works, it also removes the need for a teach pendant.

**Research value:** Tests whether pre-execution intent disclosure reduces operator errors and perceived workload; could also support studies on human-robot trust calibration. The VR trajectory authoring interaction is a novel research contribution in itself.

**Implementation (in progress):**
- `TrajectoryTarget`: a grabbable sphere the operator places at the desired end-effector position. Driven by Meta Interaction SDK hand grab. The sphere's world pose is sent to MoveIt as `pick_pose`.
- `GhostArm`: transparent mesh clone of the robot hierarchy, created at runtime. Animated through MoveIt trajectory waypoints with joint angle interpolation. Also used to precompute the end-effector world path for the ribbon.
- `MoveItPlanner`: wraps `niryo_moveit/MoverService`. Sends current joint angles and target pose; receives `RobotTrajectoryMsg[]`. Target pose is converted from Unity world space to robot-local space before FLU coordinate conversion.
- `TrajectoryPathVisualiser`: LineRenderer ribbon tracing the end-effector world path through all trajectory waypoints.
- `TrajectoryController`: state machine (Idle, Targeting, Planning, Previewing) driven by tablet buttons. Caches current joint angles from `ROSSubscriptionManager`. Execute button is present but wired to a stub pending a robot command publisher.

---

### Floating Tablet UI

All operator-facing panels live on a single world-space floating tablet. The tablet is a grabbable rigid body with cylindrical handles on the left and right edges. When released, it retains the hand's velocity and drifts in place (no gravity, low drag) -- the microgravity interaction model felt appropriate for a tool meant to reduce cognitive load. Grab handles use the Meta Interaction SDK; the `FloatingTablet` script configures the `Rigidbody` and provides a `ReturnHome()` method.

`TabletViewController` manages named views via prev/next navigation buttons with a view name label. Adding a new view requires only appending to the `_views` and `_viewNames` arrays in the Inspector. Current views: Topics (topic monitor), Planning (trajectory authoring controls).

---

### D: Scene & Interaction Architecture

**Objective:** Establish the scene hierarchy, component organisation, and hand-tracked interaction design system that directions A–C will be built on. This includes: the robot model import and link-to-GameObject mapping, the VR interaction rig (Meta XR hand tracking), a consistent spatial UI component library (panels, annotations, gauges), and a ROS subscription manager that feeds all visualisation components.

**What we stand to gain:**  
Without a deliberate architecture, each visualisation feature ends up with its own ad-hoc approach to polling ROS, positioning UI elements, and responding to hand input. The result accumulates cognitive overhead for both the operator using the tool and the researchers extending it. A shared architecture means:

- One `ROSSubscriptionManager` component owns all topic subscriptions; visualisation components only read from it.
- A spatial UI component library enforces visual consistency (same annotation style, same gauge style, same font), reducing perceptual noise.
- Link-to-GameObject mapping is defined once; Joint State Visualisation (A) and Trajectory Preview (C) both reference it.
- Hand interaction uses Meta XR's `InteractorGroup` consistently, so the operator's learned interaction model transfers across all features.

**Research value:** The architecture itself is a research artefact: a demonstration that VR robotics tooling can be structured to *not* add cognitive overhead through inconsistent interaction patterns or visual clutter.

---

## Setup

### Prerequisites

| Package | Version |
|---|---|
| Meta XR Core SDK | 85.0.0 |
| Meta XR Interaction SDK OVR | 85.0.0 |
| com.unity.xr.openxr | 1.16.1 |
| com.unity.xr.meta-openxr | 2.4.0 |
| com.unity.robotics.ros-tcp-connector | git (Unity-Robotics-Hub) |
| com.unity.robotics.urdf-importer | git (Unity-Robotics-Hub) |
| com.unity.render-pipelines.universal | 17.4.0 |

Open `Unity_Projects/Virtual_Reality/Hands-on Robotics/` in **Unity 6000.4.0f1**. Build target: **Android**.

Do not hand-edit files under `Assets/RosMessages/`: regenerate via **Robotics > Generate ROS Messages** in the Unity menu if the ROS interface changes.

---

### ROS (Docker)

ROS 1 Noetic has no native Arch packages. The `docker/` directory contains a Docker setup with:

- `roscore`
- `ros-tcp-endpoint` (Unity bridge, port 10000)
- `niryo_one_msgs` message definitions
- `niryo_moveit` custom messages (`NiryoMoveitJoints`, `MoverService`)
- MoveIt msgs and planning interface

**Build and start:**

```bash
cd docker
docker compose up --build
```

Unity connects to `localhost:10000` (set in **Robotics > ROS Settings** in the editor, or via `ROSConnectionPrefab`).

**On-device (Quest Pro over wifi):** Unity's ROS Settings IP must be your machine's LAN IP (not localhost). The port mapping handles the rest.

**Attach a shell to the running ROS environment:**

```bash
docker compose exec ros-tcp-endpoint bash
```

---

### Linux Developers: Meta XR Simulator Patch

Meta does not ship a Linux binary for the XR Simulator (automatically triggered by Core SDK 85.0.0). Apply this patch before opening the project, then **do not click any simulator install prompts**.

1. Install Core SDK 85.0.0 via Package Manager.
2. Locate `Installer.cs` at:
   ```
   Library/PackageCache/com.meta.xr.sdk.core@<hash>/Editor/MetaXRSimulator/Installer.cs
   ```
3. Replace lines 81–88 with:

```csharp
#if UNITY_EDITOR_WIN
            var downloadedInstallerPath =
                            Path.Combine(XRSimConstants.DownloadFolderPath, $"meta_xr_simulator.msi");
#elif UNITY_EDITOR_OSX
            var downloadedInstallerPath =
                            Path.Combine(XRSimConstants.DownloadFolderPath, $"meta_xr_simulator.dmg");
#else
            var downloadedInstallerPath = string.Empty;
            return false;
#endif
            if (!await DownloadInstaller(downloadedInstallerPath, downloadUrl, errorMessage =>
                {
                    onError?.Invoke(errorMessage);
                }))
            {
                return false;
            }
```

4. In case off DllNotFoundException libdl:
On modern glibc (Arch and CachyOS) libdl was merged into libc. We can create a symlink to circumvent this issue.

```bash
sudo ln -s /usr/lib/libdl.so.2 /usr/lib/libdl.so
```
