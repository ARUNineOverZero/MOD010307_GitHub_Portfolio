using MobileGameProject.Framework;
using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public class EngGameTrigger : MonoBehaviour
    {
        [SerializeField] private AMicrogameBehaviour _microgameBehaviour;

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Vehicle vehicle))
                _microgameBehaviour.EndGame();
        }
    }
}