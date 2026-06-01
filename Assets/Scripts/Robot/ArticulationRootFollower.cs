using UnityEngine;

public class ArticulationRootFollower : MonoBehaviour
{
  [SerializeField] ArticulationBody _root;

  void FixedUpdate()
  {
    _root.TeleportRoot(_root.transform.position, _root.transform.rotation);
  }
}
