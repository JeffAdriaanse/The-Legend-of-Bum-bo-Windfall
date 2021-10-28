using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using DG.Tweening;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Reflection;

namespace The_Legend_of_Bum_bo_Windfall
{
    class CollectibleChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(CollectibleChanges));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying collectible changes");
        }

		//Patch: Changes starting stats and collectibles of characters
		//Increases the cost of Bum-bo the Dead's attack fly
		[HarmonyPostfix, HarmonyPatch(typeof(CharacterSheet), "Awake")]
		static void CharacterSheet_Awake(CharacterSheet __instance)
		{
			StartingSpell[] deadStartingSpells = __instance.bumboList[(int)CharacterSheet.BumboType.TheDead].startingSpells;
			for (int i = 0; i < deadStartingSpells.Length; i++)
            {
				StartingSpell deadStartingSpell = deadStartingSpells[i];
				if (deadStartingSpell.spell == SpellName.AttackFly)
                {
					deadStartingSpell.toothCost = 6;
				}
			}
		}

		//Patch: Mana colors now consider spell cost modifier when deciding whether to deactivate
		[HarmonyPostfix, HarmonyPatch(typeof(BumboController), "UpdateSpellManaText", new Type[] { typeof(int), typeof(SpellElement) })]
		static void BumboController_UpdateSpellManaText(BumboController __instance, int _spell_index, SpellElement _spell)
		{
			if (!_spell.IsChargeable)
			{
				for (short colorCounter = 0; colorCounter < 5; colorCounter += 1)
				{
					int colorCounterSkipRed = (int)colorCounter;
					if (colorCounter > 0)
					{
						colorCounterSkipRed++;
					}
					if (_spell.Cost[colorCounterSkipRed] + _spell.CostModifier[colorCounterSkipRed] <= 0)
					{
						__instance.app.view.spells[_spell_index].manaIconViews[colorCounterSkipRed].gameObject.SetActive(false);
					}
				}
			}
		}

		//Access base method
		[HarmonyReversePatch]
		[HarmonyPatch(typeof(SpellElement), nameof(SpellElement.CastSpell))]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static bool CastSpellDummy_GoldenTickSpell(GoldenTickSpell instance) { return false; }
		//Patch: Golden Tick rework
		[HarmonyPrefix, HarmonyPatch(typeof(GoldenTickSpell), "CastSpell")]
		static bool SleightOfHandSpell_CastSpell(GoldenTickSpell __instance, ref bool __result)
		{
			if (!CastSpellDummy_GoldenTickSpell(__instance))
			{
				__result = false;
				return false;
			}
			__instance.app.model.spellModel.currentSpell = null;
			__instance.app.model.spellModel.spellQueued = false;
			for (int i = 0; i < __instance.app.model.characterSheet.spells.Count; i++)
			{
				SpellElement spellElement = __instance.app.model.characterSheet.spells[i];
				if (!spellElement.IsChargeable)
				{
					int totalSpellCost = 0;
					for (int j = 0; j < 6; j++)
					{
						totalSpellCost += (int)spellElement.Cost[j] + spellElement.CostModifier[j];
					}

					//Reduce mana cost by 40%
					int costReduction = Mathf.RoundToInt((float)totalSpellCost * 0.4f);

					//Do not reduce total cost below two
					while (totalSpellCost - costReduction < 2)
					{
						costReduction--;
						if (costReduction <= 0)
						{
							break;
						}
					}

					for (int k = costReduction; k > 0; k--)
					{
						List<int> availableColors = new List<int>();
						for (int l = 0; l < 6; l++)
						{
							if (spellElement.Cost[l] + spellElement.CostModifier[l] != 0)
							{
								availableColors.Add(l);
							}
						}
						if (availableColors.Count > 0)
						{
							int randomColor = availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
							spellElement.CostModifier[randomColor] -= 1;
						}
					}
				}
				else if (spellElement != __instance)

				{
					spellElement.ChargeSpell();
				}
			}
			__instance.charge = 0;
			__instance.app.controller.UpdateSpellManaText();
			__instance.app.controller.SetActiveSpells(true, true);
			__instance.app.controller.GUINotification("Make\nSpells Easier\nTo Cast!", GUINotificationView.NotifyType.Spell, __instance, true);
			__instance.app.controller.eventsController.SetEvent(new IdleEvent());
			__result = true;

			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing Golden Tick effect");
			return false;
		}

		//Access base method
		[HarmonyReversePatch]
		[HarmonyPatch(typeof(SpellElement), nameof(SpellElement.CastSpell))]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static bool CastSpellDummy_SleightOfHandSpell(SleightOfHandSpell instance) { return false; }
		//Patch: Sleight of Hand rework
		[HarmonyPrefix, HarmonyPatch(typeof(SleightOfHandSpell), "CastSpell")]
		static bool SleightOfHandSpell_CastSpell(SleightOfHandSpell __instance, ref bool __result)
		{
			if (!CastSpellDummy_SleightOfHandSpell(__instance))
			{
				__result = false;
				return false;
			}
			__instance.app.model.spellModel.currentSpell = null;
			__instance.app.model.spellModel.spellQueued = false;
			for (int i = 0; i < __instance.app.model.characterSheet.spells.Count; i++)
			{
				SpellElement spellElement = __instance.app.model.characterSheet.spells[i];
				if (!spellElement.IsChargeable && spellElement != __instance)
				{
					int totalSpellCost = 0;
					for (int j = 0; j < 6; j++)
					{
						totalSpellCost += (int)spellElement.Cost[j] + spellElement.CostModifier[j];
					}

					//Reduce mana cost by 25%
					int costReduction = Mathf.RoundToInt((float)totalSpellCost * 0.25f);

					//Do not reduce total cost below two (failsafe)
					while (totalSpellCost - costReduction < 2)
					{
						costReduction--;
						if (costReduction <= 0)
						{
							break;
						}
					}

					for (int k = costReduction; k > 0; k--)
					{
						List<int> availableColors = new List<int>();
						for (int l = 0; l < 6; l++)
						{
							if (spellElement.Cost[l] + spellElement.CostModifier[l] != 0)
							{
								availableColors.Add(l);
							}
						}
						if (availableColors.Count > 0)
						{
							int randomColor = availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
							spellElement.CostModifier[randomColor] -= 1;
						}
					}
				}
			}
			__instance.app.controller.UpdateSpellManaText();
			__instance.app.controller.SetActiveSpells(true, true);
			__instance.app.controller.GUINotification("Spells\nCost Less\nIn Room!", GUINotificationView.NotifyType.Spell, __instance, true);
			__instance.app.controller.eventsController.SetEvent(new IdleEvent());
			SoundsView.Instance.PlaySound(SoundsView.eSound.Spell_LowerCost, SoundsView.eAudioSlot.Default, false);
			__result = true;

			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing Sleight of Hand effect");
			return false;
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
		[HarmonyPrefix, HarmonyPatch(typeof(Shop), "AddDamagePrick")]
		static bool Shop_AddDamagePrick(Shop __instance, ref List<TrinketName> ___needles)
		{
			short num = 0;
			while ((int)num < __instance.app.model.characterSheet.spells.Count)
			{
				SpellElement spellElement = __instance.app.model.characterSheet.spells[(int)num];
				if (spellElement.Category == SpellElement.SpellCategory.Attack && !(spellElement.spellName == SpellName.Ecoli || spellElement.spellName == SpellName.ExorcismKit || spellElement.spellName == SpellName.MegaBean || spellElement.spellName == SpellName.PuzzleFlick))
				{
					___needles.Add(TrinketName.DamagePrick);
					return false;
				}
				num += 1;
			}
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing damage needle appearance condition");
			return false;
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

		//Patch: Mana needle rework
		[HarmonyPostfix, HarmonyPatch(typeof(ManaPrickTrinket), "QualifySpell")]
		static void ManaPrickTrinket_QualifySpell(ManaPrickTrinket __instance, int _spell_index)
		{
			int totalManaCost = 0;
			for (int i = 0; i < 6; i++)
			{
				totalManaCost += (int)__instance.app.model.characterSheet.spells[_spell_index].Cost[i];
				if (totalManaCost > 2)
				{
					__instance.app.view.spells[_spell_index].EnableSpell();
					return;
				}
			}
			__instance.app.view.spells[_spell_index].DisableSpell();

			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing mana needle spell qualification");
		}
		[HarmonyPrefix, HarmonyPatch(typeof(ManaPrickTrinket), "UpdateSpell")]
		static bool ManaPrickTrinket_UpdateSpell(ManaPrickTrinket __instance, int _spell_index)
		{
			SpellElement spellElement = __instance.app.model.characterSheet.spells[_spell_index];
			int totalSpellCost = 0;
			for (int i = 0; i < 6; i++)
			{
				totalSpellCost += (int)spellElement.Cost[i];
			}

			//Reduce mana cost by 25%
			int costReduction = Mathf.RoundToInt((float)totalSpellCost * 0.25f);

			//Do not reduce total cost below two (failsafe)
			while (totalSpellCost - costReduction < 2)
            {
				costReduction--;
				if (costReduction <= 0)
                {
					break;
                }
            }

			for (int j = costReduction; j > 0; j--)
			{
				//Find colors with cost above 0
				List<int> availableColors = new List<int>();
				for (int k = 0; k < 6; k++)
				{
					if (spellElement.Cost[k] != 0)
					{
						availableColors.Add(k);
					}
				}
				//Choose random color to reduce
				int randomColor = availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
				short[] cost = __instance.app.model.characterSheet.spells[_spell_index].Cost;
				cost[randomColor] -= 1;
			}
			__instance.app.controller.UpdateSpellManaText();
			__instance.app.view.soundsView.PlaySound(SoundsView.eSound.ItemUpgraded, SoundsView.eAudioSlot.Default, false);
			__instance.app.view.spells[_spell_index].spellParticles.Play();

			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing mana needle effect");
			return false;
		}
		[HarmonyPrefix, HarmonyPatch(typeof(Shop), "AddManaPrick")]
		static bool Shop_AddManaPrick(Shop __instance, ref List<TrinketName> ___needles)
		{
			short num = 0;
			while ((int)num < __instance.app.model.characterSheet.spells.Count)
			{
				int num2 = 0;
				for (int i = 0; i < 6; i++)
				{
					num2 += (int)__instance.app.model.characterSheet.spells[(int)num].Cost[i];
					if (num2 >= 2)
					{
						___needles.Add(TrinketName.ManaPrick);
						return false;
					}
				}
				num += 1;
			}
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing mana needle appearance condition");
			return false;
		}

		//Patch: Bum-bo the Dead will now not encounter Shuffle Needles instead of Mana Needles
		//Since spell mana cost reduction is now preserved when the cost is rerolled, mana needles are useful to Bum-bo the Dead
		//Shuffle needles on the other hand are pretty pointless
		//Also saves and loads shop
		[HarmonyPrefix, HarmonyPatch(typeof(Shop), "Init")]
		static bool Shop_Init(Shop __instance, ref List<TrinketName> ___needles, ref GameObject ___item1Pickup, ref GameObject ___item2Pickup, ref GameObject ___item3Pickup, ref GameObject ___item4Pickup, ref TrinketModel ___trinketModel)
		{
			if (PlayerPrefs.GetInt("loadGambling", 0) == 1)
            {
				//Load shop
				XmlDocument lDoc = (XmlDocument)typeof(SavedStateController).GetField("lDoc", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.app.controller.savedStateController);

				if (lDoc != null)
				{
					XmlNode xmlNode = lDoc.SelectSingleNode("/save/gambling");

					if (xmlNode != null)
					{
						XmlNodeList xmlNodeList = xmlNode.SelectNodes("pickup");

						bool[] savedPickups = new bool[4];

						for (int i = 0; i < xmlNodeList.Count; i++)
						{
							XmlNode xmlNode2 = xmlNodeList.Item(i);

							int index = Convert.ToInt32(xmlNode2.Attributes["index"].Value);
							savedPickups[i] = true;

							string type = xmlNode2.Attributes["type"].Value;

							GameObject itemPickup = null;
							GameObject item = null;
							ItemPriceView itemPrice = null;
							switch (index)
							{
								case 0:
									item = __instance.item1;
									itemPrice = __instance.item1Price;
									break;
								case 1:
									item = __instance.item2;
									itemPrice = __instance.item2Price;
									break;
								case 2:
									item = __instance.item3;
									itemPrice = __instance.item3Price;
									break;
								case 3:
									item = __instance.item4;
									itemPrice = __instance.item4Price;
									break;
							}

							if (type == "heart")
                            {
								itemPickup = (GameObject)AccessTools.Method(typeof(Shop), "AddHeart").Invoke(__instance, new object[] { item, itemPrice });
							}
							else if (type == "trinket")
                            {
								TrinketName trinketName = (TrinketName)Enum.Parse(typeof(TrinketName), xmlNode2.Attributes["trinketName"].Value);

								if (__instance.app.model.trinketModel.trinkets[trinketName].Category != TrinketElement.TrinketCategory.Prick)
                                {
									if (___trinketModel == null)
                                    {
										___trinketModel = __instance.gameObject.AddComponent<TrinketModel>();
									}
								}

								itemPickup = (GameObject)AccessTools.Method(typeof(Shop), "AddTrinket").Invoke(__instance, new object[] { item, itemPrice });
								int price = 7;
								switch (trinketName)
                                {
									case TrinketName.ManaPrick:
										price = 8;
										break;
									case TrinketName.DamagePrick:
										price = 5;
										break;
									case TrinketName.ChargePrick:
										price = 8;
										break;
									case TrinketName.ShufflePrick:
										price = 2;
										break;
									case TrinketName.RandomPrick:
										price = 5;
										break;
								}

								TrinketPickupView trinketPickupView = itemPickup.GetComponent<TrinketPickupView>();
								trinketPickupView.SetTrinket(trinketName, price);
								itemPrice.SetPrice(price);
								trinketPickupView.removePickup = true;

								switch (index)
								{
									case 0:
										 ___item1Pickup = itemPickup;
										break;
									case 1:
										___item2Pickup = itemPickup;
										break;
									case 2:
										___item3Pickup = itemPickup;
										break;
									case 3:
										___item4Pickup = itemPickup;
										break;
								}
							}
						}

						if (___item1Pickup != null)
						{
							___item1Pickup.GetComponent<TrinketPickupView>().shopIndex = 0;
						}
						if (___item2Pickup != null)
						{
							___item2Pickup.GetComponent<TrinketPickupView>().shopIndex = 1;
						}
						if (___item3Pickup != null)
						{
							___item3Pickup.GetComponent<TrinketPickupView>().shopIndex = 2;
						}

						return false;
					}
				}
            }

			___needles = new List<TrinketName>();
			if (__instance.app.model.characterSheet.bumboType != CharacterSheet.BumboType.Eden)
			{
				if (__instance.app.model.characterSheet.bumboType != CharacterSheet.BumboType.TheDead)
				{
					//Shuffle Needle
					AccessTools.Method(typeof(Shop), "AddShufflePrick").Invoke(__instance, null);
				}
				AccessTools.Method(typeof(Shop), "AddDamagePrick").Invoke(__instance, null);
				AccessTools.Method(typeof(Shop), "AddChargePrick").Invoke(__instance, null);
				AccessTools.Method(typeof(Shop), "AddRandomPrick").Invoke(__instance, null);
				//Mana Needle
				AccessTools.Method(typeof(Shop), "AddManaPrick").Invoke(__instance, null);

				if (___needles.Count > 0)
				{
					___item1Pickup = (GameObject)AccessTools.Method(typeof(Shop), "AddNeedle").Invoke(__instance, new object[] { __instance.item1, __instance.item1Price });
				}
				if (___needles.Count > 0)
				{
					___item2Pickup = (GameObject)AccessTools.Method(typeof(Shop), "AddNeedle").Invoke(__instance, new object[] { __instance.item2, __instance.item2Price });
				}
				if (___needles.Count > 0)
				{
					___item3Pickup = (GameObject)AccessTools.Method(typeof(Shop), "AddNeedle").Invoke(__instance, new object[] { __instance.item3, __instance.item3Price });
				}
			}
			else
			{
				___trinketModel = __instance.gameObject.AddComponent<TrinketModel>();
				___item1Pickup = (GameObject)AccessTools.Method(typeof(Shop), "AddTrinket").Invoke(__instance, new object[] { __instance.item1, __instance.item1Price });
				___item2Pickup = (GameObject)AccessTools.Method(typeof(Shop), "AddTrinket").Invoke(__instance, new object[] { __instance.item2, __instance.item2Price });
				___item3Pickup = (GameObject)AccessTools.Method(typeof(Shop), "AddTrinket").Invoke(__instance, new object[] { __instance.item3, __instance.item3Price });
			}
			if (__instance.app.model.characterSheet.bumboType != CharacterSheet.BumboType.TheLost)
			{
				___item4Pickup = (GameObject)AccessTools.Method(typeof(Shop), "AddHeart").Invoke(__instance, new object[] { __instance.item4, __instance.item4Price });
			}
			if (___item1Pickup != null)
			{
				___item1Pickup.GetComponent<TrinketPickupView>().shopIndex = 0;
			}
			if (___item2Pickup != null)
			{
				___item2Pickup.GetComponent<TrinketPickupView>().shopIndex = 1;
			}
			if (___item3Pickup != null)
			{
				___item3Pickup.GetComponent<TrinketPickupView>().shopIndex = 2;
			}
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing shop generation");

			if (PlayerPrefs.GetInt("loadGambling", 0) == 0)
			{
				//Save shop
				SaveChanges.SaveShop(__instance);
			}
			return false;
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

        //Patch: Pentagram no longer provides puzzle damage
        [HarmonyPostfix, HarmonyPatch(typeof(PentagramSpell), "CastSpell")]
        static void PentagramSpell_CastSpell(PentagramSpell __instance, bool __result)
        {
			if (__result)
            {
				__instance.app.model.characterSheet.bumboRoomModifiers.damage--;
				//Item damage room modifier is not implemented in the base game and must be added in
				__instance.app.model.characterSheet.bumboRoomModifiers.itemDamage++;
				__instance.app.controller.UpdateStats();
				Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing Pentagram from granting puzzle damage");
			}
		}

		//Patch: Implements room spell damage
		[HarmonyPostfix, HarmonyPatch(typeof(CharacterSheet), "getItemDamage")]
		static void CharacterSheet_getItemDamage(CharacterSheet __instance, ref int __result)
		{
			int damage = Mathf.RoundToInt((float)Mathf.Clamp(__result + __instance.app.model.characterSheet.bumboRoomModifiers.itemDamage, 1, 5));
			__result = damage;
		}
		//Patch: Implements room spell damage
		[HarmonyPrefix, HarmonyPatch(typeof(CharacterSheet), "getTemporaryItemDamage")]
		static bool CharacterSheet_getTemporaryItemDamage(CharacterSheet __instance, ref int __result)
		{
			int num = __instance.bumboBaseInfo.itemDamage + __instance.app.model.characterSheet.hiddenTrinket.AddToSpellDamage();
			short num2 = 0;
			while ((int)num2 < __instance.app.model.characterSheet.trinkets.Count)
			{
				num += __instance.app.controller.GetTrinket((int)num2).AddToSpellDamage();
				num2 += 1;
			}
			int num3 = Mathf.Max(0, 5 - num);
			int num4 = 0;
			num4 += __instance.app.model.characterSheet.bumboRoundModifiers.itemDamage;
			num4 += __instance.app.model.characterSheet.bumboRoundModifiers.damage;
			//Adding room item damage
			num4 += __instance.app.model.characterSheet.bumboRoomModifiers.itemDamage;
			num4 += __instance.app.model.characterSheet.bumboRoomModifiers.damage;
			__result = Mathf.Clamp(num4, 0, num3);
			return false;
		}

		static int rockCounter = 0;
		//Patch: Rock Friends now drops a number of rocks equal to the player's spell damage stat
		[HarmonyPrefix, HarmonyPatch(typeof(RockFriendsSpell), "DropRock")]
		static bool RockFriendsSpell_DropRock(RockFriendsSpell __instance, ref int _rock_number)
		{
			rockCounter++;
			_rock_number = 1;
			if (rockCounter > __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier())
            {
				_rock_number = 4;
				rockCounter = 0;
			}
			return true;
		}
		//Patch: Changes Rock Friends description
		[HarmonyPostfix, HarmonyPatch(typeof(RockFriendsSpell), MethodType.Constructor)]
		static void RockFriendsSpell_Constructor(RockFriendsSpell __instance)
		{
			__instance.Name = "Hits Random Enemies = to Spell Damage";
		}

		//Patch: Changes Attack Fly spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(AttackFlySpell), "Damage")]
		static void AttackFlySpell_Damage(AttackFlySpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}
		//Patch: Removes Bum-bo the Dead's special Attack Fly cost reroll
		[HarmonyPrefix, HarmonyPatch(typeof(BumboController), "SetSpellCostForTheDeadsAttackFly")]
		static bool BumboController_SetSpellCostForTheDeadsAttackFly(BumboController __instance, SpellElement _spell, bool[] _ignore_mana, ref SpellElement __result)
		{
			__result = __instance.SetSpellCost(_spell, _ignore_mana);
			return false;
		}

		//Patch: Prevents Mama Foot from killing the player
		[HarmonyPrefix, HarmonyPatch(typeof(MamaFootSpell), "Reward")]
		static bool MamaFootSpell_Reward(MamaFootSpell __instance)
		{
			float damage = 0.5f * __instance.app.model.characterSheet.bumboRoomModifiers.damageMultiplier;
			while (damage >= __instance.app.model.characterSheet.hitPoints + __instance.app.model.characterSheet.soulHearts)
            {
				damage -= 0.5f;
            }

			if (damage > 0)
            {
				__instance.app.controller.TakeDamage(damage / __instance.app.model.characterSheet.bumboRoomModifiers.damageMultiplier, null);
			}
			__instance.app.Notify("reward.spell", null, new object[0]);
			return false;
		}

		//Patch: Changes Brimstone spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(BrimstoneSpell), "Damage")]
		static void BrimstoneSpell_Damage(BrimstoneSpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}

		//Patch: Changes Lemon spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(LemonSpell), "Damage")]
		static void LemonSpell_Damage(LemonSpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}

		//Patch: Changes Pliers spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(PliersSpell), "Damage")]
		static void PliersSpell_Damage(PliersSpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}

		//Patch: Changes Dog Tooth spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(DogToothSpell), "Damage")]
		static void DogToothSpell_Damage(DogToothSpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}
		//Patch: Changes Dog Tooth description
		[HarmonyPostfix, HarmonyPatch(typeof(DogToothSpell), MethodType.Constructor)]
		static void DogToothSpell_Constructor(DogToothSpell __instance)
		{
			__instance.Name = "Attack that Heals You";
		}
		//***************************************************
		//***************Calling Base Method*****************
		//***************************************************

		////Create dummy method of base method
		////This allows for calling the base method from the child class
		//[HarmonyReversePatch]
		//[HarmonyPatch(typeof(SpellElement), nameof(SpellElement.CastSpell))]
		//[MethodImpl(MethodImplOptions.NoInlining)]
		//static bool BaseCastSpellDummy(PentagramSpell instance)
		//{
		//	return true;
		//}

		////Patch: Pentagram no longer provides puzzle damage
		//[HarmonyPrefix, HarmonyPatch(typeof(PentagramSpell), "CastSpell")]
		//static bool PentagramSpell_CastSpell(PentagramSpell __instance, ref bool __result)
		//{
		//	if (!BaseCastSpellDummy(__instance))
		//	{
		//		__result = false;
		//		return false;
		//	}
		//	__instance.app.model.spellModel.currentSpell = null;
		//	__instance.app.model.spellModel.spellQueued = false;

		//	//Only increase spell damage
		//	__instance.app.model.characterSheet.bumboRoomModifiers.itemDamage++;

		//	__instance.app.controller.ShowDamageUp();
		//	__instance.app.controller.UpdateStats();
		//	__instance.app.controller.GUINotification("Hit Harder\nIn Room!", GUINotificationView.NotifyType.General, __instance, true);
		//	__instance.app.controller.eventsController.SetEvent(new IdleEvent());
		//	__result = true;
		//	Console.WriteLine("[The Legend of Bum-bo: Windfall] ");
		//	return false;
		//}

		//***************************************************
		//***************************************************
		//***************************************************
	}

	static class SpellManaCosts
	{
		public static Dictionary<SpellName, int> manaCosts = new Dictionary<SpellName, int>()
		{
			{ SpellName.TwentyTwenty, 7 },
			{ SpellName.Pentagram, 8 },
			{ SpellName.AttackFly, 8 },
			{ SpellName.MamaFoot, 13 },
			{ SpellName.Lemon, 5 },
			{ SpellName.Pliers, 5 },
		};
	}
}
