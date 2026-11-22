using System;
using UnityEngine;

namespace MobileGameProject.DragRacer
{
    [RequireComponent(typeof(Rigidbody))]
    public class Vehicle : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;
        [SerializeField] private Pedal _accelerator;
        public Pedal Accelerator => _accelerator;
        [SerializeField] private Pedal _brake;
        public Pedal Brake => _brake;
        [SerializeField] private Pedal _clutch;
        public Pedal Clutch => _clutch;
        [SerializeField] private GearSelector _gearSelector;
        public GearSelector GearSelector => _gearSelector;
        
        [Header("Meters")] [SerializeField] private float _currentRPM;
        public float CurrentRPM => _currentRPM;
        [SerializeField] private int _currentGear;
        public int CurrentGear => _currentGear;

        private VehicleState _state;

        private void Awake()
        {
            if (_rigidbody == null) 
                _rigidbody = GetComponent<Rigidbody>();
        }
    }
}