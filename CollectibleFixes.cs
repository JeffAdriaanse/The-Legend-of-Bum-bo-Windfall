using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Linq;
using DG.Tweening;

namespace The_Legend_of_Bum_bo_Windfall
{
    class CollectibleFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(CollectibleFixes));
        }

        //Patch: Fixes TrinketController.NewRound not triggering when starting a room
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.StartRoom))]
        static void BumboController_StartRoom(BumboController __instance)
        {
            if ((__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.EnemyEncounter || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Boss || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Start) && !__instance.app.model.mapModel.currentRoom.cleared)
            {
                __instance.app.controller.trinketController.NewRound();
            }
        }

        //Fixes Butter Bean trinket effect
        //Replace method
        [HarmonyPrefix, HarmonyPatch(typeof(PoopAtStartEvent), nameof(PoopAtStartEvent.Toot))]
        static bool PoopAtStartEvent_Toot(PoopAtStartEvent __instance)
        {
            BumboView view = __instance.app.view;
            AIModel aiModel = __instance.app.model.aiModel;

            view.soundsView.PlaySound(SoundsView.eSound.Poop, SoundsView.eAudioSlot.Default, false);
            view.fartParticles.Play();

            //Repeat for both ground and air enemies; 0 for ground, 1 for air
            for (int enemyTypeIterator = 0; enemyTypeIterator < 2; enemyTypeIterator++)
            {
                //Iterate through all battlefield positions
                for (int laneIterator = 0; laneIterator < 3; laneIterator++)
                {
                    for (int rowIterator = 0; rowIterator < 3; rowIterator++)
                    {
                        //Get target enemy
                        GameObject targetEnemy;
                        if (enemyTypeIterator == 0)
                        {
                            targetEnemy = aiModel.battlefieldPositions[aiModel.battlefieldPositionIndex[laneIterator, rowIterator]].owner_ground;
                        }
                        else
                        {
                            targetEnemy = aiModel.battlefieldPositions[aiModel.battlefieldPositionIndex[laneIterator, rowIterator]].owner_air;
                        }

                        //Detect whether there is an enemy behind
                        GameObject enemyBehind = null;
                        if (rowIterator > 0)
                        {
                            if (enemyTypeIterator == 0)
                            {
                                enemyBehind = aiModel.battlefieldPositions[aiModel.battlefieldPositionIndex[laneIterator, rowIterator - 1]].owner_ground;
                            }
                            else
                            {
                                enemyBehind = aiModel.battlefieldPositions[aiModel.battlefieldPositionIndex[laneIterator, rowIterator - 1]].owner_air;
                            }
                        }

                        //Only knockback if there is space behind
                        if (rowIterator > 0 && targetEnemy != null && enemyBehind == null)
                        {
                            targetEnemy.GetComponent<Enemy>().Knockback();
                        }
                    }
                }
            }
            return false;
        }


        //Patch: Removes ChargeSpell from BossDyingEvent (function is being moved to GrabBossRewardEvent)
        [HarmonyPrefix, HarmonyPatch(typeof(BossDyingEvent), nameof(BossDyingEvent.NextEvent))]
        static bool BossDyingEvent_NextEvent(BossDyingEvent __instance, ref BumboEvent __result)
        {
            __result = new GrabBossRewardEvent();
            return false;
        }
        //Patch: Adds ChargeSpell to GrabBossRewardEvent
        [HarmonyPostfix, HarmonyPatch(typeof(GrabBossRewardEvent), nameof(GrabBossRewardEvent.Execute))]
        static void GrabBossRewardEvent_Execute(GrabBossRewardEvent __instance)
        {
            int spellCounter = 0;
            while (spellCounter < __instance.app.model.characterSheet.spells.Count)
            {
                __instance.app.model.characterSheet.spells[spellCounter].ChargeSpell();
                spellCounter += 1;
            }
        }

        //Patch: Prevents glitched trinkets from randomizing a second time when starting a chapter or reloading a save
        //Specifically, the method will be aborted if 'goIdle' is true (which is the case when roomStartEvent is triggered by floorStartEvent, not moveIntoRoomEvent)
        //Also prevents glitched trinkets from randomizing the first time when reloading a save so as to not overwrite loaded glitched trinkets
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), nameof(BumboController.PrestartRoom))]
        static bool BumboController_PrestartRoom(BumboController __instance)
        {
            if (WindfallSavedState.IsLoading() || (__instance.app.model.bumboEvent.GetType() == typeof(RoomStartEvent) && (bool)AccessTools.Field(typeof(RoomStartEvent), "goIdle").GetValue(__instance.app.model.bumboEvent)))
            {
                return false;
            }
            return true;
        }

        //Patch: Fixes Trash Lid not restoring the second block when used while there is one block remaining
        [HarmonyPostfix, HarmonyPatch(typeof(TrashLidSpell), nameof(TrashLidSpell.CastSpell))]
        static void TrashLidSpell_CastSpell(TrashLidSpell __instance, ref bool __result)
        {
            if (__result)
            {
                CharacterSheet.BumboModifierObject trashLidObject = __instance.app.model.characterSheet.bumboModifierObjects.Find(delegate (CharacterSheet.BumboModifierObject bumboModifierObject) { return bumboModifierObject.spellName == SpellName.TrashLid; });
                if (trashLidObject != null) trashLidObject.blockCounter = 2;
            }
        }

        //Patch: Fixes Breakfast activating a second time when reloading a save
        [HarmonyPostfix, HarmonyPatch(typeof(BreakfastTrinket), "AddToHealth")]
        static void BreakfastTrinket_AddToHealth(BreakfastTrinket __instance, ref float __result)
        {
            if (__instance.app.controller.savedStateController.IsLoading())
            {
                __result = 0f;
            }
        }

        //Patch: Fixes Pink Bow granting soul health past the maximum of six total hearts
        [HarmonyPrefix, HarmonyPatch(typeof(PinkBowTrinket), "EndChapter")]
        static bool PinkBowTrinket_EndChapter(PinkBowTrinket __instance)
        {
            __instance.app.view.hearts.GetComponent<HealthController>().modifyHealth(0f, (float)__instance.app.controller.trinketController.EffectMultiplier());
            return false;
        }

        //Patch: Fixes D20 granting soul health past the maximum of six total hearts
        [HarmonyPostfix, HarmonyPatch(typeof(D20Spell), "CastSpell")]
        static void D20Spell_CastSpell(D20Spell __instance)
        {
            while (__instance.app.model.characterSheet.bumboBaseInfo.hitPoints + __instance.app.model.characterSheet.soulHearts > 6f && __instance.app.model.characterSheet.soulHearts > 0f)
            {
                __instance.app.model.characterSheet.soulHearts -= 0.5f;
            }
            __instance.app.view.hearts.GetComponent<HealthController>().UpdateHearts(true);
        }

        //Patch: Fixes Prayer Card granting soul health past the maximum of six total hearts
        [HarmonyPostfix, HarmonyPatch(typeof(PrayerCardSpell), "CastSpell")]
        static void PrayerCardSpell_CastSpell(PrayerCardSpell __instance)
        {
            while (__instance.app.model.characterSheet.bumboBaseInfo.hitPoints + __instance.app.model.characterSheet.soulHearts > 6f && __instance.app.model.characterSheet.soulHearts > 0f)
            {
                __instance.app.model.characterSheet.soulHearts -= 0.5f;
            }
            __instance.app.view.hearts.GetComponent<HealthController>().UpdateHearts(true);
        }

        //Patch: Fixes The Relic granting soul health past the maximum of six total hearts
        [HarmonyPostfix, HarmonyPatch(typeof(TheRelicSpell), "CastSpell")]
        static void TheRelicSpell_CastSpell(TheRelicSpell __instance)
        {
            while (__instance.app.model.characterSheet.bumboBaseInfo.hitPoints + __instance.app.model.characterSheet.soulHearts > 6f && __instance.app.model.characterSheet.soulHearts > 0f)
            {
                __instance.app.model.characterSheet.soulHearts -= 0.5f;
            }
            __instance.app.view.hearts.GetComponent<HealthController>().UpdateHearts(true);
        }

        //Patch: Fixes Glitch trinket not reducing shop prices when acting as Steam Sale
        [HarmonyPrefix, HarmonyPatch(typeof(Shop), "UpdatePrices")]
        static bool Shop_UpdatePrices(Shop __instance, GameObject ___item1Pickup, GameObject ___item2Pickup, GameObject ___item3Pickup, GameObject ___item4Pickup)
        {
            int num = 0;
            for (int i = 0; i < __instance.app.model.characterSheet.trinkets.Count; i++)
            {
                //Use GetTrinket to account for glitched trinkets
                if (__instance.app.controller.GetTrinket(i).trinketName == TrinketName.SteamSale)
                {
                    num += 2;
                }
            }
            if (___item1Pickup != null)
            {
                ___item1Pickup.GetComponent<IPriceTag>().ReducePrice(num);
                ___item1Pickup.GetComponent<IPriceTag>().UpdatePriceTag(__instance.item1Price);
            }
            if (___item2Pickup != null)
            {
                ___item2Pickup.GetComponent<IPriceTag>().ReducePrice(num);
                ___item2Pickup.GetComponent<IPriceTag>().UpdatePriceTag(__instance.item2Price);
            }
            if (___item3Pickup != null)
            {
                ___item3Pickup.GetComponent<IPriceTag>().ReducePrice(num);
                ___item3Pickup.GetComponent<IPriceTag>().UpdatePriceTag(__instance.item3Price);
            }
            if (___item4Pickup != null)
            {
                ___item4Pickup.GetComponent<IPriceTag>().ReducePrice(num);
                ___item4Pickup.GetComponent<IPriceTag>().UpdatePriceTag(__instance.item4Price);
            }
            return false;
        }

        //Access base method
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(SpellElement), nameof(SpellElement.CastSpell))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool CastSpellDummy(MetronomeSpell instance) { return false; }
        //Patch: Metronome now overrides the triggered spell's cost
        //Also removes Metronome mana refund
        [HarmonyPrefix, HarmonyPatch(typeof(MetronomeSpell), "CastSpell")]
        static bool MetronomeSpell_CastSpell(MetronomeSpell __instance, ref bool __result)
        {
            if (!CastSpellDummy(__instance))
            {
                __result = false;
                return false;
            }
            __instance.app.model.costRefundOverride = true;
            __instance.app.model.costRefundAmount = new short[6];

            if (!WindfallPersistentDataController.LoadData().implementBalanceChanges)
            {
                __instance.app.model.costRefundAmount = __instance.Cost;
            }

            SpellName spellName = (SpellName)UnityEngine.Random.Range(1, 139);
            while (spellName == SpellName.Metronome || !__instance.app.model.spellModel.spells.ContainsKey(spellName))
            {
                spellName = (SpellName)UnityEngine.Random.Range(1, 139);
            }
            MonoBehaviour.print("Metronome is casting " + spellName.ToString());
            __instance.app.model.spellModel.currentSpell = __instance.app.model.spellModel.spells[spellName];
            __instance.app.model.spellModel.spellQueued = true;

            //Cost override
            __instance.app.model.spellModel.currentSpell.CostOverride = true;

            __instance.app.model.spellModel.currentSpell.CastSpell();
            SoundsView.Instance.PlaySound(SoundsView.eSound.Spell_DamageUp, SoundsView.eAudioSlot.Default, false);
            __result = true;

            return false;
        }

        //Patch: Removes fake trinket when a fake trinket is replaced
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketChosenToReplaceEvent), "Execute")]
        static bool TrinketChosenToReplaceEvent_Execute(TrinketChosenToReplaceEvent __instance, int ___trinketIndex)
        {
            if (___trinketIndex >= 0 && __instance.app.model.trinketIsFake[___trinketIndex])
            {
                //Remove fake trinket
                __instance.app.model.fakeTrinkets[___trinketIndex] = null;
                __instance.app.model.trinketIsFake[___trinketIndex] = false;
            }
            return true;
        }
        //Patch: Glitch now displays its number of uses when acting as an activated trinket
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "UpdateTrinkets")]
        static void BumboController_UpdateTrinkets(BumboController __instance)
        {
            for (short num = 0; num < 4; num += 1)
            {
                if ((int)num < __instance.app.model.characterSheet.trinkets.Count)
                {
                    TrinketElement trinket = __instance.GetTrinket((int)num);
                    __instance.app.view.GUICamera.GetComponent<GUISide>().trinketUses[(int)num].SetActive(trinket.Category == TrinketElement.TrinketCategory.Use);
                    if (trinket.Category == TrinketElement.TrinketCategory.Use)
                    {
                        __instance.app.view.GUICamera.GetComponent<GUISide>().trinketUsesCount[(int)num].text = trinket.uses + string.Empty;
                    }
                }
            }
        }

        //Patch: Repositions fake trinkets when 1up! is triggered
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "InsteadOfDeath")]
        public static void TrinketController_InsteadOfDeath(TrinketController __instance, ref bool __result)
        {
            short num = 0;
            while ((int)num < __instance.app.model.characterSheet.trinkets.Count)
            {
                if (__instance.app.controller.GetTrinket((int)num).InsteadOfDeath())
                {
                    //Check whether trinket is fake 
                    if (__instance.app.model.trinketIsFake[num])
                    {
                        //Remove fake trinket
                        __instance.app.model.fakeTrinkets[num] = null;
                        __instance.app.model.trinketIsFake[num] = false;
                    }

                    //Reposition trinkets in fake trinket array
                    for (int i = num + 1; i < __instance.app.model.fakeTrinkets.Length; i++)
                    {
                        if (__instance.app.model.fakeTrinkets[i] != null)
                        {
                            //Copy fake trinket to new position
                            __instance.app.model.fakeTrinkets[i - 1] = __instance.app.model.fakeTrinkets[i];
                            __instance.app.model.trinketIsFake[i - 1] = true;
                            //Clear fake trinket from old position
                            __instance.app.model.fakeTrinkets[i] = null;
                            __instance.app.model.trinketIsFake[i] = false;
                        }
                    }

                    __instance.app.model.characterSheet.trinkets.RemoveAt((int)num);
                    __instance.app.controller.UpdateTrinkets();
                    __result = true;
                    return;
                }
                num += 1;
            }
            __result = false;
            return;
        }

        //Patch: Removes fake trinkets when they are used while acting as activated trinkets
        public static void UseTrinket_Use_Prefix(UseTrinket __instance, int _index)
        {
            //Check whether trinket will run out of uses
            if (__instance.uses <= 1)
            {
                //Check whether trinket is fake 
                if (__instance.app.model.trinketIsFake[_index])
                {
                    //Remove fake trinket
                    __instance.app.model.fakeTrinkets[_index] = null;
                    __instance.app.model.trinketIsFake[_index] = false;
                }

                //Reposition trinkets in fake trinket array
                for (int i = _index + 1; i < __instance.app.model.fakeTrinkets.Length; i++)
                {
                    if (__instance.app.model.fakeTrinkets[i] != null)
                    {
                        //Copy fake trinket to new position
                        __instance.app.model.fakeTrinkets[i - 1] = __instance.app.model.fakeTrinkets[i];
                        __instance.app.model.trinketIsFake[i - 1] = true;
                        //Clear fake trinket from old position
                        __instance.app.model.fakeTrinkets[i] = null;
                        __instance.app.model.trinketIsFake[i] = false;
                    }
                }
            }
        }
        public static void UseTrinket_Use_Base_Method(UseTrinket __instance, int _index)
        {
            __instance.uses--;
            if (__instance.uses <= 0)
            {
                __instance.app.model.characterSheet.trinkets.RemoveAt(_index);
            }
            __instance.app.controller.UpdateTrinkets();
        }
        public static void UseTrinket_Use_Postfix(UseTrinket __instance)
        {
            //Disabled for Boom and Death, since they manually set a new event when used
            if (__instance.trinketName != TrinketName.Boom && __instance.trinketName != TrinketName.Death)
            {
                __instance.app.controller.eventsController.SetEvent(new IdleEvent());
            }
        }
        //Patch: Removes fake trinket when a fake trinket is used
        //Also repositions fake trinkets when a trinket expires
        [HarmonyPrefix, HarmonyPatch(typeof(UseTrinket), "Use")]
        static bool UseTrinket_Use(UseTrinket __instance, int _index)
        {
            UseTrinket_Use_Prefix(__instance, _index);
            return true;
        }
        //Patch: Removes fake trinket when a fake Modeling Clay is used (Modeling Clay does not call the base use method and must be changed separately)
        [HarmonyPrefix, HarmonyPatch(typeof(ModelingClayTrinket), "Use")]
        static bool ModelingClayTrinket_Use(ModelingClayTrinket __instance, int _index)
        {
            UseTrinket_Use_Prefix(__instance, _index);
            return true;
        }
        //Patch: Fixes Experimental giving red heart containers to Bum-bo the Dead and Bum-bo the Lost
        [HarmonyPrefix, HarmonyPatch(typeof(ExperimentalTrinket), "Use")]
        static bool ExperimentalTrinket_Use(ExperimentalTrinket __instance, int _index)
        {
            //Base use method
            UseTrinket_Use_Prefix(__instance, _index);

            UseTrinket_Use_Base_Method(__instance, _index);

            UseTrinket_Use_Postfix(__instance);

            //Experimental use method
            int num;
            if (__instance.app.model.characterSheet.bumboType.ToString() == "TheLost")
            {
                num = UnityEngine.Random.Range(0, 4);
                //Prevent Experimental from granting red heart containers to Bum-bo the Lost
            }
            else
            {
                num = UnityEngine.Random.Range(0, 5);
            }
            switch (num)
            {
                case 1:
                    __instance.app.model.characterSheet.addPuzzleDamage(1);
                    __instance.app.controller.GUINotification("GAINED_PUZZLE_DAMAGE", GUINotificationView.NotifyType.Stats, null, true);
                    break;
                case 2:
                    __instance.app.model.characterSheet.addItemDamage(1);
                    __instance.app.controller.GUINotification("GAINED_ITEM_DAMAGE", GUINotificationView.NotifyType.Stats, null, true);
                    break;
                case 3:
                    __instance.app.model.characterSheet.addDex(1);
                    __instance.app.controller.GUINotification("GAINED_DEXTERITY", GUINotificationView.NotifyType.Stats, null, true);
                    break;
                case 4:
                    if (__instance.app.model.characterSheet.bumboType.ToString() == "TheDead")
                    {
                        __instance.app.view.hearts.GetComponent<HealthController>().modifyHealth(0f, 1f);
                        //Prevent Experimental from granting red heart containers to Bum-bo the Dead (grant soul heart instead)
                    }
                    else
                    {
                        __instance.app.model.characterSheet.addHitPoints(1);
                    }
                    __instance.app.controller.GUINotification("GAINED_HP", GUINotificationView.NotifyType.Stats, null, true);
                    break;
                default:
                    __instance.app.model.characterSheet.addLuck(1);
                    __instance.app.controller.GUINotification("GAINED_LUCK", GUINotificationView.NotifyType.Stats, null, true);
                    break;
            }
            __instance.app.controller.AddCurse(1);
            __instance.app.view.hearts.GetComponent<HealthController>().UpdateHearts(true);
            __instance.app.controller.UpdateStats();
            return false;
        }

        //Patch: Fixes ExorcismKit often not healing all enemies
        //Heals 1 health
        [HarmonyPostfix, HarmonyPatch(typeof(ExorcismKitSpell), "HurtAndHeal")]
        static void ExorcismKitSpell_HurtAndHeal(ExorcismKitSpell __instance, ref List<Enemy> enemies_to_heal, ref Enemy enemy_to_hurt)
        {
            for (int enemyCounter = 0; enemyCounter < __instance.app.model.enemies.Count; enemyCounter++)
            {
                if ((__instance.app.model.enemies[enemyCounter].alive || __instance.app.model.enemies[enemyCounter].enemyName == EnemyName.Shit) && !enemies_to_heal.Contains(__instance.app.model.enemies[enemyCounter]) && enemy_to_hurt != __instance.app.model.enemies[enemyCounter])
                {
                    if (WindfallPersistentDataController.LoadData().implementBalanceChanges)
                    {
                        __instance.app.model.enemies[enemyCounter].AddHealth(1f);
                    }
                    else
                    {
                        __instance.app.model.enemies[enemyCounter].AddHealth(2f);
                    }
                }
            }
        }

        //Patch: Randomly removes all but one converter spell from valid spells list each time the list is retrieved; this reduces the chance of encountering Converter during gameplay
        //Also modifies Lost banned spells
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModel), "validSpells", MethodType.Getter)]
        static void SpellModel_validSpells(SpellModel __instance, ref List<SpellName> __result)
        {
            List<SpellName> returnedList = new List<SpellName>(__result);

            //Removing all but one Converter
            List<SpellName> convertersFound = new List<SpellName>();
            for (int spellIndex = 0; spellIndex < __result.Count; spellIndex++)
            {
                if (__result[spellIndex].ToString().Contains("Converter"))
                {
                    convertersFound.Add(__result[spellIndex]);
                }
            }
            while (convertersFound.Count > 1)
            {
                int randomConverterIndex = UnityEngine.Random.Range(0, convertersFound.Count);
                returnedList.Remove(convertersFound[randomConverterIndex]);
                convertersFound.RemoveAt(randomConverterIndex);
            }

            //Changing Lost banned spells
            if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheLost)
            {
                if (WindfallPersistentDataController.LoadData().implementBalanceChanges)
                {
                    SpellName[] bannedSpells = new SpellName[]
                    {
                        //Old
                        SpellName.TheRelic,
                        SpellName.CatPaw,
                        SpellName.PrayerCard /*(broken in original method)*/,

                        //New
                        SpellName.CatHeart,
                        SpellName.Lard,
                        SpellName.RottenMeat,
                        SpellName.Snack,
                        SpellName.MomsLipstick,
                        SpellName.YumHeart
                    };
                    returnedList.RemoveAll((SpellName x) => bannedSpells.Contains(x));
                }
            }

            __result = returnedList;
        }

        //Patch: Modifies Lost banned trinkets
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), "validTrinkets", MethodType.Getter)]
        static void TrinketModel_validTrinkets(TrinketModel __instance, ref List<TrinketName> __result)
        {
            if (!WindfallPersistentDataController.LoadData().implementBalanceChanges)
            {
                return;
            }

            List<TrinketName> returnedList = new List<TrinketName>(__result);

            //Changing Lost banned trinkets
            if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheLost)
            {
                TrinketName[] bannedTrinkets = new TrinketName[]
                {
                //Old
				TrinketName.PinkBow,
                TrinketName.Hierophant,
                TrinketName.RatHeart,
                TrinketName.SantaSangre,
                TrinketName.Feather,

                //New
                TrinketName.DrakulaTeeth,
                TrinketName.Artery,
                TrinketName.Breakfast,
                TrinketName.StemCell,
                TrinketName.TurtleShell,
                TrinketName.Dinner,
                TrinketName.SwallowedPenny
                };
                returnedList.RemoveAll((TrinketName x) => bannedTrinkets.Contains(x));
            }

            __result = returnedList;
        }

        //Patch: Prevents Ecoli from affecting flying enemies that are above a boss, which would cause a softlock
        [HarmonyPrefix, HarmonyPatch(typeof(EcoliSpell), "ChangeEnemy")]
        static bool EcoliSpell_ChangeEnemy(EcoliSpell __instance, ref Enemy _enemy)
        {
            GameObject owner_ground = __instance.app.model.aiModel.battlefieldPositions[__instance.app.model.aiModel.battlefieldPositionIndex[_enemy.position.x, _enemy.position.y]].owner_ground;
            if (owner_ground != null && _enemy.enemyType == Enemy.EnemyType.Flying && owner_ground.GetComponent<Enemy>().boss)
            {
                return false;
            }
            return true;
        }

        //Patch: Fixes a softlock that occurs when attempting to counter a null enemy
        //Also prevents countering dead enemies
        [HarmonyPrefix, HarmonyPatch(typeof(BumboCounterEvent), "Execute")]
        static bool BumboCounterEvent_Execute(BumboCounterEvent __instance)
        {
            if (__instance.app.model.aiModel.attackingEnemies.Count > 0 && (__instance.app.model.aiModel.attackingEnemies[0] == null || __instance.app.model.aiModel.attackingEnemies[0].health <= 0f))
            {
                //Null check has failed. End event
                __instance.End();
                return false;
            }
            return true;
        }

        //Patch: Fixes Mega Bean forcefully ending the player's turn
        [HarmonyPatch(typeof(MegaBeanSpell), "AttackAnimation")]
        class MegaBeanSpell_End_Event_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var code = new List<CodeInstruction>(instructions);
                for (int startIndex = 1; startIndex < code.Count - 1; startIndex++)
                {
                    if (code[startIndex].opcode == OpCodes.Call && code[startIndex - 1].opcode == OpCodes.Blt)
                    {
                        for (int endIndex = startIndex; endIndex < code.Count; endIndex++)
                        {
                            if (code[endIndex].opcode == OpCodes.Ret)
                            {
                                code.RemoveRange(startIndex + 1, endIndex - startIndex - 1);
                                return code;
                            }
                        }
                    }
                }
                return code;
            }
        }

        //Patch: Fixes flush sometimes trying to damage enemies that are already dead, which can cause a softlock
        [HarmonyPrefix, HarmonyPatch(typeof(FlushSpell), "DoYourWorst")]
        static bool FlushSpell_DoYourWorst(FlushSpell __instance)
        {
            List<Enemy> list = new List<Enemy>();
            list.AddRange(__instance.app.model.enemies);
            short num = 0;
            while (num < list.Count)
            {
                if (list[num] != null && (list[num].GetComponent<Enemy>().alive || list[num].GetComponent<Enemy>().isPoop))
                {
                    if (list[num].enemyName == EnemyName.Shit)
                    {
                        list[num].timeToDie();
                    }
                    else if (list[num].enemyName != EnemyName.Stone)
                    {
                        list[num].GetComponent<Enemy>().Hurt(__instance.Damage(), Enemy.AttackImmunity.ReduceSpellDamage, __instance.statusEffects, -1);
                    }
                }
                num += 1;
            }
            return false;
        }

        //***************************************************
        //*****************Price Tag fix*********************
        //***************************************************
        //These patches prevent spell mana cost reroll (from Bag-O-Trash/Bum-bo the Dead) when using the Price Tag spell, fixing a bug that causes the wrong spell slot to have its mana cost rerolled

        //Patch: Sets last used spell to null when using Price Tag
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "RemoveSpell")]
        static void BumboController_RemoveSpell(BumboController __instance)
        {
            __instance.app.model.spellViewUsed = null;
        }

        //Patch: Prevents spell mana cost reroll (from Bag-O-Trash/Bum-bo the Dead) if the last used spell is null
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "SpellAfterEffects")]
        static bool BumboController_SpellAfterEffects(BumboController __instance)
        {
            if (__instance.app.model.spellViewUsed != null)
            {
                return true;
            }
            __instance.app.model.iAmSpelling = false;
            __instance.app.controller.trinketController.RechargeOnSpell();

            return false;
        }
        //***************************************************
        //***************************************************
        //***************************************************

        private static readonly string flyLaneIteratorKey = "flyLaneIterator";
        //Patch: Reworks Attack Fly Spell sequence to account for changes to enemy states between fly attacks
        [HarmonyPrefix, HarmonyPatch(typeof(AttackFlySpell), "EndOfMonsterRound")]
        static bool AttackFlySpell_EndOfMonsterRound(AttackFlySpell __instance, bool[] ___enemies_to_bother)
        {
            GameObject fly = __instance.app.view.spellAttackView.attackFly;
            Sequence sequence = DOTween.Sequence();
            bool isFinalLane = false;
            int currentLane = -1;

            if (!ObjectDataStorage.HasData<int>(__instance, flyLaneIteratorKey))
            {
                ObjectDataStorage.StoreData<int>(__instance, flyLaneIteratorKey, 0);
            }

            for (int laneCounter = 0; laneCounter < 3; laneCounter++)
            {
                if (ObjectDataStorage.GetData<int>(__instance, flyLaneIteratorKey) == laneCounter)
                {
                    if (___enemies_to_bother[laneCounter])
                    {
                        currentLane = laneCounter;
                    }
                    break;
                }
            }
            
            ObjectDataStorage.StoreData<int>(__instance, flyLaneIteratorKey, ObjectDataStorage.GetData<int>(__instance, flyLaneIteratorKey) + 1);

            if (ObjectDataStorage.GetData<int>(__instance, flyLaneIteratorKey) > 2)
            {
                ObjectDataStorage.StoreData<int>(__instance, flyLaneIteratorKey, 0);
                isFinalLane = true;
            }

            //Lane 0
            if (currentLane != -1)
            {
                Transform transform = __instance.app.controller.ClosestEnemy(currentLane);
                if (transform != null && (transform.GetComponent<Enemy>().alive || transform.GetComponent<Enemy>().enemyName == EnemyName.Shit))
                {
                    Enemy enemy = transform.GetComponent<Enemy>();
                    TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.AppendCallback(sequence, delegate ()
                    {
                        //Fly is reset manually because ResetFly method is private
                        fly.SetActive(true);
                        float num = (enemy.enemyType != Enemy.EnemyType.Ground) ? 1f : 0.25f;
                        fly.transform.position = new Vector3((currentLane - 1f) * 1.25f, num, -6f);
                    }), 0.1f), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveZ(fly.transform, enemy.transform.position.z - 0.25f, 0.25f, false), Ease.InQuad)), delegate ()
                    {
                        //Enemy is hurt manually because HurtEnemies method is private
                        enemy.Hurt(1f, Enemy.AttackImmunity.ReduceSpellDamage, null, enemy.position.x);
                    }), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveZ(fly.transform, -6f, 0.5f, false), Ease.OutQuad));
                }
            }
            TweenSettingsExtensions.OnComplete<Sequence>(sequence, delegate ()
            {
                if (!isFinalLane)
                {
                    __instance.EndOfMonsterRound();
                }
                else
                {
                    fly.SetActive(false);
                    __instance.app.controller.eventsController.EndEvent();
                }
            });
            return false;
        }

        //Patch: Prevents Fish Hook from granting red mana
        [HarmonyPrefix, HarmonyPatch(typeof(FishHookSpell), "Reward")]
        static bool FishHookSpell_Reward(FishHookSpell __instance)
        {
            short[] array = new short[6];
            int num = UnityEngine.Random.Range(0, 5);
            if (num > 0)
            {
                num++;
            }
            array[num] = 1;
            __instance.app.view.stolenManaView.gameObject.SetActive(true);
            __instance.app.view.stolenManaView.SetManaStolen(1, (Block.BlockType)num);
            __instance.app.controller.UpdateMana(array, false);
            __instance.app.controller.ShowManaGain();
            __instance.app.Notify("reward.spell", null, new object[0]);

            return false;
        }

        //Patch: Paper Straw now interacts with ghost tiles
        [HarmonyPrefix, HarmonyPatch(typeof(PaperStrawSpell), "CastSpell")]
        static bool PaperStrawSpell_CastSpell(PaperStrawSpell __instance)
        {
            if (!__instance.CostOverride)
            {
                for (short i = 0; i < 6; i += 1)
                {
                    if (__instance.Cost[(int)i] + __instance.CostModifier[(int)i] > __instance.app.model.mana[(int)i])
                    {
                        return true;
                    }
                }
            }
            List<int> list = new List<int>
            {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            };
            for (int i = 0; i < __instance.app.view.puzzle.width; i++)
            {
                for (int j = 0; j < __instance.app.view.puzzle.height; j++)
                {
                    List<int> list2;
                    int block_type;
                    (list2 = list)[block_type = (int)__instance.app.view.puzzle.blocks[i, j].GetComponent<Block>().block_type] = list2[block_type] + 1;
                }
            }
            int num = 0;
            short num2 = 0;
            for (int k = 0; k < list.Count; k++)
            {
                if (list[k] > (int)num2)
                {
                    num = k;
                    num2 = (short)list[k];
                }
            }
            if (num == 8)
            {
                for (int l = 0; l < __instance.app.model.characterSheet.spells.Count; l++)
                {
                    __instance.app.model.characterSheet.spells[l].ChargeSpell();
                    __instance.app.controller.eventsController.SetEvent(new IdleEvent());
                }
            }
            return true;
        }

        //Changing all use trinkets such that they can be used on turn end
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketElement), "CanBeUsedOnTurnEnd", MethodType.Getter)]
        public static void TrinketElement_CanBeUsedOnTurnEnd_Getter(TrinketElement __instance, ref bool __result)
        {
            if (__instance.Category == TrinketElement.TrinketCategory.Use)
            {
                __result = true;
            }
        }

        //Patch: Causes the player's turn to end automatically after using an activated trinket while out of moves if they can't do anything
        [HarmonyPostfix, HarmonyPatch(typeof(UseTrinket), "Use")]
        static void UseTrinket_Use(UseTrinket __instance)
        {
            UseTrinket_Use_Postfix(__instance);
        }

        //Patch: Causes the player's turn to end automatically after using Modeling Clay while out of moves if they can't do anything (unlike other use trinkets, Modeling Clay doesn't call the base Use method)
        [HarmonyPostfix, HarmonyPatch(typeof(ModelingClayTrinket), "Use")]
        static void ModelingClayTrinket_Use(ModelingClayTrinket __instance)
        {
            UseTrinket_Use_Postfix(__instance);
        }

        //Patch: Prevents additional Lose Move notification from spawning when using Lard
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "ReduceActionPoints")]
        static bool BumboController_LoseActionPoints(BumboController __instance)
        {
            BumboModel model = __instance.app.model;
            model.actionPointModifier -= 1;

            SoundsView.Instance.PlaySound(SoundsView.eSound.Spell_MovementDrain, SoundsView.eAudioSlot.Default, false);

            return false;
        }

        //Patch: Fixes the player's stats not updating immediately after using The Devil
        [HarmonyPostfix, HarmonyPatch(typeof(TheDevilTrinket), "Use")]
        static void TheDevilTrinket_Use(TheDevilTrinket __instance)
        {
            __instance.app.controller.UpdateStats();
        }

        //***************************************************
        //**********Spell Retrieval Performance**************
        //***************************************************
        //Patch: Improves performance of Empty Hidden Trinket
        //Also fixes Empty Hidden Trinket activating a second time when reloading a save
        [HarmonyPrefix, HarmonyPatch(typeof(EmptyHiddenTrinket), "StartRoom")]
        static bool EmptyHiddenTrinket_StartRoom(EmptyHiddenTrinket __instance)
        {
            if (__instance.app.controller.savedStateController.IsLoading()) //Save is loading
            {
                return false;
            }

            List<List<SpellName>> list = new List<List<SpellName>>();
            //Replace validSpells spell categorization with FastSpellRetrieval spell categorization
            list.Add(new List<SpellName>(FastSpellRetrieval.AttackSpells));
            list.Add(new List<SpellName>(FastSpellRetrieval.DefenseSpells));
            list.Add(new List<SpellName>(FastSpellRetrieval.PuzzleSpells));
            list.Add(new List<SpellName>(FastSpellRetrieval.UseSpells));
            list.Add(new List<SpellName>(FastSpellRetrieval.OtherSpells));

            //Remove non valid spells
            foreach (List<SpellName> spellList in list)
            {
                spellList.RemoveAll((SpellName x) => !__instance.app.model.spellModel.validSpells.Contains(x));
            }

            List<SpellElement.SpellCategory> list2 = new List<SpellElement.SpellCategory>();
            for (int j = 0; j < __instance.app.model.characterSheet.spells.Count; j++)
            {
                list2.Add(__instance.app.model.characterSheet.spells[j].Category);
            }

            __instance.app.model.characterSheet.spells.Clear();

            for (int l = 0; l < list2.Count; l++)
            {
                SpellElement spellElement = __instance.app.model.spellModel.spells[list[list2[l] - SpellElement.SpellCategory.Attack][UnityEngine.Random.Range(0, list[list2[l] - SpellElement.SpellCategory.Attack].Count)]];
                spellElement = __instance.app.controller.SetSpellCost(spellElement);
                __instance.app.model.characterSheet.spells.Add(spellElement);
                __instance.app.controller.SetSpell(l, __instance.app.model.characterSheet.spells[l]);
            }

            return false;
        }

        //Patch: Improves performance of Rainbow Bag
        //Also fixes Rainbow Bag activating a second time when reloading a save
        [HarmonyPrefix, HarmonyPatch(typeof(RainbowBagTrinket), "StartRoom")]
        static bool RainbowBagTrinket_StartRoom(RainbowBagTrinket __instance)
        {
            if (__instance.app.controller.savedStateController.IsLoading()) //Save is loading
            {
                return false;
            }

            List<List<SpellName>> list = new List<List<SpellName>>();
            //Replace validSpells spell categorization with FastSpellRetrieval spell categorization
            list.Add(new List<SpellName>(FastSpellRetrieval.AttackSpells));
            list.Add(new List<SpellName>(FastSpellRetrieval.DefenseSpells));
            list.Add(new List<SpellName>(FastSpellRetrieval.PuzzleSpells));
            list.Add(new List<SpellName>(FastSpellRetrieval.UseSpells));
            list.Add(new List<SpellName>(FastSpellRetrieval.OtherSpells));

            //Remove non valid spells
            foreach (List<SpellName> spellList in list)
            {
                spellList.RemoveAll((SpellName x) => !__instance.app.model.spellModel.validSpells.Contains(x));
            }

            List<SpellElement.SpellCategory> list2 = new List<SpellElement.SpellCategory>();
            for (int j = 0; j < __instance.app.model.characterSheet.spells.Count; j++)
            {
                list2.Add(__instance.app.model.characterSheet.spells[j].Category);
            }

            __instance.app.model.characterSheet.spells.Clear();

            for (int l = 0; l < list2.Count; l++)
            {
                SpellElement spellElement = __instance.app.model.spellModel.spells[list[(int)list2[l] - 1][UnityEngine.Random.Range(0, list[(int)list2[l] - 1].Count)]];
                spellElement = __instance.app.controller.SetSpellCost(spellElement);
                __instance.app.model.characterSheet.spells.Add(spellElement);
                __instance.app.controller.SetSpell(l, __instance.app.model.characterSheet.spells[l]);
            }

            return false;
        }

        //Patch: Improves performance of BumboController SpellsFromCategory (improves spell pickup spawning efficiency)
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "SpellsFromCategory")]
        static bool BumboController_SpellsFromCategory(BumboController __instance, SpellElement.SpellCategory _category, ref List<SpellName> __result)
        {
            //Add all valid spells
            List<SpellName> returnSpellsFromCategory = new List<SpellName>();
            returnSpellsFromCategory.AddRange(__instance.app.model.spellModel.validSpells);

            //Replace validSpells spell categorization with FastSpellRetrieval spell categorization
            List<SpellName> retrievedSpellsFromCategory = new List<SpellName>();
            switch (_category)
            {
                case SpellElement.SpellCategory.Attack:
                    retrievedSpellsFromCategory = new List<SpellName>(FastSpellRetrieval.AttackSpells);
                    break;
                case SpellElement.SpellCategory.Defense:
                    retrievedSpellsFromCategory = new List<SpellName>(FastSpellRetrieval.DefenseSpells);
                    break;
                case SpellElement.SpellCategory.Puzzle:
                    retrievedSpellsFromCategory = new List<SpellName>(FastSpellRetrieval.PuzzleSpells);
                    break;
                case SpellElement.SpellCategory.Use:
                    retrievedSpellsFromCategory = new List<SpellName>(FastSpellRetrieval.UseSpells);
                    break;
                case SpellElement.SpellCategory.Other:
                    retrievedSpellsFromCategory = new List<SpellName>(FastSpellRetrieval.OtherSpells);
                    break;
            }

            //Remove spells from the wrong categories
            returnSpellsFromCategory.RemoveAll((SpellName x) => !retrievedSpellsFromCategory.Contains(x));

            __result = returnSpellsFromCategory;
            return false;
        }
        //***************************************************
        //***************************************************
        //***************************************************
    }
}

