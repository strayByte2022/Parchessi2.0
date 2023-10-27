using System;
using _Scripts.Player.Pawn;
using UnityEngine;

namespace _Scripts.Player.Dice
{
    [RequireComponent(typeof(HandDice))]
    public class HandDiceDragAndTargeter : DragAndTargeterObject
    {
        private void Start()
        {
            this.DisableDrag();
        }
    }
}