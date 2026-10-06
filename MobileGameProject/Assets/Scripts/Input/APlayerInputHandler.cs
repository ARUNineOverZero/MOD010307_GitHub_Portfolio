using System;
using UnityEngine;

namespace MobileGameProject.MGPInputSystem
{
    public abstract class APlayerInputHandler : MonoBehaviour
    {
        protected InputSystem _inputSystem;

        public void SetUnputSystem(InputSystem inputSystem)
        {
            if(_inputSystem != null)
                DeregisterActions();

            _inputSystem = inputSystem;

            if(_inputSystem != null)
                RegisterActions();
        }

        protected abstract void EnableActions();
        protected abstract void DisableActions();

        protected abstract void RegisterActions();

        protected abstract void DeregisterActions();

        private void OnEnable()
        {
            if(_inputSystem != null)
                EnableActions();
        }

        private void OnDisable()
        {
            if (_inputSystem != null)
                DisableActions();
        }

        private void OnDestroy()
        {
            if(_inputSystem != null)
                DeregisterActions();
        }
    }
}