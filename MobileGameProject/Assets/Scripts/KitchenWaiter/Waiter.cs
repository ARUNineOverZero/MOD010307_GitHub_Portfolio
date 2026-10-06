using MobileGameProject.MGPInputSystem;
using UnityEngine;

namespace MobileGameProject.KitchenWaiter
{
    public class Waiter : MonoBehaviour
    {
        [SerializeField] private GameObject _moveInputObject;
        [SerializeField] private IVector2Value _moveInput;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private float _acceleration = 0.01f; 
        [SerializeField] private float _deceleration = 0.05f; 
        [SerializeField] private float _speed; 
        [SerializeField] private float _rotationSpeed = 90f;

        private const float Epsilon = 0.01f;

        private Vector2 _velocity = Vector2.zero;

        public void Awake()
        {
            _moveInput = _moveInputObject.GetComponent<IVector2Value>();
        }

        public void Update()
        {
            var val = _moveInput.Value;
            _velocity = _rb.linearVelocity;
            if(val == Vector2.zero)
            {
                if(_velocity != Vector2.zero)
                {
                    var newVel = Vector2.MoveTowards(_velocity, Vector2.zero, _deceleration * Time.deltaTime);

                    if(newVel != Vector2.zero)
                    {
                        if(Mathf.Abs(newVel.x) < Epsilon)
                        {
                            Debug.Log($"newVel.x: {newVel.x} is less than {Epsilon}");
                            newVel.x = 0f;
                        }

                        if(Mathf.Abs(newVel.y) < Epsilon)
                        {
                            Debug.Log($"newVel.y: {newVel.y} is less than {Epsilon}");
                            newVel.y = 0f;
                        }
                    }

                    _velocity = newVel;
                }
            }
            else
            {
                _velocity += _moveInput.Normalized * _acceleration * Time.deltaTime;
                _velocity = Vector2.ClampMagnitude(_velocity, _speed * _moveInput.Magnitude);
            }

            _rb.linearVelocity = _velocity;

            if(_velocity == Vector2.zero) return;

            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg - 90f;
            Quaternion desiredRotation = Quaternion.Euler(0f,0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, _rotationSpeed * Time.deltaTime);
            transform.position += (Vector3)_velocity * Time.deltaTime;
        }
    }
}