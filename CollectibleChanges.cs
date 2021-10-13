using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using DG.Tweening;


namespace The_Legend_of_Bum_bo_Windfall
{
    class CollectibleChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(CollectibleChanges));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying collectible changes");
        }

		//Patch: Damage needles can no longer be used on spells that will not be upgraded by the needle
		[HarmonyPostfix, HarmonyPatch(typeof(DamagePrickTrinket), "QualifySpell")]
		static void DamagePrickTrinket_QualifySpell(DamagePrickTrinket __instance, int _spell_index)
		{
			SpellElement spellElement = __instance.app.model.characterSheet.spells[_spell_index];
			if (spellElement.spellName == SpellName.Ecoli || spellElement.spellName == SpellName.ExorcismKit || spellElement.spellName == SpellName.MegaBean || spellElement.spellName == SpellName.PuzzleFlick)
			{
				__instance.app.view.spells[_spell_index].DisableSpell();
				Console.WriteLine("[The Legend of Bum-bo: Windfall] Disabling attack spell that won't be affected by damage needle");
			}
		}

		//Patch: Charge needles can no longer be used on spells that will not be upgraded by the needle
		[HarmonyPostfix, HarmonyPatch(typeof(ChargePrickTrinket), "QualifySpell")]
		static void ChargePrickTrinket_QualifySpell(ChargePrickTrinket __instance, int _spell_index)
		{
			if (__instance.app.model.characterSheet.spells[_spell_index].requiredCharge == 0)
			{
				__instance.app.view.spells[_spell_index].DisableSpell();
				Console.WriteLine("[The Legend of Bum-bo: Windfall] Disabling item that won't be affected by charge needle");
			}
		}

		//Patch: Reworks mana cost generation
		//Certain spells have new base mana costs
		//The number of mana colors is now determined in a flexible way
		//Permanent and temporary mana cost reduction is now preserved when rerolling spell costs
		[HarmonyPrefix, HarmonyPatch(typeof(BumboController), "SetSpellCost", new Type[] { typeof(SpellElement), typeof(bool[]) })]
        static bool BumboController_SetSpellCost(BumboController __instance, SpellElement _spell, bool[] _ignore_mana, ref SpellElement __result)
        {
			if (_ignore_mana == null)
			{
				_ignore_mana = new bool[6];
			}
			if (!_spell.IsChargeable && _spell.setCost)
			{
				//New mana costs
				int totalSpellCost = -1;

				int value;
				if (SpellManaCosts.manaCosts.TryGetValue(_spell.spellName, out value))
				{
					totalSpellCost = value;
				}

				List<short> spellCost = new List<short>();

				//Old mana costs
				if (totalSpellCost == -1)
				{
					switch (_spell.manaSize)
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

				//Preserve mana cost reduction
				int currentSpellCost = 0;
				for (int i = 0; i < _spell.Cost.Length; i++)
				{
					currentSpellCost += _spell.Cost[i];
				}
				if (totalSpellCost > currentSpellCost && currentSpellCost > 0)
				{
					totalSpellCost = currentSpellCost;
				}

				//Choose number of colors of mana
				int maximumColorCount;
				if (totalSpellCost < 4)
				{
					maximumColorCount = 1;
				}
				else if (totalSpellCost < 6)
				{
					maximumColorCount = 2;
				}
				else
				{
					maximumColorCount = 3;
				}

				int minimumColorCount;
				if (totalSpellCost > 16)
				{
					minimumColorCount = 3;
				}
				else if (totalSpellCost > 7)
				{
					minimumColorCount = 2;
				}
				else
				{
					minimumColorCount = 1;
				}

				int colorCount;
				if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheStout)
				{
					colorCount = minimumColorCount;
				}
				else
				{
					//Random float
					float rand = UnityEngine.Random.Range((float)minimumColorCount - 0.5f, (float)maximumColorCount + 0.5f);

					//Another random float
					float rand2 = UnityEngine.Random.Range((float)minimumColorCount - 0.5f, (float)maximumColorCount + 0.5f);

					//Lower number is chosen, then rounded to the nearest integer; lower color counts are more likely
					colorCount = Mathf.RoundToInt(rand < rand2 ? rand : rand2);

					//Reduce impact of weighted randomness
					if (UnityEngine.Random.Range(0, 1f) < 0.5f)
                    {
						colorCount = Mathf.RoundToInt(rand);
					}

					//Failsafe
					if (colorCount < minimumColorCount)
                    {
						colorCount = minimumColorCount;
                    }
					else if (colorCount > maximumColorCount)
                    {
						colorCount = maximumColorCount;
                    }
				}

				for (int j = colorCount; j > 0; j--)
				{
					spellCost.Add(0);
				}

				//Generate spell cost
				int cheapestColorIndex = 0;
				for (int k = totalSpellCost; k > 0; k--)
				{
					int cheapestColor = 99;
					for (int l = 0; l < spellCost.Count; l++)
					{
						if ((int)spellCost[l] < cheapestColor)
						{
							cheapestColor = (int)spellCost[l];
							cheapestColorIndex = l;
						}
					}
					spellCost[cheapestColorIndex] += 1;
				}
				List<ManaType> bannedColors = new List<ManaType>
				{
					ManaType.Bone,
					ManaType.Booger,
					ManaType.Pee,
					ManaType.Poop,
					ManaType.Tooth
				};
				for (int m = 0; m < __instance.app.model.characterSheet.spells.Count; m++)
				{
					if (bannedColors.Count > 0)
					{
						for (int n = bannedColors.Count - 1; n >= 0; n--)
						{
							if (_ignore_mana[(int)bannedColors[n]])
							{
								//Ban ignored colors
								bannedColors.RemoveAt(n);
							}
							else if (__instance.app.model.characterSheet.spells[m].Cost[(int)bannedColors[n]] != 0)
							{
								//Ban colors of existing spells
								bannedColors.RemoveAt(n);
							}
						}
					}
				}
				List<ManaType> list6 = new List<ManaType>();
				_spell.Cost = new short[6];
				for (int num10 = 0; num10 < spellCost.Count; num10++)
				{
					if (bannedColors.Count == 0)
					{
						for (int num11 = 0; num11 < 6; num11++)
						{
							if (num11 != 1 && list6.IndexOf((ManaType)num11) < 0)
							{
								bannedColors.Add((ManaType)num11);
							}
						}
					}
					int index2 = UnityEngine.Random.Range(0, bannedColors.Count);
					_spell.Cost[(int)bannedColors[index2]] = spellCost[num10];
					list6.Add(bannedColors[index2]);
					bannedColors.RemoveAt(index2);
				}

				//Preserve temporary mana cost reduction
				short num12 = 0;
				for (int num13 = 0; num13 < _spell.CostModifier.Length; num13++)
				{
					num12 -= _spell.CostModifier[num13];
					_spell.CostModifier[num13] = 0;
				}
				for (int num14 = (int)num12; num14 > 0; num14--)
				{
					List<int> list7 = new List<int>();
					for (int num15 = 0; num15 < 6; num15++)
					{
						if (_spell.Cost[num15] + _spell.CostModifier[num15] > 0)
						{
							list7.Add(num15);
						}
					}
					if (list6.Count > 0)
					{
						int num16 = list7[UnityEngine.Random.Range(0, list7.Count)];
						short[] costModifier = _spell.CostModifier;
						int num17 = num16;
						costModifier[num17] -= 1;
					}
				}
			}
			__result = _spell;

			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing spell mana cost generation");
            return false;
        }
    }

	static class SpellManaCosts
	{
		public static Dictionary<SpellName, int> manaCosts = new Dictionary<SpellName, int>()
		{
			{ SpellName.TwentyTwenty, 6 },
		};
	}
}
