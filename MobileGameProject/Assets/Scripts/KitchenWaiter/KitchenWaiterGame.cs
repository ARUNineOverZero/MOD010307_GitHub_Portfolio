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
        [SerializeField] private AnalogueStick _analogueStick;
        [SerializeField] private Waiter _waiter;
        [SerializeField] private Tray _tray;
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
            int tableCount = Random.Range(2, 6);

            _tables = new Table[tableCount];

            float sizeX = 2, sizeY = _mainCamera.orthographicSize;

            var bounds = new Vector2(sizeX, sizeY);

            for (int i = 0; i < tableCount; i++)
            {
                var t = Instantiate(_table, Vector3.zero, Quaternion.identity).GetComponent<Table>();
                t.SetTableNumber(i + 1);
                t.onEnterTable.AddListener(EnterTable);
                t.onLeaveTable.AddListener(LeaveTable);
                _tables[i] = t;
            }

            var trayTableNumber = _tables[UnityEngine.Random.Range(0, tableCount)].TableNumber;
            _tray.SetTableNumber(tableCount);
        }

        private Vector2[] GetTablePositions(Vector2 size, float maxDistance, Vector2 startPosition, int count)
        {
            Vector2[] positions = new Vector2[count];
            positions[0] = startPosition;

            bool IsTooClose(Vector2 candidate, int alreadyPlaced)
            {
                for (int i = 0; i < alreadyPlaced; i++)
                {
                    if (Vector2.Distance(positions[i], candidate) < maxDistance)
                        return true;
                }
                return false;
            }

            for (int i = 1; i < count; i++)
            {
                Vector2 candidate;
                do
                {
                    candidate = new Vector2(
                        Random.Range(0f, size.x),
                        Random.Range(0f, size.y)
                    );
                }
                while (IsTooClose(candidate, i)); 

                positions[i] = candidate;
            }

            return positions;
        }

        private void UpdateTableNumber(int tableNumber = -1)
        {
            _targetTableNumber = tableNumber;
            //_tableNumberText.text = $"Table Number: {(_targetTableNumber == -1 ? "None" : _targetTableNumber.ToString()) }";
        }

        public void ItemPickedUp(IPickUp item)
        {

        }

        public void ItemHeld(IHeldItem item)
        {
            switch (item)
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
            switch (item)
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
            switch (item)
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
            if (_targetTableNumber == -1) return;

            if (table.TableNumber == _targetTableNumber)
            {
                table.CorrectTableEntered();
                Win();
            }
            else
            {
                table.IncorrectTableEntered();
                Lose();
            }
        }

        public void LeaveTable(Table table)
        {
            if (_targetTableNumber == -1) return;

            table.TableExited();
        }
    }
}