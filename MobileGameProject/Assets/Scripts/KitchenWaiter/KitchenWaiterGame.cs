using UnityEngine;
using MobileGameProject.Framework;
using MobileGameProject.MGPInputSystem.Virtual;
using TMPro;

namespace MobileGameProject.KitchenWaiter
{
    public class KitchenWaiterGame : AMicrogameBehaviour
    {
        [Header("Scene UI Objects")]
        [SerializeField] private TextMeshPro _tableNumberText;

        [Header("Scene Game Objects")]
        [SerializeField] private  AnalogueStick _analogueStick;
        [SerializeField] private  Waiter _waiter;
        [SerializeField] private  Tray _tray;

        private int _tableNumber = -1;

        public override void Begin(MicrogameSession session)
        {
            base.Begin(session);
        }

        private void UpdateTableNumber(int tableNumber = -1)
        {
            _tableNumber = tableNumber;
            _tableNumberText.text = $"Table Number: {(_tableNumber == -1 ? "None" : _tableNumber.ToString()) }";
        }

        public void ItemPickedUp(IPickUp item)
        {
            
        }

        public void ItemHeld(IHeldItem item)
        {
            switch(item)
            {
                case Tray t:
                    UpdateTableNumber(t.TableNumber);
                    break;
                default:
                    break;
            }
        }

        public void ItemDropped(IHeldItem item)
        {
            switch(item)
            {
                case Tray t:
                    UpdateTableNumber();
                    break;
                default:
                    break;
            }
        }
    }
}