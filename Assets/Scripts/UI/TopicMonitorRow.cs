using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HandsOnRobotics.UI
{
    /* Single row in the topic monitor panel.

    Set up as a prefab with a Horizontal Layout Group. Wire the four
    references in the Inspector. TopicMonitorPanel calls Refresh() every
    Update with the latest stats. */
    public class TopicMonitorRow : MonoBehaviour
    {
        [SerializeField] Image _statusDot;
        [SerializeField] TextMeshProUGUI _topicLabel;
        [SerializeField] TextMeshProUGUI _typeLabel;
        [SerializeField] TextMeshProUGUI _hzLabel;

        static readonly Color Grey   = new(0.5f, 0.5f, 0.5f);
        static readonly Color Green  = new(0.2f, 0.85f, 0.2f);
        static readonly Color Yellow = new(1.0f, 0.75f, 0.0f);
        static readonly Color Red    = new(0.9f, 0.15f, 0.15f);

        /* secondsSinceLastMsg < 0 means never received. */
        public void Refresh(string topic, string msgType, float hz, float secondsSinceLastMsg)
        {
            if (_topicLabel) _topicLabel.text = topic;
            if (_typeLabel)  _typeLabel.text  = msgType;
            if (_hzLabel)    _hzLabel.text    = hz > 0f ? $"{hz:0.0} Hz" : "--";

            if (_statusDot)
                _statusDot.color = secondsSinceLastMsg < 0f ? Grey
                                 : secondsSinceLastMsg < 2f ? Green
                                 : secondsSinceLastMsg < 5f ? Yellow
                                 : Red;
        }
    }
}
