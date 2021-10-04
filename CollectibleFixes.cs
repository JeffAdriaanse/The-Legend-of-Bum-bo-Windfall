using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using DG.Tweening;

namespace The_Legend_of_Bum_bo_Windfall
{
    class CollectibleFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(CollectibleFixes));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying collectible related bug fixes");
        }

        //Patch: Fixes ExorcismKit often not healing all enemies
        [HarmonyPostfix, HarmonyPatch(typeof(ExorcismKitSpell), "HurtAndHeal")]
        static void ExorcismKitSpell_HurtAndHeal(ExorcismKitSpell __instance, ref List<Enemy> enemies_to_heal, ref Enemy enemy_to_hurt)
        {
            for (int enemyCounter = 0; enemyCounter < __instance.app.model.enemies.Count; enemyCounter++)
            {
                if ((__instance.app.model.enemies[enemyCounter].alive || __instance.app.model.enemies[enemyCounter].enemyName == EnemyName.Shit) && !enemies_to_heal.Contains(__instance.app.model.enemies[enemyCounter]) && enemy_to_hurt != __instance.app.model.enemies[enemyCounter])
                {
                    __instance.app.model.enemies[enemyCounter].AddHealth(2f);
                    Console.WriteLine("[The Legend of Bum-bo: Windfall] Healing " + __instance.app.model.enemies[enemyCounter].enemyName + "; it should have been healed by ExorcismKit, but was skipped over");
                }
            }
        }

        //Patch: Randomly removes all but one converter spell from valid spells list each time the list is retrieved; this reduces the chance of encountering Converter during gameplay
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModel), "validSpells", MethodType.Getter)]
        static void SpellModel_validSpells(ref List<SpellName> __result)
        {
            List<SpellName> returnedList = new List<SpellName>(__result);

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

            __result = returnedList;
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Removing all but one Converter spell from valid spells list");
        }

        //Patch: Prevents Ecoli from affecting flying enemies that are above a boss, which would cause a softlock
        [HarmonyPrefix, HarmonyPatch(typeof(EcoliSpell), "ChangeEnemy")]
        static bool EcoliSpell_ChangeEnemy(EcoliSpell __instance, ref Enemy _enemy)
        {
            GameObject owner_ground = __instance.app.model.aiModel.battlefieldPositions[__instance.app.model.aiModel.battlefieldPositionIndex[_enemy.position.x, _enemy.position.y]].owner_ground;
            if (owner_ground != null && _enemy.enemyType == Enemy.EnemyType.Flying && owner_ground.GetComponent<Enemy>().boss)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Aborting Ecoli spell effect; enemy is above a boss");
                return false;
            }
            return true;
        }

        //Patch: Fixes canceling spells with temporarily reduced mana costs refunding more mana than they cost
        //Patch also counteracts reduction in mana refund from Sucker enemies
        [HarmonyPrefix, HarmonyPatch(typeof(EventsController), "OnNotification")]
        static bool EventsController_OnNotification(EventsController __instance, ref string _event_path)
        {
            if (_event_path == "cancel.spell"
                && !__instance.app.model.costRefundOverride
                && !(__instance.app.model.bumboEvent.GetType().ToString() == "TreasureStartEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "GrabBossRewardEvent")
                && !(__instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceEvent" && __instance.app.view.gamblingView != null)
                && !(__instance.app.view.gamblingView != null)
                && !(__instance.app.view.boxes.treasureRoom != null && __instance.app.view.boxes.treasureRoom.gameObject.activeSelf)
                && !(__instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceEvent"))
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing mana cost refund");

                //**********Step 1: Counteract mana loss from base method**********

                //Get number of Sucker enemies
                int SuckerCount = 0;
                for (int i = 0; i < __instance.app.model.enemies.Count; i++)
                {
                    if (__instance.app.model.enemies[i].enemyName == EnemyName.Sucker)
                    {
                        SuckerCount++;
                    }
                }

                //Get amount of each mana type that is lost from Suckers and add it directly to player mana
                for (int i = 0; i < 6; i++)
                {
                    __instance.app.model.mana[i] += (short)Mathf.Max(Mathf.Min(SuckerCount, __instance.app.model.spellModel.currentSpell.Cost[i] - 1), 0);
                }

                //**********Step 2: Reduce mana gain by incorporating cost modifiers**********

                //Get amount of each mana type that is not lost from cost modifier (these values will be negative) and directly remove it from player mana
                for (int i = 0; i < 6; i++)
                {
                    __instance.app.model.mana[i] += __instance.app.model.spellModel.currentSpell.CostModifier[i];
                }
            }
            return true;
        }

        //Patch: Fixes a softlock that occurs when attempting to counter a null enemy
        [HarmonyPrefix, HarmonyPatch(typeof(BumboCounterEvent), "Execute")]
        static bool BumboCounterEvent_Execute(BumboCounterEvent __instance)
        {
            if (__instance.app.model.aiModel.attackingEnemies.Count > 0 && (__instance.app.model.aiModel.attackingEnemies[0] == null || __instance.app.model.aiModel.attackingEnemies[0].health < 0f))
            {
                //Null check has failed. End event
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Progressing to next event when counter damage attempts to target a null enemy");
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
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Reworking Flush to prevent it from targeting null enemies");
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
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing last used spell to null when removing spell");
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

            Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing spell mana cost reroll since last used spell is null");
            return false;
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Reworks Attack Fly Spell sequence to account for changes to enemy states between fly attacks
        [HarmonyPrefix, HarmonyPatch(typeof(AttackFlySpell), "EndOfMonsterRound")]
        static bool AttackFlySpell_EndOfMonsterRound(AttackFlySpell __instance, bool[] ___enemies_to_bother)
        {
            GameObject flyCounter;
            if (GameObject.Find("Attack Fly Counter"))
            {
                flyCounter = GameObject.Find("Attack Fly Counter");
            }
            else
            {
                flyCounter = new GameObject("Attack Fly Counter");
                flyCounter.transform.position = new Vector3(0, 0, 0);
            }
            GameObject fly = __instance.app.view.spellAttackView.attackFly;
            Sequence sequence = DOTween.Sequence();
            bool isFinalLane = false;
            int currentLane = -1;
            for (int laneCounter = 0; laneCounter < 3; laneCounter++)
            {
                if (flyCounter.transform.position.x == laneCounter)
                {
                    if (___enemies_to_bother[laneCounter])
                    {
                        currentLane = laneCounter;
                    }
                    break;
                }
            }

            flyCounter.transform.position += new Vector3(1, 0, 0);
            if (flyCounter.transform.position.x > 2)
            {
                flyCounter.transform.position = new Vector3(0, 0, 0);
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
                    Console.WriteLine("[The Legend of Bum-bo: Windfall] Reworked Attack Fly Spell sequence");
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

            Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing Fish Hook from granting red mana");
            return false;
        }

        //Patch: Prevents Tweezers from granting red mana
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketController), "GainRandomManaFromAttack")]
        static bool TrinketController_GainRandomManaFromAttack(TrinketController __instance)
        {
            float num = 0f;
            short num2 = 0;
            while ((int)num2 < __instance.app.model.characterSheet.trinkets.Count)
            {
                num += __instance.app.controller.GetTrinket((int)num2).ChanceOfGainingRandomManaFromAttack();
                num2 += 1;
            }
            num *= (float)__instance.EffectMultiplier();
            if (UnityEngine.Random.Range(0f, 1f) < num)
            {
                short[] array = new short[6];
                int num3 = UnityEngine.Random.Range(0, 5);
                if (num3 > 0)
                {
                    num3++;
                }
                array[num3] = 1;
                __instance.app.view.stolenManaView.gameObject.SetActive(true);
                __instance.app.view.stolenManaView.SetManaStolen(1, (Block.BlockType)num3);
                __instance.app.controller.UpdateMana(array, false);
                __instance.app.controller.ShowManaGain();
                __instance.app.controller.SetActiveSpells(true, true);

                Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing tweezers from granting red mana");
            }
            return false;
        }

        //Patch: Fixes Experimental giving red heart containers to Bum-bo the Dead and Bum-bo the Lost
        [HarmonyPrefix, HarmonyPatch(typeof(ExperimentalTrinket), "Use")]
        static bool ExperimentalTrinket_Use(ExperimentalTrinket __instance, int _index)
        {
            Debug.Log("[Bum-bo Update Mod] Changing Experimental result");

            __instance.uses--;
            if (__instance.uses <= 0)
            {
                __instance.app.model.characterSheet.trinkets.RemoveAt(_index);
            }
            __instance.app.controller.UpdateTrinkets();
            __instance.app.controller.eventsController.SetEvent(new IdleEvent());

            int num;
            if (__instance.app.model.characterSheet.bumboType.ToString() == "TheLost")
            {
                num = UnityEngine.Random.Range(0, 4);
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing Experimental from granting red heart container to Bum-bo the Lost");
            }
            else
            {
                num = UnityEngine.Random.Range(0, 5);
            }
            switch (num)
            {
                case 1:
                    __instance.app.model.characterSheet.addPuzzleDamage(1);
                    __instance.app.controller.GUINotification("Gained Puzzle Damage!", GUINotificationView.NotifyType.Stats, null, true);
                    break;
                case 2:
                    __instance.app.model.characterSheet.addItemDamage(1);
                    __instance.app.controller.GUINotification("Gained Item Damage!", GUINotificationView.NotifyType.Stats, null, true);
                    break;
                case 3:
                    __instance.app.model.characterSheet.addDex(1);
                    __instance.app.controller.GUINotification("Gained Dexterity!", GUINotificationView.NotifyType.Stats, null, true);
                    break;
                case 4:
                    if (__instance.app.model.characterSheet.bumboType.ToString() == "TheDead")
                    {
                        __instance.app.view.hearts.GetComponent<HealthController>().modifyHealth(0f, 1f);
                        Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing Experimental from granting red heart container to Bum-bo the Dead; granting soul heart instead");
                    }
                    else
                    {
                        __instance.app.model.characterSheet.addHitPoints(1);
                    }
                    __instance.app.controller.GUINotification("Gained Hit Points!", GUINotificationView.NotifyType.Stats, null, true);
                    break;
                default:
                    __instance.app.model.characterSheet.addLuck(1);
                    __instance.app.controller.GUINotification("Gained Luck!", GUINotificationView.NotifyType.Stats, null, true);
                    break;
            }
            __instance.app.controller.AddCurse(1);
            __instance.app.view.hearts.GetComponent<HealthController>().UpdateHearts(true);
            __instance.app.controller.UpdateStats();
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
                    Console.WriteLine("[The Legend of Bum-bo: Windfall] Fixed Paper Straw not interacting with ghost tiles");

                }
            }
            return true;
        }

        ////Patch: Paper Straw now interacts with ghost tiles
        //[HarmonyPatch(typeof(PaperStrawSpell), "CastSpell")]
        //static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        //{
        //    var code = new List<CodeInstruction>(instructions);

        //    int insertionIndex = -1;
        //    for (int i = 0; i < code.Count - 1; i++)
        //    {
        //        //Checking for a specific order of IL codes
        //        if (code[i].opcode == OpCodes.Br && code[i - 1].opcode == OpCodes.Callvirt && code[i - 1].operand == AccessTools.Method(typeof(BumboApplication), nameof(BumboApplication.Notify)))
        //        {
        //            //Insertion index
        //            insertionIndex = i;
        //            break;
        //        }
        //    }
        //    LocalBuilder localInt = il.DeclareLocal(typeof(int));
        //    var instructionsToInsert = new List<CodeInstruction>();

        //    CodeInstruction firstInstruction = new CodeInstruction(OpCodes.Ldloc_1);

        //    code[insertionIndex + 1].MoveLabelsTo(firstInstruction);

        //    instructionsToInsert.Add(firstInstruction);
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_8));

        //    //Add label for line after insertion index
        //    Label afterInsertion = il.DefineLabel();
        //    code[insertionIndex + 1].labels.Add(afterInsertion);

        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Bne_Un_S, afterInsertion));

        //    Label label1 = il.DefineLabel();

        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, localInt));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Br_S, label1)); //Label directed

        //    Label label2 = il.DefineLabel();

        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0).WithLabels(new[] { label2 }));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SpellElement), "get_app")));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BumboApplication), nameof(BumboApplication.model))));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BumboModel), nameof(BumboModel.characterSheet))));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CharacterSheet), nameof(CharacterSheet.spells))));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, localInt));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<SpellElement>), "get_Item")));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SpellElement), nameof(SpellElement.ChargeSpell))));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, localInt));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Add));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, localInt));

        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, localInt).WithLabels(new[] { label1 }));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SpellElement), "get_app")));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BumboApplication), nameof(BumboApplication.model))));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BumboModel), nameof(BumboModel.characterSheet))));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CharacterSheet), nameof(CharacterSheet.spells))));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<SpellElement>), "get_Count")));
        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Blt_S, label2)); //Label directed

        //    Label label3 = il.DefineLabel();

        //    for (int i = 1; i < code.Count - 1; i++)
        //    {
        //        //Checking for a specific order of IL codes
        //        if (code[i].opcode == OpCodes.Ldarg_0 && code[i - 1].opcode == OpCodes.Callvirt && code[i - 1].operand == AccessTools.Method(typeof(BumboController), nameof(BumboController.ShowManaGain)))
        //        {
        //            //Add label
        //            code[i].labels.Add(label3);
        //            break;
        //        }
        //    }

        //    instructionsToInsert.Add(new CodeInstruction(OpCodes.Br_S, label3)); //Label directed

        //    if (insertionIndex != -1)
        //    {
        //        code.InsertRange(insertionIndex, instructionsToInsert);
        //    }

        //    Console.WriteLine("[The Legend of Bum-bo: Windfall] Fixed Paper Straw not interacting with ghost tiles");
        //    return code;
        //}
    }
}