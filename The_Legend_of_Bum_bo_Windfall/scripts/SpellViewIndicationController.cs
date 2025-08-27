using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static The_Legend_of_Bum_bo_Windfall.SpellViewIndicator;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class SpellViewIndicationController : MonoBehaviour
    {
        private static readonly float SPELL_VIEW_INDICATOR_SCALE = 0.045f;
        private static readonly Vector3 SPELL_VIEW_INDICATOR_BASE_LOCALPOSITION = new Vector3(0.185f, 0.015f, -0.003f);
        private static readonly float SPELL_VIEW_INDICATOR_SPACING_Y = 0.01f;
        private static readonly float SPELL_VIEW_INDICATOR_SPACING_X = 0.01f;
        private static readonly Vector3 SPELL_VIEW_INDICATOR_LOCALROTATION = new Vector3(270f, 0f, 0f);
        private static readonly Vector3 SPELL_VIEW_INDICATOR_LOCALSCALE = new Vector3(SPELL_VIEW_INDICATOR_SCALE, SPELL_VIEW_INDICATOR_SCALE, SPELL_VIEW_INDICATOR_SCALE);

        public void UpdateSpellViewIndicators(SpellView spellView)
        {
            List<SpellViewIndicator> spellViewIndicators = new List<SpellViewIndicator>();

            //Spell scales with spell damage stat

            SpellViewIndicator spellDamageScalingIndicator = spellView.transform.Find("Spell Damage Scaling Indicator")?.GetComponent<SpellViewIndicator>();
            if (SpellsThatScaleWithSpellDamageStat.Contains(spellView.SpellObject.spellName))
            {
                if (spellDamageScalingIndicator == null) spellDamageScalingIndicator = CreateSpellViewIndicator(spellView, SpellViewIndicatorType.SpellDamageScaling).GetComponent<SpellViewIndicator>();
                spellDamageScalingIndicator.gameObject.name = "Spell Damage Scaling Indicator";

                spellViewIndicators.Add(spellDamageScalingIndicator);
                spellDamageScalingIndicator.gameObject.SetActive(true);
            }
            else spellDamageScalingIndicator?.gameObject.SetActive(false);

            int numberOfRows = 1;
            int firstRowCutoff = 99;
            if (spellViewIndicators.Count > 4)
            {
                numberOfRows = 2;
                firstRowCutoff = Mathf.CeilToInt(spellViewIndicators.Count / 2);
            }

            for (int i = 0; i < spellViewIndicators.Count; i++)
            {
                Vector3 localposition = new Vector3(SPELL_VIEW_INDICATOR_BASE_LOCALPOSITION.x, SPELL_VIEW_INDICATOR_BASE_LOCALPOSITION.y, SPELL_VIEW_INDICATOR_BASE_LOCALPOSITION.z);
                bool firstRow = i < firstRowCutoff;

                //X offset
                int rowSize = spellViewIndicators.Count;
                if (numberOfRows > 1) rowSize = firstRow ? firstRowCutoff : spellViewIndicators.Count - firstRowCutoff;
                rowSize--;

                int indexInRow = i;
                if (numberOfRows > 1 && !firstRow) indexInRow -= firstRowCutoff;

                float xOffset = ((rowSize / 2) - indexInRow) * -SPELL_VIEW_INDICATOR_SPACING_X;

                //Y offset
                float yOffset = 0f;
                if (numberOfRows > 1) yOffset = firstRow ? SPELL_VIEW_INDICATOR_SPACING_Y : -SPELL_VIEW_INDICATOR_SPACING_Y;

                localposition += new Vector3(xOffset, yOffset, 0f);

                WindfallHelper.ReTransform(spellViewIndicators[i].gameObject, localposition, SPELL_VIEW_INDICATOR_LOCALROTATION, SPELL_VIEW_INDICATOR_LOCALSCALE, string.Empty);
            }
        }

        private GameObject CreateSpellViewIndicator(SpellView spellView, SpellViewIndicatorType type)
        {
            GameObject spellViewIndicatorObject = GameObject.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>("Spell View Indicator"), spellView.transform);
            SpellViewIndicator spellViewIndicator = spellViewIndicatorObject.AddComponent<SpellViewIndicator>();
            spellViewIndicator.type = type;
            spellViewIndicator.spell = spellView.SpellObject;
            spellViewIndicatorObject.AddComponent<WindfallTooltip>();

            if (spellViewIndicatorTypeNames.TryGetValue(type, out string value)) WindfallHelper.Reskin(spellViewIndicatorObject, Windfall.assetBundle.LoadAsset<Mesh>(value), null, Windfall.assetBundle.LoadAsset<Texture2D>(value));

            return spellViewIndicatorObject;
        }

        private readonly Dictionary<SpellViewIndicatorType, string> spellViewIndicatorTypeNames = new Dictionary<SpellViewIndicatorType, string>()
        {
            { SpellViewIndicatorType.SpellDamageScaling, "Spell Damage Scaling" },
        };

        private List<SpellName> SpellsThatScaleWithSpellDamageStat
        {
            get
            {
                List<SpellName> spells = new List<SpellName>
                {
                    { (SpellName)1000 }, //Chain distance
                    { SpellName.BarbedWire }, //Stacking limit
                    { SpellName.BeeButt },
                    { SpellName.BigRock },
                    { SpellName.BorfBucket },
                    { SpellName.BumboSmash },
                    { SpellName.MamaFoot },
                    { SpellName.MeatHook },
                    { SpellName.NailBoard },
                    { SpellName.Number1 },
                    { SpellName.OrangeBelt }, //Stacking limit
                    { SpellName.PuzzleFlick },
                    { SpellName.Rock },
                    { SpellName.Stick },
                    { SpellName.TheNegative },
                };

                List<SpellName> balancedSpells = new List<SpellName>
                {
                    { SpellName.AttackFly },
                    { SpellName.Brimstone },
                    { SpellName.DogTooth },
                    { SpellName.HairBall },
                    { SpellName.HatPin },
                    { SpellName.Lemon },
                    { SpellName.MamaShoe },
                    { SpellName.Pliers },
                    { SpellName.RockFriends }, //Attack count
                };

                if (WindfallPersistentDataController.LoadData().implementBalanceChanges) spells.AddRange(balancedSpells);

                return spells;
            }
        }
    }

    public class SpellViewIndicationControllerPatches()
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.SetSpell))]
        static void BumboController_SetSpell(BumboController __instance, int _spell_index, SpellElement _spell)
        {
            SpellView spellView = __instance.app.view.spells[_spell_index];

            //Adjust collider size
            BoxCollider boxCollider = spellView.GetComponent<BoxCollider>();
            if (boxCollider != null && boxCollider.size.z > 0.02f) boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, 0.01f);

            WindfallHelper.SpellViewIndicationController.UpdateSpellViewIndicators(spellView);
        }
    }

    class SpellViewIndicator : MonoBehaviour
    {
        public enum SpellViewIndicatorType
        {
            DamageUpgrade,
            ManaCostUpgrade,
            SpellDamageScaling,
            TurnTimer,
            FreeUse,
        }

        public SpellViewIndicatorType type;
        public SpellElement spell;

        public string TooltipDescription()
        {
            string tooltipDescription;
            switch (type)
            {
                case SpellViewIndicatorType.SpellDamageScaling:
                    tooltipDescription = "DAMAGE_SCALING_SPELL";
                    switch (spell.spellName)
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
                    break;
                default:
                    tooltipDescription = string.Empty;
                    break;
            }
            return LocalizationModifier.GetLanguageText(tooltipDescription, "Indicators");
        }
    }
}