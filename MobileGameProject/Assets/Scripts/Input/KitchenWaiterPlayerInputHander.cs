using System;
using System.Numerics;
using UnityEngine.InputSystem;

namespace MobileGameProject.MGPInputSystem
{
    public sealed class KitchenWaiterPlayerInputHander : APlayerInputHandler
    {
        private Vector2 _position;
        public event Action OnPressed;
        public event Action OnRelease;


        protected override void RegisterActions()
        {
            var input = _inputSystem.KitchenWaiter;

            input.Position.performed += OnPositionMove;
            input.Contact.performed += OnContactPerformed;
            input.Contact.canceled += OnContactCanceled;
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


        protected override void DeregisterActions()
        {
            
        }


        protected override void DisableActions()
        {
            throw new System.NotImplementedException();
        }

        protected override void EnableActions()
        {
            _inputSystem.KitchenWaiter.Enable();
        }

        
    }
}