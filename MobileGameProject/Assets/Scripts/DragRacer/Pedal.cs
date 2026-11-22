using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public class Pedal : MonoBehaviour
    {
        private float _value;
        public float Value => _value;
        
        public void UpdateKeyboarValue(float value) => _value = value;
    }
}