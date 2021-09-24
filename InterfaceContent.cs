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

        //Patch: Hijacking BumboController update method to use tooltip for enemy box colliders
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Update")]
        static void BumboController_Update(BumboController __instance)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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

            Color tintColor = new Color(0.5f, 0.5f, 0.5f);

            if (selectedEnemy != null && (__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent"))
            {
                selectedEnemy.objectTinter.Tint(tintColor);

                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane + 0.8f;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

                //Grab enemy name
                string enemyName = selectedEnemy.enemyName.ToString() == "None" ? "" : selectedEnemy.enemyName.ToString();
                //Grab boss name if boss
                Boss selectedBoss = selectedEnemy.GetComponent<Boss>();
                if (enemyName == "" && selectedBoss)
                {
                    enemyName = selectedBoss.bossName.ToString();
                    if (selectedEnemy.GetComponent<BygoneGhostBoss>())
                    {
                        enemyName = "Bygone";
                    }
                }
                //Translate enemy name
                if (enemyName != "")
                {
                    switch (enemyName)
                    {
                        case "Shit":
                            enemyName = "Poop";
                            break;

                        case "Stone":
                            enemyName = "Rock";
                            break;

                        case "Arsemouth":
                            enemyName = "Tall Boy";
                            break;

                        case "Butthead":
                            enemyName = "Squat";
                            break;

                        case "Hopper":
                            enemyName = "Leaper";
                            break;

                        case "Tado":
                            enemyName = "Tato Kid";
                            break;

                        case "WillOWisp":
                            enemyName = "Whisp";
                            break;

                        case "DigDig":
                            enemyName = "Dig Dig";
                            break;

                        case "Longit":
                            enemyName = "Longits";
                            break;

                        case "Imposter":
                            if (selectedEnemy.GetComponent<BlueBoneyEnemy>())
                            {
                                enemyName = "Skully B.";
                            }
                            else if (selectedEnemy.GetComponent<PurpleBoneyEnemy>())
                            {
                                enemyName = "Skully P.";
                            }
                            else if (selectedEnemy.GetComponent<IsaacsEnemy>())
                            {
                                enemyName = "Isaac";
                            }
                            else if (selectedEnemy.GetComponent<MaskedImposterEnemy>())
                            {
                                enemyName = "Mask";
                            }
                            else
                            {
                                enemyName = "Imposter";
                            }
                            break;

                        case "BoomFly":
                            enemyName = "Boom Fly";
                            break;

                        case "GreenBlobby":
                            enemyName = "Green Blobby";
                            break;

                        case "Tader":
                            enemyName = "Daddy Tato";
                            break;

                        case "CornyDip":
                            enemyName = "Corn Dip";
                            break;

                        case "PeepEye":
                            enemyName = "Peeper Eye";
                            break;

                        case "Tutorial":
                            enemyName = "Tutorial Keeper";
                            break;

                        case "MegaPoofer":
                            enemyName = "Mega Poofer";
                            break;

                        case "RedBlobby":
                            enemyName = "Red Blobby";
                            break;

                        case "BlackBlobby":
                            enemyName = "Black Blobby";
                            break;

                        case "MirrorHauntLeft":
                            enemyName = "Mirror";
                            break;

                        case "MirrorHauntRight":
                            enemyName = "Mirror";
                            break;

                        case "Hanger":
                            enemyName = "Keeper";
                            break;

                        case "MeatGolem":
                            enemyName = "Meat Golum";
                            break;

                        case "FloatingCultist":
                            enemyName = "Floater";
                            break;

                        case "WalkingCultist":
                            enemyName = "Cultist";
                            break;

                        case "Leechling":
                            enemyName = "Suck";
                            break;

                        case "RedCultist":
                            enemyName = "Red Floater";
                            break;

                        case "Flipper":
                            if (selectedEnemy.attackImmunity == Enemy.AttackImmunity.ReduceSpellDamage)
                            {
                                enemyName = "Jib";
                            }
                            else
                            {
                                enemyName = "Nib";
                            }
                            break;

                        case "GreenBlib":
                            enemyName = "Green Blib";
                            break;

                        case "ManaWisp":
                            enemyName = "Mana Wisp";
                            break;

                        case "TaintedPeepEye":
                            enemyName = "Tainted Peeper Eye";
                            break;
                        //Translate boss name
                        case "ShyGal":
                            enemyName = "Shy Gal";
                            break;

                        case "TaintedDusk":
                            enemyName = "Tainted Dusk";
                            break;

                        case "TaintedPeeper":
                            enemyName = "Tainted Peeper";
                            break;

                        case "TaintedShyGal":
                            enemyName = "Tainted Shy Gal";
                            break;
                    }
                }

                //Grab enemy turns count
                string enemyTurns = selectedEnemy.turns == 0 ? "" : "\nMoves: " + selectedEnemy.turns;

                //Record enemy previous conditions & next action
                EnemyReaction.ReactionCause oldConditions = selectedEnemy.conditions;
                EnemyReaction oldAction = selectedEnemy.nextAction;
                //Update enemy next action
                selectedEnemy.PlanNextMove();
                //Grab next enemy action
                string enemyNextAction = selectedEnemy.nextAction == null ? "" : selectedEnemy.nextAction.ResultingCondition().ToString();
                //Revert enemy state
                selectedEnemy.conditions = oldConditions;
                selectedEnemy.nextAction = oldAction;

                //Override action name
                if (selectedEnemy.primed)
                {
                    enemyNextAction = "Attacking";
                }
                //Translate action name
                if (enemyNextAction == "Moving")
                {
                    enemyNextAction = "Move";
                }
                if (enemyNextAction == "Primed")
                {
                    enemyNextAction = "Prime";
                }
                if (enemyNextAction == "Attacking")
                {
                    if (enemyName == "Tainted Peeper" || enemyName == "Red Floater")
                    {
                        enemyNextAction = "Double Attack";
                    }
                    else
                    {
                        enemyNextAction = "Attack";
                    }
                }
                if (enemyNextAction == "Spelled" || enemyNextAction == "SpellCasting")
                {
                    enemyNextAction = "Cast Spell";
                }
                if (enemyNextAction == "Spawned")
                {
                    enemyNextAction = "Spawn Enemy";
                }
                if (enemyNextAction == "HasStatusFog")
                {
                    enemyNextAction = "Create Fog";
                }
                if (enemyNextAction == "Nothing")
                {
                    enemyNextAction = "";
                }
                //Add Next Action text
                if (enemyNextAction != "")
                {
                    enemyNextAction = "\nNext Action: " + enemyNextAction;
                }

                //Display tooltip
                __instance.app.view.toolTip.Show(enemyName + enemyTurns + enemyNextAction, ToolTip.Anchor.BottomLeft);
                __instance.app.view.toolTip.transform.position = worldPosition;
                __instance.app.view.toolTip.transform.rotation = Quaternion.Euler(51f, 180f, 0);
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
