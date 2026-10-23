using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MobileGameProject.KitchenWaiter
{
    public class Tray : MonoBehaviour, IHeldItem
    {
        private int _tableNumber;
        public int TableNumber => _tableNumber;
        public Transform Transform => transform;

        public void SetTableNumber(int tableNumber)
        {
            _tableNumber = tableNumber;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Waiter waiter = collision.gameObject.GetComponent<Waiter>();

            if (waiter == null) return;

            waiter.TryPickup(this);
        }

        public void Holding()
        {
            GetComponent<Collider2D>().enabled = false;
        }
    }
}