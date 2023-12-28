using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (!base.CastSpell())
            {
                return false;
            }
            app.model.spellModel.currentSpell = this;
            app.controller.eventsController.SetEvent(new PuzzleSpellEvent());
            app.controller.GUINotification("ENLARGE_TILE", GUINotificationView.NotifyType.Puzzle, this, false);
            return true;
        }

        public override void AlterTile(Block _block)
        {
            //Abort if the tile is already in a BlockGroup
            if (BlockGroupModel.FindGroupOfBlock(_block) != null) return;

            //Logic
            if (!BlockGroupModel.CreateBlockGroup(_block, new Vector2Int(2, 2))) return;

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
