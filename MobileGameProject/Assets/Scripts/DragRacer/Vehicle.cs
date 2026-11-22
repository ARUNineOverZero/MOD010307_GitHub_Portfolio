using System.Linq;
using MobileGameProject.MGPInputSystem.DragRacer;
using UnityEngine;

namespace MobileGameProject.DragRacer
{
    [RequireComponent(typeof(Rigidbody))]
    public class Vehicle : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
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
        public float AcceleratorValue => _accelerator.Value;

        [SerializeField] private Pedal _brake;
        public Pedal Brake => _brake;
        public float BrakeValue => _brake.Value;

        [SerializeField] private Pedal _clutch;
        public Pedal Clutch => _clutch;
        public float ClutchValue => _clutch.Value;

        [SerializeField] private GearSelector _gearSelector;
        public GearSelector GearSelector => _gearSelector;
        public Gear RequestedGear => _gearSelector.RequestedGear;

        [Header("Input")]
        [SerializeField] private PlayerInputHandler _inputHandler;

        [Header("Engine")]
        [SerializeField] private float _idleRPM = 800f;
        public float IdleRPM => _idleRPM;

        [SerializeField] private float _maxRPM = 7000f;
        public float MaxRPM => _maxRPM;

        [SerializeField] private float _maxEngineTorque = 400f;
        public float MaxEngineTorque => _maxEngineTorque;

        [SerializeField] private float _engineRPM;
        public float EngineRPM => _engineRPM;

        [Header("Transmission")]
        [SerializeField] private float[] _gearRatios = { 3.5f, 2.2f, 1.5f, 1.1f, 0.85f };
        public float[] GearRatios => _gearRatios;

        [SerializeField] private float _reverseRatio = 3.2f;
        public float ReverseRatio => _reverseRatio;

        [SerializeField] private float _finalDrive = 3.7f;
        public float FinalDrive => _finalDrive;

        [Header("Wheels")]
        [SerializeField] private float _maxBrakeTorque = 4000f;
        public float MaxBrakeTorque => _maxBrakeTorque;

        private WheelCollider[] _drivenWheels;
        public WheelCollider[] DrivenWheels => _drivenWheels;

        [Header("Speed")]
        [SerializeField] private float _velocitySpeed;
        [SerializeField] private float _velocityMPH;
        [SerializeField] private float _wheelSpeed;
        [SerializeField] private float _wheelMPH;

        [Header("States")]
        private IdleState _idleState;
        public IdleState IdleState => _idleState;

        private InGearState _inGearState;
        public InGearState InGearState => _inGearState;

        private ShiftingState _shiftingState;
        public ShiftingState ShiftingState => _shiftingState;

        private VehicleState _currentState;

        private void Awake()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            _drivenWheels = new[]
            {
                _wheelFrontLeft.WheelCollider,
                _wheelFrontRight.WheelCollider
            };

            _idleState = new IdleState(this);
            _inGearState = new InGearState(this);
            _shiftingState = new ShiftingState(this);
        }

        private void Start()
        {
            ChangeState(_idleState);
        }

        private void Update()
        {
#if UNITY_EDITOR
            _accelerator.UpdateKeyboarValue(_inputHandler.Accelerator);
            _brake.UpdateKeyboarValue(_inputHandler.Brake);
            _clutch.UpdateKeyboarValue(_inputHandler.Clutch);
#endif
            UpdateSpeed();
        }

        private void FixedUpdate()
        {
            UpdateWheels();
            _currentState?.Update(Time.fixedDeltaTime);
        }

        public void ChangeState(VehicleState newState)
        {
            if (_currentState == newState) return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public void ApplyMotorTorque(float motorTorque, float brakeTorque)
        {
            if (_drivenWheels == null || _drivenWheels.Length == 0) return;

            float torquePerWheel = motorTorque / _drivenWheels.Length;

            foreach (var wheel in _drivenWheels)
            {
                if (wheel == null) continue;
                wheel.motorTorque = torquePerWheel;
                wheel.brakeTorque = brakeTorque;
            }
        }

        public void UpdateEngineRPM(float throttle, float clutchFactor)
        {
            float targetRPM = _idleRPM;
            bool inGear = RequestedGear != Gear.Neutral;
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

                if (count > 0)
                    avgWheelRPM /= count;

                float ratio = RequestedGear == Gear.Reverse
                    ? _reverseRatio
                    : _gearRatios[Mathf.Clamp((int)RequestedGear - 1, 0, _gearRatios.Length - 1)];

                targetRPM = avgWheelRPM * ratio * _finalDrive;
            }
            else
                targetRPM = _idleRPM + throttle * (_maxRPM - _idleRPM);

            _engineRPM = Mathf.Lerp(_engineRPM, targetRPM, Time.fixedDeltaTime * 4f);
            _engineRPM = Mathf.Clamp(_engineRPM, _idleRPM * 0.5f, _maxRPM + 200f);
        }

        private void UpdateSpeed()
        {
            _velocitySpeed = _rigidbody.linearVelocity.magnitude;
            _velocityMPH = _velocitySpeed * 2.23694f;

            var wheel = _drivenWheels?.FirstOrDefault();
            if (wheel == null) return;

            float circumference = 2f * Mathf.PI * wheel.radius;
            _wheelSpeed = Mathf.Abs((wheel.rpm * circumference) / 60f);
            _wheelMPH = _wheelSpeed * 2.23694f;
        }

        private void UpdateWheels()
        {
            _wheelFrontLeft.UpdateVisual();
            _wheelFrontRight.UpdateVisual();
            _wheelRearLeft.UpdateVisual();
            _wheelRearRight.UpdateVisual();
        }

        public void TryShiftGear(Gear gear)
        {
            if (ClutchValue > 0.9f)
                _gearSelector.RequestGear(gear);
        }
    }
}