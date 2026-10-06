using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MobileGameProject.MGPInputSystem
{
    public sealed class KitchenWaiterPlayerInputHandler : APlayerInputHandler
    {
        private Vector2 _position;
        public Vector2 Position => _position;
        public  UnityEvent OnPressed;
        public  UnityEvent OnRelease;

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

        private void OnPositionMove(InputAction.CallbackContext context)
        {
            _position = context.ReadValue<Vector2>();
        }

        private void OnContactPerformed(InputAction.CallbackContext context)
        {
            OnPressed?.Invoke();
        }

        private void OnContactCanceled(InputAction.CallbackContext context)
        {
            OnRelease?.Invoke();
        }


    


        
    }
}