using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Scene Reference")] [SerializeField]
        public Transform _followTransform;

        public void Update()
        {
            var t = transform;
            t.position = _followTransform.position;
            t.rotation = _followTransform.rotation;
        }
    }
}