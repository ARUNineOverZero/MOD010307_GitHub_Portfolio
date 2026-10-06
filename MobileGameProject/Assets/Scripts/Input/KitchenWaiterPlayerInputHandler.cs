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

        private void OnPositionMove(InputAction.CallbackContext context)
        {
            var temp = context.ReadValue<Vector2>();
            if(_position == temp)
                return;

            _position = temp;
            OnPositionChanged?.Invoke(_position);
        }

        private void OnContactPerformed(InputAction.CallbackContext context)
        {
            Debug.Log($"down {context.control.path}");
            OnPressed?.Invoke();
        }

        private void OnContactCanceled(InputAction.CallbackContext context)
        {
            Debug.Log($"Up {context.control.path}");
            OnRelease?.Invoke();
        }


    


        
    }
}