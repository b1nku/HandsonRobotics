using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HandsOnRobotics.UI
{
    /* Manages named views on the floating tablet via prev/next navigation.

    Each view is a child panel (RectTransform) that fills the tablet face.
    A nav bar with a Previous button, view name label, and Next button sits
    at the top. To add a new view: add its root GO to _views and its display
    name to _viewNames (indices must match). No other changes needed. */
    public class TabletViewController : MonoBehaviour
    {
        [SerializeField] GameObject[] _views;
        [SerializeField] string[]     _viewNames;

        [Header("Nav bar")]
        [SerializeField] Button          _prevButton;
        [SerializeField] Button          _nextButton;
        [SerializeField] TextMeshProUGUI _viewNameLabel;

        int _activeIndex = 0;

        void Start()
        {
            _prevButton?.onClick.AddListener(Previous);
            _nextButton?.onClick.AddListener(Next);
            SwitchTo(0);
        }

        public void Next()     => SwitchTo((_activeIndex + 1) % _views.Length);
        public void Previous() => SwitchTo((_activeIndex - 1 + _views.Length) % _views.Length);

        public void SwitchTo(int index)
        {
            if (index < 0 || index >= _views.Length) return;
            _activeIndex = index;

            for (int i = 0; i < _views.Length; i++)
                if (_views[i]) _views[i].SetActive(i == index);

            if (_viewNameLabel)
                _viewNameLabel.text = index < _viewNames.Length ? _viewNames[index] : $"View {index}";
        }
    }
}
