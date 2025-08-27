using System.Collections.Generic;
using UnityEngine;


namespace The_Legend_of_Bum_bo_Windfall
{
    public abstract class SpellUpgrade
    {
        public abstract bool ValidateSpell(SpellElement spell);
        public abstract void ApplyUpgrade(ref SpellElement spell);
    }

    class SpellDamageUpgrade : SpellUpgrade
    {
        private int damageIncrease;
        public SpellDamageUpgrade(int damageIncrease)
        {
            this.damageIncrease = damageIncrease;
        }

        public override bool ValidateSpell(SpellElement spell)
        {
            return spell.Category == SpellElement.SpellCategory.Attack && !attackSpellsThatCannotBeDamageUpgraded.Contains(spell.spellName);
        }

        public override void ApplyUpgrade(ref SpellElement spell)
        {
            spell.baseDamage += damageIncrease;
        }

        private readonly List<SpellName> attackSpellsThatCannotBeDamageUpgraded = new List<SpellName>()
        {
            SpellName.Ecoli,
            SpellName.ExorcismKit,
            SpellName.MegaBean,
        };
    }

    class SpellManaCostUpgrade : SpellUpgrade
    {
        private float manaCostReductionPercentage;
        public SpellManaCostUpgrade(float manaCostReductionPercentage)
        {
            this.manaCostReductionPercentage = manaCostReductionPercentage;
        }

        public override bool ValidateSpell(SpellElement spell)
        {
            return !spell.IsChargeable && CollectibleStatistics.CalculateManaCostReduction(spell, manaCostReductionPercentage, false) > 0;
        }

        public override void ApplyUpgrade(ref SpellElement spell)
        {
            int costReduction = CollectibleStatistics.CalculateManaCostReduction(spell, manaCostReductionPercentage, false);

            for (int j = costReduction; j > 0; j--)
            {
                //Find colors with cost above 0
                List<int> availableColors = new List<int>();
                for (int k = 0; k < 6; k++)
                {
                    if (spell.Cost[k] != 0) availableColors.Add(k);
                }
                //Choose random color to reduce
                int randomColor = availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
                short[] cost = spell.Cost;
                cost[randomColor] -= 1;

                int totalCombinedCost = 0;
                //Increase the reduced color's cost modifier if the spell's total cost (including modifier) would be reduced below minimum OR if the reduced color's cost (including modifier) would be reduced below zero
                for (int costCounter = 0; costCounter < 6; costCounter++) totalCombinedCost += (short)(spell.Cost[costCounter] + spell.CostModifier[costCounter]);
                if (totalCombinedCost < CollectibleStatistics.SpellMinimumManaCost(spell) || spell.Cost[randomColor] + spell.CostModifier[randomColor] < 0) spell.CostModifier[randomColor] += 1;
            }
        }
    }

    class SpellManaCostRerollUpgrade : SpellUpgrade
    {
        public override bool ValidateSpell(SpellElement spell)
        {
            return !spell.IsChargeable;
        }

        public override void ApplyUpgrade(ref SpellElement spell)
        {
            bool[] previousManaColors = new bool[6];
            previousManaColors[1] = true;
            for (int i = 0; i < 6; i++)
            {
                if (spell.Cost[i] > 0) previousManaColors[i] = true;
            }

            spell.setCost = true;
            WindfallHelper.app.controller.SetSpellCost(spell, previousManaColors);
        }
    }

    class SpellRechargeTimeUpgrade : SpellUpgrade
    {
        private int rechargeTimeReduction;
        public SpellRechargeTimeUpgrade(int rechargeTimeReduction)
        {
            this.rechargeTimeReduction = rechargeTimeReduction;
        }

        public override bool ValidateSpell(SpellElement spell)
        {
            return spell.IsChargeable && spell.requiredCharge > CollectibleStatistics.SpellMinimumRechargeTime(spell);
        }

        public override void ApplyUpgrade(ref SpellElement spell)
        {
            spell.requiredCharge = Mathf.Max(spell.requiredCharge - rechargeTimeReduction, CollectibleStatistics.SpellMinimumRechargeTime(spell), 0);
            if (spell.requiredCharge < spell.charge) spell.charge = spell.requiredCharge;
            if (spell.requiredCharge == 0)
            {
                spell.chargeEveryRound = true;
                spell.usedInRound = false;
            }
        }
    }

    class SpellRerollUpgrade : SpellUpgrade
    {
        public override bool ValidateSpell(SpellElement spell)
        {
            return true;
        }

        public override void ApplyUpgrade(ref SpellElement spell)
        {
            int index = Random.Range(1, WindfallHelper.app.model.spellModel.validSpells.Count);
            SpellElement spellElement = WindfallHelper.app.model.spellModel.spells[WindfallHelper.app.model.spellModel.validSpells[index]];
            WindfallHelper.app.controller.SetSpellCost(spellElement);
            spell = spellElement;
        }
    }
}
