using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PhysicsSandbox.Motion
{
    public sealed class Drone : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Direction and speed of travel, in world units per second.")]
        [SerializeField] private Vector2 _velocity = new Vector2(2.5f, 1.5f);
        [Tooltip("Top speed, in world units per second.")]
        [SerializeField, Min(0f)] private float _maxSpeed = 6f;
        [Tooltip("How quickly the drone can change its velocity (its acceleration), in world units per second per second.")]
        [SerializeField, Min(0f)] private float _maxSteering = 8f;
        [Tooltip("The drone starts slowing down when it is this close to its target.")]
        [SerializeField, Min(0.01f)] private float _slowingRadius = 2f;

        [Header("Bounds")]
        [SerializeField] private Vector2 _boundsMin = new Vector2(-4.6f, -9.0f);
        [SerializeField] private Vector2 _boundsMax = new Vector2(4.6f, 9.0f);

        [Header("Turning")]
        [Tooltip("Fastest turn, in degrees per second.")]
        [SerializeField, Min(0f)]
        private float _turnSpeed = 540f;

        private Camera _mainCamera;
        private Vector2 _target;
        private bool _hasTarget;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                _target = _mainCamera.ScreenToWorldPoint(Pointer.current.position.ReadValue());
                _hasTarget = true;
            }

            Vector2 position = transform.position;

            if (_hasTarget)
                Steer(position);

            position += _velocity * Time.deltaTime;
            transform.position = BounceOffBounds(position);
            FaceVelocity();
        }

        private void Steer(Vector2 position)
        {
            Vector2 toTarget = _target - position;

            float speed = _maxSpeed * Mathf.Min(1f, toTarget.magnitude / _slowingRadius);
            Vector2 desired = toTarget.normalized * speed;

            Vector2 steering = Vector2.ClampMagnitude(desired - _velocity, _maxSteering);
            _velocity += steering * Time.deltaTime;
            _velocity = Vector2.ClampMagnitude(_velocity, _maxSpeed);
        }

        private Vector2 BounceOffBounds(Vector2 position)
        {
            if (position.x < _boundsMin.x || position.x > _boundsMax.x)
            {
                _velocity = Vector2.Reflect(_velocity, Vector2.right);
                position.x = Mathf.Clamp(position.x, _boundsMin.x, _boundsMax.x);
            }

            if (position.y < _boundsMin.y || position.y > _boundsMax.y)
            {
                _velocity = Vector2.Reflect(_velocity, Vector2.up);
                position.y = Mathf.Clamp(position.y, _boundsMin.y, _boundsMax.y);
            }
            return position;
        }

        private void FaceVelocity()
        {
            if (_velocity.sqrMagnitude < .01f) return;

            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg - 90f;
            Quaternion wanted = Quaternion.Euler(0f,0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, wanted, _turnSpeed * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)_velocity);

            if (!_hasTarget) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_target, .2f);
            Gizmos.DrawWireSphere(_target, _slowingRadius);
        }
    }
}