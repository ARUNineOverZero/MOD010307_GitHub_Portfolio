using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public class VehicleWheel : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private WheelCollider _wheelCollider;
        public WheelCollider WheelCollider => _wheelCollider;
        [SerializeField] private Transform _visualMesh;
        public Transform VisualMesh => _visualMesh;

        [Header("Settings")] public Vector3 positionOffset;

        [ContextMenu("Update Visual")]
        public void UpdateVisual()
        {
            if (_wheelCollider == null || _visualMesh == null) return;

            _wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            _visualMesh.position = pos + positionOffset;
            _visualMesh.rotation = rot;
        }
    }
}