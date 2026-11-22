using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public abstract class VehicleState
    {
        protected Vehicle _vehicle;

        public virtual void Enter()
        {
        }

        public abstract void Update(float deltaTime);

        public virtual void Exit()
        {
        }

        public VehicleState(Vehicle vehicle) => _vehicle = vehicle;
    }

    public class IdleState : VehicleState
    {
        public IdleState(Vehicle vehicle) : base(vehicle)
        {
        }

        public override void Enter() => _vehicle.ApplyMotorTorque(0f, 0f);

        public override void Update(float deltaTime)
        {
            _vehicle.UpdateEngineRPM(_vehicle.AcceleratorValue, 0f);

            if (_vehicle.RequestedGear != Gear.Neutral && _vehicle.ClutchValue < 0.4f)
                _vehicle.ChangeState(_vehicle.InGearState);
        }
    }

    public class InGearState : VehicleState
    {
        public InGearState(Vehicle vehicle) : base(vehicle)
        {
        }

        public override void Update(float deltaTime)
        {
            float clutchFactor = 1f - Mathf.Clamp01(_vehicle.ClutchValue);
            _vehicle.UpdateEngineRPM(_vehicle.AcceleratorValue, clutchFactor);

            float engineTorque = _vehicle.EngineRPM < _vehicle.MaxRPM
                ? _vehicle.AcceleratorValue * _vehicle.MaxEngineTorque
                : 0f;

            float wheelTorque = 0f;
            Gear gear = _vehicle.RequestedGear;

            if (gear == Gear.Reverse)
                wheelTorque = -engineTorque * _vehicle.ReverseRatio * _vehicle.FinalDrive * clutchFactor;
            else if (gear != Gear.Neutral)
            {
                int index = Mathf.Clamp((int) gear - 1, 0, _vehicle.GearRatios.Length - 1);
                float ratio = _vehicle.GearRatios[index];
                wheelTorque = engineTorque * ratio * _vehicle.FinalDrive * clutchFactor;
            }

            float brakeTorque = _vehicle.BrakeValue * _vehicle.MaxBrakeTorque;

            if (_vehicle.AcceleratorValue < 0.05f && gear != Gear.Neutral && _vehicle.ClutchValue < 0.3f)
                brakeTorque += 150f;

            _vehicle.ApplyMotorTorque(wheelTorque, brakeTorque);

            if (gear == Gear.Neutral && _vehicle.ClutchValue > 0.8f)
                _vehicle.ChangeState(_vehicle.IdleState);
        }
    }

    public class ShiftingState : VehicleState
    {
        private Gear _targetGear;

        public ShiftingState(Vehicle vehicle) : base(vehicle)
        {
        }

        public void SetTargetGear(Gear gear) => _targetGear = gear;

        public override void Enter() => _vehicle.ApplyMotorTorque(0f, 0f);

        public override void Update(float deltaTime)
        {
            _vehicle.UpdateEngineRPM(_vehicle.AcceleratorValue * 0.3f, 0f);

            if (_vehicle.ClutchValue < 0.35f)
            {
                _vehicle.GearSelector.RequestGear(_targetGear);
                _vehicle.ChangeState(_vehicle.InGearState);
            }
        }
    }
}