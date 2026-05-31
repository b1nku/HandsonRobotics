using TMPro;
using UnityEngine;

namespace HandsOnRobotics.Robot
{
    /* Renders one TF frame: three tapered axis lines (X=red, Y=green, Z=blue),
    an optional grey line back to the parent frame, and an optional billboarded
    name label. Initialised and owned by TFFrameDisplay. */
    public class TFFrameVisualiser : MonoBehaviour
    {
        LineRenderer _xAxis;
        LineRenderer _yAxis;
        LineRenderer _zAxis;
        LineRenderer _parentLine;
        TextMeshPro  _label;
        Transform    _parentLink;
        float        _axisLength;

        public void Initialise(string frameName, Transform parentLink,
                               float axisLength, float axisWidth,
                               float labelSize,  bool showLabel, bool showParentLine)
        {
            _parentLink = parentLink;
            _axisLength = axisLength;

            _xAxis = CreateAxis("X", new Color(0.9f, 0.15f, 0.15f), axisWidth);
            _yAxis = CreateAxis("Y", new Color(0.2f, 0.85f, 0.2f),  axisWidth);
            _zAxis = CreateAxis("Z", new Color(0.2f, 0.45f, 0.9f),  axisWidth);

            if (showParentLine && parentLink != null)
                _parentLine = CreateLine("Parent", new Color(0.6f, 0.6f, 0.6f), axisWidth * 0.4f);

            if (showLabel)
                _label = CreateLabel(frameName, labelSize);
        }

        public void SetVisible(bool visible)
        {
            foreach (var lr in new[] { _xAxis, _yAxis, _zAxis, _parentLine })
                if (lr) lr.enabled = visible;
            if (_label) _label.gameObject.SetActive(visible);
        }

        void LateUpdate()
        {
            var pos = transform.position;

            UpdateAxis(_xAxis, pos, transform.right);
            UpdateAxis(_yAxis, pos, transform.up);
            UpdateAxis(_zAxis, pos, transform.forward);

            if (_parentLine != null && _parentLink != null)
            {
                _parentLine.SetPosition(0, _parentLink.position);
                _parentLine.SetPosition(1, pos);
            }

            if (_label != null && Camera.main != null)
            {
                _label.transform.position = pos + transform.up * _axisLength * 1.2f;
                _label.transform.rotation = Quaternion.LookRotation(
                    _label.transform.position - Camera.main.transform.position,
                    Vector3.up);
            }
        }

        void UpdateAxis(LineRenderer lr, Vector3 origin, Vector3 dir)
        {
            lr.SetPosition(0, origin);
            lr.SetPosition(1, origin + dir * _axisLength);
        }

        LineRenderer CreateAxis(string axisName, Color color, float width)
        {
            var lr = CreateLine(axisName, color, width);
            // taper: wider at origin, thinner at tip to indicate direction
            lr.startWidth = width * 1.5f;
            lr.endWidth   = width * 0.4f;
            return lr;
        }

        LineRenderer CreateLine(string goName, Color color, float width)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform);
            var lr = go.AddComponent<LineRenderer>();
            lr.material         = new Material(Shader.Find("Sprites/Default"));
            lr.startColor       = color;
            lr.endColor         = color;
            lr.startWidth       = width;
            lr.endWidth         = width;
            lr.positionCount    = 2;
            lr.useWorldSpace    = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows   = false;
            return lr;
        }

        TextMeshPro CreateLabel(string frameName, float fontSize)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(transform);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text      = frameName;
            tmp.fontSize  = fontSize;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.06f);
            return tmp;
        }
    }
}
