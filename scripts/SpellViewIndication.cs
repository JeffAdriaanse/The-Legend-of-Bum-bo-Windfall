using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static The_Legend_of_Bum_bo_Windfall.SpellViewIndicator;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class SpellViewIndication
    {
        //Up to four indicators active at once
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(SpellViewIndication));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.SetSpell))]
        static void BumboController_SetSpell(BumboController __instance, int _spell_index, SpellElement _spell)
        {
            SpellView spellView = __instance.app.view.spells[_spell_index];
            UpdateSpellViewIndicators(spellView);
        }

        private static readonly Vector3 SPELL_VIEW_INDICATOR_LOCALPOSITION = new Vector3(0f, 0f, 0f);
        private static readonly float SPELL_VIEW_INDICATOR_OFFSET_X = 0f;
        private static void UpdateSpellViewIndicators(SpellView spellView)
        {
            List<SpellViewIndicator> spellViewIndicators = new List<SpellViewIndicator>();

            //Spell scales with spell damage stat
            if (SpellsThatScaleWithSpellDamageStat.Contains(spellView.SpellObject.spellName))
            {
                SpellViewIndicator spellDamageScalingIndicator = spellView.transform.Find("Spell Damage Scaling Indicator")?.GetComponent<SpellViewIndicator>();
                if (spellDamageScalingIndicator == null) spellDamageScalingIndicator = PlaceSpellViewIndicator(spellView, SpellViewIndicatorType.SpellDamageScaling).GetComponent<SpellViewIndicator>();
            }

            for (int i = 0; i < spellViewIndicators.Count; i++)
            {
                spellViewIndicators[i].transform.localPosition = SPELL_VIEW_INDICATOR_LOCALPOSITION;
                spellViewIndicators[i].transform.localPosition += new Vector3(0f, SPELL_VIEW_INDICATOR_OFFSET_X * i, 0f);
            }
        }

        private static GameObject PlaceSpellViewIndicator(SpellView spellView, SpellViewIndicatorType type)
        {
            GameObject spellViewIndicatorObject = GameObject.Instantiate(new GameObject()); //TODO: load asset
        }

        public static List<SpellName> SpellsThatScaleWithSpellDamageStat
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

    class SpellViewIndicator : MonoBehaviour
    {
        public enum SpellViewIndicatorType
        {
            DamageUpgrade,
            ManaCostUpgrade,
            SpellDamageScaling,
            TurnTimer,
        }

        SpellViewIndicatorType type;
        SpellElement spell;

        public string TooltipDescription()
        {
            string tooltipDescription;
            switch (type)
            {
                case SpellViewIndicatorType.SpellDamageScaling:
                    tooltipDescription = "Damage";
                    switch (spell.spellName)
                    {
                        case (SpellName)1000:
                            tooltipDescription = "Chain distance";
                            break;
                        case SpellName.BarbedWire:
                        case SpellName.OrangeBelt:
                            tooltipDescription = "Maximum damage limit";
                            break;
                        case SpellName.RockFriends:
                            tooltipDescription = "Number of rocks";
                            break;
                    }
                    tooltipDescription += " scales with Bum-bo's spell damage stat";
                    break;
                default:
                    tooltipDescription = string.Empty;
                    break;
            }
            return tooltipDescription;
        }
    }
}
