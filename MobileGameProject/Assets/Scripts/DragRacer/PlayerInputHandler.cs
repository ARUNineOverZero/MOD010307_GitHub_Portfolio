using MobileGameProject.DragRacer;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MobileGameProject.MGPInputSystem.DragRacer
{
    public sealed class PlayerInputHandler : APlayerInputHandler
    {
        private float _accelerator;
        public float Accelerator => _accelerator;
        
        private float _brake;
        public float Brake => _brake;
        
        private float _clutch;
        public float Clutch => _clutch;

        private Gear _gear = MobileGameProject.DragRacer.Gear.Neutral;
        public Gear Gear => _gear;

        public UnityEvent<Gear> OnTryGearChange;
        
        public void Awake() => SetInputSystem(GameManager.Instance.InputSystem);

        protected override void RegisterActions()
        {
            var input = _inputSystem.DragRacer;
            
            input.Accelerator.performed += OnAcceleratorPerformed;
            input.Accelerator.canceled += OnAcceleratorCanceled;
            
            input.Break.performed += OnBreakPerformed;
            input.Break.canceled += OnBreakCanceled;
            
            input.Clutch.performed += OnClutchOnPerformed;
            input.Clutch.canceled += OnClutchCanceled;
            
            input.GearUp.performed += OnGearUpOnPerformed;

            input.GearDown.performed += OnGearDownPerformed;

        }

        protected override void DeregisterActions()
        {
            var input = _inputSystem.DragRacer;

            input.Accelerator.performed -= OnAcceleratorPerformed;
            input.Accelerator.canceled -= OnAcceleratorCanceled;
            
            input.Break.performed -= OnBreakPerformed;
            input.Break.canceled -= OnBreakCanceled;
            
            input.Clutch.performed -= OnClutchOnPerformed;
            input.Clutch.canceled -= OnClutchCanceled;
            
            input.GearUp.performed -= OnGearUpOnPerformed;

            input.GearDown.performed -= OnGearDownPerformed;
        }

        protected override void DisableActions()
        {
            _inputSystem.DragRacer.Disable();
        }

        protected override void EnableActions()
        {
            _inputSystem.DragRacer.Enable();
        }
        
        #region Events
        
        private void OnAcceleratorPerformed(InputAction.CallbackContext obj) => _accelerator = 1f;

        private void OnAcceleratorCanceled(InputAction.CallbackContext obj) => _accelerator = 0f;

        private void OnBreakPerformed(InputAction.CallbackContext obj) => _brake = 1f;

        private void OnBreakCanceled(InputAction.CallbackContext obj) => _brake = 0f;

        private void OnClutchOnPerformed(InputAction.CallbackContext obj) => _clutch = 1f;

        private void OnClutchCanceled(InputAction.CallbackContext obj) => _clutch = 0f;

        private void OnGearUpOnPerformed(InputAction.CallbackContext obj)
        {
            _gear = (Gear)Mathf.Clamp((int)_gear + 1, -1, 5);
            OnTryGearChange?.Invoke(_gear);
        }

        private void OnGearDownPerformed(InputAction.CallbackContext obj)
        {
            _gear = (Gear)Mathf.Clamp((int)_gear - 1, -1, 5);
            OnTryGearChange?.Invoke(_gear);
        }
        
        #endregion Events

        
    }
}