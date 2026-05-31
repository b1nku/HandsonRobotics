using UnityEngine;

namespace HandsOnRobotics.UI
{
    /* Floating grabbable tablet panel.

    Attach to the root tablet GO alongside OVRGrabbable. The Rigidbody has no
    gravity and low drag, so on release the tablet keeps the hand's velocity and
    drifts like an object in microgravity. ReturnHome() snaps it back to its
    spawn position and kills all momentum. */
    [RequireComponent(typeof(Rigidbody))]
    public class FloatingTablet : MonoBehaviour
    {
        [Tooltip("Linear drag -- lower is more ISS-like, higher settles faster.")]
        [SerializeField] float _linearDrag  = 0.1f;
        [Tooltip("Angular drag -- prevents infinite spin after release.")]
        [SerializeField] float _angularDrag = 0.8f;

        Rigidbody  _rb;
        Vector3    _homePosition;
        Quaternion _homeRotation;

        void Awake()
        {
            _rb             = GetComponent<Rigidbody>();
            _rb.useGravity  = false;
            _rb.linearDamping        = _linearDrag;
            _rb.angularDamping = _angularDrag;

            _homePosition = transform.position;
            _homeRotation = transform.rotation;
        }

        public void ReturnHome()
        {
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(_homePosition, _homeRotation);
        }
    }
}
