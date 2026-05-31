using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Trajectory;
using UnityEngine;

namespace HandsOnRobotics.Robot
{
    /* Direction C: transparent ghost arm used to preview planned trajectories.

    Clones the real robot's mesh hierarchy at runtime and applies a transparent
    material. Joint angles are applied by rotating each cloned joint transform
    around a configurable local axis. Call AnimateTrajectory() to play back a
    MoveIt trajectory response.

    JOINT AXES: these must match the URDF joint axes as imported. Default is
    Vector3.forward (local Z) for all joints. Adjust in the Inspector if the
    ghost arm poses incorrectly. */
    public class GhostArm : MonoBehaviour
    {
        [SerializeField] Transform _robotRoot;
        [SerializeField] Material  _ghostMaterial;

        [Tooltip("Local rotation axis per joint (joint_1..joint_6). Default Z for all.")]
        [SerializeField] Vector3[] _jointAxes = new Vector3[]
        {
            Vector3.forward, Vector3.forward, Vector3.forward,
            Vector3.forward, Vector3.forward, Vector3.forward
        };

        Transform[] _ghostJoints;
        Quaternion[] _neutralRotations;
        string[]     _rosNames;
        Transform    _endEffector;
        Coroutine    _animation;

        void Start()
        {
            var map = NiryoOneJointMap.Instance;
            if (map == null)        { Debug.LogError("[GhostArm] NiryoOneJointMap instance not found."); return; }
            if (_robotRoot == null) { Debug.LogError("[GhostArm] Robot Root not assigned."); return; }
            if (_ghostMaterial == null) Debug.LogWarning("[GhostArm] Ghost Material not assigned -- ghost will be invisible.");

            var ghostRoot = CloneHierarchy(_robotRoot, transform);
            ghostRoot.position = _robotRoot.position;
            ghostRoot.rotation = _robotRoot.rotation;

            var joints = map.AllJoints;
            int count  = joints.Count;
            _ghostJoints      = new Transform[count];
            _neutralRotations = new Quaternion[count];
            _rosNames         = new string[count];

            int resolved = 0;
            for (int i = 0; i < count; i++)
            {
                var realLink = joints[i].link;
                _rosNames[i] = joints[i].rosName;
                if (realLink == null)
                {
                    Debug.LogWarning($"[GhostArm] Joint {i} ({joints[i].rosName}): link not assigned in NiryoOneJointMap.");
                    continue;
                }
                _ghostJoints[i]      = FindByName(ghostRoot, realLink.name);
                _neutralRotations[i] = _ghostJoints[i] != null ? _ghostJoints[i].localRotation : Quaternion.identity;
                if (_ghostJoints[i] != null) resolved++;
                else Debug.LogWarning($"[GhostArm] Could not find '{realLink.name}' in cloned hierarchy.");

                // Auto-detect rotation axis from ArticulationBody on the real link.
                // parentAnchorRotation gives the anchor frame in the parent link's space;
                // revolute joints rotate around that anchor's X axis.
                var ab = realLink.GetComponent<ArticulationBody>();
                if (ab != null && ab.jointType == ArticulationJointType.RevoluteJoint)
                {
                    var detected = ab.parentAnchorRotation * Vector3.right;
                    if (i < _jointAxes.Length) _jointAxes[i] = detected;
                    Debug.Log($"[GhostArm] Joint {i} ({joints[i].rosName}): axis auto-detected = {detected}");
                }
                else
                {
                    var fallback = i < _jointAxes.Length ? _jointAxes[i] : Vector3.forward;
                    Debug.Log($"[GhostArm] Joint {i} ({joints[i].rosName}): no ArticulationBody, using configured axis = {fallback}");
                }
            }

            _endEffector = _ghostJoints[count - 1];
            Debug.Log($"[GhostArm] Initialised: {resolved}/{count} joints resolved, end-effector = {(_endEffector != null ? _endEffector.name : "NULL")}.");
            gameObject.SetActive(false);
        }