static class FastSpellRetrieval
{
    public static SpellElement.SpellCategory GetSpellCategory(SpellName spell)
    {
        //Gets category of a given spell name (unused)
        SpellElement.SpellCategory category = SpellElement.SpellCategory.None;
        switch (spell)
        {
            case (SpellName)1000:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Addy:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.AttackFly:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Backstabber:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.BarbedWire:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.BeckoningFinger:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.BeeButt:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.BigRock:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.BigSlurp:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.BlackCandle:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.BlackD12:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.BlenderBlade:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.BlindRage:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.BloodRights:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.BorfBucket:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Box:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.Brimstone:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.BrownBelt:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.BumboShake:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.BumboSmash:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.ButterBean:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.BuzzDown:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.BuzzRight:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.BuzzUp:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.CatHeart:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.CatPaw:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.Chaos:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.CoinRoll:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.ConverterBrown:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.ConverterGreen:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.ConverterGrey:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.ConverterWhite:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.ConverterYellow:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.CraftPaper:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.CrazyStraw:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.CursedRainbow:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.D10:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.D20:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.D4:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.D6:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.D8:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.DarkLotus:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.DeadDove:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.DogTooth:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Ecoli:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Eraser:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Euthanasia:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.ExorcismKit:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.FishHook:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.FlashBulb:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.Flip:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Flush:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.GoldenTick:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.HairBall:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.HatPin:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Juiced:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.KrampusCross:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Lard:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.LeakyBattery:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.Lemon:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Libra:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.LilRock:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.LithiumBattery:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.LooseChange:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.LuckyFoot:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.Magic8Ball:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.MagicMarker:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Mallot:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.MamaFoot:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.MamaShoe:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.MeatHook:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.MegaBattery:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.MegaBean:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Melatonin:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.Metronome:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.MirrorMirror:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.MissingPiece:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.MomsLipstick:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.MomsPad:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.MsBang:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Mushroom:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.NailBoard:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.NavyBean:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Needle:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Number1:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.OldPillow:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.OrangeBelt:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.PaperStraw:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Pause:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.Peace:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.Pentagram:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.Pepper:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.PintoBean:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.Pliers:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.PotatoMasher:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.PrayerCard:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.PriceTag:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.PuzzleFlick:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.Quake:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.RainbowFinger:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.RainbowFlag:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.RedD12:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Refresh:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.Rock:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.RockFriends:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.RoidRage:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.RottenMeat:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.RubberBat:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.SilverChip:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.Skewer:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.SleightOfHand:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.SmokeMachine:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.Snack:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.SnotRocket:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.Stick:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.StopWatch:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.Teleport:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.TheNegative:
                category = SpellElement.SpellCategory.Attack;
                break;
            case SpellName.ThePoop:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.TheRelic:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.TheVirus:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.TimeWalker:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.TinyDice:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.Toothpick:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.TracePaper:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.TrapDoor:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.TrashLid:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.TwentyLbsWeight:
                category = SpellElement.SpellCategory.Puzzle;
                break;
            case SpellName.TwentyTwenty:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.WatchBattery:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.WhiteBelt:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.WoodenNickel:
                category = SpellElement.SpellCategory.Use;
                break;
            case SpellName.WoodenSpoon:
                category = SpellElement.SpellCategory.Other;
                break;
            case SpellName.YellowBelt:
                category = SpellElement.SpellCategory.Defense;
                break;
            case SpellName.YumHeart:
                category = SpellElement.SpellCategory.Use;
                break;
        }
        return category;
    }

