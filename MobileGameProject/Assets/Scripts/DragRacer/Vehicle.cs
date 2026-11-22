using System;
using MobileGameProject.MGPInputSystem.DragRacer;
using UnityEngine;
using UnityEngine.Serialization;

namespace MobileGameProject.DragRacer
{
    [RequireComponent(typeof(Rigidbody))]
    public class Vehicle : MonoBehaviour
    {
        [Header("Self References")] 
        [SerializeField] private Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;
        
        [SerializeField] private VehicleWheel _wheelFrontLeft;
        public VehicleWheel WheelFrontLeft => _wheelFrontLeft;
        
        [SerializeField] private VehicleWheel _wheelFrontRight;
        public VehicleWheel WheelFrontRight => _wheelFrontRight;
        
        [SerializeField] private VehicleWheel _wheelRearLeft;
        public VehicleWheel WheelRearLeft => _wheelRearLeft;
        
        [SerializeField] private VehicleWheel _wheelRearRight;

        [SerializeField] private Pedal _accelerator;
        public Pedal Accelerator => _accelerator;
        
        [SerializeField] private Pedal _brake;
        public Pedal Brake => _brake;
        
        [SerializeField] private Pedal _clutch;
        public Pedal Clutch => _clutch;
        
        [SerializeField] private GearSelector _gearSelector;
        public GearSelector GearSelector => _gearSelector;

        [Header("Scene Reference")] 
        [SerializeField]
        private PlayerInputHandler _inputHandler;

        [Header("Meters")] 
        [SerializeField] private float _currentRPM;
        public float CurrentRPM => _currentRPM;
        [SerializeField] private int _currentGear;
        public int CurrentGear => _currentGear;

        private VehicleState _state;

        [Header("Wheel Setting")] private WheelCollider[] _drivenWheels;
        public VehicleWheel WheelRearRight => _wheelRearRight;
        [SerializeField] private float _maxMotorTorque = 2500f;
        [SerializeField] private float _maxBrakeTorque = 4000f;

        [Header("Engine/Gear")] [SerializeField]
        private float[] _gearRatios = {3.5f, 2.2f, 1.5f, 1.1f, 0.85f};

        [SerializeField] private float _reverseRatio = 3.2f;
        [SerializeField] private float _finalDrive = 3.7f;

        [SerializeField] private float _speed;
        
        

        public void Update()
        {
#if UNITY_EDITOR
            _accelerator.UpdateKeyboarValue(_inputHandler.Accelerator);
            _brake.UpdateKeyboarValue(_inputHandler.Brake);
            _clutch.UpdateKeyboarValue(_inputHandler.Clutch);
#endif

            _speed = _rigidbody.linearVelocity.magnitude;
        }

        private void Awake()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            _drivenWheels = new[] { _wheelFrontLeft.WheelCollider, _wheelFrontRight.WheelCollider };
        }

        private void FixedUpdate()
        {
            UpdateWheels();
            ApplyDriveForce();
        }

        private void ApplyDriveForce()
        {
            float throttle = _accelerator.Value;
            float brake = _brake.Value;
            float clutch = _clutch.Value;

            float clutchFactor = 1f - Mathf.Clamp01(clutch);

            float motorTorque = 0f;
            float brakeTorque = brake * _maxBrakeTorque;

            switch (_gearSelector.RequestedGear)
            {
                case Gear.Neutral:
                    motorTorque = 0f;
                    break;

                case Gear.Reverse:
                    motorTorque = -throttle * _maxMotorTorque * _reverseRatio * _finalDrive * clutchFactor;
                    break;

                case Gear.First:
                case Gear.Second:
                case Gear.Third:
                case Gear.Fourth:
                case Gear.Fifth:
                    int gearIndex = (int) _gearSelector.RequestedGear - 1;
                    float ratio = _gearRatios[Mathf.Clamp(gearIndex, 0, _gearRatios.Length - 1)];
                    motorTorque = throttle * _maxMotorTorque * ratio * _finalDrive * clutchFactor;
                    break;
            }

            foreach (var wheel in _drivenWheels)
            {
                if (wheel == null) continue;
                
                wheel.motorTorque = motorTorque / _drivenWheels.Length;
                wheel.brakeTorque = brakeTorque;
            }

            if (throttle < 0.05f && _gearSelector.RequestedGear != Gear.Neutral && clutch < 0.3f)
                foreach (var wheel in _drivenWheels)
                    wheel.brakeTorque += 200f;
        }

        public void TryShiftGear(Gear gear)
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