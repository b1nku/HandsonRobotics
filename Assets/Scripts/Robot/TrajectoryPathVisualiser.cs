using UnityEngine;

namespace HandsOnRobotics.Robot
{
    /* Draws a LineRenderer ribbon along the end-effector path of a planned trajectory.
    Call BuildPath() with world-space waypoints computed by GhostArm.ComputePath(),
    then Show()/Hide() to toggle visibility. */
    public class TrajectoryPathVisualiser : MonoBehaviour
    {
        [SerializeField] Material _pathMaterial;
        [SerializeField] float    _lineWidth   = 0.004f;
        [SerializeField] Color    _pathColor   = new(0.4f, 0.8f, 1f, 0.9f);

        LineRenderer _line;

        void Awake()
        {
            _line = gameObject.AddComponent<LineRenderer>();
            _line.material       = _pathMaterial != null
                ? _pathMaterial
                : new Material(Shader.Find("Sprites/Default"));
            _line.startColor     = _pathColor;
            _line.endColor       = _pathColor;
            _line.startWidth     = _lineWidth;
            _line.endWidth       = _lineWidth;
            _line.useWorldSpace  = true;
            _line.positionCount  = 0;
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _line.receiveShadows = false;
            _line.enabled        = false;
        }

        public void BuildPath(Vector3[] waypoints)
        {
            _line.positionCount = waypoints.Length;
            _line.SetPositions(waypoints);
        }

        public void Show() => _line.enabled = true;
        public void Hide() => _line.enabled = false;
        public void Clear()
        {
            _line.positionCount = 0;
            _line.enabled = false;
        }
    }
}
