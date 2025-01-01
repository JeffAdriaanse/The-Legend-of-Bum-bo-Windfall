using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class ReadingStoneSpell : PuzzleSpell
    {
        public ReadingStoneSpell()
        {
            Name = "READING_STONE_DESCRIPTION";
            Category = SpellCategory.Puzzle;
            texturePage = 1;
            IconPosition = new Vector2(0f, 0f);
            spellName = (SpellName)1002;
            manaSize = ManaSize.M;
        }

        public override bool CastSpell()
        {
            if (!base.CastSpell())
            {
                return false;
            }
            app.model.spellModel.currentSpell = this;
            app.controller.eventsController.SetEvent(new PuzzleSpellEvent());
            app.controller.GUINotification("ENLARGE_TILE", GUINotificationView.NotifyType.Puzzle, this, false);
            return true;
        }

        public override bool CanAlterTile()
        {
            return true;
        }

        public override void AlterTile(Block _block)
        {
            //Logic
            //Default BlockGroup size is 2
            BlockGroupData blockGroupData = new BlockGroupData(2);

            //If the tile is already in a BlockGroup, the BlockGroup is replaced by a bigger BlockGroup
            BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(_block);
            if (blockGroup != null)
            {
                blockGroupData = new BlockGroupData(blockGroup.GetBlockGroupData());
                blockGroupData.ChangeSize(1);
            }

            //Effect
            if (!BlockGroupModel.PlaceBlockGroup(_block, blockGroupData, false, true, true)) return;

            //Sound
            app.view.soundsView.PlaySound(SoundsView.eSound.TileDestroyed, _block.transform.position, SoundsView.eAudioSlot.Default, false);

            //Boilerplate
            app.controller.HideNotifications(true);
            app.model.spellModel.currentSpell = null;
            app.model.spellModel.spellQueued = false;
            app.controller.eventsController.SetEvent(new ClearPuzzleEvent());
        }
    }
}
