using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class Ball : MonoBehaviour
    {
        [Header("Self Reference")]
        [SerializeField] private Rigidbody _rb;
    }
}