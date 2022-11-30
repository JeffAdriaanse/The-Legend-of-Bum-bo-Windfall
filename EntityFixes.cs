using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace The_Legend_of_Bum_bo_Windfall
{
    class EntityFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(EntityFixes));
        }

        //Access child method
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PooferEnemy), nameof(PooferEnemy.timeToDie))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void timeToDieDummy_MegaPooferEnemy(MegaPooferEnemy instance) { }
        //Patch: Fixed Mega Poofers triggering a Poofer explosion alongside their regular explosion
        [HarmonyPrefix, HarmonyPatch(typeof(MegaPooferEnemy), nameof(MegaPooferEnemy.timeToDie))]
        static bool MegaPooferEnemy_timeToDie(MegaPooferEnemy __instance)
        {
            timeToDieDummy_MegaPooferEnemy(__instance);
            return false;
        }

        //Patch: Prevents vein from appearing on Keepers on Init
        [HarmonyPostfix, HarmonyPatch(typeof(HangerEnemy), "Init")]
        static void HangerEnemy_Init(HangerEnemy __instance)
        {
            if (__instance.berserkView != null && __instance.championType != Enemy.ChampionType.ExtraMove)
            {
                __instance.berserkView.gameObject.SetActive(false);
            }
        }

        //Patch: Fixes champion enemies creating dust twice when spawning
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "SpawnDust")]
        static bool Enemy_SpawnDust(Enemy __instance)
        {
            if (__instance.championType == Enemy.ChampionType.NotAChampion)
            {
                return true;
            }
            return false;
        }

        //Patch: Fixes status effects being invisible on the eyes of Peeper/Tainted Peeper
        [HarmonyPostfix, HarmonyPatch(typeof(PeepEyeEnemy), "Init")]
        static void PeepEyeEnemy_Init(PeepEyeEnemy __instance)
        {
            __instance.poisonView.transform.Rotate(0f, 180f, 0f);
            __instance.poisonView.transform.Translate(0f, 0f, -0.08f);
            __instance.blindObject.transform.Rotate(0f, 180f, 0f);
            __instance.blindObject.transform.Translate(0f, 0f, -0.08f);
        }

        //Patch: Fixes TadoEnemy init not setting turns 1 before baseinit
        [HarmonyPrefix, HarmonyPatch(typeof(TadoEnemy), "Init")]
        static bool TadoEnemy_Init(TadoEnemy __instance)
        {
            __instance.turns = 1;
            return true;
        }

        //***************************************************
        //*************Blobby enemy health states************
        //***************************************************
        //These patches fix Blobby type enemies spawning with incorrect health state if enemies of the same type have already been defeated in previous rooms of the current chapter

        //Patch: Forces Blobby health state to be of type full on init
        [HarmonyPostfix, HarmonyPatch(typeof(GreenBlobbyEnemy), "Init")]
        static void GreenBlobbyEnemy_Init(GreenBlobbyEnemy __instance)
        {
            if (__instance.healthState == GreenBlobbyEnemy.HealthState.half)
            {
                __instance.healthState = GreenBlobbyEnemy.HealthState.full;
            }
        }

        //Patch: Forces Red Blobby health state to be of type full on init
        [HarmonyPostfix, HarmonyPatch(typeof(RedBlobbyEnemy), "Init")]
        static void RedBlobbyEnemy_Init(RedBlobbyEnemy __instance)
        {
            if (__instance.healthState == RedBlobbyEnemy.HealthState.half)
            {
                __instance.healthState = RedBlobbyEnemy.HealthState.full;
            }
        }

        //Patch: Forces Black Blobby health state to be of type full on init
        [HarmonyPostfix, HarmonyPatch(typeof(BlackBlobbyEnemy), "Init")]
        static void BlackBlobbyEnemy_Init(BlackBlobbyEnemy __instance)
        {
            if (__instance.healthState == BlackBlobbyEnemy.HealthState.half)
            {
                __instance.healthState = BlackBlobbyEnemy.HealthState.full;
            }
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Prevents MonsterCounterEvent from occuring if there are no living enemies
        [HarmonyPrefix, HarmonyPatch(typeof(MonsterCounterEvent), "Execute")]
        static bool MonsterCounterEvent_Execute(MonsterCounterEvent __instance)
        {
            if (__instance.app.model.isEnemyCountering || __instance.app.model.isFogCountering)
            {
                bool anyAliveEnemy = false;
                foreach (Enemy enemy in __instance.app.model.enemies)
                {
                    if (enemy.boss)
                    {
                        if (enemy.getHealth() > 0f)
                        {
                            anyAliveEnemy = true;
                            break;
                        }
                    }
                    else
                    {
                        if (enemy.alive)
                        {
                            anyAliveEnemy = true;
                            break;
                        }
                    }
                }

                if (!anyAliveEnemy)
                {
                    __instance.app.model.isFogCountering = false;
                    __instance.app.model.isEnemyCountering = false;
                    __instance.app.model.counteringEnemy = null;
                    __instance.app.model.counteringFog.Clear();

                    __instance.app.controller.eventsController.SetEvent((BumboEvent)AccessTools.Field(typeof(MonsterCounterEvent), "nextEvent").GetValue(__instance));
                    return false;
                }
            }
            return true;
        }

        //Patch: Fixes a bug in the Tainted Peeper spawn blib logic; it now checks spaces on the y axis instead of the x axis
        [HarmonyPatch(typeof(PeepsBoss), "SpawnBlib")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            bool finishedFirstField = false;
            for (int i = 0; i < code.Count - 1; i++)
            {
                //Checking for a specific order of IL codes
                if (code[i].opcode == OpCodes.Ldfld && code[i - 1].opcode == OpCodes.Ldfld && code[i + 1].opcode == OpCodes.Call)
                {
                    //Change operands from enemyposition.x to enemyposition.y
                    var operand = AccessTools.Field(typeof(EnemyPosition), nameof(EnemyPosition.y));
                    if (finishedFirstField == false)
                    {
                        finishedFirstField = true;
                        code[i].operand = operand;
                    }
                    else
                    {
                        code[i].operand = operand;
                        break;
                    }
                }
            }
            return code;
        }

        //Patch: Fixes enemies spawning with incorrect health amounts if enemies of the same type have already been defeated in previous rooms of the current chapter
        //This patch overrides enemy starting health values and uses hard coded health instead
        //Also fixes enemies incorrectly spawning with champion crowns if an enemy of the same type has already been defeated in a previous room of the current chapter
        [HarmonyPostfix, HarmonyPatch(typeof(AIController), "DeleteEnemy")]
        static void AIController_DeleteEnemy(AIController __instance, ref GameObject _enemy, bool _destroy)
        {
            if (_enemy != null && _destroy && !_enemy.activeSelf)
            {
                Enemy enemy = _enemy.GetComponent<Enemy>();

                if (enemy != null && !enemy.boss)
                {

                    enemy.championType = Enemy.ChampionType.NotAChampion;

                    if (EnemyBaseHealth.TryGetValue(enemy.enemyName, out int baseHealth))
                    {
                        enemy.health = baseHealth;
                    }
                }
            }
        }

        public static Dictionary<EnemyName, int> EnemyBaseHealth
        {
            get
            {
                return new Dictionary<EnemyName, int>
                {
                    { EnemyName.Arsemouth, 4 },
                    { EnemyName.Butthead, 3 },
                    { EnemyName.Dip, 1 },
                    { EnemyName.Fly, 1 },
                    { EnemyName.Hopper, 2 },
                    { EnemyName.Pooter, 2 },
                    { EnemyName.Tado, 2 },
                    { EnemyName.Blib, 2 },
                    { EnemyName.WillOWisp, 3 },
                    { EnemyName.DigDig, 1 },
                    { EnemyName.Host, 4 },
                    { EnemyName.Longit, 4 },
                    { EnemyName.Imposter, 3 },
                    { EnemyName.MaskedImposter, 2 },
                    { EnemyName.BlueBoney, 3 },
                    { EnemyName.PurpleBoney, 4 },
                    { EnemyName.BoomFly, 2 },
                    { EnemyName.Larry, 5 },
                    { EnemyName.Burfer, 3 },
                    { EnemyName.GreenBlobby, 3 },
                    { EnemyName.Tader, 8 },
                    { EnemyName.CornyDip, 1 },
                    { EnemyName.Screecher, 2 },
                    { EnemyName.Sucker, 2 },
                    { EnemyName.Curser, 3 },
                    { EnemyName.Poofer, 3 },
                    { EnemyName.MegaPoofer, 6 },
                    { EnemyName.RedBlobby, 4 },
                    { EnemyName.BlackBlobby, 5 },
                    { EnemyName.Spookie, 2 },
                    { EnemyName.MirrorHauntLeft, 4 },
                    { EnemyName.MirrorHauntRight, 4 },
                    { EnemyName.Hanger, 6 },
                    { EnemyName.Isaacs, 4 },
                    { EnemyName.MeatGolem, 7 },
                    { EnemyName.FloatingCultist, 5 },
                    { EnemyName.WalkingCultist, 5 },
                    { EnemyName.Leechling, 3 },
                    { EnemyName.Greedling, 3 },
                    { EnemyName.RedCultist, 5 },
                    { EnemyName.Flipper, 5 },
                    { EnemyName.GreenBlib, 2 },
                    { EnemyName.ManaWisp, 1 },
                };
            }
        }
            
    }
}