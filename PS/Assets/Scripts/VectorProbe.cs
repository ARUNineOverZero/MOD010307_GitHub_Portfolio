using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PhysicsSandbox
{
    public sealed class VectorProbe : MonoBehaviour
    {
        [Header("Scene reference")]
        [Tooltip("Line from this object to the pointer: the full offset vector.")]
        [SerializeField] private LineRenderer _offsetLine;
        [Tooltip("Line one unit long in the same direction: the normalized vector.")]

        [SerializeField] private LineRenderer _directionLine;
        [SerializeField] private TextMeshProUGUI _readout;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if(Pointer.current == null) return;

            Vector2 origin = transform.position;
            Vector2 target = PointerWorldPosition();

            Vector2 offset = target - origin;
            float length = offset.magnitude;
            Vector2 direction = offset.normalized;

            _offsetLine.SetPosition(0, origin);
            _offsetLine.SetPosition(1, target);

            _directionLine.SetPosition(0, origin);
            _directionLine.SetPosition(1, origin + direction);

            _readout.text = $"Offset: {offset:F2}\nMagnitude: {length:F2}\nNormalized: {direction:F2}";
        }

        private Vector2 PointerWorldPosition()
        {
            Vector2 screenPosition = Pointer.current.position.ReadValue();
            return _mainCamera.ScreenToWorldPoint(screenPosition);
        }
    }
}