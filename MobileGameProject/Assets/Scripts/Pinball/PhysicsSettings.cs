using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class PhysicsSettings : MonoBehaviour
    {
        public Vector3 gravity = new Vector3(0, -981f, 0);
        public float defaultContactOffset = 1f;
        public float bounceThreshold = 150f;
        public int solverIterations = 12;
        public int solverVelocityIterations = 8;
        public float fixedTimestep = .01f;

        [SerializeField] private Vector3 originalGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] private float originalDefaultContactOffset = .01f;
        [SerializeField] private float originalBounceThreshold = 2f;
        [SerializeField] private int originalSolverIterations = 6;
        [SerializeField] private int originalSolverVelocityIterations = 1;
        [SerializeField] private float originalFixedTimestep = 0.01999999f;

        void Awake()
        {
            Physics.gravity = gravity;
            Physics.defaultContactOffset = defaultContactOffset;
            Physics.bounceThreshold = bounceThreshold;
            Physics.defaultSolverIterations = solverIterations;
            Physics.defaultSolverVelocityIterations = solverVelocityIterations;
            Time.fixedDeltaTime = fixedTimestep;
        }

        [ContextMenu("Physics Settings/Set Original Settings Fields")]
        private void SetOriginalSettingValues()
        {
            originalGravity = Physics.gravity;
            originalDefaultContactOffset = Physics.defaultContactOffset;
            originalBounceThreshold = Physics.bounceThreshold;
            originalSolverIterations = Physics.defaultSolverIterations;
            originalSolverVelocityIterations = Physics.defaultSolverVelocityIterations;
            originalFixedTimestep = Time.fixedDeltaTime;
        }

        [ContextMenu("Physics Settings/Set Project Settings To Original Values")]
        private void SetProjectSettingsPhysicsToOriginalValues()
        {
            Physics.gravity = originalGravity;
            Physics.defaultContactOffset = originalDefaultContactOffset;
            Physics.bounceThreshold = originalBounceThreshold;
            Physics.defaultSolverIterations = originalSolverIterations;
            Physics.defaultSolverVelocityIterations = originalSolverVelocityIterations;
            Time.fixedDeltaTime = originalFixedTimestep;
        }

        private void OnDestroy() => SetProjectSettingsPhysicsToOriginalValues();
    }
}