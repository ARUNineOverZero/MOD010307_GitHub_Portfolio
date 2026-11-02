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

        private bool _isFlipped;

        private void FixedUpdate()
        {
            Flip(_isFlipped ? _torque : -_torque);
        }

        public void Flip(float torquAmount)
        {
            Vector3 worldAxis = transform.TransformDirection(_hinge.axis);
            _rb.AddTorque(worldAxis * torquAmount, ForceMode.VelocityChange);
        }

        public void OnPress()
        {
            _isFlipped = true;
        }

        public void OnRelease()
        {
            _isFlipped = false;
        }
    }
}