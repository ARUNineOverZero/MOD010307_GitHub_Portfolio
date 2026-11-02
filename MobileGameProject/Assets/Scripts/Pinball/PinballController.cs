using MobileGameProject.MGPInputSystem.Pinball;
using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class PinballController : MonoBehaviour
    {
        [Header("Scene Objects")]
        [SerializeField] private Camera _camera;
        [SerializeField] private PlayerInputHandler _inputHandler;
        [SerializeField] private Flipper _leftFlipper;
        [SerializeField] private Flipper _rightFlipper;
        [SerializeField] private Ball _ball;

        public void OnPressed()
        {
            var screenPosition = _camera.ScreenToViewportPoint(_inputHandler.Position);

            Debug.Log($"{_inputHandler.Position} - {screenPosition}");

            if(screenPosition.x < 0.5)
                _leftFlipper.OnPress();
            else
                _rightFlipper.OnPress();
        }

        public void OnRelease()
        {
            _leftFlipper.OnRelease();
            _rightFlipper.OnRelease();
        }
    }
}