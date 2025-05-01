using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class MagnifyingGlassSpell : PuzzleSpell
    {
        private static readonly float tileAnimationDuration = 0.25f;
        public MagnifyingGlassSpell()
        {
            Name = "MAGNIFYING_GLASS_DESCRIPTION";
            Category = SpellCategory.Puzzle;
            texturePage = 2;
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

            Sequence magnifyingGlassSequence = DOTween.Sequence();

            //Logic
            Block block = null;
            Position blockGroupPosition = null;
            BlockGroupData blockGroupData = new BlockGroupData(2);

            List<Block> blocks = PuzzleHelper.GetBlocks(true, true, true, null);
            while (blocks.Count > 0)
            {
                //Default BlockGroup size is 2
                blockGroupData = new BlockGroupData(2);

                //Choose a random tile
                int randomIndex = UnityEngine.Random.Range(0, blocks.Count);
                block = blocks[randomIndex];
                blocks.RemoveAt(randomIndex);

                //If the tile is already in a BlockGroup, the BlockGroup is replaced by a bigger BlockGroup
                BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
                if (blockGroup != null)
                {
                    blockGroupData = new BlockGroupData(blockGroup.GetBlockGroupData());
                    blockGroupData.ChangeSize(1);
                    block = blockGroup.MainBlock;
                }

                //Effect
                blockGroupPosition = BlockGroupModel.FindValidGroupOffset(block.position, blockGroupData.dimensions, true, true);
                if (blockGroupPosition != null) break;
            }

            if (block != null && blockGroupPosition != null)
            {
                //Set BumboEvent to prevent player input before the spell effect has finished
                app.controller.eventsController.SetEvent(new BumboEvent());

                magnifyingGlassSequence.Append(ShortcutExtensions.DOShakePosition(block.transform, tileAnimationDuration, 0.05f, 20, 90f, false, true));
                magnifyingGlassSequence.Join(ShortcutExtensions.DOShakeRotation(block.transform, tileAnimationDuration, 10f, 20, 90f, true));
                magnifyingGlassSequence.AppendCallback(delegate { BlockGroupModel.PlaceBlockGroup(blockGroupPosition, block.block_type, blockGroupData, false, true); });
                magnifyingGlassSequence.AppendCallback(delegate { app.controller.eventsController.SetEvent(new MovePuzzleEvent(0f)); });
                return true;
            }

            app.controller.eventsController.SetEvent(new MovePuzzleEvent(0f));
            return true;
        }
    }
}
