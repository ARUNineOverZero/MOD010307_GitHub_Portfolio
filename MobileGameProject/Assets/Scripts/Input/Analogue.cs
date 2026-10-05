using UnityEngine;
using UnityEngine.InputSystem;

namespace MobileGameProject.InputSystem
{
    public class Analogue : MonoBehaviour, IVector2Value
    {
        [SerializeField] public Transform _stick;
        [SerializeField] public float _maxDistance = .5f;

        private Vector2 _start, _current;
        private Vector2 _value = Vector2.zero; 

        private bool _isActive = true; 
        Vector2 IVector2Value.Value => _value;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void StartInteraction(Vector2 start)
        {
            transform.position = _start = _current = start;
            _value = Vector2.zero;
            _isActive = true;
        }

        public void Update()
        {
            if(!_isActive) return;

            Vector3 worldPoint = _mainCamera.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            _current = worldPoint - transform.position;
            Vector2 direction = _current - _start;
            var clamped = Vector2.ClampMagnitude(direction, _maxDistance);
            _stick.localPosition = clamped;
            _value = clamped.normalized;
        }

        public void FinishInteraction()
        {
            _value = _start = _current = Vector2.zero;
            _stick.localPosition = Vector3.zero;
            _isActive = false;
        }
    }
}