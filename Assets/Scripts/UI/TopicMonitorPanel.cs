using System.Collections.Generic;
using UnityEngine;

namespace HandsOnRobotics.UI
{
    /* Direction B: Live ROS Topic Monitor.

    Place once in the scene, positioned next to the robot. Any subscriber
    calls TopicMonitorPanel.Register(topic, type) once at startup and
    TopicMonitorPanel.RecordMessage(topic) on every received message.
    The panel instantiates a row per topic and refreshes Hz and status
    every Update using a sliding time window.

    To add a new topic: call Register + RecordMessage from the relevant
    subscriber -- no changes to this class needed. */
    public class TopicMonitorPanel : MonoBehaviour
    {
        public static TopicMonitorPanel Instance { get; private set; }

        [SerializeField] TopicMonitorRow _rowPrefab;
        [SerializeField] Transform _rowContainer;
        [Tooltip("Sliding window in seconds used to calculate Hz.")]
        [SerializeField] float _hzWindow = 3f;

        class TopicStats
        {
            public string             MessageType;
            public readonly Queue<float> Times = new();
            public float              LastReceived = -1f; // -1 = never received
            public TopicMonitorRow    Row;
        }

        readonly Dictionary<string, TopicStats> _topics = new();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            float now = Time.time;
            foreach (var (topic, stats) in _topics)
            {
                while (stats.Times.Count > 0 && now - stats.Times.Peek() > _hzWindow)
                    stats.Times.Dequeue();

                float hz            = stats.Times.Count / _hzWindow;
                float sinceLastMsg  = stats.LastReceived < 0f ? -1f : now - stats.LastReceived;
                stats.Row?.Refresh(topic, stats.MessageType, hz, sinceLastMsg);
            }
        }

        public static void Register(string topic, string msgType)
            => Instance?.RegisterInternal(topic, msgType);

        public static void RecordMessage(string topic)
            => Instance?.RecordInternal(topic);

        void RegisterInternal(string topic, string msgType)
        {
            if (_topics.ContainsKey(topic)) return;

            var stats = new TopicStats { MessageType = msgType };
            if (_rowPrefab && _rowContainer)
            {
                stats.Row = Instantiate(_rowPrefab, _rowContainer);
                stats.Row.Refresh(topic, msgType, 0f, -1f);
            }
            _topics[topic] = stats;
        }

        void RecordInternal(string topic)
        {
            if (!_topics.TryGetValue(topic, out var stats)) return;
            stats.Times.Enqueue(Time.time);
            stats.LastReceived = Time.time;
        }

        /* Returns a snapshot of all registered topics for external consumers (e.g. ROSDebugOverlay). */
        public static System.Collections.Generic.IEnumerable<(string topic, string type, float hz, float secSinceLast)>
            GetTopicStats()
        {
            if (Instance == null) yield break;
            float now = Time.time;
            foreach (var (topic, stats) in Instance._topics)
            {
                float hz          = stats.Times.Count / Instance._hzWindow;
                float secSinceLast = stats.LastReceived < 0f ? -1f : now - stats.LastReceived;
                yield return (topic, stats.MessageType, hz, secSinceLast);
            }
        }
    }
}
