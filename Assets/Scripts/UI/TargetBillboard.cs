using TMPro;
using UnityEngine;

namespace HandsOnRobotics.UI
{
    /* Direction G: billboard text label that floats above the trajectory target sphere.

    Add this component directly to the trajectory target GameObject (the red sphere).
    It creates a TextMeshPro 3D object as a child at runtime - no prefab or Canvas needed.
    The label always faces the main camera regardless of how the sphere is rotated.

    Because it is a child of the target sphere it automatically shows and hides with it;
    no extra wiring required. */
    public class TargetBillboard : MonoBehaviour
    {
        [SerializeField] string _line1 = "Trajectory Target";
        [SerializeField] string _line2 = "Grab & place";
        [SerializeField] float  _heightOffset = 0.14f;
        [SerializeField] float  _fontSize     = 0.06f;
        [SerializeField] Color  _color        = Color.white;

        Transform _label;

        void Awake()
        {
            var go = new GameObject("BillboardLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up * _heightOffset;

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text      = $"{_line1}\n<size=70%><color=#CCCCCC>{_line2}</color></size>";
            tmp.fontSize  = _fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = _color;

            // ZTest Always - visible through other geometry in editor scene view.
            // TMP_SDF.shader reads ZTest from the "unity_GUIZTestMode" material property.
            tmp.fontMaterial = new Material(tmp.fontSharedMaterial);
            tmp.fontMaterial.SetFloat("unity_GUIZTestMode",
                (float)UnityEngine.Rendering.CompareFunction.Always);

            _label = go.transform;
        }

        void LateUpdate()
        {
            if (Camera.main == null || _label == null) return;
            _label.rotation = Quaternion.LookRotation(
                _label.position - Camera.main.transform.position);
        }
    }
}
