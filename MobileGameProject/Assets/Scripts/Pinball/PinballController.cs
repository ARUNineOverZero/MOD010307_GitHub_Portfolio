using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class PinballController : MonoBehaviour
    {
        [Header("Scene Game Objects")]
        [SerializeField] private Flipper _leftFlipper;
        [SerializeField] private Flipper _rightFlipper;
        [SerializeField] private Ball _ball;
    }
}