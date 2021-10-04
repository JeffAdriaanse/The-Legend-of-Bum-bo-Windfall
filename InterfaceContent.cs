using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

namespace The_Legend_of_Bum_bo_Windfall
{
    class InterfaceContent
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(InterfaceContent));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Implementing interface related mod content");
        }

        //Patch: Adding box collider to enemy on BaseInit
        [HarmonyPostfix, HarmonyPatch(typeof(Enemy), "BaseInit")]
        static void Enemy_BaseInit(Enemy __instance)
        {
            BoxCollider boxCollider;
            if (__instance.gameObject.GetComponent<BoxCollider>())
            {
                boxCollider = __instance.gameObject.GetComponent<BoxCollider>();
                boxCollider.enabled = true;
            }
            else
            {
                boxCollider = __instance.gameObject.AddComponent<BoxCollider>();
            }
            boxCollider.center = new Vector3(0, __instance.enemyType != Enemy.EnemyType.Ground ? 1.2f : 0.25f, 0);
            boxCollider.size = new Vector3(0.8f, 0.8f, 0.2f);
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Adding box collider to " + __instance.enemyName);
        }

        ////Patch: Hiding tooltip when StartMonsterTurnEvent is executed 
        //[HarmonyPostfix, HarmonyPatch(typeof(StartMonsterTurnEvent), "Execute")]
        //static void StartMonsterTurnEvent_Execute(StartMonsterTurnEvent __instance)
        //{
        //    __instance.app.view.toolTip.Hide();
        //    Console.WriteLine("[The Legend of Bum-bo: Windfall] Hiding tooltip on StartMonsterTurnEvent");
        //}

        //Patch: Hijacking BumboController update method to use tooltip for enemy box colliders
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Update")]
        static void BumboController_Update(BumboController __instance)
        {
            if (__instance.app.controller.loadingController != null || __instance.app.view.gamblingView != null)
            {
                //Abort if game is loading or if player is in Wooden Nickel
                return;
            }
            //Disable enemy box colliders when not in IdleEvent or ChanceToCastSpellEvent
            for (int j = 0; j < __instance.app.model.enemies.Count; j++)
            {
                if (__instance.app.model.enemies[j].GetComponent<BoxCollider>())
                {
                    if (__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent")
                    {
                        if (!__instance.app.model.enemies[j].GetComponent<BoxCollider>().enabled)
                        {
                            __instance.app.model.enemies[j].GetComponent<BoxCollider>().enabled = true;
                        }
                    }
                    else
                    {
                        if (__instance.app.model.enemies[j].GetComponent<BoxCollider>().enabled)
                        {
                            __instance.app.model.enemies[j].GetComponent<BoxCollider>().enabled = false;
                        }
                    }
                }
            }

            Color tintColor = new Color(0.5f, 0.5f, 0.5f);

            Ray ray = __instance.app.view.GUICamera.cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            bool hitValidCollider = false;

            float closestEnemyDistance = float.PositiveInfinity;
            Enemy selectedEnemy = null;

            //Find closest enemy
            Enemy closestHitEnemy = null;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                //Check whether any tooltip related colliders were hit
                if (enemy || hit.collider.GetComponent<BumboFacesController>() || hit.collider.GetComponent<TrinketView>())
                {
                    hitValidCollider = true;
                }
                if (enemy && hit.distance < closestEnemyDistance)
                {
                    closestEnemyDistance = hit.distance;
                    closestHitEnemy = enemy;
                }
            }

            if (__instance.app.view.gamblingView == null || __instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent")
            {
                //Find second closest enemy if mouse button is currently pressed
                closestEnemyDistance = float.PositiveInfinity;
                Enemy secondClosestHitEnemy = null;
                if (Input.GetMouseButton(0))
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        RaycastHit hit = hits[i];
                        Enemy enemy = hit.collider.GetComponent<Enemy>();
                        //Exclude closest hit enemy
                        if (enemy && hit.distance < closestEnemyDistance && enemy != closestHitEnemy)
                        {
                            closestEnemyDistance = hit.distance;
                            secondClosestHitEnemy = enemy;
                        }
                    }

                    if (secondClosestHitEnemy != null)
                    {
                        selectedEnemy = secondClosestHitEnemy;
                    }
                }

                if (selectedEnemy == null && closestHitEnemy != null)
                {
                    selectedEnemy = closestHitEnemy;
                }

                if (selectedEnemy != null && (__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent"))
                {
                    selectedEnemy.objectTinter.Tint(tintColor);

                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = Camera.main.nearClipPlane + 0.8f;
                    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

                    //Grab enemy name
                    string newEnemyName = selectedEnemy.enemyName.ToString() == "None" ? "" : selectedEnemy.enemyName.ToString();
                    //Grab boss name if boss
                    Boss selectedBoss = selectedEnemy.GetComponent<Boss>();
                    if (newEnemyName == "" && selectedBoss)
                    {
                        newEnemyName = selectedBoss.bossName.ToString();
                        if (selectedEnemy.GetComponent<BygoneGhostBoss>())
                        {
                            newEnemyName = "Bygone";
                        }
                    }
                    //Translate enemy name
                    if (newEnemyName != "")
                    {
                        switch (newEnemyName)
                        {
                            case "Shit":
                                newEnemyName = "Poop";
                                break;

                            case "Stone":
                                newEnemyName = "Rock";
                                break;

                            case "Arsemouth":
                                newEnemyName = "Tall Boy";
                                break;

                            case "Butthead":
                                newEnemyName = "Squat";
                                break;

                            case "Hopper":
                                newEnemyName = "Leaper";
                                break;

                            case "Tado":
                                newEnemyName = "Tato Kid";
                                break;

                            case "WillOWisp":
                                newEnemyName = "Whisp";
                                break;

                            case "DigDig":
                                newEnemyName = "Dig Dig";
                                break;

                            case "Longit":
                                newEnemyName = "Longits";
                                break;

                            case "Imposter":
                                if (selectedEnemy.enemyName == EnemyName.BlueBoney)
                                {
                                    newEnemyName = "Skully B.";
                                }
                                else if (selectedEnemy.enemyName == EnemyName.PurpleBoney)
                                {
                                    newEnemyName = "Skully P.";
                                }
                                else if (selectedEnemy.enemyName == EnemyName.Isaacs)
                                {
                                    newEnemyName = "Isaac";
                                }
                                else if (selectedEnemy.enemyName == EnemyName.MaskedImposter)
                                {
                                    newEnemyName = "Mask";
                                }
                                else
                                {
                                    newEnemyName = "Imposter";
                                }
                                break;

                            case "BoomFly":
                                newEnemyName = "Boom Fly";
                                break;

                            case "GreenBlobby":
                                newEnemyName = "Green Blobby";
                                break;

                            case "Tader":
                                newEnemyName = "Daddy Tato";
                                break;

                            case "CornyDip":
                                newEnemyName = "Corn Dip";
                                break;

                            case "PeepEye":
                                newEnemyName = "Peeper Eye";
                                break;

                            case "Tutorial":
                                newEnemyName = "Tutorial Keeper";
                                break;

                            case "MegaPoofer":
                                newEnemyName = "Mega Poofer";
                                break;

                            case "RedBlobby":
                                newEnemyName = "Red Blobby";
                                break;

                            case "BlackBlobby":
                                newEnemyName = "Black Blobby";
                                break;

                            case "MirrorHauntLeft":
                                newEnemyName = "Mirror";
                                break;

                            case "MirrorHauntRight":
                                newEnemyName = "Mirror";
                                break;

                            case "Hanger":
                                newEnemyName = "Keeper";
                                break;

                            case "MeatGolem":
                                newEnemyName = "Meat Golum";
                                break;

                            case "FloatingCultist":
                                newEnemyName = "Floater";
                                break;

                            case "WalkingCultist":
                                newEnemyName = "Cultist";
                                break;

                            case "Leechling":
                                newEnemyName = "Suck";
                                break;

                            case "RedCultist":
                                newEnemyName = "Red Floater";
                                break;

                            case "Flipper":
                                if (selectedEnemy.attackImmunity == Enemy.AttackImmunity.ReduceSpellDamage)
                                {
                                    newEnemyName = "Jib";
                                }
                                else
                                {
                                    newEnemyName = "Nib";
                                }
                                break;

                            case "GreenBlib":
                                newEnemyName = "Green Blib";
                                break;

                            case "ManaWisp":
                                newEnemyName = "Mana Wisp";
                                break;

                            case "TaintedPeepEye":
                                newEnemyName = "Tainted Peeper Eye";
                                break;
                            //Translate boss name
                            case "ShyGal":
                                newEnemyName = "Shy Gal";
                                break;

                            case "TaintedDusk":
                                newEnemyName = "Tainted Dusk";
                                break;

                            case "TaintedPeeper":
                                newEnemyName = "Tainted Peeper";
                                break;

                            case "TaintedShyGal":
                                newEnemyName = "Tainted Shy Gal";
                                break;
                        }
                    }

                    //Grab enemy turns count
                    string enemyTurns = selectedEnemy.turns == 0 ? "" : selectedEnemy.turns.ToString();
                    //Override enemy turns count in certain cases
                    if (selectedEnemy.turns > 0 && __instance.app.model.characterSheet.bumboRoundModifiers.skipEnemyTurns > 0)
                    {
                        //If player has used the Pause spell, the enemy is paused and will not use actions
                        enemyTurns = "Paused";
                    }
                    else if (selectedEnemy.turns > 1 && __instance.app.model.characterSheet.bumboRoundModifiers.slow == true)
                    {
                        //If player has used the Stop Watch spell, the enemy is slowed and will use at most one action
                        enemyTurns = "1";
                    }

                    //Add Moves text
                    if (enemyTurns != null && enemyTurns != "")
                    {
                        enemyTurns = "\nActions: " + enemyTurns;
                    }

                    //Grab next enemy action
                    string enemyNextAction = null;
                    if (selectedEnemy.nextAction != null)
                    {
                        enemyNextAction = selectedEnemy.nextAction.ResultingCondition().ToString();
                    }

                    //If next enemy action has not yet been determined, calculate it in advance instead
                    if (enemyNextAction == null)
                    {
                        //Record enemy previous conditions & next action
                        EnemyReaction.ReactionCause oldConditions = selectedEnemy.conditions;
                        EnemyReaction oldAction = selectedEnemy.nextAction;

                        //Update enemy next action
                        selectedEnemy.PlanNextMove();

                        //Grab next enemy action
                        enemyNextAction = selectedEnemy.nextAction == null ? "" : selectedEnemy.nextAction.ResultingCondition().ToString();

                        //Revert enemy state
                        selectedEnemy.conditions = oldConditions;
                        selectedEnemy.nextAction = oldAction;
                    }

                    //Override action name in certain cases
                    if (selectedEnemy.primed)
                    {
                        //If enemy is primed, it must be attacking next
                        enemyNextAction = "Attacking";
                    }
                    if (!selectedBoss && enemyNextAction == "Attacking" && !selectedEnemy.primed)
                    {
                        //If enemy is not primed, it usually won't be attacking next (bosses excluded)
                        //Default to prime instead (most likely option)
                        enemyNextAction = "Prime";
                    }
                    if (selectedEnemy.enemyName == EnemyName.Curser && enemyNextAction == "Attacking")
                    {
                        //Curser 'attack' actions are really just spell casts
                        enemyNextAction = "Spelled";
                    }
                    if (selectedEnemy.enemyName == EnemyName.Larry && enemyNextAction == "Moving" || enemyNextAction == "Primed")
                    {
                        //Larry move and prime actions are determined randomly; consequently, its action cannot be forseen
                        enemyNextAction = "";
                    }

                    //Translate action name
                    if (enemyNextAction != null && enemyNextAction != "")
                    {
                        switch (enemyNextAction)
                        {
                            case "Moving":
                                enemyNextAction = "Move";
                                break;

                            case "Primed":
                                enemyNextAction = "Prime";
                                break;

                            case "Attacking":
                                if (newEnemyName == "Tainted Peeper" || newEnemyName == "Red Floater")
                                {
                                    enemyNextAction = "Double Attack";
                                }
                                else
                                {
                                    enemyNextAction = "Attack";
                                }
                                break;

                            case "Spelled":
                                enemyNextAction = "Cast Spell";
                                break;

                            case "Spellcasting":
                                enemyNextAction = "Cast Spell";
                                break;

                            case "Spawned":
                                enemyNextAction = "Spawn Enemy";
                                break;

                            case "HasStatusFog":
                                enemyNextAction = "Create Fog";
                                break;

                            case "Nothing":
                                enemyNextAction = "";
                                break;
                        }
                    }

                    //Add Next Action text
                    if (enemyNextAction != null && enemyNextAction != "")
                    {
                        enemyNextAction = "\nNext Action: " + enemyNextAction;
                    }

                    //Display tooltip
                    __instance.app.view.toolTip.Show(newEnemyName + enemyTurns + enemyNextAction, ToolTip.Anchor.BottomLeft);
                    __instance.app.view.toolTip.transform.position = worldPosition;
                    __instance.app.view.toolTip.transform.rotation = Quaternion.Euler(51f, 180f, 0);
                }
            }

            foreach (Enemy enemy in __instance.app.model.enemies)
            {
                if (enemy != selectedEnemy && enemy.objectTinter.tintColor == tintColor)
                {
                    enemy.objectTinter.NoTint();
                }
            }

            if (!hitValidCollider)
            {
                __instance.app.view.toolTip.Hide();
            }
        }
    }
}
