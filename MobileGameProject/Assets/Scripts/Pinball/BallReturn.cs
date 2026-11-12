using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class BallReturn : MonoBehaviour
    {
        public void OnTriggerExit(Collider other)
        {
            var ball = other.gameObject.GetComponent<Ball>();
            if(ball == null) return;

            ball.ResetPosition();
        }
    }
}
