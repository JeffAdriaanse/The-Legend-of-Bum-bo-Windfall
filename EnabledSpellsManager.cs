using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Legend_of_Bum_bo_Windfall
{
    /// <summary>
    /// Temporarily changes the enabled state of Bum-bo's spells. Useful for disabling spells during <see cref="ChanceToCastSpellEvent"/> without losing track of which spells have been disabled by enemies.
    /// </summary>
    public static class EnabledSpellsManager
    {
        /// <summary>
        /// Changes the enabled state of Bum-bo's spells while keeping track of their exising enabled state.
        /// </summary>
        /// <param name="spellsToDisable">The spells owned by Bum-bo that should be disabled.</param>
        public static void ChangeTemporaryState(List<SpellElement> spellsToDisable)
        {
            foreach (SpellView spellView in WindfallHelper.app.view.spells)
            {
                float enabled = spellView.disableObject.activeSelf ? 1f : 0f;
                ObjectDataStorage.StoreData(spellView, "enabled", enabled);

                if (spellsToDisable.Contains(spellView.SpellObject))
                {
                    spellView.DisableSpell();
                }
                else
                {
                    spellView.EnableSpell();
                }
            }
        }

        /// <summary>
        /// Returns all spells to their previous enabled states. Should be used when <see cref="ChanceToCastSpellEvent"/> is ending.
        /// </summary>
        public static void ResetState()
        {
            foreach (SpellView spellView in WindfallHelper.app.view.spells)
            {
                float enabled = ObjectDataStorage.GetData(spellView, "enabled");

                if (enabled == 1f)
                {
                    spellView.EnableSpell();
                }
                else if (enabled == 0f)
                {
                    spellView.DisableSpell();
                }
            }
        }
    }
}
