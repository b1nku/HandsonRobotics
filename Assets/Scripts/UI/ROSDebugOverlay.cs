using System.Text;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace HandsOnRobotics.UI
{
    /* Direction H: world-space ROS connection info panel on the wall behind the robot.

    Toggle visibility via the tablet debug button (wire _tabletToggleButton in the
    Inspector, or call Toggle() from a UnityEvent). The panel renders with ZTest Always
    so it is visible through geometry in the editor Scene view.

    Setup in the Inspector:
      1. Create an empty GO on the wall, position and rotate it to face into the room.
      2. Add this component.
      3. A TextMeshPro 3D label is created at runtime — no Canvas needed.
      4. Assign the optional _tabletToggleButton to wire the tablet button automatically. */
    public class ROSDebugOverlay : MonoBehaviour
    {
        [Tooltip("How many seconds between content refreshes.")]
        [SerializeField] float _refreshInterval = 1f;

        [Tooltip("Font size in world-space metres.")]
        [SerializeField] float _fontSize = 0.04f;

        [Tooltip("Optional tablet button that calls Toggle() — wire in Inspector.")]
        [SerializeField] UnityEngine.UI.Button _tabletToggleButton;

        TextMeshPro _tmp;
        float       _nextRefresh;
        StringBuilder _sb = new();

        void Awake()
        {
            var go = new GameObject("ROSDebugLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;

            _tmp = go.AddComponent<TextMeshPro>();
            _tmp.fontSize  = _fontSize;
            _tmp.alignment = TextAlignmentOptions.TopLeft;
            _tmp.color     = new Color(0.9f, 1f, 0.9f);

            // ZTest Always — visible through geometry in the editor Scene view.
            // TMP_SDF.shader reads ZTest from the "unity_GUIZTestMode" material property.
            _tmp.fontMaterial = new Material(_tmp.fontSharedMaterial);
            _tmp.fontMaterial.SetFloat("unity_GUIZTestMode",
                (float)UnityEngine.Rendering.CompareFunction.Always);
        }

        void Start()
        {
            _tabletToggleButton?.onClick.AddListener(Toggle);
            Refresh();
        }

        void OnEnable()  => Refresh();

        void Update()
        {
            if (Time.time < _nextRefresh) return;
            _nextRefresh = Time.time + _refreshInterval;
            Refresh();
        }

        public void Toggle() => gameObject.SetActive(!gameObject.activeSelf);

        void Refresh()
        {
            if (_tmp == null) return;

            _sb.Clear();
            var ros = ROSConnection.GetOrCreateInstance();
            _sb.AppendLine("<b>── ROS Connection ──</b>");
            _sb.AppendLine($"  Endpoint  {ros.RosIPAddress}:{ros.RosPort}");
            _sb.AppendLine();
            _sb.AppendLine("<b>── Topics ──</b>");

            bool any = false;
            foreach (var (topic, type, hz, sec) in TopicMonitorPanel.GetTopicStats())
            {
                any = true;
                string status = sec < 0f   ? "<color=#888888>no msgs</color>"
                              : sec < 2f   ? $"<color=#44FF44>{hz:0.0} Hz</color>"
                              : sec < 8f   ? $"<color=#FFAA00>stale {sec:0}s</color>"
                              : $"<color=#FF4444>dead {sec:0}s</color>";
                _sb.AppendLine($"  {topic}");
                _sb.AppendLine($"    {type}  {status}");
            }
            if (!any) _sb.AppendLine("  <color=#888888>(no topics registered)</color>");

            _tmp.text = _sb.ToString();
        }
    }
}
