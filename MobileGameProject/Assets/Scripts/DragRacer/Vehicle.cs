using System;
using MobileGameProject.MGPInputSystem.DragRacer;
using UnityEngine;
using UnityEngine.Serialization;

namespace MobileGameProject.DragRacer
{
    [RequireComponent(typeof(Rigidbody))]
    public class Vehicle : MonoBehaviour
    {
        [Header("Self References")] [SerializeField]
        private Rigidbody _rigidbody;

        public Rigidbody Rigidbody => _rigidbody;
        [SerializeField] private VehicleWheel _wheelFrontLeft;
        public VehicleWheel WheelFrontLeft => _wheelFrontLeft;
        [SerializeField] private VehicleWheel _wheelFrontRight;
        public VehicleWheel WheelFrontRight => _wheelFrontRight;
        [SerializeField] private VehicleWheel _wheelRearLeft;
        public VehicleWheel WheelRearLeft => _wheelRearLeft;
        [SerializeField] private VehicleWheel _wheelRearRight;
        public VehicleWheel WheelRearRight => _wheelRearRight;
        [SerializeField] private Pedal _accelerator;
        public Pedal Accelerator => _accelerator;
        [SerializeField] private Pedal _brake;
        public Pedal Brake => _brake;
        [SerializeField] private Pedal _clutch;
        public Pedal Clutch => _clutch;
        [SerializeField] private GearSelector _gearSelector;
        public GearSelector GearSelector => _gearSelector;

        [Header("Scene Reference")] [SerializeField]
        private PlayerInputHandler _inputHandler;

        [Header("Meters")] [SerializeField] private float _currentRPM;
        public float CurrentRPM => _currentRPM;
        [SerializeField] private int _currentGear;
        public int CurrentGear => _currentGear;

        private VehicleState _state;

        public void Update()
        {
#if UNITY_EDITOR
            _accelerator.UpdateKeyboarValue(_inputHandler.Accelerator);
            _brake.UpdateKeyboarValue(_inputHandler.Brake);
            _clutch.UpdateKeyboarValue(_inputHandler.Clutch);
#endif
        }

        private void Awake()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() => UpdateWheels();

        public void TryShiftGear(int gear)
        {
            if (_clutch.Value > .9f)
                _gearSelector.RequestGear(gear);
        }

        [ContextMenu("Update Wheel Visuals")]
        private void UpdateWheels()
        {
            _wheelFrontLeft.UpdateVisual();
            _wheelFrontRight.UpdateVisual();
            _wheelRearLeft.UpdateVisual();
            _wheelRearRight.UpdateVisual();
        }
    }
}