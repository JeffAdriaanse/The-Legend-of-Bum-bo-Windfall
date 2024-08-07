﻿using System.Collections.Generic;

namespace The_Legend_of_Bum_bo_Windfall
{
    /// <summary>
    /// Temporarily changes the enabled state of Bum-bo's spells. Useful for disabling spells during <see cref="ChanceToCastSpellEvent"/> without losing track of which spells have been disabled by enemies.
    /// </summary>
    public static class EnabledSpellsManager
    {
        private static readonly string spellViewEnabledKey = "spellViewEnabledKey";

        /// <summary>
        /// Changes the enabled state of Bum-bo's spells while keeping track of their exising enabled state.
        /// </summary>
        /// <param name="spellsToDisable">The spells owned by Bum-bo that should be disabled.</param>
        public static void ChangeTemporaryState(List<SpellElement> spellsToDisable)
        {
            foreach (SpellView spellView in WindfallHelper.app.view.spells)
            {
                ObjectDataStorage.StoreData<bool>(spellView, spellViewEnabledKey, !spellView.disableObject.activeSelf);

                if (spellView.SpellObject != null && spellsToDisable != null && spellsToDisable.Contains(spellView.SpellObject))
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
                bool enabled = true;
                if (ObjectDataStorage.HasData<bool>(spellView, spellViewEnabledKey))
                {
                    enabled = ObjectDataStorage.GetData<bool>(spellView, spellViewEnabledKey);
                }

                if (enabled)
                {
                    spellView.EnableSpell();
                }
                else
                {
                    spellView.DisableSpell();
                }
            }
        }
    }
}
