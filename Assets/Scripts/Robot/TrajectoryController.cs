using HandsOnRobotics.ROS;
using RosMessageTypes.Sensor;
using UnityEngine;
using TMPro;

namespace HandsOnRobotics.Robot
{
    /* Direction C: orchestrates the VR trajectory planning loop.

    States:
      Idle       -- target sphere hidden, ghost arm hidden
      Targeting  -- user has placed the target sphere, ready to plan
      Planning   -- MoveIt service call in flight
      Previewing -- trajectory received, ghost arm animating, path ribbon visible

    Wire up references in the Inspector. The target sphere is a scene GO
    the operator grabs and places -- make it grabbable via the Meta Interaction
    SDK (Grabbable + HandGrabInteractable). The tablet Plan/Execute/Cancel
    buttons call the public methods on this component. */
    public class TrajectoryController : MonoBehaviour
    {
        public enum State { Idle, Targeting, Planning, Previewing }

        [Header("Scene references")]
        [SerializeField] Transform                 _trajectoryTarget;
        [SerializeField] GhostArm                  _ghostArm;
        [SerializeField] MoveItPlanner             _planner;
        [SerializeField] TrajectoryPathVisualiser  _pathVisualiser;

        [Header("Direction F: workspace envelope")]
        [SerializeField] WorkspaceEnvelope _workspaceEnvelope;

        [Header("Tablet UI")]
        [SerializeField] TextMeshProUGUI _statusLabel;
        [SerializeField] UnityEngine.UI.Button _planButton;
        [SerializeField] UnityEngine.UI.Button _executeButton;
        [SerializeField] UnityEngine.UI.Button _cancelButton;
        [SerializeField] UnityEngine.UI.Button _resetTargetButton;

        [Header("Direction H: ROS debug overlay")]
        [SerializeField] HandsOnRobotics.UI.ROSDebugOverlay _rosDebugOverlay;
        [SerializeField] UnityEngine.UI.Button _debugToggleButton;

        State    _state = State.Idle;
        double[] _currentJoints = new double[6];
        Vector3    _targetSpawnPos;
        Quaternion _targetSpawnRot;

        void OnEnable()  => ROSSubscriptionManager.OnJointState += CacheJoints;
        void OnDisable() => ROSSubscriptionManager.OnJointState -= CacheJoints;

        void Start()
        {
            if (_trajectoryTarget != null)
                _trajectoryTarget.gameObject.SetActive(false);

            _resetTargetButton?.onClick.AddListener(ResetTarget);
            _debugToggleButton?.onClick.AddListener(() => _rosDebugOverlay?.Toggle());

            SetState(State.Idle);
        }

        /* Called by a "Place Target" tablet button to activate the target sphere. */
        public void ActivateTarget()
        {
            if (_state != State.Idle) return;
            _trajectoryTarget.gameObject.SetActive(true);
            _targetSpawnPos = _trajectoryTarget.position;
            _targetSpawnRot = _trajectoryTarget.rotation;
            SetState(State.Targeting);
        }

        /* Direction E: snaps the target sphere back to where it appeared when activated.
           Clears any current preview so the user can re-plan from the fresh position. */
        public void ResetTarget()
        {
            if (_state != State.Targeting && _state != State.Previewing) return;
            _trajectoryTarget.SetPositionAndRotation(_targetSpawnPos, _targetSpawnRot);
            _ghostArm?.StopAnimation();
            _pathVisualiser?.Clear();
            if (_state == State.Previewing) SetState(State.Targeting);
        }

        /* Called by the Plan tablet button. */
        public void Plan()
        {
            Debug.Log($"[TrajectoryController] Plan() called. Current state: {_state}, planner: {(_planner != null ? "assigned" : "NULL")}");
            if (_state != State.Targeting) return;
            if (_planner == null) { Debug.LogError("[TrajectoryController] MoveItPlanner not assigned."); return; }

            SetState(State.Planning);
            _ghostArm?.StopAnimation();
            _pathVisualiser?.Clear();

            _planner.Plan(
                _trajectoryTarget.position,
                _trajectoryTarget.rotation,
                _currentJoints,
                trajectories =>
                {
                    if (trajectories == null || trajectories.Length == 0)
                    {
                        Debug.LogWarning("[TrajectoryController] MoveIt returned no trajectories.");
                        SetState(State.Targeting);
                        return;
                    }

                    var traj = trajectories[0].joint_trajectory;
                    if (traj.points.Length == 0)
                    {
                        Debug.LogWarning("[TrajectoryController] Trajectory has no points.");
                        SetState(State.Targeting);
                        return;
                    }

                    // Build path ribbon from ghost arm FK
                    if (_ghostArm != null)
                    {
                        var path = _ghostArm.ComputePath(traj);
                        _pathVisualiser?.BuildPath(path);
                        _pathVisualiser?.Show();
                        _ghostArm.AnimateTrajectory(traj, onComplete: null);
                    }

                    SetState(State.Previewing);
                });
        }

        /* Called by the Cancel tablet button. */
        public void Cancel()
        {
            _ghostArm?.StopAnimation();
            _pathVisualiser?.Clear();
            _trajectoryTarget?.gameObject.SetActive(false);
            SetState(State.Idle);
        }

        /* Called by the Execute tablet button. Placeholder -- execution requires
           sending joint commands back to the real robot, which is out of scope for
           the Direction C preview. Wire this to your robot command publisher when ready. */
        public void Execute()
        {
            if (_state != State.Previewing) return;
            Debug.Log("[TrajectoryController] Execute requested -- implement robot command publisher.");
        }

        void CacheJoints(JointStateMsg msg)
        {
            var map = NiryoOneJointMap.Instance;
            if (map == null) return;

            for (int i = 0; i < map.AllJoints.Count && i < msg.name.Length; i++)
            {
                for (int j = 0; j < msg.name.Length; j++)
                {
                    if (msg.name[j] == map.AllJoints[i].rosName && j < msg.position.Length)
                    {
                        _currentJoints[i] = msg.position[j];
                        break;
                    }
                }
            }
        }

        void SetState(State next)
        {
            _state = next;

            if (_statusLabel) _statusLabel.text = next switch
            {
                State.Idle       => "Idle",
                State.Targeting  => "Target placed -- ready to plan",
                State.Planning   => "Planning...",
                State.Previewing => "Preview ready",
                _                => ""
            };

            if (_planButton)         _planButton.interactable         = next == State.Targeting;
            if (_executeButton)      _executeButton.interactable      = next == State.Previewing;
            if (_cancelButton)       _cancelButton.interactable       = next != State.Idle && next != State.Planning;
            if (_resetTargetButton)  _resetTargetButton.interactable  = next == State.Targeting || next == State.Previewing;

            _workspaceEnvelope?.SetVisible(next == State.Targeting);
        }
    }
}
