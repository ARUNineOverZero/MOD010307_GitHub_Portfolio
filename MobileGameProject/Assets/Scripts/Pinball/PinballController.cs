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

        public void OnLeftPressed()
        {
            Debug.Log(nameof(OnLeftPressed));
            _leftFlipper.OnPress();
        }

        public void OnRightPressed()
        {
            Debug.Log(nameof(OnRightPressed));

            _rightFlipper.OnPress();
        }

        public void OnLeftReleased()
        {
            _leftFlipper.OnRelease();
        }

        public void OnRightReleased()
        {
            _rightFlipper.OnRelease();
        }
    }
}