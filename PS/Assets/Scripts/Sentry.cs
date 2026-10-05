using UnityEngine;
using UnityEngine.InputSystem;

namespace PhysicsSandbox
{
    public sealed class Sentry : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private SpriteRenderer cone;

        [Header("Vision")]
        [Tooltip("Total width of the view, in degrees. The sentry looks along its green (up) axis.")]
        [SerializeField, Range(1f, 180f)] private float _viewAngle = 40f;
        [Tooltip("How far the sentry can see, in world units.")]
        [SerializeField, Min(0f)] private float _viewRange = 4f;

        [Header("Presentation")]
        [SerializeField] private Color _calmColor = new Color(0.3f, 0.9f, 0.5f, 0.35f);
        [SerializeField] private Color _alertColor = new Color(1f, 0.25f, 0.25f, 0.6f);

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if(Pointer.current == null) return;

            Vector2 target = _mainCamera.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            cone.color = CanSee(target) ? _alertColor : _calmColor;
        }

        private bool CanSee(Vector2 target)
        {
            Vector2 direction = target - (Vector2)transform.position;
            
            if(direction.sqrMagnitude > _viewRange * _viewRange) return false;

            float dot = Vector2.Dot(transform.up, direction.normalized);
            float limit = Mathf.Cos(_viewAngle * .5f * Mathf.Deg2Rad);

            return dot >= limit;
        }
    }
}