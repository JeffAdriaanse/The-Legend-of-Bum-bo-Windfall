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
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying entity related bug fixes");
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
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Mega Poofer death");
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
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Deactivating vein icon on Hanger enemy");
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
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Aborting enemy dust spawn; enemy is a champion");
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
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Rotating status effects of " + __instance.enemyName);
        }

        //Patch: Fixes TadoEnemy init not setting turns 1 before baseinit
        [HarmonyPrefix, HarmonyPatch(typeof(TadoEnemy), "Init")]
        static bool TadoEnemy_Init(TadoEnemy __instance)
        {
            __instance.turns = 1;
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Setting TadoEnemy turns amount on init to 1");
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
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Overriding starting health state of newly spawned Blobby");
            }
        }

        //Patch: Forces Red Blobby health state to be of type full on init
        [HarmonyPostfix, HarmonyPatch(typeof(RedBlobbyEnemy), "Init")]
        static void RedBlobbyEnemy_Init(RedBlobbyEnemy __instance)
        {
            if (__instance.healthState == RedBlobbyEnemy.HealthState.half)
            {
                __instance.healthState = RedBlobbyEnemy.HealthState.full;
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Overriding starting health state of newly spawned Red Blobby");
            }
        }

        //Patch: Forces Black Blobby health state to be of type full on init
        [HarmonyPostfix, HarmonyPatch(typeof(BlackBlobbyEnemy), "Init")]
        static void BlackBlobbyEnemy_Init(BlackBlobbyEnemy __instance)
        {
            if (__instance.healthState == BlackBlobbyEnemy.HealthState.half)
            {
                __instance.healthState = BlackBlobbyEnemy.HealthState.full;
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Overriding starting health state of newly spawned Black Blobby");
            }
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //***************************************************
        //*****************Green fog fixes*******************
        //***************************************************
        //These patches cause the game to check for triggered fog more often, preventing triggered fog from lingering

        //Patch: Changing AttackFlyEvent such that it checks for countering enemies when all AttackFlyEvents are finished; consequently, remaining green fog will always be triggered at the end of the enemy phase
        [HarmonyPostfix, HarmonyPatch(typeof(AttackFlyEvent), "NextEvent")]
        static void AttackFlyEvent_NextEvent(AttackFlyEvent __instance, ref BumboEvent __result)
        {
            if (__result.ToString() == "NewRoundEvent" && (__instance.app.model.isEnemyCountering || __instance.app.model.isFogCountering))
            {
                __result = new MonsterCounterEvent(new NewRoundEvent());
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing AttackFlyEvent return event to MonsterCounterEvent");
            }
        }

        //Patch: Changing BoneMegaAttackEvent such that it checks for countering enemies
        [HarmonyPostfix, HarmonyPatch(typeof(BoneMegaAttackEvent), "NextEvent")]
        static void BoneMegaAttackEvent_NextEvent(BoneMegaAttackEvent __instance, ref BumboEvent __result)
        {
            if (__result.ToString() == "NextComboEvent" && (__instance.app.model.isEnemyCountering || __instance.app.model.isFogCountering))
            {
                __result = new MonsterCounterEvent(new NextComboEvent());
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing BoneMegaAttackEvent return event to MonsterCounterEvent");
            }
        }

        //Patch: Changing ToothMegaAttackEvent such that it checks for countering enemies
        [HarmonyPostfix, HarmonyPatch(typeof(ToothMegaAttackEvent), "NextEvent")]
        static void ToothMegaAttackEvent_NextEvent(ToothMegaAttackEvent __instance, ref BumboEvent __result)
        {
            if (__result.ToString() == "NextComboEvent" && (__instance.app.model.isEnemyCountering || __instance.app.model.isFogCountering))
            {
                __result = new MonsterCounterEvent(new NextComboEvent());
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing ToothMegaAttackEvent return event to MonsterCounterEvent");
            }
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Fixes enemies spawning with incorrect health amounts if enemies of the same type have already been defeated in previous rooms of the current chapter
        //This patch overrides enemy starting health values and uses hard coded health instead
        [HarmonyPostfix, HarmonyPatch(typeof(Enemy), "Spawn")]
        static void Enemy_Spawn(Enemy __instance)
        {
            int newHealthValue = 0;
            switch (__instance.app.model.enemies[__instance.app.model.enemies.Count - 1].enemyName)
            {
                case EnemyName.Arsemouth:
                    newHealthValue = 4;
                    break;
                case EnemyName.Butthead:
                    newHealthValue = 3;
                    break;
                case EnemyName.Dip:
                    newHealthValue = 1;
                    break;
                case EnemyName.Fly:
                    newHealthValue = 1;
                    break;
                case EnemyName.Hopper:
                    newHealthValue = 2;
                    break;
                case EnemyName.Pooter:
                    newHealthValue = 2;
                    break;
                case EnemyName.Tado:
                    newHealthValue = 2;
                    break;
                case EnemyName.Blib:
                    newHealthValue = 2;
                    break;
                case EnemyName.WillOWisp:
                    newHealthValue = 3;
                    break;
                case EnemyName.DigDig:
                    newHealthValue = 1;
                    break;
                case EnemyName.Host:
                    newHealthValue = 4;
                    break;
                case EnemyName.Longit:
                    newHealthValue = 4;
                    break;
                case EnemyName.Imposter:
                    newHealthValue = 3;
                    break;
                case EnemyName.MaskedImposter:
                    newHealthValue = 2;
                    break;
                case EnemyName.BlueBoney:
                    newHealthValue = 3;
                    break;
                case EnemyName.PurpleBoney:
                    newHealthValue = 4;
                    break;
                case EnemyName.BoomFly:
                    newHealthValue = 2;
                    break;
                case EnemyName.Larry:
                    newHealthValue = 5;
                    break;
                case EnemyName.Burfer:
                    newHealthValue = 3;
                    break;
                case EnemyName.GreenBlobby:
                    newHealthValue = 3;
                    break;
                case EnemyName.Tader:
                    newHealthValue = 8;
                    break;
                case EnemyName.CornyDip:
                    newHealthValue = 1;
                    break;
                case EnemyName.Screecher:
                    newHealthValue = 2;
                    break;
                case EnemyName.Sucker:
                    newHealthValue = 2;
                    break;
                case EnemyName.Curser:
                    newHealthValue = 3;
                    break;
                case EnemyName.Poofer:
                    newHealthValue = 3;
                    break;
                case EnemyName.MegaPoofer:
                    newHealthValue = 6;
                    break;
                case EnemyName.RedBlobby:
                    newHealthValue = 4;
                    break;
                case EnemyName.BlackBlobby:
                    newHealthValue = 5;
                    break;
                case EnemyName.Spookie:
                    newHealthValue = 2;
                    break;
                case EnemyName.MirrorHauntLeft:
                    newHealthValue = 4;
                    break;
                case EnemyName.MirrorHauntRight:
                    newHealthValue = 4;
                    break;
                case EnemyName.Hanger:
                    newHealthValue = 6;
                    break;
                case EnemyName.Isaacs:
                    newHealthValue = 4;
                    break;
                case EnemyName.MeatGolem:
                    newHealthValue = 7;
                    break;
                case EnemyName.FloatingCultist:
                    newHealthValue = 5;
                    break;
                case EnemyName.WalkingCultist:
                    newHealthValue = 5;
                    break;
                case EnemyName.Leechling:
                    newHealthValue = 3;
                    break;
                case EnemyName.Greedling:
                    newHealthValue = 3;
                    break;
                case EnemyName.RedCultist:
                    newHealthValue = 5;
                    break;
                case EnemyName.Flipper:
                    newHealthValue = 5;
                    break;
                case EnemyName.GreenBlib:
                    newHealthValue = 2;
                    break;
                case EnemyName.ManaWisp:
                    newHealthValue = 1;
                    break;
            }

            if (newHealthValue != 0)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Overriding starting health of newly spawned " + __instance.app.model.enemies[__instance.app.model.enemies.Count - 1].enemyName);
                __instance.app.model.enemies[__instance.app.model.enemies.Count - 1].health = newHealthValue;
            }
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
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Correcting Tainted Peeper spawn blib logic");
            return code;
        }
    }
}