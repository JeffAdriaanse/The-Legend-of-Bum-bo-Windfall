using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class TileDragFinishEvent : BumboEvent
    {
        private Block selected_block;
        public TileDragFinishEvent(GameObject selected_block)
        {
            if (selected_block != null) this.selected_block = selected_block.GetComponent<Block>();
        }

        public override void Execute()
        {
            bool turnWildOnDrag = false;

            for (int trinketIterator = 0; trinketIterator < WindfallHelper.app.model.characterSheet.trinkets.Count; trinketIterator++)
            {
                TrinketElement trinketElement = WindfallHelper.app.controller.GetTrinket(trinketIterator);
                if (turnWildOnDragTrinkets.Contains(trinketElement.trinketName)) turnWildOnDrag = true;
            }
            if (turnWildOnDragTrinkets.Contains(WindfallHelper.app.model.characterSheet.hiddenTrinket.trinketName)) turnWildOnDrag = true;

            if (turnWildOnDrag && selected_block != null) PuzzleHelper.PlaceBlock(selected_block.position, Block.BlockType.Wild, false, true);

            End();
        }

        public override BumboEvent NextEvent()
        {
            return new UpdatePuzzleEvent();
        }

        private static readonly List<TrinketName> turnWildOnDragTrinkets = new List<TrinketName>()
        {
            (TrinketName)1001,
            (TrinketName)1002,
        };
    }
}
