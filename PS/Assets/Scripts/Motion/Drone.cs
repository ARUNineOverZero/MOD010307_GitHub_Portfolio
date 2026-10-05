using System;
using Unity.VisualScripting;
using UnityEngine;

namespace PhysicsSandbox.Motion
{
    public sealed class Drone : MonoBehaviour
    {
                [Header("Movement")]
        [Tooltip("Direction and speed of travel, in world units per second.")]
        [SerializeField] private Vector2 _velocity = new Vector2(2.5f, 1.5f);

        [Header("Bounds")]
        [SerializeField]  private Vector2 _boundsMin = new Vector2(-4.6f, -9.0f);
        [SerializeField]  private Vector2 _boundsMax = new Vector2(4.6f, 9.0f);

        private void Update()
        {
            Vector2 position = transform.position;
            position += _velocity * Time.deltaTime;
            transform.position = BounceOffBounds(position);
        }

        private Vector2 BounceOffBounds(Vector2 position)
        {
            if(position.x < _boundsMin.x || position.x > _boundsMax.x)
            {
                _velocity = Vector2.Reflect(_velocity, Vector2.right);
                position.x = Mathf.Clamp(position.x, _boundsMin.x, _boundsMax.x);
            }

            if(position.y < _boundsMin.y  || position.y > _boundsMax.y)
            {
                _velocity = Vector2.Reflect(_velocity, Vector2.up);
                position.y = Mathf.Clamp(position.y, _boundsMin.y, _boundsMax.y);
            }
            return position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)_velocity);           
        }
    }
}