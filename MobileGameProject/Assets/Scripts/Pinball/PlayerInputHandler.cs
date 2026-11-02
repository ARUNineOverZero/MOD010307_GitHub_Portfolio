using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MobileGameProject.MGPInputSystem.Pinball
{
    public sealed class PlayerInputHandler : APlayerInputHandler
    {
        private Vector2 _position;
        public Vector2 Position => _position;
        public UnityEvent OnLeftPressed;
        public UnityEvent OnLeftRelease;
        public UnityEvent OnRightPressed;
        public UnityEvent OnRightRelease;
        private Vector3 _tilt;
        public Vector3 Tilt => _tilt;
        private bool _leftActive;
        private bool _rightActive;

        protected override void RegisterActions()
        {
            var input = _inputSystem.Pinball;

            input.Position.performed += OnPositionMove;
            input.Contact.performed += OnContactPerformed;
            input.Contact.canceled += OnContactCanceled;
            input.Tilt.performed += OnTiltPerformed;
        }

        protected override void DeregisterActions()
        {
            var input = _inputSystem.Pinball;

            input.Position.performed -= OnPositionMove;
            input.Contact.performed -= OnContactPerformed;
            input.Contact.canceled -= OnContactCanceled;
            input.Tilt.performed -= OnTiltPerformed;
        }

        private void Awake()
        {
            SetInputSystem(GameManager.Instance.InputSystem);
        }

        protected override void DisableActions()
        {
            _inputSystem.Pinball.Disable();
        }

        protected override void EnableActions()
        {
            _inputSystem.Pinball.Enable();
        }

        private void Update()
        {
            EvaluateSideTouchedOrMouse();
        }

        private void EvaluateSideTouchedOrMouse()
        {
            bool leftActive = false;
            bool rightActive = false;
            float half = Screen.width * .5f;

            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                    continue;

                float x = touch.screenPosition.x;
                if (x < half)
                    leftActive = true;
                else
                    rightActive = true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos.x < half)
                    leftActive = true;
                else
                    rightActive = true;
            }

            if (leftActive && !_leftActive)
                OnLeftPressed?.Invoke();
            else if (!leftActive && _leftActive)
                OnLeftRelease?.Invoke();

            if (rightActive && !_rightActive)
                OnRightPressed?.Invoke();
            else if (!rightActive && _rightActive)
                OnRightRelease?.Invoke();

            _leftActive = leftActive;
            _rightActive = rightActive;
        }

        private void OnContactPerformed(InputAction.CallbackContext context)
        {
            MovePointer(GetPositionByType(context));
            //OnPressed?.Invoke();
        }

        private void MovePointer(Vector2 moveTo)
        {
            if (_position == moveTo)
                return;

            _position = moveTo;
        }

        private void OnPositionMove(InputAction.CallbackContext context)
        {
            MovePointer(context.ReadValue<Vector2>());
        }

        private Vector2 GetPositionByType(InputAction.CallbackContext context)
        {
            if (context.control?.device is Touchscreen t)
                return t.primaryTouch.position.ReadValue();
            else if (context.control?.device is Pointer p)
                return p.position.ReadValue();

            return Vector2.zero;
        }

        private void OnContactCanceled(InputAction.CallbackContext context)
        {
            //OnRelease?.Invoke();
        }

        private void OnTiltPerformed(InputAction.CallbackContext context)
        {
            _tilt = context.ReadValue<Vector3>();
        }
    }
}