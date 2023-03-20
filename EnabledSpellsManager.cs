using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class EnabledSpellsManager
    {
        public static void ResetState()
        {
            foreach (SpellView spellView in WindfallHelper.app.view.spells)
            {
                bool enabled = ObjectDataStorage.GetData(spellView, "enabled") == 1f;

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

        public static void ChangeTemporaryState(List<SpellElement> spellsToDisable)
        {
            foreach (SpellView spellView in WindfallHelper.app.view.spells)
            {
                float enabled = spellView.enabled ? 1f : 0f;
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

    }
}
