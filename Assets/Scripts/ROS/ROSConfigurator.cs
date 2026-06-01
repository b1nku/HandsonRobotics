using System.Text.RegularExpressions;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;

namespace HandsOnRobotics.ROS
{
    /* Runtime ROS endpoint configurator.

    Reads the saved IP/port from PlayerPrefs before ROSConnection.Start() runs
    (execution order -100). The tablet ROS Config view lets the user type an IP
    and port manually; pressing Save stores them to PlayerPrefs for the next launch.

    Tablet view setup:
      1. Add a new panel to the tablet (add to TabletViewController _views).
      2. Add two TMP_InputFields (IP and Port), a Save button, and two status labels.
      3. Wire references in the Inspector on this component.
      4. Focusing an InputField calls TouchScreenKeyboard.Open() to raise the Quest
         system keyboard; OpenKeyboard/Update poll its text back into the field. */
    [DefaultExecutionOrder(-100)]
    public class ROSConfigurator : MonoBehaviour
    {
        const string PrefKey_IP   = "ROS_IP";
        const string PrefKey_Port = "ROS_PORT";

        [SerializeField] string _defaultIP   = "192.168.1.100";
        [SerializeField] int    _defaultPort = 10000;

        [Header("Tablet - ROS Config view")]
        [SerializeField] TMP_InputField  _ipInput;
        [SerializeField] TMP_InputField  _portInput;
        [SerializeField] Button          _saveButton;
        [SerializeField] TextMeshProUGUI _activeLabel;   // "Active: x.x.x.x:port"
        [SerializeField] TextMeshProUGUI _pendingLabel;  // "Saved - restart to apply"

        TouchScreenKeyboard _overlayKeyboard;
        TMP_InputField      _activeField;

        void Awake()
        {
            string ip   = PlayerPrefs.GetString(PrefKey_IP,   _defaultIP);
            int    port = PlayerPrefs.GetInt   (PrefKey_Port,  _defaultPort);

            var ros = ROSConnection.GetOrCreateInstance();
            ros.RosIPAddress = ip;
            ros.RosPort      = port;
        }

        void Start()
        {
            var ros = ROSConnection.GetOrCreateInstance();

            if (_ipInput)
            {
                _ipInput.contentType    = TMP_InputField.ContentType.Standard;
                _ipInput.characterLimit = 15;
                _ipInput.text           = ros.RosIPAddress;

                // Per-character: allow digits and dots only
                _ipInput.onValidateInput += (text, index, addedChar) =>
                    (char.IsDigit(addedChar) || addedChar == '.') ? addedChar : '\0';

                // On submit: revert to active IP if not a valid IPv4 address
                _ipInput.onEndEdit.AddListener(value =>
                {
                    if (!Regex.IsMatch(value, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
                        _ipInput.text = ROSConnection.GetOrCreateInstance().RosIPAddress;
                });
            }

            if (_portInput)
            {
                _portInput.contentType    = TMP_InputField.ContentType.IntegerNumber;
                _portInput.characterLimit = 5;
                _portInput.text           = ros.RosPort.ToString();

                // On submit: revert to active port if out of valid range
                _portInput.onEndEdit.AddListener(value =>
                {
                    if (!int.TryParse(value, out int p) || p < 1 || p > 65535)
                        _portInput.text = ROSConnection.GetOrCreateInstance().RosPort.ToString();
                });
            }

            _saveButton?.onClick.AddListener(Save);

            _ipInput?.onSelect.AddListener(_   => OpenKeyboard(_ipInput));
            _portInput?.onSelect.AddListener(_ => OpenKeyboard(_portInput));

            SetPendingVisible(false);
            RefreshActiveLabel();
        }

        void OpenKeyboard(TMP_InputField field)
        {
            _activeField      = field;
            _overlayKeyboard  = TouchScreenKeyboard.Open(
                field.text, TouchScreenKeyboardType.Default);
        }

        void Update()
        {
            if (_overlayKeyboard == null || _activeField == null) return;
            var status = _overlayKeyboard.status;
            if (status == TouchScreenKeyboard.Status.Visible)
            {
                _activeField.text = _overlayKeyboard.text;
            }
            else if (status == TouchScreenKeyboard.Status.Done ||
                     status == TouchScreenKeyboard.Status.LostFocus)
            {
                _activeField.text = _overlayKeyboard.text;
                _overlayKeyboard  = null;
                _activeField      = null;
            }
            else if (status == TouchScreenKeyboard.Status.Canceled)
            {
                // Dismissed without confirming: drop the reference, leave the field as-is.
                _overlayKeyboard  = null;
                _activeField      = null;
            }
        }

        public void Save()
        {
            string ip   = _ipInput  != null ? _ipInput.text.Trim()  : string.Empty;
            string port = _portInput != null ? _portInput.text.Trim() : string.Empty;

            // Validate here too: text set programmatically from the system keyboard
            // bypasses the field's onValidateInput / onEndEdit guards.
            if (!Regex.IsMatch(ip, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
            {
                SetPendingVisible(false);
                if (_pendingLabel != null)
                {
                    _pendingLabel.text    = "Invalid IP";
                    _pendingLabel.enabled = true;
                }
                return;
            }
            if (!int.TryParse(port, out int portNum) || portNum < 1 || portNum > 65535)
                portNum = _defaultPort;

            PlayerPrefs.SetString(PrefKey_IP,   ip);
            PlayerPrefs.SetInt   (PrefKey_Port,  portNum);
            PlayerPrefs.Save();

            SetPendingVisible(true);
        }

        void RefreshActiveLabel()
        {
            if (_activeLabel == null) return;
            var ros = ROSConnection.GetOrCreateInstance();
            _activeLabel.text = $"Active: {ros.RosIPAddress}:{ros.RosPort}";
        }

        void SetPendingVisible(bool visible)
        {
            if (_pendingLabel == null) return;
            _pendingLabel.text    = visible ? "Saved - restart to apply" : string.Empty;
            _pendingLabel.enabled = visible;
        }
    }
}