    public static List<SpellName> AttackSpells
    {
        get
        {
            List<SpellName> list = new List<SpellName>
            {
                (SpellName)1000,
                SpellName.AttackFly,
                SpellName.Backstabber,
                SpellName.BeeButt,
                SpellName.BigRock,
                SpellName.BorfBucket,
                SpellName.Brimstone,
                SpellName.BumboSmash,
                SpellName.DogTooth,
                SpellName.Ecoli,
                SpellName.ExorcismKit,
                SpellName.FishHook,
                SpellName.Flush,
                SpellName.HairBall,
                SpellName.HatPin,
                SpellName.Lemon,
                SpellName.LilRock,
                SpellName.MamaFoot,
                SpellName.MamaShoe,
                SpellName.MeatHook,
                SpellName.MegaBean,
                SpellName.NailBoard,
                SpellName.Needle,
                SpellName.Number1,
                SpellName.Pliers,
                SpellName.PuzzleFlick,
                SpellName.Rock,
                SpellName.RockFriends,
                SpellName.RubberBat,
                SpellName.Stick,
                SpellName.TheNegative
            };
            return list;
        }
    }

    public static List<SpellName> DefenseSpells
    {
        get
        {
            List<SpellName> list = new List<SpellName>
            {
                SpellName.BarbedWire,
                SpellName.BeckoningFinger,
                SpellName.BrownBelt,
                SpellName.CatHeart,
                SpellName.Euthanasia,
                SpellName.FlashBulb,
                SpellName.Lard,
                SpellName.Melatonin,
                SpellName.MomsPad,
                SpellName.OldPillow,
                SpellName.OrangeBelt,
                SpellName.Peace,
                SpellName.Pepper,
                SpellName.PintoBean,
                SpellName.PrayerCard,
                SpellName.RottenMeat,
                SpellName.SmokeMachine,
                SpellName.Snack,
                SpellName.SnotRocket,
                SpellName.StopWatch,
                SpellName.TheVirus,
                SpellName.TrashLid,
                SpellName.WhiteBelt,
                SpellName.YellowBelt
            };
            return list;
        }
    }

