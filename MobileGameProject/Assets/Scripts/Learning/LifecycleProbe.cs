using UnityEngine;

namespace MobileGameProject.Learning
{
    public class LifecycleProbe : MonoBehaviour
    {
        private bool _hasLoggedFirstUpdate;

        private void Log(string eventName) => Debug.Log($"[frame {Time.frameCount}] {gameObject.name}: {eventName}");


        private void Awake()
        {
            Log(nameof(Awake));
        }

        private void OnEnable()
        {
            Log(nameof(OnEnable));
        }

        private void OnDisabled()
        {
            Log(nameof(SOnDisabledtart));
        }

        private void Start()
        {
            Log(nameof(Start));
        }

         private void Update()
        {
            if(!_hasLoggedFirstUpdate)
            {
                Log(nameof(Update));
                _hasLoggedFirstUpdate = true;
            }
        }

        private void OnDestroy()
        {
            Log(nameof(OnDestroy));
        }
    }
}