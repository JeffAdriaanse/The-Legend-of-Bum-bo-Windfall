using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Runtime.CompilerServices;
using TMPro;

namespace The_Legend_of_Bum_bo_Windfall
{
	class CollectibleChanges
	{
		public static void Awake()
		{
			Harmony.CreateAndPatchAll(typeof(CollectibleChanges));
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying collectible changes");
		}

		public static float TrinketLuckModifier(CharacterSheet characterSheet)
		{
			return 1 + (characterSheet.getLuck() / 10);
		}

		public static int EffectActivationCounter(float effectValue)
		{
			int floor = Mathf.FloorToInt(effectValue);
			float remainder = effectValue - floor;

			return UnityEngine.Random.Range(0f, 1f) < remainder ? floor + 1 : floor;
		}

		//Patch: Reduces ExorcismKit enemy heal from 2 to 1
		[HarmonyPrefix, HarmonyPatch(typeof(ExorcismKitSpell), "HurtAndHeal")]
		static bool ExorcismKitSpell_HurtAndHeal(ExorcismKitSpell __instance, ref List<Enemy> enemies_to_heal, ref Enemy enemy_to_hurt)
		{
			for (int i = 0; i < enemies_to_heal.Count; i++)
			{
				enemies_to_heal[i].AddHealth(1f);
			}
			enemy_to_hurt.Hurt((float)__instance.Damage(), Enemy.AttackImmunity.ReduceSpellDamage, __instance.statusEffects, enemy_to_hurt.position.x);
			return false;
		}

		//Patch: Prevents Craft Paper from being used on Craft Paper
		[HarmonyPrefix, HarmonyPatch(typeof(CraftPaperSpell), "AlterSpell")]
		static bool CraftPaperSpell_AlterSpell(CraftPaperSpell __instance, int _spell_index)
		{
			if (__instance.app.model.characterSheet.spells[_spell_index].spellName == SpellName.CraftPaper)
			{
				return false;
			}
			return true;
		}

		//Patch: AAA Battery effect now stacks non-independently
		//AAABattery effect now incorporates Luck stat and Curio effect multiplier
		//AAABattery effect now grants movement independently of Hoof
		[HarmonyPrefix, HarmonyPatch(typeof(BumboController), "ResetActionPoints")]
		static bool BumboController_ResetActionPoints(BumboController __instance)
		{
			BumboModel model = __instance.app.model;
			model.actionPoints = (short)model.characterSheet.getDex();

			//Implement Hoof effect
			int HoofCount = 0;

			//Change AAA Battery effect
			//Chance: 1/10
			float activationChance = 0.1f;

			int AAABatteryCount = 0;
			short trinketCounter = 0;
			while ((int)trinketCounter < model.characterSheet.trinkets.Count)
			{
				HoofCount += __instance.GetTrinket((int)trinketCounter).trinketName == TrinketName.Hoof ? 1 : 0;

				//Bypass AddToDex
				AAABatteryCount += __instance.GetTrinket((int)trinketCounter).trinketName == TrinketName.AAABattery ? 1 : 0;
				trinketCounter += 1;
			}

			if (HoofCount > 0)
			{
				HoofCount *= __instance.app.controller.trinketController.EffectMultiplier();

				__instance.app.controller.ModifyActionPoint(HoofCount);
			}

			if (AAABatteryCount > 0)
			{
				float movementChance = AAABatteryCount * activationChance * TrinketLuckModifier(__instance.app.model.characterSheet) * __instance.app.controller.trinketController.EffectMultiplier();

				int movementGain = EffectActivationCounter(movementChance);

				//Trigger effect
				if (movementGain > 0)
				{
					__instance.app.controller.ModifyActionPoint(movementGain);
				}
			}

			model.actionPoints += (short)model.characterSheet.bumboRoundModifiers.actionPoints;
			model.actionPoints += (short)model.characterSheet.bumboRoomModifiers.actionPoints;
			model.actionPoints += model.actionPointModifier;
			model.actionPointModifier = 0;
			if (model.actionPoints <= 0)
			{
				model.actionPoints = 1;
			}
			__instance.app.view.actionPoints.SetActionPoints((int)model.actionPoints);
			if (model.iOS)
			{
				__instance.app.view.IOSActionPoints.GetComponent<TextMeshPro>().text = model.actionPoints.ToString();
			}
			return false;
		}

