using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class MagnifyingGlassSpell : PuzzleSpell
    {
        public MagnifyingGlassSpell()
        {
            Name = "MAGNIFYING_GLASS_DESCRIPTION";
            Category = SpellCategory.Puzzle;
            texturePage = 1;
            IconPosition = new Vector2(0f, 0f);
            spellName = (SpellName)1001;
            manaSize = ManaSize.M;
        }

        public override bool CastSpell()
        {
            //Boilerplate
            if (!base.CastSpell())
            {
                return false;
            }
            app.model.spellModel.currentSpell = null;
            app.model.spellModel.spellQueued = false;

            //Logic
            BlockGroupData blockGroupData;

            List<Block> blocks = PuzzleHelper.GetBlocks(true, true, null);
            while (blocks.Count > 0)
            {
                //Default BlockGroup size is 2
                blockGroupData = new BlockGroupData(2);

                //Choose a random tile
                int randomIndex = UnityEngine.Random.Range(0, blocks.Count);
                Block block = blocks[randomIndex];
                blocks.RemoveAt(randomIndex);

                //If the tile is already in a BlockGroup, the BlockGroup is replaced by a bigger BlockGroup
                BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
                if (blockGroup != null)
                {
                    blockGroupData = new BlockGroupData(blockGroup.GetBlockGroupData());
                    blockGroupData.ChangeSize(1);
                }

                //Effect
                if (BlockGroupModel.PlaceBlockGroup(block, blockGroupData, false, true, true)) break;
            }

            app.controller.eventsController.SetEvent(new MovePuzzleEvent(0f));
            return true;
        }
    }
}
