using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

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
    }
}