		//Patch: Allows AA Battery and Soul Bag effects to stack past 100% and incorporate Luck stat
		//Effect chances for AA Battery and Soul Bag now function independently
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "GainAPFromKill")]
		static bool TrinketController_GainAPFromKill(TrinketController __instance)
		{
			//Increase effect value for each trinket
			float AABatteryEffectValue = 0f;
			float SoulBagEffectValue = 0f;
			float OtherEffectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				if (__instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.AABattery)
				{
					//Chance: 1/4
					AABatteryEffectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceOfGainingAPFromKill();
				}
				else if (__instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.SoulBag)
				{
					//Chance: 1/4
					SoulBagEffectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceOfGainingAPFromKill();
				}
				else
				{
					OtherEffectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceOfGainingAPFromKill();
				}
				trinketCounter += 1;
			}
			AABatteryEffectValue *= (float)__instance.EffectMultiplier();
			AABatteryEffectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);
			SoulBagEffectValue *= (float)__instance.EffectMultiplier();
			SoulBagEffectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);
			OtherEffectValue *= (float)__instance.EffectMultiplier();
			OtherEffectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int AABatteryEffectActivationCounter = EffectActivationCounter(AABatteryEffectValue);
			int SoulBagEffectActivationCounter = EffectActivationCounter(SoulBagEffectValue);
			int OtherEffectActivationCounter = EffectActivationCounter(OtherEffectValue);

			bool effectTriggered = false;
			//Main effect
			if (AABatteryEffectActivationCounter > 0)
			{
				__instance.app.controller.ModifyActionPoint(AABatteryEffectActivationCounter);
				effectTriggered = true;
			}
			if (SoulBagEffectActivationCounter > 0)
			{
				__instance.app.controller.ModifyActionPoint(SoulBagEffectActivationCounter);
				effectTriggered = true;
			}
			if (OtherEffectActivationCounter > 0)
			{
				__instance.app.controller.ModifyActionPoint(OtherEffectActivationCounter);
				effectTriggered = true;
			}
			//Additional effect if one of the trinkets was activated
			if (effectTriggered)
			{
				__instance.app.controller.ShowActionPointGain();
			}
			return false;
		}

		//Patch: Allows Bag-O-Sucking effect to stack past 100% and incorporate Luck stat
		//Bag-O-Sucking effect now stacks non-independently with itself
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "OnEnemyHit")]
		static bool TrinketController_OnEnemyHit(TrinketController __instance, Enemy.AttackImmunity _immunity)
		{
			//Increase effect value
			//Chance: 1/2
			float activationChance = 0.5f;

			float effectValue = 0f;

			short num = 0;
			while ((int)num < __instance.app.model.characterSheet.trinkets.Count)
			{
				__instance.app.controller.GetTrinket((int)num).OnEnemyHit();
				if (_immunity == Enemy.AttackImmunity.ReducePuzzleDamage)
				{
					__instance.app.controller.GetTrinket((int)num).OnEnemyPuzzleHit();
				}
				if (_immunity == Enemy.AttackImmunity.ReduceSpellDamage)
				{
					if (__instance.app.controller.GetTrinket((int)num).trinketName == TrinketName.BagOSucking)
					{
						effectValue += activationChance;
					}
				}
				num += 1;
			}
			__instance.app.model.characterSheet.hiddenTrinket.OnEnemyHit();
			if (_immunity == Enemy.AttackImmunity.ReducePuzzleDamage)
			{
				__instance.app.model.characterSheet.hiddenTrinket.OnEnemyPuzzleHit();
			}
			if (_immunity == Enemy.AttackImmunity.ReduceSpellDamage)
			{
				__instance.app.model.characterSheet.hiddenTrinket.OnEnemySpellHit();
			}

			effectValue *= (float)__instance.EffectMultiplier();
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			//Replace Bag-O-Sucking effect
			if (activationCounter > 0)
			{
				List<ManaType> list = new List<ManaType>
				{
					ManaType.Bone,
					ManaType.Booger,
					ManaType.Pee,
					ManaType.Poop,
					ManaType.Tooth
				};
				short[] array = new short[6];
				for (int i = 0; i < activationCounter * 3; i++)
				{
					List<ManaType> list2 = new List<ManaType>();
					for (int j = 0; j < list.Count; j++)
					{
						if (__instance.app.model.mana[(int)list[j]] + array[(int)list[j]] < 9)
						{
							list2.Add(list[j]);
						}
					}
					if (list2.Count > 0)
					{
						ManaType manaType = list2[UnityEngine.Random.Range(0, list2.Count)];
						array[(int)manaType] += 1;
					}
				}
				__instance.app.controller.UpdateMana(array, false);
				__instance.app.controller.ShowManaGain();
				__instance.app.controller.SetActiveSpells(true, true);
			}
			return false;
		}

		//Patch: Allows Bloody Battery effect to stack past 100% and incorporate Luck stat
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "GainAPFromAttack")]
		static bool TrinketController_GainAPFromAttack(TrinketController __instance)
		{
			//Increase effect value
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/10
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceOfGainingAPFromAttack();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			//Trigger effect
			if (activationCounter > 0)
			{
				__instance.app.controller.ModifyActionPoint(activationCounter);
				__instance.app.controller.ShowActionPointGain();
			}
			return false;
		}
		//Patch: Reduces Bloody Battery trigger chance
		[HarmonyPostfix, HarmonyPatch(typeof(BloodyBatteryTrinket), "ChanceOfGainingAPFromAttack")]
		static void BloodyBatteryTrinket_ChanceOfGainingAPFromAttack(BloodyBatteryTrinket __instance, ref float __result)
		{
			__result = 0.15f;
		}

		//Patch: Allows CurvedHorn effect to stack past 100% and incorporate Luck stat
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "NewRound")]
		static bool TrinketController_NewRound(TrinketController __instance)
		{
			__instance.RemoveCurseOnRoundStart();
			__instance.AtHalfHeart();

			//Increase effect value
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				if (__instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.CurvedHorn)
				{
					//Chance: 1/3
					effectValue += (float)1f / 3f;
				}
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			//Trigger effect
			if (activationCounter > 0)
			{
				__instance.app.model.characterSheet.bumboRoundModifiers.itemDamage += activationCounter;
				__instance.app.controller.UpdateStats();
				__instance.app.controller.UpdateSpellDamage();
				__instance.app.controller.ShowDamageUp();
			}
			return false;
		}

		//Patch: Allows Drakula Teeth effect to stack past 100% and incorporate Luck stat
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "GainHealthFromAttack")]
		static bool TrinketController_GainHealthFromAttack(TrinketController __instance)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/10
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceOfGainingHealthFromAttack();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			if (activationCounter > 0)
			{
				__instance.app.view.hearts.GetComponent<HealthController>().modifyHealth(0.5f * activationCounter, 0f);
				__instance.app.view.soundsView.PlaySound(SoundsView.eSound.Gulp, SoundsView.eAudioSlot.Default, false);
			}
			return false;
		}

		//Patch: Allows Magnet effect to stack past 100% and incorporate Luck stat
		//Override trinket controller method
		[HarmonyPostfix, HarmonyPatch(typeof(BumboController), "CalculateReward")]
		static void BumboController_CalculateReward(BumboController __instance, ref int __result)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/3
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.Magnet ? (float)1f / 3f : 0f;
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.trinketController.EffectMultiplier();
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			__result += activationCounter * 2;
		}
		//Disable original Magnet effect
		[HarmonyPostfix, HarmonyPatch(typeof(MagnetTrinket), "DoubleReward")]
		static void MagnetTrinket_DoubleReward(MagnetTrinket __instance, ref float __result)
		{
			__result = 0f;
		}

		//Patch: Mom's Photo now incorporates Luck stat
		[HarmonyPostfix, HarmonyPatch(typeof(TrinketController), "WillBlind")]
		static void TrinketController_WillBlind(TrinketController __instance, ref bool __result)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/4
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceOfBlind();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);
			__result = UnityEngine.Random.Range(0f, 1f) < effectValue;
		}

		//Patch: Allows Nine Volt effect to stack past 100% and incorporate Luck stat
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "RechargeOnSpell")]
		static bool TrinketController_RechargeOnSpell(TrinketController __instance)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/4
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).RechargeOnSpell();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int NineVoltActivationCounter = EffectActivationCounter(effectValue);
			bool effectTriggered = false;

			while (NineVoltActivationCounter > 0)
			{
				NineVoltActivationCounter--;
				List<int> list = new List<int>();
				short num3 = 0;
				while ((int)num3 < __instance.app.model.characterSheet.spells.Count)
				{
					if (__instance.app.model.characterSheet.spells[(int)num3].IsChargeable && __instance.app.model.characterSheet.spells[(int)num3].charge < __instance.app.model.characterSheet.spells[(int)num3].requiredCharge)
					{
						list.Add((int)num3);
					}
					num3 += 1;
				}
				if (list.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, list.Count);
					__instance.app.model.characterSheet.spells[list[index]].ChargeSpell();
					effectTriggered = true;
				}
			}

			if (effectTriggered)
			{
				__instance.app.controller.SetActiveSpells(true, true);
			}
			return false;
		}

		//Patch: Pink Eye now incorporates Luck stat
		[HarmonyPostfix, HarmonyPatch(typeof(TrinketController), "WillPoison")]
		static void TrinketController_WillPoison(TrinketController __instance, ref bool __result)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/4
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceOfPoison();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);
			__result = UnityEngine.Random.Range(0f, 1f) < effectValue;
		}

		//Patch: Allows Pinky effect to stack past 100% and incorporate Luck stat
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "WildTileOnKill")]
		static bool TrinketController_WildTileOnKill(TrinketController __instance)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/3
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.Pinky ? 1 / 3 : 0f;
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			if (activationCounter > 0)
			{
				List<Block> list = new List<Block>();
				for (int i = 0; i < __instance.app.view.puzzle.width; i++)
				{
					for (int j = 0; j < __instance.app.view.puzzle.height; j++)
					{
						Block component = __instance.app.view.puzzle.blocks[i, j].GetComponent<Block>();
						if (component.block_type != Block.BlockType.Wild)
						{
							list.Add(component);
						}
					}
				}
				while (list.Count > 0 && activationCounter > 0)
				{
					activationCounter--;

					int index = UnityEngine.Random.Range(0, list.Count);
					short x = (short)list[index].position.x;
					short y = (short)list[index].position.y;
					list[index].Despawn(false);
					__instance.app.view.puzzle.setBlock(Block.BlockType.Wild, x, y, false, true);
				}
			}
			return false;
		}

		//Disable original Rat Heart effect
		[HarmonyPrefix, HarmonyPatch(typeof(RatHeartTrinket), "StartRoom")]
		static bool RatHeartTrinket_StartRoom(RatHeartTrinket __instance)
		{
			return false;
		}
		//Disable original Small Box effect
		[HarmonyPrefix, HarmonyPatch(typeof(SmallBoxTrinket), "StartRoom")]
		static bool SmallBoxTrinket_StartRoom(SmallBoxTrinket __instance)
		{
			return false;
		}
		//Patch: Allows Rat Heart effect to stack past 100% and incorporate Luck stat and Curio effect multiplier
		//Also fixes Rat Heart activating a second time when reloading a save
		//Also causes Small Box to stack non-independently and incoporate Curio effect multiplier
		[HarmonyPrefix, HarmonyPatch(typeof(BumboController), "StartRoomWithTrinkets")]
		static bool BumboController_StartRoomWithTrinkets(BumboController __instance)
		{
			if ((__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.EnemyEncounter || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Boss || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Start) && !__instance.app.model.mapModel.currentRoom.cleared)
			{
				float RatHeartEffectValue = 0f;
				int SmallBoxEffectValue = 0;
				short trinketCounter = 0;
				while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
				{
					//Prevent Rat Heart from triggering when reloading a save
					if (!__instance.app.controller.savedStateController.IsLoading())
					{
						//Chance: 1/4
						RatHeartEffectValue += __instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.RatHeart ? 0.25f : 0f;
					}
					SmallBoxEffectValue += __instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.SmallBox ? 1 : 0;
					__instance.GetTrinket((int)trinketCounter).StartRoom();
					trinketCounter += 1;
				}
				__instance.app.model.characterSheet.hiddenTrinket.StartRoom();
				__instance.UpdateMana(new short[6], false);

				//Incorporate Luck modifier and Curio multiplier
				RatHeartEffectValue *= (float)__instance.trinketController.EffectMultiplier();
				RatHeartEffectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

				SmallBoxEffectValue *= __instance.trinketController.EffectMultiplier();

				int RatHeartActivationCounter = EffectActivationCounter(RatHeartEffectValue);

				if (RatHeartActivationCounter > 0)
				{
					__instance.app.view.hearts.GetComponent<HealthController>().modifyHealth(0f, 0.5f * RatHeartActivationCounter);
				}

				if (SmallBoxEffectValue > 0)
				{
					short[] array = new short[6];

					while (SmallBoxEffectValue > 0)
					{
						SmallBoxEffectValue--;

						List<ManaType> list = new List<ManaType>
						{
							ManaType.Bone,
							ManaType.Booger,
							ManaType.Pee,
							ManaType.Poop,
							ManaType.Tooth
						};
						while (list.Count > 3)
						{
							int index = UnityEngine.Random.Range(0, list.Count);
							list.RemoveAt(index);
						}
						for (int i = 0; i < list.Count; i++)
						{
							ManaType manaType = list[i];
							array[(int)manaType] += 1;
						}
					}

					__instance.app.controller.UpdateMana(array, false);
					__instance.app.controller.ShowManaGain();
					__instance.app.controller.SetActiveSpells(true, true);
				}
			}
			return false;
		}

		//Patch: Rat Tail effect now stacks non-independently and incorporates Luck stat and Curio effect multiplier
		[HarmonyPostfix, HarmonyPatch(typeof(RatTailTrinket), "AddToDodge")]
		static void RatTailTrinket_AddToDodge(RatTailTrinket __instance, ref float __result)
		{
			//Abort for all but one Rat Tail
			int firstRatTailIndex = -1;
			short trinketCounter1 = 0;
			while ((int)trinketCounter1 < __instance.app.model.characterSheet.trinkets.Count)
			{
				if (__instance.app.controller.GetTrinket((int)trinketCounter1).trinketName == TrinketName.RatTail)
				{
					firstRatTailIndex = trinketCounter1;
				}
				trinketCounter1 += 1;
			}

			if (firstRatTailIndex == -1 || __instance != __instance.app.controller.GetTrinket((int)firstRatTailIndex))
			{
				return;
			}

			//Return chance of all Rat Tail trinkets combined
			float effectValue = 0f;
			short trinketCounter2 = 0;
			while ((int)trinketCounter2 < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/10
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter2).trinketName == TrinketName.RatTail ? 0.1f : 0f;
				trinketCounter2 += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);
			__result = effectValue;
		}

		//Patch: Allows Santa Sangre effect to stack past 100% and incorporate Luck stat
		//Also fixes Santa Sangre granting soul health past the maximum of six total hearts
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "SoulOnKill")]
		static bool TrinketController_SoulOnKill(TrinketController __instance)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/10
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).SoulOnKill();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			if (activationCounter > 0)
			{
				__instance.app.view.hearts.GetComponent<HealthController>().modifyHealth(0f, 0.5f * activationCounter);
			}
			return false;
		}

		//Patch: Allows Sharp Nail effect to stack past 100% and incorporate Luck stat
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "HurtMovingEnemies")]
		static bool TrinketController_HurtMovingEnemies(TrinketController __instance)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/10
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceToHurtMovingEnemies();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			if (activationCounter > 0)
			{
				for (int enemyCounter = 0; enemyCounter < __instance.app.model.aiModel.movingEnemies.Count; enemyCounter++)
				{
					__instance.app.model.aiModel.movingEnemies[enemyCounter].Hurt(activationCounter, Enemy.AttackImmunity.ReduceSpellDamage, null, __instance.app.model.aiModel.movingEnemies[enemyCounter].position.x);
				}
			}
			return false;
		}

		//Patch: Causes Silver Skull to stack non-independently and incorporate Curio multiplier
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "RewardKill")]
		static bool TrinketController_RewardKill(TrinketController __instance)
		{
			int effectValue = 0;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.SilverSkull ? 1 : 0;
				trinketCounter += 1;
			}
			effectValue *= __instance.EffectMultiplier();

			if (effectValue > 0)
			{
				short[] array = new short[6];

				while (effectValue > 0)
				{
					effectValue--;

					List<Block.BlockType> list = new List<Block.BlockType>();
					for (int i = 0; i < 6; i++)
					{
						if (__instance.app.model.mana[i] < 9)
						{
							list.Add((Block.BlockType)i);
						}
					}
					Block.BlockType blockType = list[UnityEngine.Random.Range(0, list.Count)];
					array[(int)blockType] += 1;
				}

				__instance.app.controller.UpdateMana(array, false);
				__instance.app.controller.ShowManaGain();
				__instance.app.controller.SetActiveSpells(true, true);
			}
			return false;
		}

		//Patch: Causes Stray Barb to stack non-independently and incorporate Luck stat
		[HarmonyPostfix, HarmonyPatch(typeof(TrinketController), "ChanceForCounterAttack")]
		static void TrinketController_ChanceForCounterAttack(TrinketController __instance, ref int __result)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/10
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceForCounterAttack();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			__result = activationCounter;
		}

		//Patch: Super Ball now incorporates Luck stat
		[HarmonyPostfix, HarmonyPatch(typeof(TrinketController), "WillKnockback")]
		static void TrinketController_WillKnockback(TrinketController __instance, ref bool __result)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/2
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceToKnockback();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);
			__result = UnityEngine.Random.Range(0f, 1f) < effectValue;
		}

		//Patch: Allows Swallowed Penny effect to stack past 100% and incorporate Luck stat
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "OnHurt")]
		static bool TrinketController_OnHurt(TrinketController __instance)
		{
			short trinketCounter1 = 0;
			while ((int)trinketCounter1 < __instance.app.model.characterSheet.trinkets.Count)
			{
				__instance.app.controller.GetTrinket((int)trinketCounter1).OnHurt();
				trinketCounter1 += 1;
			}
			float effectValue = 0f;
			short trinketCounter2 = 0;
			while ((int)trinketCounter2 < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/4
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter2).CoinOnHurt();
				trinketCounter2 += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			if (activationCounter > 0)
			{
				__instance.app.controller.ModifyCoins(activationCounter);
			}
			return false;
		}

		//Patch: Allows Tweezers effect to stack past 100% and incorporate Luck stat
		//Also prevents Tweezers from granting red mana
		[HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "GainRandomManaFromAttack")]
		static bool TrinketController_GainRandomManaFromAttack(TrinketController __instance)
		{
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/2
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).ChanceOfGainingRandomManaFromAttack();
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			if (activationCounter > 0)
			{
				short[] array = new short[6];
				int randomColor = UnityEngine.Random.Range(0, 5);
				if (randomColor > 0)
				{
					randomColor++;
				}
				array[randomColor] = (short)activationCounter;
				__instance.app.view.stolenManaView.gameObject.SetActive(true);
				__instance.app.view.stolenManaView.SetManaStolen(activationCounter, (Block.BlockType)randomColor);
				__instance.app.controller.UpdateMana(array, false);
				__instance.app.controller.ShowManaGain();
				__instance.app.controller.SetActiveSpells(true, true);

				Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing tweezers from granting red mana");
			}
			return false;
		}

		//Patch: Allows Wet Diaper effect to stack past 100% and incorporate Luck stat
		[HarmonyPrefix, HarmonyPatch(typeof(BumboController), "ModifyActionPoint")]
		static bool BumboController_ModifyActionPoint(BumboController __instance, int _modifier)
		{
			__instance.app.model.actionPoints += (short)_modifier;

			//Replace Wet Diaper effect
			float effectValue = 0f;
			short trinketCounter = 0;
			while ((int)trinketCounter < __instance.app.model.characterSheet.trinkets.Count)
			{
				//Chance: 1/4
				effectValue += __instance.app.controller.GetTrinket((int)trinketCounter).trinketName == TrinketName.WetDiaper ? 0.2f : 0f;
				trinketCounter += 1;
			}
			effectValue *= (float)__instance.trinketController.EffectMultiplier();
			//Incorporate Luck modifier
			effectValue *= TrinketLuckModifier(__instance.app.model.characterSheet);

			int activationCounter = EffectActivationCounter(effectValue);

			if (_modifier > 0 && activationCounter > 0)
			{
				__instance.app.model.actionPoints += (short)activationCounter;
			}
			if (__instance.app.model.actionPoints < 0)
			{
				__instance.app.model.actionPoints = 0;
			}
			if (__instance.app.model.actionPoints > 9)
			{
				__instance.app.model.actionPoints = 9;
			}
			__instance.app.view.actionPoints.SetActionPoints((int)__instance.app.model.actionPoints);
			__instance.app.view.IOSActionPoints.GetComponent<TextMeshPro>().text = __instance.app.model.actionPoints.ToString();
			return false;
		}

		//Patch: Reduces Death damage dealt to bosses
		[HarmonyPrefix, HarmonyPatch(typeof(DeathTrinket), "Use")]
		static bool DeathTrinket_Use(DeathTrinket __instance, int _index)
		{
			CollectibleFixes.UseTrinket_Use_Prefix(__instance, currentTrinketIndex);
			CollectibleFixes.UseTrinket_Use_Base_Method(__instance, currentTrinketIndex);
			CollectibleFixes.UseTrinket_Use_Postfix(__instance);

			List<Enemy> list = new List<Enemy>();
			list.AddRange(__instance.app.model.enemies);
			short num = 0;
			while ((int)num < list.Count)
			{
				if (list[(int)num] != null && list[(int)num].GetComponent<Enemy>().alive)
				{
					float damage = list[(int)num].boss ? 3f : list[(int)num].getHealth();
					list[(int)num].GetComponent<Enemy>().Hurt(damage, Enemy.AttackImmunity.SuperAttack, null, -1);
				}
				num += 1;
			}
			__instance.app.controller.eventsController.SetEvent(new NextComboEvent());

			return false;
		}

		public static UseTrinket currentTrinket;
		static int currentTrinketIndex;
		public static bool[] enabledSpells = new bool[6];
		//Patch: Allows the player to choose which spell to use Rainbow Tick on 
		[HarmonyPrefix, HarmonyPatch(typeof(RainbowTickTrinket), "Use")]
		static bool RainbowTickTrinket_Use(RainbowTickTrinket __instance, int _index)
		{
			//Loop through spells
			bool anyActiveSpells = false;
			int spellCounter = 0;
			while (spellCounter < __instance.app.model.characterSheet.spells.Count)
			{
				//Record which spells are disabled
				if (!__instance.app.view.spells[spellCounter].disableObject.activeSelf)
				{
					enabledSpells[spellCounter] = true;
				}
				else
				{
					enabledSpells[spellCounter] = false;
				}

				//Enable/disable spells
				if (CalculateCostReduction(spellCounter, 0.15f, __instance.app, false) > 0)
				{
					__instance.app.view.spells[spellCounter].EnableSpell();
					anyActiveSpells = true;
				}
				else
				{
					__instance.app.view.spells[spellCounter].DisableSpell();
				}
				spellCounter += 1;
			}

			//Abort if there are no viable spells
			if (!anyActiveSpells)
			{
				for (int spellCounter2 = 0; spellCounter2 < __instance.app.model.characterSheet.spells.Count; spellCounter2++)
				{
					if (enabledSpells[spellCounter2])
					{
						__instance.app.view.spells[spellCounter2].EnableSpell();
					}
					else
					{
						__instance.app.view.spells[spellCounter2].DisableSpell();
					}
				}
				__instance.app.controller.GUINotification("No Viable Spells", GUINotificationView.NotifyType.General, null, true);
				return false;
			}

			currentTrinket = __instance;
			currentTrinketIndex = _index;

			__instance.app.model.spellModel.currentSpell = null;
			__instance.app.model.spellModel.spellQueued = false;

			__instance.app.model.spellViewUsed = null;

			__instance.app.controller.eventsController.SetEvent(new SpellModifySpellEvent());
			__instance.app.controller.GUINotification("Pick A Spell To Modify", GUINotificationView.NotifyType.Spell, null, false);

			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing Rainbow Tick effect");
			return false;
		}

		//Patch: Allows the player to choose which spell to use Brown Tick on 
		[HarmonyPrefix, HarmonyPatch(typeof(BrownTickTrinket), "Use")]
		static bool BrownTickTrinket_Use(BrownTickTrinket __instance, int _index)
		{
			//Loop through spells
			bool anyActiveSpells = false;
			int spellCounter = 0;
			while (spellCounter < __instance.app.model.characterSheet.spells.Count)
			{
				//Record which spells are disabled
				if (!__instance.app.view.spells[spellCounter].disableObject.activeSelf)
				{
					enabledSpells[spellCounter] = true;
				}
				else
				{
					enabledSpells[spellCounter] = false;
				}

				//Enable/disable spells
				if (__instance.app.model.characterSheet.spells[spellCounter].IsChargeable && __instance.app.model.characterSheet.spells[spellCounter].requiredCharge > 0)
				{
					__instance.app.view.spells[spellCounter].EnableSpell();
					anyActiveSpells = true;
				}
				else
				{
					__instance.app.view.spells[spellCounter].DisableSpell();
				}
				spellCounter += 1;
			}

			//Abort if there are no viable spells
			if (!anyActiveSpells)
			{
				for (int spellCounter2 = 0; spellCounter2 < __instance.app.model.characterSheet.spells.Count; spellCounter2++)
				{
					if (enabledSpells[spellCounter2])
					{
						__instance.app.view.spells[spellCounter2].EnableSpell();
					}
					else
					{
						__instance.app.view.spells[spellCounter2].DisableSpell();
					}
				}
				__instance.app.controller.GUINotification("No Viable Spells", GUINotificationView.NotifyType.General, null, true);
				return false;
			}

			currentTrinket = __instance;
			currentTrinketIndex = _index;

			__instance.app.model.spellModel.currentSpell = null;
			__instance.app.model.spellModel.spellQueued = false;

			__instance.app.model.spellViewUsed = null;

			__instance.app.controller.eventsController.SetEvent(new SpellModifySpellEvent());
			__instance.app.controller.GUINotification("Pick A Spell To Modify", GUINotificationView.NotifyType.Spell, null, false);

			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing Brown Tick effect");
			return false;
		}

		//Patch: Implements trinket modify spell effects
		//Rainbow Tick
		//Brown Tick
		[HarmonyPrefix, HarmonyPatch(typeof(SpellView), "OnMouseDown")]
		static bool SpellView_OnMouseDown(SpellView __instance, bool ___exit, SpellElement ___spell)
		{
			if (!__instance.app.model.paused && !___exit && ___spell != null && !__instance.disableObject.activeSelf && __instance.app.model.bumboEvent.GetType().ToString() == "SpellModifySpellEvent" && currentTrinket != null)
			{
				__instance.app.view.soundsView.PlaySound(SoundsView.eSound.Button, SoundsView.eAudioSlot.Default, false);

				CollectibleFixes.UseTrinket_Use_Prefix(currentTrinket, currentTrinketIndex);
				CollectibleFixes.UseTrinket_Use_Base_Method(currentTrinket, currentTrinketIndex);
				CollectibleFixes.UseTrinket_Use_Postfix(currentTrinket);

				switch (currentTrinket.trinketName)
				{
					case TrinketName.RainbowTick:
						SpellElement spellElement = ___spell;

						int costReduction = CalculateCostReduction(__instance.spellIndex, 0.15f, __instance.app, false);

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
							short[] cost = __instance.app.model.characterSheet.spells[__instance.spellIndex].Cost;
							cost[randomColor] -= 1;

							int totalCombinedCost = 0;
							//Increase the reduced color's cost modifier if the spell's total cost (including modifier) would be reduced below minimum OR if the reduced color's cost (including modifier) would be reduced below zero
							for (int costCounter = 0; costCounter < 6; costCounter++)
							{
								totalCombinedCost += (short)(__instance.app.model.characterSheet.spells[__instance.spellIndex].Cost[costCounter] + __instance.app.model.characterSheet.spells[__instance.spellIndex].CostModifier[costCounter]);
							}
							if (totalCombinedCost < SpellManaCosts.MinimumManaCost(__instance.app.model.characterSheet.spells[__instance.spellIndex]) || __instance.app.model.characterSheet.spells[__instance.spellIndex].Cost[randomColor] + __instance.app.model.characterSheet.spells[__instance.spellIndex].CostModifier[randomColor] < 0)
							{
								__instance.app.model.characterSheet.spells[__instance.spellIndex].CostModifier[randomColor] += 1;
							}
						}
						break;
					case TrinketName.BrownTick:
						//Reduce recharge time
						if (__instance.app.model.characterSheet.spells[__instance.spellIndex].requiredCharge > 0)
						{
							__instance.app.model.characterSheet.spells[__instance.spellIndex].requiredCharge--;
						}
						if (__instance.app.model.characterSheet.spells[__instance.spellIndex].requiredCharge < __instance.app.model.characterSheet.spells[__instance.spellIndex].charge)
						{
							__instance.app.model.characterSheet.spells[__instance.spellIndex].charge = __instance.app.model.characterSheet.spells[__instance.spellIndex].requiredCharge;
						}
						if (__instance.app.model.characterSheet.spells[__instance.spellIndex].requiredCharge == 0)
						{
							__instance.app.model.characterSheet.spells[__instance.spellIndex].chargeEveryRound = true;
							__instance.app.model.characterSheet.spells[__instance.spellIndex].usedInRound = false;
						}
						break;
				}

				for (int spellCounter2 = 0; spellCounter2 < __instance.app.model.characterSheet.spells.Count; spellCounter2++)
				{
					if (enabledSpells[spellCounter2])
					{
						__instance.app.view.spells[spellCounter2].EnableSpell();
					}
					else
					{
						__instance.app.view.spells[spellCounter2].DisableSpell();
					}
				}

				__instance.app.controller.SetActiveSpells(true, true);
				__instance.app.controller.UpdateSpellManaText();
				currentTrinket = null;

				__instance.app.view.soundsView.PlaySound(SoundsView.eSound.ItemUpgraded, SoundsView.eAudioSlot.Default, false);
				__instance.app.view.spells[__instance.spellIndex].spellParticles.Play();
				__instance.Shake(1f);
				__instance.app.controller.HideNotifications(false);

				__instance.app.model.spellModel.currentSpell = null;
				__instance.app.model.spellModel.spellQueued = false;

				if (__instance.app.model.bumboEvent.GetType().ToString() == "SpellModifySpellEvent")
				{
					__instance.app.controller.eventsController.EndEvent();
				}

				Console.WriteLine("[The Legend of Bum-bo: Windfall] Implementing trinket modify spell effect");
				return false;
			}
			return true;
		}

		//Patch: Changes starting stats and collectibles of characters
		//Increases the cost of Bum-bo the Dead's Attack Fly
		//Increases the cost of Bum-bo the Weird's Magic Marker
		[HarmonyPostfix, HarmonyPatch(typeof(CharacterSheet), "Awake")]
		static void CharacterSheet_Awake(CharacterSheet __instance)
		{
			StartingSpell[] deadStartingSpells = __instance.bumboList[(int)CharacterSheet.BumboType.TheDead].startingSpells;
			for (int i = 0; i < deadStartingSpells.Length; i++)
			{
				StartingSpell deadStartingSpell = deadStartingSpells[i];
				if (deadStartingSpell.spell == SpellName.AttackFly)
				{
					deadStartingSpell.toothCost = 5;
				}
			}

			StartingSpell[] weirdStartingSpells = __instance.bumboList[(int)CharacterSheet.BumboType.TheWeird].startingSpells;
			for (int i = 0; i < weirdStartingSpells.Length; i++)
			{
				StartingSpell weirdStartingSpell = weirdStartingSpells[i];
				if (weirdStartingSpell.spell == SpellName.MagicMarker)
				{
					weirdStartingSpell.peeCost = 6;
				}
			}
		}

		//Patch: Spell mana cost text now indicates whether the cost is temporarily modified
		[HarmonyPrefix, HarmonyPatch(typeof(BumboController), "UpdateSpellManaText", new Type[] { typeof(int), typeof(SpellElement) })]
		static bool BumboController_UpdateSpellManaText(BumboController __instance, int _spell_index, SpellElement _spell)
		{
			if (!_spell.IsChargeable)
			{
				float num = 0f;
				__instance.app.view.spells[_spell_index].spellMana1.SetActive(false);
				for (short num2 = 0; num2 < 5; num2 += 1)
				{
					int num3 = (int)num2;
					if (num2 > 0)
					{
						num3++;
					}
					if (_spell.Cost[num3] > 0 || _spell.CostModifier[num3] > 0)
					{
						__instance.app.view.spells[_spell_index].manaIconViews[num3].gameObject.transform.localPosition = new Vector3(-0.13f + 0.085f * num, 0.02f, 0f);
						__instance.app.view.spells[_spell_index].manaIconViews[num3].gameObject.SetActive(true);
						__instance.app.view.spells[_spell_index].manaIconViews[num3].SetMana((int)(_spell.Cost[num3] + _spell.CostModifier[num3]));
						num += 1f;

						//Change text color
						if (_spell.CostModifier[num3] < 0)
						{
							__instance.app.view.spells[_spell_index].manaIconViews[num3].amount.color = new Color(0.005f, 0.05f, 0.2f);
						}
						else if (_spell.CostModifier[num3] > 0)
						{
							__instance.app.view.spells[_spell_index].manaIconViews[num3].amount.color = new Color(0.2f, 0.005f, 0.005f);
						}
						else
						{
							__instance.app.view.spells[_spell_index].manaIconViews[num3].amount.color = Color.black;
						}
					}
					else
					{
						__instance.app.view.spells[_spell_index].manaIconViews[num3].gameObject.SetActive(false);
					}
				}
			}
			else
			{
				__instance.app.view.spells[_spell_index].spellMana1.SetActive(true);
				for (short num4 = 0; num4 < 5; num4 += 1)
				{
					int num5 = (int)num4;
					if (num4 > 0)
					{
						num5++;
					}
					__instance.app.view.spells[_spell_index].manaIconViews[num5].gameObject.SetActive(false);
				}
				short num6 = 0;
				while ((int)num6 < __instance.app.view.spells[_spell_index].spellMana2.Length)
				{
					__instance.app.view.spells[_spell_index].spellMana2[(int)num6].SetActive(false);
					num6 += 1;
				}
				short num7 = 0;
				while ((int)num7 < __instance.app.view.spells[_spell_index].spellMana3.Length)
				{
					__instance.app.view.spells[_spell_index].spellMana3[(int)num7].SetActive(false);
					num7 += 1;
				}
				__instance.app.view.spells[_spell_index].spellMana1.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.255f));
				__instance.app.view.spells[_spell_index].spellMana1.transform.GetChild(0).GetComponent<TextMeshPro>().text = _spell.charge + " / " + _spell.requiredCharge;
			}
			return false;
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
					int costReduction = CalculateCostReduction(i, 0.4f, __instance.app, true);

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
					int costReduction = CalculateCostReduction(i, 0.25f, __instance.app, true);

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

		public static int CalculateCostReduction(int _spell_index, float reductionPercentage, BumboApplication bumboApplication, bool temporaryCost)
		{
			//Calculate cost reduction
			int totalManaCost = 0;
			for (int i = 0; i < 6; i++)
			{
				totalManaCost += (int)bumboApplication.model.characterSheet.spells[_spell_index].Cost[i];
				if (temporaryCost)
				{
					totalManaCost += (int)bumboApplication.model.characterSheet.spells[_spell_index].CostModifier[i];
				}
			}

			int costReduction = Mathf.RoundToInt((float)totalManaCost * reductionPercentage);

			//Do not reduce total cost below minimum
			while (totalManaCost - costReduction < SpellManaCosts.MinimumManaCost(bumboApplication.model.characterSheet.spells[_spell_index]))
			{
				costReduction--;
				if (costReduction <= 0)
				{
					break;
				}
			}
			return costReduction;
		}

		//Patch: Mana needle rework
		[HarmonyPostfix, HarmonyPatch(typeof(ManaPrickTrinket), "QualifySpell")]
		static void ManaPrickTrinket_QualifySpell(ManaPrickTrinket __instance, int _spell_index)
		{
			int costReduction = CalculateCostReduction(_spell_index, 0.25f, __instance.app, false);

			//Enable spell if cost reduction is above zero
			if (costReduction > 0)
			{
				__instance.app.view.spells[_spell_index].EnableSpell();
			}
			else
			{
				__instance.app.view.spells[_spell_index].DisableSpell();
			}

			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing mana needle spell qualification");
		}
		[HarmonyPrefix, HarmonyPatch(typeof(ManaPrickTrinket), "UpdateSpell")]
		static bool ManaPrickTrinket_UpdateSpell(ManaPrickTrinket __instance, int _spell_index)
		{
			SpellElement spellElement = __instance.app.model.characterSheet.spells[_spell_index];

			int costReduction = CalculateCostReduction(_spell_index, 0.25f, __instance.app, false);

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
				int costReduction = CalculateCostReduction(num, 0.25f, __instance.app, false);
				if (costReduction > 0)
				{
					___needles.Add(TrinketName.ManaPrick);
					Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing mana needle appearance condition");
					return false;
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
			//Load shop
			WindfallSavedState.LoadStart(__instance.app);
			bool loadShop = WindfallSavedState.LoadShop(__instance, ___trinketModel);
			WindfallSavedState.LoadEnd();

			if (loadShop)
			{
				((GamepadGamblingController)UnityEngine.Object.FindObjectsOfType(typeof(GamepadGamblingController))[0]).UpdateShopItems();
				return false;
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
			((GamepadGamblingController)UnityEngine.Object.FindObjectsOfType(typeof(GamepadGamblingController))[0]).UpdateShopItems();
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing shop generation");

			//Save shop
			WindfallSavedState.SaveShop(__instance);
			return false;
		}

		//Patch: Reworks mana cost generation
		//Certain spells have new base mana costs
		//The number of mana colors is now determined in a flexible way
		//Permanent and temporary mana cost reduction is now preserved when rerolling spell costs
		//Converter special mana cost generation is preserved when its mana cost is rerolled
		[HarmonyPrefix, HarmonyPatch(typeof(BumboController), "SetSpellCost", new Type[] { typeof(SpellElement), typeof(bool[]) })]
		static bool BumboController_SetSpellCost(BumboController __instance, SpellElement _spell, bool[] _ignore_mana, ref SpellElement __result)
		{
			if (_spell.spellName.ToString().Contains("Converter"))
			{
				Block.BlockType blockType = Block.BlockType.Bone;
				switch (_spell.spellName)
				{
					case SpellName.ConverterWhite:
						blockType = Block.BlockType.Bone;
						break;
					case SpellName.ConverterBrown:
						blockType = Block.BlockType.Poop;
						break;
					case SpellName.ConverterGreen:
						blockType = Block.BlockType.Booger;
						break;
					case SpellName.ConverterGrey:
						blockType = Block.BlockType.Tooth;
						break;
					case SpellName.ConverterYellow:
						blockType = Block.BlockType.Pee;
						break;
				}

				List<Block.BlockType> list = new List<Block.BlockType>();
				for (int i = 0; i < 6; i++)
				{
					if (i != 1 && i != (int)blockType)
					{
						list.Add((Block.BlockType)i);
					}
				}
				short[] array = new short[6];
				for (int j = 0; j < 2; j++)
				{
					int index = UnityEngine.Random.Range(0, list.Count);
					Block.BlockType chosenBlockType = list[index];
					array[(int)chosenBlockType] += 1;
					list.RemoveAt(index);
				}
				_spell.Cost = array;
				__result = _spell;

				Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing Converter mana cost generation");
				return false;
			}

			if (_ignore_mana == null)
			{
				_ignore_mana = new bool[6];
			}
			if (!_spell.IsChargeable && _spell.setCost)
			{
				//Get total mana cost
				int totalSpellCost = SpellManaCosts.GetManaCost(_spell);

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

				List<short> spellCost = new List<short>();

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
				List<ManaType> allowedColors = new List<ManaType>
				{
					ManaType.Bone,
					ManaType.Booger,
					ManaType.Pee,
					ManaType.Poop,
					ManaType.Tooth
				};
				for (int m = 0; m < __instance.app.model.characterSheet.spells.Count; m++)
				{
					if (allowedColors.Count > 0)
					{
						for (int n = allowedColors.Count - 1; n >= 0; n--)
						{
							if (_ignore_mana[(int)allowedColors[n]])
							{
								//Ban ignored colors
								allowedColors.RemoveAt(n);
							}
							else if (__instance.app.model.characterSheet.spells[m].Cost[(int)allowedColors[n]] != 0)
							{
								//Ban colors of existing spells
								allowedColors.RemoveAt(n);
							}
						}
					}
				}
				List<ManaType> list6 = new List<ManaType>();
				_spell.Cost = new short[6];
				for (int num10 = 0; num10 < spellCost.Count; num10++)
				{
					if (allowedColors.Count == 0)
					{
						for (int num11 = 0; num11 < 6; num11++)
						{
							if (num11 != 1 && list6.IndexOf((ManaType)num11) < 0)
							{
								allowedColors.Add((ManaType)num11);
							}
						}
					}
					int index2 = UnityEngine.Random.Range(0, allowedColors.Count);
					_spell.Cost[(int)allowedColors[index2]] = spellCost[num10];
					list6.Add(allowedColors[index2]);
					allowedColors.RemoveAt(index2);
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

		//Patch: Increases mana gain from Converter
		[HarmonyPrefix, HarmonyPatch(typeof(ConverterSpell), "ConvertMana")]
		static bool ConverterSpell_ConvertMana(ConverterSpell __instance, Block.BlockType _type)
		{
			short[] array = new short[6];
			//Increase mana gain to 2
			array[(int)_type] += 2;
			__instance.app.controller.UpdateMana(array, true);
			__instance.app.controller.ShowManaGain();
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

		//Patch: Prevents Mama Foot from killing the player (broken)
		[HarmonyPrefix, HarmonyPatch(typeof(MamaFootSpell), "Reward")]
		static bool MamaFootSpell_Reward(MamaFootSpell __instance)
		{
			float damage = 0.5f * __instance.app.model.characterSheet.bumboRoomModifiers.damageMultiplier;
			while (damage >= __instance.app.model.characterSheet.hitPoints + __instance.app.model.characterSheet.soulHearts)
			{
				damage -= 0.5f;
			}

			__instance.app.controller.TakeDamage(-damage / __instance.app.model.characterSheet.bumboRoomModifiers.damageMultiplier, null);

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

		//Patch: Changes Mama Shoe spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(MamaShoeSpell), "Damage")]
		static void MamaShoeSpell_Damage(MamaShoeSpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}

		//Patch: Changes Dog Tooth spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(DogToothSpell), "Damage")]
		static void DogToothSpell_Damage(DogToothSpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}

		//Patch: Changes Hair Ball spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(HairBallSpell), "Damage")]
		static void HairBallSpell_Damage(HairBallSpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}

		//Patch: Changes Hat Pin spell damage to incorporate the player's spell damage stat
		[HarmonyPostfix, HarmonyPatch(typeof(HatPinSpell), "Damage")]
		static void HatPinSpell_Damage(HatPinSpell __instance, ref int __result)
		{
			__result = __instance.baseDamage + __instance.app.model.characterSheet.getItemDamage() + __instance.SpellDamageModifier();
		}

		//Patch: Changes Yellow belt modifier to last for the room instead of only the current round
		[HarmonyPostfix, HarmonyPatch(typeof(YellowBeltSpell), "CastSpell")]
		static void YellowBeltSpell_CastSpell(YellowBeltSpell __instance, bool __result)
		{
			if (__result)
			{
				for (int i = 0; i < __instance.app.model.characterSheet.bumboModifierObjects.Count; i++)
				{
					//Detect whether there is a Yellow Belt modifier
					if (__instance.app.model.characterSheet.bumboModifierObjects[i].spellName == __instance.spellName)
					{
						//Change modifier type if it is a round modifier
						if (__instance.app.model.characterSheet.bumboModifierObjects[i].modifierType == CharacterSheet.BumboModifierObject.ModifierType.Round)
						{
							//Change modifier type
							__instance.app.model.characterSheet.bumboModifierObjects[i].modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
						}

						//Cap dodgeChance at 75%
						if (__instance.app.model.characterSheet.bumboModifierObjects[i].dodgeChance > 0.75f) __instance.app.model.characterSheet.bumboModifierObjects[i].dodgeChance = 0.75f;
					}
				}
			}
		}
	}

	static class SpellManaCosts
	{
		public static int GetManaCost(SpellElement spell)
		{
			//New mana costs
			int totalSpellCost = -1;

			if (manaCosts.TryGetValue(spell.spellName, out int value))
			{
				totalSpellCost = value;
			}

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

		public static Dictionary<SpellName, int> manaCosts = new Dictionary<SpellName, int>()
		{
			{ SpellName.AttackFly, 7 },
			{ SpellName.BigSlurp, 11 },
			{ SpellName.BlenderBlade, 5 },
			{ SpellName.HairBall, 7 },
			{ SpellName.HatPin, 8 },
			{ SpellName.Juiced, 6 },
			{ SpellName.KrampusCross, 5 },
			{ SpellName.Lemon, 5 },
			{ SpellName.MagicMarker, 6 },
			{ SpellName.MamaFoot, 13 },
			{ SpellName.Pentagram, 7 },
			{ SpellName.Pliers, 5 },
			{ SpellName.TimeWalker, 16 },
			{ SpellName.TwentyTwenty, 5 }
		};

		public static int MinimumManaCost(SpellElement spell)
		{
			return minimumManaCosts.TryGetValue(spell.spellName, out int value) ? value : Mathf.Max(2, Mathf.RoundToInt(GetManaCost(spell) / 2));
		}

		public static Dictionary<SpellName, int> minimumManaCosts = new Dictionary<SpellName, int>()
		{
			{ SpellName.MagicMarker, 5 }
		};
	}
}