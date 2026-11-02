using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MobileGameProject.MGPInputSystem.KitchenWaiter
{
    public sealed class PlayerInputHandler : APlayerInputHandler
    {
        private Vector2 _position;
        public Vector2 Position => _position;
        public  UnityEvent OnPressed;
        public  UnityEvent OnRelease;
        public  UnityEvent<Vector2> OnPositionChanged;

        protected override void RegisterActions()
        {
            var input = _inputSystem.KitchenWaiter;

            input.Position.performed += OnPositionMove;
            input.Contact.performed += OnContactPerformed;
            input.Contact.canceled += OnContactCanceled;
        }

        protected override void DeregisterActions()
        {
            var input = _inputSystem.KitchenWaiter;

            input.Position.performed -= OnPositionMove;
            input.Contact.performed -= OnContactPerformed;
            input.Contact.canceled -= OnContactCanceled;
        }

        public void Awake()
        {
            SetInputSystem(GameManager.Instance.InputSystem);
        }

        protected override void DisableActions()
        {
            _inputSystem.KitchenWaiter.Disable();
        }

        protected override void EnableActions()
        {
            _inputSystem.KitchenWaiter.Enable();
        }

        private void OnContactPerformed(InputAction.CallbackContext context)
        {
            MovePointer(GetPositionByType(context));
            OnPressed?.Invoke();
        }

        private void MovePointer(Vector2 moveTo)
        {
            if(_position == moveTo)
                return;

            _position = moveTo;
            OnPositionChanged?.Invoke(_position);
        }

        private void OnPositionMove(InputAction.CallbackContext context)
        {
            MovePointer(context.ReadValue<Vector2>());
        }

        private Vector2 GetPositionByType(InputAction.CallbackContext context)
        {
            if(context.control?.device is Touchscreen t)
                return t.primaryTouch.position.ReadValue();
            else if (context.control?.device is Pointer p)
                return p.position.ReadValue();

            return Vector2.zero;
        }

        private void OnContactCanceled(InputAction.CallbackContext context)
        {
            OnRelease?.Invoke();
        }
    }
}