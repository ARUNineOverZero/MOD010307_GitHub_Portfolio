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

        [ContextMenu("Update Visual")]
        public void UpdateVisual()
        {
            if (_wheelCollider == null || _visualMesh == null) return;

            _wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            _visualMesh.position = pos;
            _visualMesh.rotation = rot;
        }
        
        [ContextMenu("")]
    }
}