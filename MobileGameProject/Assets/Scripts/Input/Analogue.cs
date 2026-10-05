using UnityEngine;
using UnityEngine.InputSystem;

namespace MobileGameProject.InputSystem
{
    public class Analogue : MonoBehaviour, IVector2Value
    {
        [Header("Scene Reference")]
        [SerializeField] public Transform _stick;

        public float maxDistance = .5f;
        private Vector2 _direction;
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
            transform.position = start;
            _direction = _value = Vector2.zero;
            _isActive = true;
        }

        public void Update()
        {
            if(!_isActive) return;

            Vector3 worldPoint = _mainCamera.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            _direction = transform.InverseTransformPoint(worldPoint);

            Vector2 clamped, normalized;
            float distance;

            GetClampedDirectionNormalizedDirectionAndDistance(_direction, maxDistance, out clamped, out normalized, out distance);

            _stick.localPosition = clamped;
            _value = normalized * Mathf.Min(distance, maxDistance) / maxDistance;
        }

        private void GetClampedDirectionNormalizedDirectionAndDistance(Vector2 direction, float maxDistance, out Vector2 clampedDirection, out Vector2 normalizedDirection, out float distance)
        {
            if( direction == Vector2.zero)
            {
                clampedDirection = normalizedDirection = Vector2.zero;
                distance = 0f;
                return;
            }

            var sqrDistance = Mathf.Pow(direction.x,2) + Mathf.Pow(direction.y,2);
            distance = Mathf.Sqrt(sqrDistance);
            var scalar = 1 / distance;
            normalizedDirection = new Vector2(direction.x * scalar, direction.y * scalar);

            if(distance < maxDistance)
                clampedDirection = direction;
            else
                clampedDirection = normalizedDirection * maxDistance;
            
        }

        public void FinishInteraction()
        {
            _value =  _direction = Vector2.zero;
            _stick.localPosition = Vector3.zero;
            _isActive = false;
        }
    }
}