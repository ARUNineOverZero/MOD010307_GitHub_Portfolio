using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class Ball : MonoBehaviour
    {
        [Header("Self Reference")]
        [SerializeField] private Rigidbody _rigidbody;

        public Rigidbody Rigidbody => _rigidbody;
        private Vector3 startPosition ;

        public void Awake()
        {
            startPosition = transform.position;
        }

        [ContextMenu("RespawnBall")]
        public void RespawnBall()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.MovePosition(startPosition);
        }
    }
}