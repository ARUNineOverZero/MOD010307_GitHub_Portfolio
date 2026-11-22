using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public class GearSelector : MonoBehaviour
    {
        private Gear _requestedGear = Gear.Neutral;
        public Gear RequestedGear => _requestedGear;
        
        public void RequestGear(Gear gear)
        {
            _requestedGear = gear;
        }
    }
    
    

    public enum Gear
    {
        Reverse = -1,
        Neutral = 0,
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Fifth = 5
    }
}