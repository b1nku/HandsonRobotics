using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HandsOnRobotics.Robot
{
    /* Single shared detail panel that floats near whichever joint is gazed at.

    Place once in the scene (not a child of any joint). JointStateVisualiser
    calls Bind(ring)/Unbind() and Refresh() as gaze changes. The panel
    positions itself between the active ring and the camera, offset upward,
    so it never clips through the robot. */
    [RequireComponent(typeof(Canvas))]
    public class JointDetailPanel : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] TextMeshProUGUI _nameLabel;
        [SerializeField] TextMeshProUGUI _positionLabel;
        [SerializeField] TextMeshProUGUI _velocityLabel;
        [SerializeField] TextMeshProUGUI _effortLabel;
        [SerializeField] TextMeshProUGUI _temperatureLabel;
        [SerializeField] TextMeshProUGUI _voltageLabel;
        [SerializeField] TextMeshProUGUI _errorLabel;

        [Header("Positioning")]
        [Tooltip("How far toward the camera to push the panel (metres).")]
        [SerializeField] float _offsetForward = 0.15f;
        [Tooltip("How far above the ring to push the panel (metres).")]
        [SerializeField] float _offsetUp = 0.08f;

        static readonly Color ColorOk       = new(0.2f, 0.85f, 0.2f);
        static readonly Color ColorCritical = new(0.9f, 0.15f, 0.15f);

        Canvas _canvas;
        JointRing _bound;

        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
        }

        void LateUpdate()
        {
            if (_bound == null || Camera.main == null) return;

            Vector3 toCam = (Camera.main.transform.position - _bound.transform.position).normalized;
            transform.position = _bound.transform.position
                                 + toCam    * _offsetForward
                                 + Vector3.up * _offsetUp;

            transform.rotation = Quaternion.LookRotation(
                transform.position - Camera.main.transform.position,
                Vector3.up);
        }

        public void Bind(JointRing ring)
        {
            _bound = ring;
            _canvas.enabled = true;
            Refresh();
        }

        public void Unbind()
        {
            _bound = null;
            _canvas.enabled = false;
        }

        public void Refresh()
        {
            if (_bound == null) return;

            if (_nameLabel)        _nameLabel.text        = _bound.JointName;
            if (_positionLabel)    _positionLabel.text    = $"{Mathf.Rad2Deg * _bound.Position:+0.0;-0.0}°";
            if (_velocityLabel)    _velocityLabel.text    = $"{_bound.Velocity:+0.00;-0.00} r/s";
            if (_effortLabel)      _effortLabel.text      = $"{_bound.Effort:+0.00;-0.00} Nm";
            if (_temperatureLabel) _temperatureLabel.text = $"{_bound.Temperature} °C";
            if (_voltageLabel)     _voltageLabel.text     = $"{_bound.Voltage:0.00} V";
            if (_errorLabel)
            {
                _errorLabel.text  = _bound.HardwareError == 0 ? "OK" : $"ERR {_bound.HardwareError}";
                _errorLabel.color = _bound.HardwareError == 0 ? ColorOk : ColorCritical;
            }
        }
    }
}
