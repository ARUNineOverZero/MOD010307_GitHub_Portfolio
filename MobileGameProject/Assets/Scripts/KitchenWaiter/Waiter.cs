using MobileGameProject.MGPInputSystem;
using UnityEngine;

namespace MobileGameProject.KitchenWaiter
{
    public class Waiter : MonoBehaviour
    {
        [SerializeField] private GameObject _moveInputObject;
        [SerializeField] private IVector2Value _moveInput; 
        [SerializeField] private float _acceleration = 0.01f; 
        [SerializeField] private float _deceleration = 0.05f; 
        [SerializeField] private float _speed; 
        
        private Vector2 _velocity = Vector2.zero;

        public void Awake()
        {
            _moveInput = _moveInputObject.GetComponent<IVector2Value>();
        }

        public void Update()
        {
            var val = _moveInput.Value;

            if(val == Vector2.zero)
            {
                _velocity = Vector2.MoveTowards(_velocity, Vector2.zero, _deceleration * Time.deltaTime);
            }
            else
            {
                _velocity += val * _acceleration * Time.deltaTime;
                _velocity = Vector2.ClampMagnitude(_velocity, _speed);
            }

            if(_velocity == Vector2.zero) return;

            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f,0f, angle);
            transform.position += (Vector3)_velocity * Time.deltaTime;
        }
    }
}