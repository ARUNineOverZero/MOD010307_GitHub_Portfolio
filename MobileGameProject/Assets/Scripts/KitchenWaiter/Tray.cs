using TMPro;
using UnityEngine;

namespace MobileGameProject.KitchenWaiter
{
    public class Tray : MonoBehaviour, IHeldItem
    {
        private int _tableNumber;
        public int TableNumber => _tableNumber;
        public Transform Transform => transform;

        [SerializeField] public TextMeshProUGUI _text;

        private void Awake()
        {
            _tableNumber = Random.Range(1,6);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Waiter waiter = collision.gameObject.GetComponent<Waiter>();

            if (waiter == null) return;

            waiter.TryPickup(this);
            //_text.text = $"Table: {_tableNumber}";

        }

        public void Holding()
        {
            GetComponent<Collider2D>().enabled = false;
        }
    }
}