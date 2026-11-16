using Unity.VisualScripting;
using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class TriggerBallRelease : MonoBehaviour
    {
        [SerializeField] private Ball _ball;
        [SerializeField] private Vector3 _direction = Vector3.forward;
        [SerializeField] private float _force;

        private void FixedUpdate()
        {
            if(_ball == null) return;
        }

        private void ApplyForce(Ball ball)
        {
            var direction = transform.TransformVector(_direction) * _force;

            ball.Rigidbody.AddForce(direction);
        }


        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.TryGetComponent<Ball>(out Ball ball))
                ApplyForce(ball);
        }

        private void OnTriggerExit(Collider other)
        {
            if(_ball != null && other.gameObject == _ball.gameObject)
                _ball = null;
        }
    }
}