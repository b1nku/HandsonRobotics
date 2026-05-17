using UnityEngine;

namespace HandsOnRobotics.UI
{
    /* Base class for world-space UI panels.
    
    Requires a child Canvas set to World Space. Subclasses implement
    Refresh() to push new data into their UI elements whenever the panel
    is visible. SpatialPanel handles positioning and visibility toggling.
    
    Usage: subclass this, override Refresh(), call Show()/Hide() externally. */

    [RequireComponent(typeof(Canvas))]
    public abstract class SpatialPanel : MonoBehaviour
    {
        [Header("Positioning")]
        [Tooltip("The panel will follow this transform (e.g. a robot link or the camera).")]
        [SerializeField] Transform _anchor;

        [Tooltip("Offset from the anchor in the anchor's local space.")]
        [SerializeField] Vector3 _localOffset = new(0f, 0.3f, 0f);

        [Tooltip("When true the panel always faces the main camera.")]
        [SerializeField] bool _faceCamera = true;

        [Header("Visibility")]
        [SerializeField] bool _visibleOnStart = true;

        Canvas _canvas;
        bool _visible;

        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        protected virtual void Start()
        {
            SetVisible(_visibleOnStart);
        }

        protected virtual void LateUpdate()
        {
            if (!_visible) return;

            if (_anchor != null)
                transform.position = _anchor.TransformPoint(_localOffset);

            if (_faceCamera && Camera.main != null)
                transform.rotation = Quaternion.LookRotation(
                    transform.position - Camera.main.transform.position);
        }

        public void Show() => SetVisible(true);
        public void Hide() => SetVisible(false);
        public void Toggle() => SetVisible(!_visible);
        public bool IsVisible => _visible;

        void SetVisible(bool visible)
        {
            _visible = visible;
            _canvas.enabled = visible;
            if (visible) Refresh();
        }

        /* Called when the panel becomes visible and whenever underlying data
        changes. Push fresh values to UI elements here. */

        protected abstract void Refresh();

        /* Call from a subclass whenever the data it displays has changed.
        Only refreshes while the panel is visible to avoid wasted work. */

        protected void OnDataChanged()
        {
            if (_visible) Refresh();
        }
    }
}
