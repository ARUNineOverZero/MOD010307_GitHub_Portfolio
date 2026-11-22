using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public class GearSelector : MonoBehaviour
    {
        public const int Reverse = -1;
        public const int Neutral = -1;
        public const int MaxGear = 5;

        private int _RequestedGear;
        public int RequestedGear => _RequestedGear;
        
        
        public void RequestGear(int gear)
        {
            throw new System.NotImplementedException();
        }
    }
}