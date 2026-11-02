using System.Collections;
using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class Flipper : MonoBehaviour
    {
        [Header("Self Reference")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private HingeJoint _hinge;

        [Header("Settings")]
        [SerializeField] private float _torque = 10000f;
        [SerializeField] private float _testInterval = 1f;

        private bool _isFlipping;
        private float _currentTime = 0;

        private void FixedUpdate()
        {
            _currentTime += Time.fixedDeltaTime;

            if (_currentTime > _testInterval)
            {
                Flip();
                _currentTime = 0;
            }
        }

        public void Flip()
        {
            Vector3 worldAxis = transform.TransformDirection(_hinge.axis);
            _rb.AddTorque(worldAxis * _torque, ForceMode.VelocityChange);
        }
    }
}