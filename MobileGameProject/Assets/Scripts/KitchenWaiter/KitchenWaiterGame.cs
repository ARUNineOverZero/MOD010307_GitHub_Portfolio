using UnityEngine;
using MobileGameProject.Framework;
using MobileGameProject.MGPInputSystem.Virtual;

namespace MobileGameProject.KitchenWaiter
{
    public class KitchenWaiterGame : AMicrogameBehaviour
    {
        [SerializeField] private  AnalogueStick _analogueStick;
        [SerializeField] private  Waiter _waiter;
        [SerializeField] private  Tray _tray;

        public override void Begin(MicrogameSession session)
        {
            base.Begin(session);
        }

        private void Update()
        {
            
        }
    }
}