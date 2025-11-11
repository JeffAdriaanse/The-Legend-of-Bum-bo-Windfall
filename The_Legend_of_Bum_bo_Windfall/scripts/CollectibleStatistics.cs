using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class CollectibleStatistics
    {
        public static int SpellBaseManaCost(SpellElement spell)
        {
            int totalSpellCost = -1;

            //New mana costs
            if (spellBaseManaCost.TryGetValue(spell.spellName, out int value)) totalSpellCost = value;

            //Old mana costs
            if (totalSpellCost == -1)
            {
                switch (spell.manaSize)
                {
                    case SpellElement.ManaSize.S:
                        totalSpellCost = 2;
                        break;
                    case SpellElement.ManaSize.M:
                        totalSpellCost = 4;
                        break;
                    case SpellElement.ManaSize.L:
                        totalSpellCost = 6;
                        break;
                    case SpellElement.ManaSize.XL:
                        totalSpellCost = 10;
                        break;
                    case SpellElement.ManaSize.XXL:
                        totalSpellCost = 16;
                        break;
                    case SpellElement.ManaSize.XXXL:
                        totalSpellCost = 20;
                        break;
                    default:
                        totalSpellCost = 1;
                        break;
                }
            }
            return totalSpellCost;
        }

        private static readonly Dictionary<SpellName, int> spellBaseManaCost = new Dictionary<SpellName, int>()
        {
            { (SpellName)1001, 3 },
            { SpellName.Addy, 3 },
            { SpellName.AttackFly, 5 },
            { SpellName.BlenderBlade, 5 },
            { SpellName.BuzzDown, 2 },
            { SpellName.BuzzRight, 2 },
            { SpellName.BuzzUp, 2 },
            { SpellName.DeadDove, 4 },
            { SpellName.DogTooth, 9 },
            { SpellName.HairBall, 5 },
            { SpellName.Juiced, 5 },
            { SpellName.KrampusCross, 5 },
            { SpellName.Lemon, 5 },
            { SpellName.MagicMarker, 6 },
            { SpellName.MamaFoot, 13 },
            { SpellName.Melatonin, 6 },
            { SpellName.MissingPiece, 8 },
            { SpellName.Peace, 3 },
            { SpellName.Pliers, 5 },
            { SpellName.RockFriends, 5 },
            { SpellName.SnotRocket, 8 },
            { SpellName.TheVirus, 3 },
            { SpellName.TimeWalker, 14 },
            { SpellName.WoodenSpoon, 12 },
        };

        public static int SpellMinimumManaCost(SpellElement spell)
        {
            if (spellMinimumManaCosts.TryGetValue(spell.spellName, out int value)) return value;

            float minimumManaCostFloat = (float)SpellBaseManaCost(spell) / 2f;
            int minimumManaCost;
            if (minimumManaCostFloat - Mathf.Floor(minimumManaCostFloat) <= 0.5f) minimumManaCost = Mathf.FloorToInt(minimumManaCostFloat);
            else minimumManaCost = Mathf.CeilToInt(minimumManaCostFloat);
            return Mathf.Max(2, minimumManaCost);
        }

        private static readonly Dictionary<SpellName, int> spellMinimumManaCosts = new Dictionary<SpellName, int>()
        {
            { SpellName.MagicMarker, 5 }
        };

        public static int SpellBaseRechargeTime(SpellElement spell)
        {
            if (CollectibleStatistics.spellBaseRechargeTime.TryGetValue(spell.spellName, out int value)) return value;
            return -1;
        }

        private static readonly Dictionary<SpellName, int> spellBaseRechargeTime = new Dictionary<SpellName, int>()
        {
            { SpellName.ButterBean, 1 },
            { SpellName.CatPaw, 1 },
            { SpellName.CraftPaper, 1 },
            { SpellName.D10, 1 },
            { SpellName.D20, 4 },
            { SpellName.D4, 1 },
            { SpellName.D6, 1 },
            { SpellName.GoldenTick, 3 },
            { SpellName.LeakyBattery, 2 },
            { SpellName.LithiumBattery, 2 },
            { SpellName.MegaBattery, 3 },
            { SpellName.Mushroom, 3 },
            { SpellName.Pause, 4 },
            { SpellName.PriceTag, 1 },
            { SpellName.Quake, 2 },
            { SpellName.SilverChip, 2 },
            { SpellName.Teleport, 3 },
            { SpellName.TheNegative, 2 },
            { SpellName.ThePoop, 1 },
            { SpellName.TheRelic, 3 },
            { SpellName.TracePaper, 2 },
            { SpellName.TrapDoor, 4 },
            { SpellName.WatchBattery, 1 },
            { SpellName.WoodenNickel, 2 },
            { SpellName.YumHeart, 2 },
        };

        public static int SpellMinimumRechargeTime(SpellElement spell)
        {
            if (spellMinimumRechargeTime.TryGetValue(spell.spellName, out int value)) return value;
            return 0;
        }

        private static readonly Dictionary<SpellName, int> spellMinimumRechargeTime = new Dictionary<SpellName, int>()
        {
            { SpellName.Pause, 1 },
            { SpellName.Teleport, 2 },
        };

        public static float SpellManaCostReductionPercentage(SpellName spellName)
        {
            switch (spellName)
            {
                case SpellName.GoldenTick:
                    return 0.4f;
                case SpellName.SleightOfHand:
                    return 0.25f;
            }
            return 0f;
        }

        public static float TrinketManaCostReductionPercentage(TrinketName trinketName)
        {
            switch (trinketName)
            {
                case TrinketName.RainbowTick:
                    return 0.15f;
                case TrinketName.ManaPrick:
                    return 0.25f;
            }
            return 0f;
        }

        public static int SpellRechargeTimeReduction(SpellName spellName)
        {
            return 0;
        }

        public static int TrinketRechargeTimeReduction(TrinketName trinketName)
        {
            switch (trinketName)
            {
                case TrinketName.BrownTick:
                    return 1;
                case TrinketName.ChargePrick:
                    return 1;
            }
            return 0;
        }

        public static int CalculateManaCostReduction(SpellElement spell, float reductionPercentage, bool includeCostModifier)
        {
            int totalManaCost = WindfallHelper.SpellTotalManaCost(spell, includeCostModifier);

            //Calculate cost reduction
            float costReductionFloat = (float)totalManaCost * reductionPercentage;
            //Round up cost reduction
            int costReduction = Mathf.CeilToInt(costReductionFloat);

            //Do not reduce total cost below minimum
            while (totalManaCost - costReduction < SpellMinimumManaCost(spell))
            {
                costReduction--;
                if (costReduction <= 0) break;
            }
            return costReduction;
        }

        public static int SpellInitialBaseDamage(SpellElement spell)
        {
            if (spellInitialBaseDamage.TryGetValue(spell.spellName, out int value)) return value;
            return 0;
        }

        private static readonly Dictionary<SpellName, int> spellInitialBaseDamage = new Dictionary<SpellName, int>()
        {
            { (SpellName)1001, 0 },
            { SpellName.AttackFly, 0 },
            { SpellName.Backstabber, 0 },
            { SpellName.BeeButt, 0 },
            { SpellName.BigRock, 0 },
            { SpellName.BorfBucket, 0 },
            { SpellName.Brimstone, 0 },
            { SpellName.BumboSmash, 0 },
            { SpellName.DogTooth, 0 },
            { SpellName.Ecoli, 0 },
            { SpellName.ExorcismKit, 0 },
            { SpellName.FishHook, 0 },
            { SpellName.Flush, 0 },
            { SpellName.HairBall, 0 },
            { SpellName.HatPin, 0 },
            { SpellName.Lemon, 0 },
            { SpellName.MamaShoe, 0 },
            { SpellName.MeatHook, 0 },
            { SpellName.MegaBean, 0 },
            { SpellName.NailBoard, 0 },
            { SpellName.Needle, 1 },
            { SpellName.Number1, 0 },
            { SpellName.Pliers, 0 },
            { SpellName.PuzzleFlick, 0 },
            { SpellName.Rock, 0 },
            { SpellName.RockFriends, 0 },
            { SpellName.RubberBat, 0 },
            { SpellName.Stick, 0 },
            { SpellName.TheNegative, 0 },
        };

        public static readonly List<SpellName> spellsThatScaleWithSpellDamageStat = new List<SpellName>()
        {
            //Vanilla
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

            //Windfall
            { (SpellName)1000 }, //Chain distance
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
    }
}