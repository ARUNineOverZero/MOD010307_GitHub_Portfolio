using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class TrackWorldRotation : MonoBehaviour
    {
        [SerializeField] private Transform _track;

        private void Update()
        {
            transform.rotation = _track.rotation;
        }
    }
}