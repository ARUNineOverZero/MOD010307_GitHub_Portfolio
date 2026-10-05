using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

namespace PhysicsSandbox
{
    public sealed class WatcherEye : MonoBehaviour
    {
        [Header("Scene Reference")]
        [SerializeField] private Transform _pupil;

        [Header("Looking")]
        [Tooltip("Furthest the pupil can move from the centre of the eye, in world units")]
        [SerializeField, Min(0f)] private float _maxLookDistance = 0.28f;
        [Tooltip("How fast the pulpil moves, in work units per second.")]
        [SerializeField, Min(0f)] private float _pupilSpeed = 3f;

        [Header("Dilating")]
        [Tooltip("At this distance or closer, the pupil is at its largest.")]
        [SerializeField, Min(0f)] private float _nearDistance = 1f;
        [Tooltip("At this distance or further, the pupil is at its smallest.")]
        [SerializeField, Min(0f)] private float _farDistance = 7f;
        [SerializeField, Min(0f)] private float _smallPupil = .3f;
        [SerializeField, Min(0f)] private float _largePupil = .55f;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if(Pointer.current == null) return;

            Vector2 eyeCenter = transform.position;
            Vector2 target = _mainCamera.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            Vector2 toTarget = target - eyeCenter;

            Look(toTarget);
            Dilate(Vector2.Distance(eyeCenter, target));
        }

       

        private void Look(Vector2 toTarget)
        {
            Vector2 wantedOffset = Vector2.ClampMagnitude(toTarget, _maxLookDistance);
            _pupil.localPosition = Vector2.MoveTowards(_pupil.localPosition, wantedOffset, _pupilSpeed * Time.deltaTime);
        }

        private void Dilate(float distance)
        {
            float farness = Mathf.InverseLerp(_nearDistance, _farDistance, distance);
            float size = Mathf.Lerp(_largePupil, _smallPupil, farness);
            _pupil.localScale = new Vector3(size, size, 1f);
        }
    }
}