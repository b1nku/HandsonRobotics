# Hands-on Robotics

A VR developer tooling project for operating and programming a ROS-enabled 6 DOF robotic arm (Niryo One). The core research question is whether making **hidden robot state visible in VR** can lower the cognitive load of robot operators and developers, drawing on NASA research supporting VR's potential in this domain.

The design constraint throughout: every visualisation must reduce cognitive load, not add to it. Interaction design principles govern how information is surfaced. We want to explore leveraging skeuomorphic design for this purpose.

**Unity:** 6000.4.0f1 | **Target:** Android (Meta Quest Pro) | **Pipeline:** URP 17.4.0

---

## Research Context

Existing literature indicates VR has considerable potential for lowering operator cognitive load in robotics. The specific research direction explored here is making **hidden variables** (internal robot state not directly visible to a human observer) explicit through VR visualisation.

A 6 DOF arm (versus a mobile platform such as TurtleBot) is chosen because it has a richer set of hidden variables: joint torques and efforts, inverse kinematics solutions, planned trajectories, motor thermal state, reachability envelopes, and collision margins are all quantities the operator cannot perceive directly from watching the physical robot.

---

## Project State

| Area | Status |
|---|---|
| Unity project & URP configured | Done |
| Meta XR Core + Interaction SDK 85.0.0 | Installed |
| Meta XR MR Utility Kit | Installed |
| OpenXR runtime (Quest Pro target) | Configured |
| Passthrough (color, underlay, reconstruction) | Configured |
| Scene understanding + Effect Mesh colliders | Configured (MRUK building blocks) |
| ROS-TCP-Connector (Unity-Robotics-Hub) | Installed (git) |
| URDF Importer | Installed (git) |
| Niryo One ROS messages generated | Done (`Assets/RosMessages/NiryoOne/`) |
| MoveIt ROS messages generated | Done (`Assets/RosMessages/Moveit/`, `NiryoMoveit/`) |
| Main scene | Exists (`Assets/Scenes/MainScene.unity`) |
| In-scene robot model / prefabs | Imported + working (with jank) |
| Floating tablet UI | Done (`FloatingTablet`, `TabletViewController`) |

| Direction | Description | Status |
|---|---|---|
| A | Joint state & motor health visualisation | Done |
| B | Live ROS topic inspector | Done |
| C | Trajectory preview & motion intent | Done |
| D | Scene & interaction architecture | Done |
| E | Reset target button | Done |
| F | Workspace envelope visualisation | Done |
| G | Trajectory target billboard | Done |
| H | ROS debug wall overlay | Done |
| I | Mixed reality passthrough & physical anchoring | In progress |

See [docs/DIRECTIONS.md](docs/DIRECTIONS.md) for full direction descriptions and implementation details.

---

## Floating Tablet UI

All operator-facing panels live on a single world-space floating tablet. The tablet is a grabbable rigid body with cylindrical handles on the left and right edges. When released, it retains the hand's velocity and drifts in place (no gravity, low drag) -- the microgravity interaction model felt appropriate for a tool meant to reduce cognitive load. In mixed reality mode the tablet collides with real room geometry via Effect Mesh colliders. Grab handles use the Meta Interaction SDK; the `FloatingTablet` script configures the `Rigidbody` and provides a `ReturnHome()` method wired to a tablet button.

`TabletViewController` manages named views via prev/next navigation buttons with a view name label. Adding a new view requires only appending to the `_views` and `_viewNames` arrays in the Inspector. Current views: Topics, Planning, ROS Config.

---

## Setup

See [docs/SETUP.md](docs/SETUP.md) for full setup instructions including ROS/Docker, prerequisites, and Linux-specific workarounds.

### Test Day

See [docs/TEST_DAY.md](docs/TEST_DAY.md) for the step-by-step guide to sideloading, network setup, and running a test session on campus.
