using System;
using MobileGameProject.Framework;
using MobileGameProject.KitchenWaiter;
using MobileGameProject.MGPInputSystem.Virtual;
using TMPro;
using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public class DragRacerGame : AMicrogameBehaviour
    {
        [Header("Scene Game Objects")] [SerializeField]
        private Vehicle _vehicle;

        public Vehicle Vehicle => _vehicle;

        public override void Begin(MicrogameSession session)
        {
            base.Begin(session);

            //LayoutTables(out Vector2 trayPosition);
            //SetTrayPositionAndNumber(trayPosition, _tables.Length);
        }

        public override void EndGame()
        {
            if (true)
                Win();
            else
                Lose();
        }
    }
}