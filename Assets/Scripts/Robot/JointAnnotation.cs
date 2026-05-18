using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HandsOnRobotics.Robot
{
    /* World-space label attached to a robot link.

    Attach this to a child GameObject of each link (shoulder_link, arm_link, etc.)
    that has a World Space Canvas. Wire up the TextMeshPro and Image references
    in the Inspector. JointStateVisualiser calls the Set* methods to push data.

    Recommended Canvas settings:
      Render Mode:        World Space
      Width/Height:       200 x 130
      Dynamic Pixels Per Unit: 10
      Scale:              0.001 on all axes (so 1 canvas unit = 1mm) */
    public class JointAnnotation : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] TextMeshProUGUI _nameLabel;
        [SerializeField] TextMeshProUGUI _positionLabel;
        [SerializeField] TextMeshProUGUI _velocityLabel;
        [SerializeField] TextMeshProUGUI _effortLabel;
        [SerializeField] TextMeshProUGUI _temperatureLabel;
        [SerializeField] TextMeshProUGUI _voltageLabel;
        [SerializeField] TextMeshProUGUI _errorLabel;

        [Header("Health indicator")]
        [SerializeField] Image _healthBar;

        [Header("Thresholds")]
        [SerializeField] float _tempWarning  = 60f;
        [SerializeField] float _tempCritical = 75f;
        [SerializeField] float _voltWarning  = 11.5f;
        [SerializeField] float _voltCritical = 11.0f;

        static readonly Color ColorOk       = new(0.2f, 0.85f, 0.2f);
        static readonly Color ColorWarning  = new(1.0f, 0.75f, 0.0f);
        static readonly Color ColorCritical = new(0.9f, 0.15f, 0.15f);

        float  _position;
        float  _velocity;
        float  _effort;
        int    _temperature;
        double _voltage;
        int    _hardwareError;

        void LateUpdate()
        {
            // Always face the main camera.
            if (Camera.main != null)
                transform.rotation = Quaternion.LookRotation(
                    transform.position - Camera.main.transform.position);
        }

        public void SetJointState(string jointName, float position, float velocity, float effort)
        {
            _position = position;
            _velocity = velocity;
            _effort   = effort;

            if (_nameLabel)     _nameLabel.text     = jointName;
            if (_positionLabel) _positionLabel.text = $"{Mathf.Rad2Deg * position:+0.0;-0.0}°";
            if (_velocityLabel) _velocityLabel.text = $"{velocity:+0.00;-0.00} r/s";
            if (_effortLabel)   _effortLabel.text   = $"{effort:+0.00;-0.00} Nm";

            UpdateHealthColor();
        }

        public void SetHardwareStatus(int temperature, double voltage, int hardwareError)
        {
            _temperature   = temperature;
            _voltage       = voltage;
            _hardwareError = hardwareError;

            if (_temperatureLabel) _temperatureLabel.text = $"{temperature} °C";
            if (_voltageLabel)     _voltageLabel.text     = $"{voltage:0.00} V";
            if (_errorLabel)
            {
                _errorLabel.text  = hardwareError == 0 ? "OK" : $"ERR {hardwareError}";
                _errorLabel.color = hardwareError == 0 ? ColorOk : ColorCritical;
            }

            UpdateHealthColor();
        }

        void UpdateHealthColor()
        {
            if (_hardwareError != 0)      { SetHealth(ColorCritical); return; }
            if (_temperature >= _tempCritical) { SetHealth(ColorCritical); return; }
            if (_voltage     <= _voltCritical) { SetHealth(ColorCritical); return; }
            if (_temperature >= _tempWarning)  { SetHealth(ColorWarning);  return; }
            if (_voltage     <= _voltWarning)  { SetHealth(ColorWarning);  return; }
            SetHealth(ColorOk);
        }

        void SetHealth(Color color)
        {
            if (_healthBar) _healthBar.color = color;
        }
    }
}
