using UnityEngine;
using MobileGameProject.Framework;
using MobileGameProject.MGPInputSystem.Virtual;
using TMPro;

//https://kenney.nl/assets/food-kit

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
        [SerializeField] private Table[] _tables;

        [Header("Prefabs")]
        [SerializeField] private GameObject _table;

        private int _targetTableNumber = -1;

        public override void Begin(MicrogameSession session)
        {
            base.Begin(session);

            LayoutTables();
        }

        private void LayoutTables()
        {
            _tables = new Table[1];

            var t = Instantiate(_table, Vector3.zero, Quaternion.identity).GetComponent<Table>();
            t.SetTableNumber(1);
            t.onEnterTable.AddListener(EnterTable);
            t.onLeaveTable.AddListener(LeaveTable);

            _tables[0] = t;

            var trayTableNumber = _tables[UnityEngine.Random.Range(0, _tables.Length)].TableNumber;
            _tray.SetTableNumber(trayTableNumber);
        }

        private void UpdateTableNumber(int tableNumber = -1)
        {
            _targetTableNumber = tableNumber;
            _tableNumberText.text = $"Table Number: {(_targetTableNumber == -1 ? "None" : _targetTableNumber.ToString()) }";
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

        public void ItemDelieveredToTable(IHeldItem item)
        {
            switch(item)
            {
                case Tray t:
                    //
                    break;
                default:
                    break;
            }
        }

        public void EnterTable(Table table)
        {
            if(_targetTableNumber == -1) return;

            if(table.TableNumber == _targetTableNumber)
                table.CorrectTableEntered();
            else
                table.IncorrectTableEntered();
        }

        public void LeaveTable(Table table)
        {
            if(_targetTableNumber == -1) return;

            table.TableExited();
        }
    }
}