using System;
using UnityEditor.Callbacks;
using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class Ball : MonoBehaviour
    {
        [Header("Self Reference")]
        [SerializeField] private Rigidbody _rb;
        private Vector3 startPosition ;

        public void Awake()
        {
            startPosition = transform.position;
        }

        public void ResetPosition()
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.MovePosition(startPosition);
        }
    }
}