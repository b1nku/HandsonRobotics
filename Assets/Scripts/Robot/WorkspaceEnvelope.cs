using UnityEngine;

namespace HandsOnRobotics.Robot
{
    /* Direction F: transparent sphere visualising the Niryo One's approximate reach envelope.

    Shown by TrajectoryController while the user is placing the trajectory target; hidden
    at all other times. The sphere is parented to _robotBase so it follows the robot
    without any per-frame position update.

    The Niryo One's nominal max reach is ~440 mm from the centre of joint_1.
    A hemisphere would be more accurate but a full sphere is clear enough as a hint. */
    public class WorkspaceEnvelope : MonoBehaviour
    {
        [Tooltip("The robot base transform - sphere origin.")]
        [SerializeField] Transform _robotBase;

        [Tooltip("Niryo One nominal max reach (m).")]
        [SerializeField] float _maxReach = 0.44f;

        [SerializeField] Color _color = new(0.25f, 0.60f, 1.00f);

        [Range(0f, 1f)]
        [SerializeField] float _alpha = 0.07f;

        GameObject _sphere;

        void Start()
        {
            _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _sphere.name = "WorkspaceEnvelopeSphere";
            _sphere.transform.SetParent(transform, false);
            _sphere.transform.localScale = Vector3.one * (_maxReach * 2f);
            Destroy(_sphere.GetComponent<SphereCollider>());

            _sphere.GetComponent<MeshRenderer>().sharedMaterial = BuildMaterial();
            _sphere.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (_sphere) _sphere.SetActive(visible);
        }

        void LateUpdate()
        {
            if (_robotBase != null && _sphere != null)
                _sphere.transform.position = _robotBase.position;
        }

        Material BuildMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            var col = _color;
            col.a = _alpha;
            mat.color = col;

            // URP transparent surface
            mat.SetFloat("_Surface",  1f);   // Transparent
            mat.SetFloat("_Blend",    0f);   // Alpha blend
            mat.SetFloat("_ZWrite",   0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            // Back-face only - inner surface must NOT render through the table/robot geometry
            mat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);

            return mat;
        }
    }
}