        /* Set all joint angles (radians). Expects angles in joint_1..joint_6 order. */
        public void SetJointAngles(double[] angles)
        {
            for (int i = 0; i < Mathf.Min(angles.Length, _ghostJoints.Length); i++)
            {
                if (_ghostJoints[i] == null) continue;
                var axis = i < _jointAxes.Length ? _jointAxes[i] : Vector3.forward;
                // Axis is in parent-local space; left-multiply so the delta operates
                // in parent frame with the neutral rotation as the base.
                _ghostJoints[i].localRotation =
                    Quaternion.AngleAxis((float)(Mathf.Rad2Deg * angles[i]), axis)
                    * _neutralRotations[i];
            }
        }

        public Vector3 GetEndEffectorPosition()
            => _endEffector != null ? _endEffector.position : transform.position;

        /* Precomputes the end-effector world path from a trajectory without animating.
           Use this to build the path ribbon before starting playback. */
        public Vector3[] ComputePath(JointTrajectoryMsg traj)
        {
            var nameToIndex = BuildNameIndex(traj.joint_names);
            var path = new Vector3[traj.points.Length];
            for (int i = 0; i < traj.points.Length; i++)
            {
                ApplyTrajectoryPoint(traj.points[i].positions, nameToIndex);
                path[i] = GetEndEffectorPosition();
            }
            return path;
        }

        /* Animate the ghost arm through a MoveIt trajectory. Calls onComplete when done. */
        public void AnimateTrajectory(JointTrajectoryMsg traj, Action onComplete = null)
        {
            if (_animation != null) StopCoroutine(_animation);
            gameObject.SetActive(true);
            _animation = StartCoroutine(AnimateCoroutine(traj, onComplete));
        }

        public void StopAnimation()
        {
            if (_animation != null) { StopCoroutine(_animation); _animation = null; }
            gameObject.SetActive(false);
        }

        IEnumerator AnimateCoroutine(JointTrajectoryMsg traj, Action onComplete)
        {
            var nameToIndex = BuildNameIndex(traj.joint_names);
            var points      = traj.points;
            float startTime = Time.time;

            for (int i = 0; i < points.Length - 1; i++)
            {
                float t0 = PointTime(points[i]);
                float t1 = PointTime(points[i + 1]);

                while (true)
                {
                    float elapsed = Time.time - startTime;
                    if (elapsed >= t1) break;
                    float lerp = Mathf.InverseLerp(t0, t1, elapsed);

                    var angles = new double[6];
                    for (int j = 0; j < angles.Length; j++)
                    {
                        if (!nameToIndex.TryGetValue(_rosNames[j], out int idx)) continue;
                        if (idx < points[i].positions.Length && idx < points[i + 1].positions.Length)
                            angles[j] = Mathf.Lerp(
                                (float)points[i].positions[idx],
                                (float)points[i + 1].positions[idx], lerp);
                    }
                    SetJointAngles(angles);
                    yield return null;
                }
            }

            // Snap to final pose
            ApplyTrajectoryPoint(points[points.Length - 1].positions, nameToIndex);
            onComplete?.Invoke();
        }

        void ApplyTrajectoryPoint(double[] positions, Dictionary<string, int> nameToIndex)
        {
            var angles = new double[_rosNames.Length];
            for (int i = 0; i < _rosNames.Length; i++)
            {
                if (nameToIndex.TryGetValue(_rosNames[i], out int idx) && idx < positions.Length)
                    angles[i] = positions[idx];
            }
            SetJointAngles(angles);
        }

        Dictionary<string, int> BuildNameIndex(string[] jointNames)
        {
            var d = new Dictionary<string, int>();
            for (int i = 0; i < jointNames.Length; i++)
                d[jointNames[i]] = i;
            return d;
        }

        static float PointTime(JointTrajectoryPointMsg p)
            => p.time_from_start.sec + p.time_from_start.nanosec * 1e-9f;

        Transform CloneHierarchy(Transform original, Transform parent)
        {
            var go = new GameObject(original.name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = original.localPosition;
            go.transform.localRotation = original.localRotation;
            go.transform.localScale    = original.localScale;

            var mf = original.GetComponent<MeshFilter>();
            var mr = original.GetComponent<MeshRenderer>();
            if (mf != null && mr != null && _ghostMaterial != null)
            {
                go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = _ghostMaterial;
            }

            foreach (Transform child in original)
                CloneHierarchy(child, go.transform);

            return go.transform;
        }

        Transform FindByName(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindByName(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
