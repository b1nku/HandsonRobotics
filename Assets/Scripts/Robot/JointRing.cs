using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HandsOnRobotics.Robot
{
    /* Compact per-joint health ring.

    Attach to a child GameObject of each joint link. Requires a World Space
    Canvas child with a Filled/Radial360 Image (the ring) and an optional
    name label. JointStateVisualiser pushes data; JointDetailPanel reads the
    public properties when the user gazes at this ring. */
    [RequireComponent(typeof(SphereCollider))]
    public class JointRing : MonoBehaviour
    {
        [Header("Ring")]
        [SerializeField] Image _ring;
        [SerializeField] TextMeshProUGUI _nameLabel;
        [SerializeField] bool _overlayGeometry = true;

        [Header("Thresholds")]
        [SerializeField] float _tempWarning  = 60f;
        [SerializeField] float _tempCritical = 75f;
        [SerializeField] float _voltWarning  = 11.5f;
        [SerializeField] float _voltCritical = 11.0f;

        static readonly Color ColorOk       = new(0.2f, 0.85f, 0.2f);
        static readonly Color ColorWarning  = new(1.0f, 0.75f, 0.0f);
        static readonly Color ColorCritical = new(0.9f, 0.15f, 0.15f);

        Material _ringMat;

        void Awake()
        {
            if (_ring != null)
            {
                _ringMat = new Material(_ring.material);
                _ring.material = _ringMat;
            }
            ApplyOverlay();
        }

        void ApplyOverlay()
        {
            if (_ringMat == null) return;
            _ringMat.SetInt("unity_GUIZTestMode", _overlayGeometry
                ? (int)UnityEngine.Rendering.CompareFunction.Always
                : (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }

        void OnValidate() => ApplyOverlay();

        public string JointName     { get; private set; }
        public float  Position      { get; private set; }
        public float  Velocity      { get; private set; }
        public float  Effort        { get; private set; }
        public int    Temperature   { get; private set; }
        public double Voltage       { get; private set; }
        public int    HardwareError { get; private set; }

        void LateUpdate()
        {
            if (Camera.main == null || _nameLabel == null) return;
            _nameLabel.transform.rotation = Quaternion.LookRotation(
                _nameLabel.transform.position - Camera.main.transform.position,
                Vector3.up);
        }

        public void SetJointState(string jointName, float position, float velocity, float effort)
        {
            JointName = jointName;
            Position  = position;
            Velocity  = velocity;
            Effort    = effort;
            if (_nameLabel) _nameLabel.text = jointName;
        }

        public void SetHardwareStatus(int temperature, double voltage, int hardwareError)
        {
            Temperature   = temperature;
            Voltage       = voltage;
            HardwareError = hardwareError;
            UpdateRing();
        }

        void UpdateRing()
        {
            if (!_ring) return;
            if      (HardwareError != 0)               _ring.color = ColorCritical;
            else if (Temperature >= _tempCritical)     _ring.color = ColorCritical;
            else if (Voltage     <= _voltCritical)     _ring.color = ColorCritical;
            else if (Temperature >= _tempWarning)      _ring.color = ColorWarning;
            else if (Voltage     <= _voltWarning)      _ring.color = ColorWarning;
            else                                       _ring.color = ColorOk;
        }
    }
}