    public static List<SpellName> PuzzleSpells
    {
        get
        {
            List<SpellName> list = new List<SpellName>
            {
                SpellName.TwentyLbsWeight,
                SpellName.Magic8Ball,
                SpellName.BlackD12,
                SpellName.BlenderBlade,
                SpellName.BumboShake,
                SpellName.BuzzDown,
                SpellName.BuzzRight,
                SpellName.BuzzUp,
                SpellName.Chaos,
                SpellName.CursedRainbow,
                SpellName.DeadDove,
                SpellName.Eraser,
                SpellName.Flip,
                SpellName.KrampusCross,
                SpellName.MagicMarker,
                SpellName.Mallot,
                SpellName.MirrorMirror,
                SpellName.MomsLipstick,
                SpellName.MsBang,
                SpellName.NavyBean,
                SpellName.PaperStraw,
                SpellName.PotatoMasher,
                SpellName.RainbowFinger,
                SpellName.RainbowFlag,
                SpellName.RedD12,
                SpellName.Skewer,
                SpellName.TinyDice,
                SpellName.Toothpick
            };
            return list;
        }
    }
    public static List<SpellName> UseSpells
    {
        get
        {
            List<SpellName> list = new List<SpellName>
            {
                SpellName.ButterBean,
                SpellName.CatPaw,
                SpellName.CraftPaper,
                SpellName.D10,
                SpellName.D20,
                SpellName.D4,
                SpellName.D6,
                SpellName.D8,
                SpellName.DarkLotus,
                SpellName.GoldenTick,
                SpellName.LeakyBattery,
                SpellName.LithiumBattery,
                SpellName.LooseChange,
                SpellName.MegaBattery,
                SpellName.Mushroom,
                SpellName.Pause,
                SpellName.PriceTag,
                SpellName.Quake,
                SpellName.SilverChip,
                SpellName.Teleport,
                SpellName.ThePoop,
                SpellName.TheRelic,
                SpellName.TracePaper,
                SpellName.TrapDoor,
                SpellName.WatchBattery,
                SpellName.WoodenNickel,
                SpellName.YumHeart
            };
            return list;
        }
    }
    public static List<SpellName> OtherSpells
    {
        get
        {
            List<SpellName> list = new List<SpellName>
            {
                SpellName.TwentyTwenty,
                SpellName.Addy,
                SpellName.BigSlurp,
                SpellName.BlackCandle,
                SpellName.BlindRage,
                SpellName.BloodRights,
                SpellName.Box,
                SpellName.CoinRoll,
                SpellName.ConverterBrown,
                SpellName.ConverterGreen,
                SpellName.ConverterGrey,
                SpellName.ConverterWhite,
                SpellName.ConverterYellow,
                SpellName.CrazyStraw,
                SpellName.Juiced,
                SpellName.Libra,
                SpellName.LuckyFoot,
                SpellName.Metronome,
                SpellName.MissingPiece,
                SpellName.Pentagram,
                SpellName.Refresh,
                SpellName.RoidRage,
                SpellName.SleightOfHand,
                SpellName.TimeWalker,
                SpellName.WoodenSpoon
            };
            return list;
        }
    }
}