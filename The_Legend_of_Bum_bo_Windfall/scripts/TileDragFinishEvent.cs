using HarmonyLib;
using System.Collections.Generic;
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
            if (WindfallHelper.WildActionPointsController.AttemptPlaceWild() && selected_block != null) PuzzleHelper.PlaceBlock(selected_block.position, Block.BlockType.Wild, false, true);
            End();
            return;
        }

        public override BumboEvent NextEvent()
        {
            return new UpdatePuzzleEvent();
        }
    }
}
