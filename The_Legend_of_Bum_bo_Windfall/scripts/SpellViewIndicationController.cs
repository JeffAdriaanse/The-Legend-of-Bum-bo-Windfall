using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class SpellViewIndicationController : MonoBehaviour
    {
        private static readonly float SPELL_VIEW_INDICATOR_SCALE = 0.045f;
        private static readonly Vector3 SPELL_VIEW_INDICATOR_BASE_LOCALPOSITION = new Vector3(0.185f, 0.015f, -0.003f);
        private static readonly float SPELL_VIEW_INDICATOR_SPACING_Y = 0.01f;
        private static readonly float SPELL_VIEW_INDICATOR_SPACING_X = 0.06f;
        private static readonly Vector3 SPELL_VIEW_INDICATOR_LOCALROTATION = new Vector3(270f, 0f, 0f);
        private static readonly Vector3 SPELL_VIEW_INDICATOR_LOCALSCALE = new Vector3(SPELL_VIEW_INDICATOR_SCALE, SPELL_VIEW_INDICATOR_SCALE, SPELL_VIEW_INDICATOR_SCALE);

        public void UpdateAllSpellViewIndicators()
        {
            for (int i = 0; i < WindfallHelper.app.model.characterSheet.spells.Count; i++)
            {
                UpdateSpellViewIndicators(WindfallHelper.app.view.spells[i]);
            }
        }

        public void UpdateSpellViewIndicators(SpellView spellView)
        {
            List<SpellViewIndicator> spellViewIndicators = new List<SpellViewIndicator>();
            spellViewIndicators.Add(UpdateSpellViewIndicator<SpellDamageScalingIndicator>(spellView));
            spellViewIndicators.Add(UpdateSpellViewIndicator<SpellDamageUpgradeIndicator>(spellView));
            spellViewIndicators.Add(UpdateSpellViewIndicator<SpellManaCostUpgradeIndicator>(spellView));
            spellViewIndicators.Add(UpdateSpellViewIndicator<SpellRechargeTimeUpgradeIndicator>(spellView));
            spellViewIndicators.Add(UpdateSpellViewIndicator<SpellFreeUseIndicator>(spellView));

            List<SpellViewIndicator> activeSpellViewIndicators = new List<SpellViewIndicator>();
            activeSpellViewIndicators.AddRange(spellViewIndicators.Where(spellViewIndicator => spellViewIndicator.gameObject.activeSelf));

            int numberOfRows = 1;
            int firstRowCutoff = 99;
            if (activeSpellViewIndicators.Count > 4)
            {
                numberOfRows = 2;
                firstRowCutoff = Mathf.CeilToInt(activeSpellViewIndicators.Count / 2);
            }

            for (int i = 0; i < activeSpellViewIndicators.Count; i++)
            {
                Vector3 localposition = new Vector3(SPELL_VIEW_INDICATOR_BASE_LOCALPOSITION.x, SPELL_VIEW_INDICATOR_BASE_LOCALPOSITION.y, SPELL_VIEW_INDICATOR_BASE_LOCALPOSITION.z);
                bool firstRow = i < firstRowCutoff;

                //X offset
                int rowSize = activeSpellViewIndicators.Count;
                if (numberOfRows > 1) rowSize = firstRow ? firstRowCutoff : activeSpellViewIndicators.Count - firstRowCutoff;
                rowSize--;

                int indexInRow = i;
                if (numberOfRows > 1 && !firstRow) indexInRow -= firstRowCutoff;

                float xOffset = ((rowSize / 2) - indexInRow) * -SPELL_VIEW_INDICATOR_SPACING_X;

                //Y offset
                float yOffset = 0f;
                if (numberOfRows > 1) yOffset = firstRow ? SPELL_VIEW_INDICATOR_SPACING_Y : -SPELL_VIEW_INDICATOR_SPACING_Y;

                localposition += new Vector3(xOffset, yOffset, 0f);

                WindfallHelper.ReTransform(activeSpellViewIndicators[i].gameObject, localposition, SPELL_VIEW_INDICATOR_LOCALROTATION, SPELL_VIEW_INDICATOR_LOCALSCALE, string.Empty);
            }
        }

        private T UpdateSpellViewIndicator<T>(SpellView spellView) where T : SpellViewIndicator
        {
            T spellViewIndicator = spellView.transform.Find(typeof(T).ToString())?.GetComponent<T>();
            if (spellViewIndicator == null) spellViewIndicator = CreateSpellViewIndicator<T>(spellView).GetComponent<T>();
            spellViewIndicator.gameObject.SetActive(spellViewIndicator.ApplicableToSpell());

            return spellViewIndicator;
        }

        private static readonly string SPELL_VIEW_INDICATOR_NAME = "SpellViewIndicator";
        private GameObject CreateSpellViewIndicator<T>(SpellView spellView) where T : SpellViewIndicator
        {
            GameObject spellViewIndicatorObject = GameObject.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>(SPELL_VIEW_INDICATOR_NAME), spellView.transform);

            spellViewIndicatorObject.AddComponent<WindfallTooltip>();

            T spellViewIndicator = spellViewIndicatorObject.AddComponent<T>();
            string typeName = typeof(T).ToString();

            //WindfallHelper.Reskin(spellViewIndicatorObject, null, null, Windfall.assetBundle.LoadAsset<Texture2D>(SPELL_VIEW_INDICATOR_NAME));
            if (spellViewIndicatorObject.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer) && spellViewIndicator != null)
            {
                meshRenderer.material?.SetTextureOffset("_MainTex", spellViewIndicator.MainTextureOffset());
            }
            WindfallHelper.ResetShader(spellViewIndicatorObject.transform);

            spellViewIndicatorObject.gameObject.name = typeName;

            return spellViewIndicatorObject;
        }
    }

    public class SpellViewIndicationControllerPatches()
    {
        //Patch: Update SpellViewIndicators on SetSpell (and adjust collider size)
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.SetSpell))]
        static void BumboController_SetSpell(BumboController __instance, int _spell_index, SpellElement _spell)
        {
            SpellView spellView = __instance.app.view.spells[_spell_index];

            //Adjust collider size
            BoxCollider boxCollider = spellView.GetComponent<BoxCollider>();
            if (boxCollider != null && boxCollider.size.z > 0.02f) boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, 0.01f);

            WindfallHelper.SpellViewIndicationController.UpdateSpellViewIndicators(spellView);
        }



        //Patch: Update SpellViewIndicators on IdleEvent
        [HarmonyPostfix, HarmonyPatch(typeof(IdleEvent), nameof(IdleEvent.Execute))]
        static void IdleEvent_Execute(IdleEvent __instance)
        {
            WindfallHelper.SpellViewIndicationController.UpdateAllSpellViewIndicators();
        }
        //Patch: Update SpellViewIndicators on ChanceToCastSpellEvent
        [HarmonyPostfix, HarmonyPatch(typeof(ChanceToCastSpellEvent), nameof(ChanceToCastSpellEvent.Execute))]
        static void ChanceToCastSpellEvent_Execute(IdleEvent __instance)
        {
            WindfallHelper.SpellViewIndicationController.UpdateAllSpellViewIndicators();
        }
        //Patch: Update SpellViewIndicators on RoomStartEvent
        [HarmonyPostfix, HarmonyPatch(typeof(RoomStartEvent), nameof(RoomStartEvent.Execute))]
        static void RoomStartEvent_Execute(RoomStartEvent __instance)
        {
            WindfallHelper.SpellViewIndicationController.UpdateAllSpellViewIndicators();
        }
        //Patch: Update SpellViewIndicators on NewRoundEvent
        [HarmonyPostfix, HarmonyPatch(typeof(NewRoundEvent), nameof(NewRoundEvent.Execute))]
        static void NewRoundEvent_Execute(NewRoundEvent __instance)
        {
            WindfallHelper.SpellViewIndicationController.UpdateAllSpellViewIndicators();
        }
    }

    public abstract class SpellViewIndicator : MonoBehaviour
    {
        public SpellView SpellView
        {
            get { return transform.parent.GetComponent<SpellView>(); }
        }
        public abstract bool ApplicableToSpell();
        public abstract string TooltipDescription();
        public abstract Vector2Int MainTexturePosition();
        public Vector2 MainTextureOffset()
        {
            Vector2Int mainTexturePosition = MainTexturePosition();
            return new Vector2(mainTexturePosition.x * 0.329f, mainTexturePosition.y * 0.27f);
        }
    }

    public class SpellDamageScalingIndicator : SpellViewIndicator
    {
        public override bool ApplicableToSpell()
        {
            if (SpellView?.SpellObject != null)
            {
                return CollectibleStatistics.spellsThatScaleWithSpellDamageStat.Contains(SpellView.SpellObject.spellName);
            }
            return false;
        }
        public override string TooltipDescription()
        {
            string tooltipDescription = "DAMAGE_SCALING_SPELL";
            if (SpellView?.SpellObject != null)
            {
                switch (SpellView.SpellObject.spellName)
                {
                    case (SpellName)1000:
                        tooltipDescription = "PLASMA_BALL_SCALING_SPELL";
                        break;
                    case SpellName.BarbedWire:
                    case SpellName.OrangeBelt:
                        tooltipDescription = "DAMAGE_LIMIT_SCALING_SPELL";
                        break;
                    case SpellName.RockFriends:
                        tooltipDescription = "ROCK_FRIENDS_SCALING_SPELL";
                        break;
                }
            }
            return LocalizationModifier.GetLanguageText(tooltipDescription, "Indicators");
        }
        public override Vector2Int MainTexturePosition()
        {
            return new Vector2Int(0, 0);
        }
    }
    public class SpellDamageUpgradeIndicator : SpellViewIndicator
    {
        public int DamageUpgradeValue
        {
            get
            {
                if (SpellView == null || SpellView.SpellObject == null) return 0;
                return SpellView.SpellObject.baseDamage - CollectibleStatistics.SpellInitialBaseDamage(SpellView.SpellObject);
            }
        }
        public override bool ApplicableToSpell()
        {
            return DamageUpgradeValue != 0;
        }
        public override string TooltipDescription()
        {
            string tooltipDescription = "DAMAGE_UPGRADE_SPELL";
            string damageText = DamageUpgradeValue > 0 ? "+" + DamageUpgradeValue : DamageUpgradeValue.ToString();
            string tooltipDescriptionWithValues = LocalizationModifier.GetLanguageText(tooltipDescription, "Indicators")?.Replace("[damage]", damageText);
            return tooltipDescriptionWithValues != null ? tooltipDescriptionWithValues : string.Empty;
        }
        public override Vector2Int MainTexturePosition()
        {
            return new Vector2Int(1, 0);
        }
    }
    public class SpellManaCostUpgradeIndicator : SpellViewIndicator
    {
        public int ManaCostUpgradeValue
        {
            get
            {
                if (SpellView == null) return 0;

                SpellElement spellElement = SpellView.SpellObject;
                if (spellElement == null) return 0;

                if (spellElement.IsChargeable == true) return 0;

                return WindfallHelper.SpellTotalManaCost(spellElement, false) - CollectibleStatistics.SpellBaseManaCost(spellElement);
            }
        }
        public override bool ApplicableToSpell()
        {
            return ManaCostUpgradeValue != 0;
        }
        public override string TooltipDescription()
        {
            string tooltipDescription = "MANA_COST_UPGRADE_SPELL";
            string manaCostText = ManaCostUpgradeValue > 0 ? "+" + ManaCostUpgradeValue : ManaCostUpgradeValue.ToString();
            string tooltipDescriptionWithValues = LocalizationModifier.GetLanguageText(tooltipDescription, "Indicators")?.Replace("[cost]", manaCostText);
            return tooltipDescriptionWithValues != null ? tooltipDescriptionWithValues : string.Empty;
        }
        public override Vector2Int MainTexturePosition()
        {
            return new Vector2Int(2, 0);
        }
    }
    public class SpellRechargeTimeUpgradeIndicator : SpellViewIndicator
    {
        public int RechargeTimeUpgradeValue
        {
            get
            {
                if (SpellView == null) return 0;

                SpellElement spellElement = SpellView.SpellObject;
                if (spellElement == null) return 0;

                if (spellElement.IsChargeable == false) return 0;
                if (CollectibleStatistics.SpellBaseRechargeTime(spellElement) == -1) return 0;

                return spellElement.requiredCharge - CollectibleStatistics.SpellBaseRechargeTime(spellElement);
            }
        }
        public override bool ApplicableToSpell()
        {
            return RechargeTimeUpgradeValue != 0;
        }
        public override string TooltipDescription()
        {
            string tooltipDescription = "RECHARGE_TIME_UPGRADE_SPELL";
            string rechargeTimeText = RechargeTimeUpgradeValue > 0 ? "+" + RechargeTimeUpgradeValue : RechargeTimeUpgradeValue.ToString();
            string tooltipDescriptionWithValues = LocalizationModifier.GetLanguageText(tooltipDescription, "Indicators")?.Replace("[recharge]", rechargeTimeText);
            return tooltipDescriptionWithValues != null ? tooltipDescriptionWithValues : string.Empty;
        }
        public override Vector2Int MainTexturePosition()
        {
            return new Vector2Int(0, -1);
        }
    }
    public class SpellFreeUseIndicator : SpellViewIndicator
    {
        public bool FreeUse
        {
            get
            {
                if (SpellView == null) return false;
                return SpellView.SpellObject.costOverride;
            }
        }
        public override bool ApplicableToSpell()
        {
            return FreeUse;
        }
        public override string TooltipDescription()
        {
            string tooltipDescription = "FREE_USE_SPELL";
            return LocalizationModifier.GetLanguageText(tooltipDescription, "Indicators");
        }
        public override Vector2Int MainTexturePosition()
        {
            return new Vector2Int(1, -1);
        }
    }
}