using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class ModifySpellHoverPreviewController : MonoBehaviour
    {
        public void DisplayUpgradePreview(SpellView spellView, SpellUpgrade spellUpgrade)
        {
            ModifySpellHoverPreview modifySpellHoverPreview = spellView.GetComponent<ModifySpellHoverPreview>();
            modifySpellHoverPreview.OriginalSpell = spellView.SpellObject;

            CacheUpgradePreview(spellView, spellUpgrade);

            //Display upgrade (temporarily). Note that this spell is not being placed in Bum-bo's CharacterSheet.
            WindfallHelper.app.controller.SetSpell(spellView.spellIndex, modifySpellHoverPreview.spellUpgradePreviews[spellUpgrade]);

            modifySpellHoverPreview.previewing = true;
        }

        public void ApplyUpgrade(SpellView spellView, SpellUpgrade spellUpgrade)
        {
            CacheUpgradePreview(spellView, spellUpgrade);

            ModifySpellHoverPreview modifySpellHoverPreview = spellView.GetComponent<ModifySpellHoverPreview>();

            SpellElement spell = modifySpellHoverPreview.spellUpgradePreviews[spellUpgrade];
            WindfallHelper.app.model.characterSheet.spells[spellView.spellIndex] = spell;
            WindfallHelper.app.controller.SetSpell(spellView.spellIndex, spell);

            //Since the spell upgrade has been applied, it must be removed to make room for another potential upgrade from the same source
            RemoveCachedUpgradePreview(spellView, spellUpgrade);
        }

        private void CacheUpgradePreview(SpellView spellView, SpellUpgrade spellUpgrade)
        {
            //The specific SpellElement upgrade results are cached within the ModifySpellHoverPreview accoring to the specific SpellUpgrade object and persist between spell upgrade events.
            //This is to prevent the player from canceling and redoing the spell upgrade event to freely reroll chance based spell upgrades (such as rerolling spell costs).
            //TODO: Save cached SpellElement upgrade results to the saved state so that they persist even when Bum-bo fully exits and re-enters the game.
            ModifySpellHoverPreview modifySpellHoverPreview = spellView.GetComponent<ModifySpellHoverPreview>();
            if (!modifySpellHoverPreview.spellUpgradePreviews.ContainsKey(spellUpgrade)) modifySpellHoverPreview.spellUpgradePreviews[spellUpgrade] = GenerateUpgradePreview(spellView.SpellObject, spellUpgrade);
        }

        private void RemoveCachedUpgradePreview(SpellView spellView, SpellUpgrade spellUpgrade)
        {
            ModifySpellHoverPreview modifySpellHoverPreview = spellView.GetComponent<ModifySpellHoverPreview>();
            if (modifySpellHoverPreview.spellUpgradePreviews.ContainsKey(spellUpgrade)) modifySpellHoverPreview.spellUpgradePreviews.Remove(spellUpgrade);
            modifySpellHoverPreview.OriginalSpell = null;
        }

        public void ClearUpgradePreviews()
        {
            foreach (SpellView spellview in WindfallHelper.app.view.spells) ClearUpgradePreview(spellview);
        }

        public void ClearUpgradePreview(SpellView spellView)
        {
            ModifySpellHoverPreview modifySpellHoverPreview = spellView.GetComponent<ModifySpellHoverPreview>();
            if (modifySpellHoverPreview == null || !modifySpellHoverPreview.previewing) return;

            //Reset spellView display
            if (modifySpellHoverPreview.OriginalSpell != null) WindfallHelper.app.controller.SetSpell(spellView.spellIndex, modifySpellHoverPreview.OriginalSpell);
            //modifySpellHoverPreview.originalSpell = null;

            modifySpellHoverPreview.previewing = false;
        }

        private SpellElement GenerateUpgradePreview(SpellElement spell, SpellUpgrade spellUpgrade)
        {
            if (spellUpgrade.ValidateSpell(spell))
            {
                SpellElement upgradedSpell = WindfallHelper.CopySpell(spell);
                spellUpgrade.ApplyUpgrade(ref upgradedSpell);
                return upgradedSpell;
            }
            return spell;
        }
    }

    public class ModifySpellHoverPreview : MonoBehaviour
    {
        public bool previewing;
        public Dictionary<object, SpellElement> spellUpgradePreviews = new Dictionary<object, SpellElement>();
        private readonly Color spellViewUpgradePreviewTintColor = new Color(0.75f, 0.75f, 0.75f);


        private SpellElement originalSpell;
        public SpellElement OriginalSpell
        {
            get { return originalSpell; }
            set
            {
                //If the spellView SpellObject has changed, the upgrade previews need to be regenerated for the new SpellObject
                if (value == null || !WindfallHelper.CompareSpells(originalSpell, value, true)) spellUpgradePreviews = new Dictionary<object, SpellElement>();
                if (value == null) originalSpell = value;
                else originalSpell = WindfallHelper.CopySpell(value);
            }
        }

        private void Update()
        {
            if (WindfallHelper.app.model.paused) return;

            SpellView spellView = GetComponent<SpellView>();
            if (spellView == null) return;

            UpdateTint();

            if (WindfallHelper.app.model.bumboEvent.ToString() != "SpellModifyEvent" && WindfallHelper.app.model.bumboEvent.ToString() != "SpellModifySpellEvent")
            {
                WindfallHelper.ModifySpellHoverPreviewController.ClearUpgradePreview(spellView);
                return;
            }

            if (spellView.gamepadSelectionObject.activeSelf) OnMouseOver();

            DetectMouseEvents();
        }

        private void OnMouseOver()
        {
            if (WindfallHelper.app.model.paused) return;
            if (previewing) return;

            SpellView spellView = GetComponent<SpellView>();

            //Clear all other upgrade previews
            for (int i = 0; i < WindfallHelper.app.view.spells.Count; i++)
            {
                SpellView currentSpellView = WindfallHelper.app.view.spells[i];
                if (spellView != currentSpellView) WindfallHelper.ModifySpellHoverPreviewController.ClearUpgradePreview(currentSpellView);
            }

            if (spellView != null && spellView.SpellObject != null && !spellView.IsDisabled())
            {
                SpellUpgrade spellUpgrade = null;
                Type upgradeSourceType = null;

                if (WindfallHelper.app.model.bumboEvent.ToString() == "SpellModifyEvent")
                {
                    PrickTrinket prick = WindfallHelper.app.model.gamblingModel?.prick;
                    if (prick != null) upgradeSourceType = prick.GetType();
                }
                else if (WindfallHelper.app.model.bumboEvent.ToString() == "SpellModifySpellEvent")
                {
                    TrinketElement currentTrinket = CollectibleChanges.currentTrinket;

                    SpellElement currentSpell = WindfallHelper.app.model.spellModel.currentSpell;
                    if (currentTrinket != null) upgradeSourceType = currentTrinket.GetType();
                    else if (currentSpell != null) upgradeSourceType = currentSpell.GetType();
                }

                if (upgradeSourceType != null && collectibleUpgradeEffects.TryGetValue(upgradeSourceType, out SpellUpgrade value)) spellUpgrade = value;
                if (spellUpgrade != null) WindfallHelper.ModifySpellHoverPreviewController.DisplayUpgradePreview(spellView, spellUpgrade);
            }
        }

        private bool isMouseOver = false;
        private void DetectMouseEvents()
        {
            Ray GUIray = WindfallHelper.app.view.GUICamera.cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(GUIray);

            bool hitThisFrame = false;

            foreach (var hit in hits)
            {
                if (hit.collider == GetComponent<Collider>())
                {
                    hitThisFrame = true;
                    if (!isMouseOver) isMouseOver = true;
                    break;
                }
            }

            if (!hitThisFrame && isMouseOver)
            {
                OnMouseFullyExit();
                isMouseOver = false;
            }
        }

        //This is a similar method to Unity's builtin OnMouseExit, except that this method doesn't trigger if the mouse gets blocked by a closer collider, only triggering if the mouse is nowhere over this collider at all.
        //This is to prevent the ModifySpellHoverPreview from clearing when the mouse is blocked from the SpellView by a SpellViewIndicator, which would cause the ModifySpellHoverPreview to clear and reappear rapidly each frame.
        private void OnMouseFullyExit()
        {
            if (WindfallHelper.app.model.paused) return;

            //Clear the upgrade preview
            SpellView spellView = GetComponent<SpellView>();
            if (spellView != null) WindfallHelper.ModifySpellHoverPreviewController.ClearUpgradePreview(spellView);
        }

        public void UpdateTint()
        {
            SpellView spellView = GetComponent<SpellView>();

            ObjectTinter objectTinter = GetComponent<ObjectTinter>();
            if (objectTinter == null) objectTinter = spellView.gameObject.AddComponent<ObjectTinter>();

            if (spellView == null) return;

            if (previewing && !InputManager.Instance.IsUsingGamepadInput())
            {
                if (!(bool)AccessTools.Field(typeof(ObjectTinter), "tinted").GetValue(objectTinter)) objectTinter.Tint(spellViewUpgradePreviewTintColor);
            }
            else
            {
                objectTinter.NoTint();
            }
        }

        public static readonly Dictionary<Type, SpellUpgrade> collectibleUpgradeEffects = new Dictionary<Type, SpellUpgrade>()
        {
            //Needles
            { typeof(DamagePrickTrinket), new SpellDamageUpgrade(1) },
            { typeof(ManaPrickTrinket), new SpellManaCostUpgrade(CollectibleStatistics.TrinketManaCostReductionPercentage(TrinketName.ManaPrick)) },
            { typeof(ShufflePrickTrinket), new SpellManaCostRerollUpgrade() },
            { typeof(ChargePrickTrinket), new SpellRechargeTimeUpgrade(CollectibleStatistics.TrinketRechargeTimeReduction(TrinketName.ChargePrick)) },
            { typeof(RandomPrickTrinket), new SpellRerollUpgrade() },

            //Other trinkets
            { typeof(RainbowTickTrinket), new SpellManaCostUpgrade(CollectibleStatistics.TrinketManaCostReductionPercentage(TrinketName.RainbowTick)) },
            { typeof(BrownTickTrinket), new SpellRechargeTimeUpgrade(CollectibleStatistics.TrinketRechargeTimeReduction(TrinketName.BrownTick)) },

            //Spells
            { typeof(D6Spell), new SpellRerollUpgrade() },
        };
    }

    public class ModifySpellHoverPreviewPatches
    {
        //Patch: Adds ModifySpellHoverPreview to spellViews
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.SetSpell))]
        static void BumboController_SetSpell(BumboController __instance, int _spell_index)
        {
            SpellView spellView = __instance.app.view.spells[_spell_index];
            ModifySpellHoverPreview modifySpellHoverPreview = spellView.GetComponent<ModifySpellHoverPreview>();
            if (modifySpellHoverPreview == null) spellView.gameObject.AddComponent<ModifySpellHoverPreview>();
        }

        //Patch: Adapts D6 to apply the previewed upgrade 
        [HarmonyPrefix, HarmonyPatch(typeof(D6Spell), nameof(D6Spell.AlterSpell))]
        static bool D6Spell_AlterSpell(D6Spell __instance, int _spell_index)
        {
            __instance.app.model.spellModel.currentSpell = null;
            __instance.app.model.spellModel.spellQueued = false;

            //Apply D6 upgrade effect
            SpellView spellView = __instance.app.view.spells[_spell_index];
            if (spellView != null && ModifySpellHoverPreview.collectibleUpgradeEffects.TryGetValue(__instance.GetType(), out SpellUpgrade spellUpgrade))
            {
                WindfallHelper.ModifySpellHoverPreviewController.ApplyUpgrade(spellView, spellUpgrade);
            }

            __instance.app.controller.eventsController.EndEvent();
            return false;
        }

        //Patch: Adapts Damage Needle spell upgrade qualification
        [HarmonyPostfix, HarmonyPatch(typeof(DamagePrickTrinket), "QualifySpell")]
        static void DamagePrickTrinket_QualifySpell(DamagePrickTrinket __instance, int _spell_index)
        {
            SpellView spellView = WindfallHelper.app.view.spells[_spell_index];
            if (ValidateNeedleEffect(__instance.GetType(), _spell_index)) spellView.EnableSpell();
            else spellView.DisableSpell();
        }
        //Patch: Adapts Damage Needle to apply the previewed upgrade 
        [HarmonyPrefix, HarmonyPatch(typeof(DamagePrickTrinket), nameof(DamagePrickTrinket.UpdateSpell))]
        static bool DamagePrickTrinket_UpdateSpell(DamagePrickTrinket __instance, int _spell_index)
        {
            ApplyNeedleEffect(__instance, _spell_index);
            __instance.app.controller.SetSpellDamage(_spell_index);
            return false;
        }

        //Patch: Adapts Mana Needle spell upgrade qualification
        [HarmonyPostfix, HarmonyPatch(typeof(ManaPrickTrinket), "QualifySpell")]
        static void ManaPrickTrinket_QualifySpell(ManaPrickTrinket __instance, int _spell_index)
        {
            SpellView spellView = WindfallHelper.app.view.spells[_spell_index];
            if (ValidateNeedleEffect(__instance.GetType(), _spell_index)) spellView.EnableSpell();
            else spellView.DisableSpell();
        }
        //Patch: Adapts Mana Needle to apply the previewed upgrade
        [HarmonyPrefix, HarmonyPatch(typeof(ManaPrickTrinket), nameof(ManaPrickTrinket.UpdateSpell))]
        static bool ManaPrickTrinket_UpdateSpell(ManaPrickTrinket __instance, int _spell_index)
        {
            ApplyNeedleEffect(__instance, _spell_index);
            __instance.app.controller.UpdateSpellManaText();
            return false;
        }

        //Patch: Adapts Shuffle Needle to apply the previewed upgrade 
        [HarmonyPrefix, HarmonyPatch(typeof(ShufflePrickTrinket), nameof(ShufflePrickTrinket.UpdateSpell))]
        static bool ShufflePrickTrinket_UpdateSpell(ShufflePrickTrinket __instance, int _spell_index)
        {
            ApplyNeedleEffect(__instance, _spell_index);
            return false;
        }

        //Patch: Adapts Charge Needle spell upgrade qualification
        [HarmonyPostfix, HarmonyPatch(typeof(ChargePrickTrinket), "QualifySpell")]
        static void ChargePrickTrinket_QualifySpell(ChargePrickTrinket __instance, int _spell_index)
        {
            SpellView spellView = WindfallHelper.app.view.spells[_spell_index];
            if (ValidateNeedleEffect(__instance.GetType(), _spell_index)) spellView.EnableSpell();
            else spellView.DisableSpell();
        }
        //Patch: Adapts Charge Needle to apply the previewed upgrade 
        [HarmonyPrefix, HarmonyPatch(typeof(ChargePrickTrinket), nameof(ChargePrickTrinket.UpdateSpell))]
        static bool ChargePrickTrinket_UpdateSpell(ChargePrickTrinket __instance, int _spell_index)
        {
            ApplyNeedleEffect(__instance, _spell_index);
            __instance.app.controller.UpdateSpellManaText();
            return false;
        }

        //Patch: Adapts Random Needle to apply the previewed upgrade 
        [HarmonyPrefix, HarmonyPatch(typeof(RandomPrickTrinket), nameof(RandomPrickTrinket.UpdateSpell))]
        static bool RandomPrickTrinket_UpdateSpell(RandomPrickTrinket __instance, int _spell_index)
        {
            ApplyNeedleEffect(__instance, _spell_index);
            return false;
        }

        //Applies a Needle upgrade effect
        private static void ApplyNeedleEffect(PrickTrinket needle, int _spell_index)
        {
            SpellView spellView = needle.app.view.spells[_spell_index];
            if (spellView != null && ModifySpellHoverPreview.collectibleUpgradeEffects.TryGetValue(needle.GetType(), out SpellUpgrade spellUpgrade))
            {
                WindfallHelper.ModifySpellHoverPreviewController.ApplyUpgrade(spellView, spellUpgrade);
            }

            needle.app.view.soundsView.PlaySound(SoundsView.eSound.ItemUpgraded, SoundsView.eAudioSlot.Default, false);
            needle.app.view.spells[_spell_index].spellParticles.Play();
        }

        //Validates a Needle upgrade effect
        private static bool ValidateNeedleEffect(Type needleType, int _spell_index)
        {
            SpellElement spellElement = WindfallHelper.app.model.characterSheet.spells[_spell_index];
            if (spellElement == null) return false;

            if (ModifySpellHoverPreview.collectibleUpgradeEffects.TryGetValue(needleType, out SpellUpgrade spellUpgrade))
            {
                return spellUpgrade.ValidateSpell(spellElement);
            }
            return false;
        }

        //Patch: Adapts shop AddManaPrick
        [HarmonyPrefix, HarmonyPatch(typeof(Shop), "AddManaPrick")]
        static bool Shop_AddManaPrick(Shop __instance, ref List<TrinketName> ___needles)
        {
            int spellIterator = 0;
            while (spellIterator < __instance.app.model.characterSheet.spells.Count)
            {
                if (ValidateNeedleEffect(typeof(ManaPrickTrinket), spellIterator))
                {
                    ___needles.Add(TrinketName.ManaPrick);
                    return false;
                }
                spellIterator++;
            }
            return false;
        }

        //Patch: Adapts shop AddDamagePrick
        [HarmonyPrefix, HarmonyPatch(typeof(Shop), "AddDamagePrick")]
        static bool Shop_AddDamagePrick(Shop __instance, ref List<TrinketName> ___needles)
        {
            Console.WriteLine("0");

            int spellIterator = 0;
            while (spellIterator < __instance.app.model.characterSheet.spells.Count)
            {
                if (ValidateNeedleEffect(typeof(DamagePrickTrinket), spellIterator))
                {
                    Console.WriteLine("1");
                    ___needles.Add(TrinketName.DamagePrick);
                    return false;
                }
                spellIterator++;
            }
            return false;
        }
    }
}
