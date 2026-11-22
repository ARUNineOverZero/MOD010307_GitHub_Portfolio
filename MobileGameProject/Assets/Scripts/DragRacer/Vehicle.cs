using System;
using System.Linq;
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

        [Header("Wheel Setting")] private WheelCollider[] _drivenWheels;
        public VehicleWheel WheelRearRight => _wheelRearRight;
        [SerializeField] private float _maxMotorTorque = 2500f;
        [SerializeField] private float _maxBrakeTorque = 4000f;

        [Header("Engine")] [SerializeField] private float _idleRPM = 800f;
        [SerializeField] private float _maxRPM = 7000f;
        [SerializeField] private float _maxEngineTorque = 400f;

        [Header("Engine/Gear")] [SerializeField]
        private float[] _gearRatios = {3.5f, 2.2f, 1.5f, 1.1f, 0.85f};

        [SerializeField] private float _reverseRatio = 3.2f;
        [SerializeField] private float _finalDrive = 3.7f;

        [SerializeField] private float _engineRPM;
        [SerializeField] private float _velocitySpeed;
        [SerializeField] private float _velocityMPH;
        [SerializeField] private float _wheelSpeed;
        [SerializeField] private float _wheelMPH;

        private void Update()
        {
#if UNITY_EDITOR
            _accelerator.UpdateKeyboarValue(_inputHandler.Accelerator);
            _brake.UpdateKeyboarValue(_inputHandler.Brake);
            _clutch.UpdateKeyboarValue(_inputHandler.Clutch);
#endif
            UpdateSpeed();
        }

        private void UpdateSpeed()
        {
            _velocitySpeed = _rigidbody.linearVelocity.magnitude;
            _velocityMPH = _velocitySpeed * 2.23694f;

            var wheel = _drivenWheels.First();
            float circumference = 2f * Mathf.PI * wheel.radius;
            _wheelSpeed = (wheel.rpm * circumference) / 60f;

            _wheelSpeed = Mathf.Abs(_wheelSpeed);
            _wheelMPH = _wheelSpeed * 2.23694f;
        }

        private void Awake()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            _drivenWheels = new[] {_wheelFrontLeft.WheelCollider, _wheelFrontRight.WheelCollider};
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

            UpdateEngineRPM(throttle, clutchFactor);
            
            float engineTorque = 0f;
            if (_engineRPM < _maxRPM)
                engineTorque = throttle * _maxEngineTorque;
            
            float wheelTorque = 0f;

            switch (_gearSelector.RequestedGear)
            {
                case Gear.Neutral:
                    wheelTorque = 0f;
                    break;

                case Gear.Reverse:
                    wheelTorque = -engineTorque * _reverseRatio * _finalDrive * clutchFactor;
                    break;

                case Gear.First:
                case Gear.Second:
                case Gear.Third:
                case Gear.Fourth:
                case Gear.Fifth:
                    int gearIndex = (int) _gearSelector.RequestedGear - 1;
                    float ratio = _gearRatios[Mathf.Clamp(gearIndex, 0, _gearRatios.Length - 1)];
                    wheelTorque = engineTorque * ratio * _finalDrive * clutchFactor;
                    break;
            }

            float torquePerWheel = wheelTorque / _drivenWheels.Length;
            float brakeTorque = brake * _maxBrakeTorque;

            foreach (var wheel in _drivenWheels)
            {
                if (wheel == null) continue;
                wheel.motorTorque = torquePerWheel;
                wheel.brakeTorque = brakeTorque;
            }

            if (throttle < 0.05f && _gearSelector.RequestedGear != Gear.Neutral && clutch < 0.3f)
            {
                foreach (var wheel in _drivenWheels)
                    wheel.brakeTorque += 150f;
            }

            _currentRPM = _engineRPM;
        }

        private void UpdateEngineRPM(float throttle, float clutchFactor)
        {
            float targetRPM = _idleRPM;

            bool inGear = _gearSelector.RequestedGear != Gear.Neutral;
            bool clutchEngaged = clutchFactor > 0.6f;

            if (inGear && clutchEngaged && _drivenWheels.Length > 0)
            {
                float avgWheelRPM = 0f;
                int count = 0;
                foreach (var wheel in _drivenWheels)
                {
                    if (wheel == null) continue;
                    avgWheelRPM += Mathf.Abs(wheel.rpm);
                    count++;
                }

                if (count > 0) avgWheelRPM /= count;

                float ratio = _gearSelector.RequestedGear == Gear.Reverse
                    ? _reverseRatio
                    : _gearRatios[Mathf.Clamp((int) _gearSelector.RequestedGear - 1, 0, _gearRatios.Length - 1)];

                targetRPM = avgWheelRPM * ratio * _finalDrive;
            }
            else
                targetRPM = _idleRPM + throttle * (_maxRPM - _idleRPM);

            _engineRPM = Mathf.Lerp(_engineRPM, targetRPM, Time.fixedDeltaTime * 4f);
            _engineRPM = Mathf.Clamp(_engineRPM, _idleRPM * 0.5f, _maxRPM + 200f);
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