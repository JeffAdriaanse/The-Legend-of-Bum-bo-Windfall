using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using DG.Tweening;

namespace The_Legend_of_Bum_bo_Windfall
{
    class EntityFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(EntityFixes));
        }

        //Patch: Fixes Bygone Body booger counter being hidden behind its sprite
        [HarmonyPostfix, HarmonyPatch(typeof(Enemy), nameof(Enemy.Booger))]
        static void Enemy_Booger(Enemy __instance)
        {
            if (__instance is not BygoneBoss) return;
            BoogerCounterView boogerCounterView = (BoogerCounterView)AccessTools.Field(typeof(Enemy), "boogerCounterView").GetValue(__instance);
            if (boogerCounterView != null) boogerCounterView.transform.localPosition = new Vector3(0.8f, boogerCounterView.transform.localPosition.y, 0.5f);
        }

        //Access child method
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PooferEnemy), nameof(PooferEnemy.timeToDie))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void timeToDieDummy_MegaPooferEnemy(MegaPooferEnemy instance) { }
        //Patch: Fixes Mega Poofers triggering a Poofer explosion alongside their regular explosion
        [HarmonyPrefix, HarmonyPatch(typeof(MegaPooferEnemy), nameof(MegaPooferEnemy.timeToDie))]
        static bool MegaPooferEnemy_timeToDie(MegaPooferEnemy __instance)
        {
            timeToDieDummy_MegaPooferEnemy(__instance);
            __instance.app.view.soundsView.PlaySound(SoundsView.eSound.Boom, __instance.enemySprites.transform.position, SoundsView.eAudioSlot.Default, false);
            return false;
        }

        //Patch: Fixes Poofer and Mega Poofer death explosions not producing a 'boom' sound effect
        [HarmonyPostfix, HarmonyPatch(typeof(PooferEnemy), nameof(PooferEnemy.timeToDie))]
        static void PooferEnemy_timeToDie(PooferEnemy __instance)
        {
            __instance.app.view.soundsView.PlaySound(SoundsView.eSound.Boom, __instance.enemySprites.transform.position, SoundsView.eAudioSlot.Default, false);
        }

        //Patch: Fixes Poofer and Mega Poofer death explosions not producing healing visuals and sound effects when healing nearby enemies
        [HarmonyPrefix, HarmonyPatch(typeof(PooferEnemy), nameof(PooferEnemy.HealSurrounding))]
        static bool PooferEnemy_HealSurrounding(PooferEnemy __instance)
        {
            for (int i = 0; i < __instance.app.model.enemies.Count; i++)
            {
                if (__instance.app.model.enemies[i] == __instance) continue;
                if (__instance.app.model.enemies[i].alive && __instance.app.model.enemies[i].position.x >= __instance.position.x - 1 && __instance.app.model.enemies[i].position.y >= __instance.position.y - 1 && __instance.app.model.enemies[i].position.x <= __instance.position.x + 1 && __instance.app.model.enemies[i].position.y <= __instance.position.y + 1)
                {
                    __instance.app.model.enemies[i].AddHealth(__instance.healAmount());
                }
            }
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
        //Also manages Mirror spawn dust
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "SpawnDust")]
        static bool Enemy_SpawnDust(Enemy __instance)
        {
            bool spawnDust = true;
            if (ObjectDataStorage.HasData<bool>(__instance.gameObject, spawnDustKey))
            {
                spawnDust = ObjectDataStorage.GetData<bool>(__instance.gameObject, spawnDustKey);
            }

            if (__instance.championType == Enemy.ChampionType.NotAChampion && spawnDust)
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

        //Patch: Changing AttackFlyEvent such that it checks for countering enemies when all AttackFlyEvents are finished; consequently, remaining green fog will always be triggered at the end of the enemy phase
        [HarmonyPostfix, HarmonyPatch(typeof(AttackFlyEvent), "NextEvent")]
        static void AttackFlyEvent_NextEvent(AttackFlyEvent __instance, ref BumboEvent __result)
        {
            if (__result.ToString() == "NewRoundEvent" && (__instance.app.model.isEnemyCountering || __instance.app.model.isFogCountering))
            {
                __result = new MonsterCounterEvent(new NewRoundEvent());
            }
        }

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
                    enemy.SetChampion(Enemy.ChampionType.NotAChampion);

                    if (EnemyBaseHealth.TryGetValue(enemy.enemyName, out int baseHealth))
                    {
                        enemy.health = baseHealth;
                    }
                }
            }
        }
        //Patch: Fixes Meat Hook breaking enemies (bug caused by above patch)
        //Also prevents Meat Hook from moving Peep Eyes
        //Also fixes Meat Hook sometimes moving enemies to the wrong positions and causing enemy visuals to be desynced from their actual positions
        [HarmonyPrefix, HarmonyPatch(typeof(MeatHookSpell), "RearrangeEnemies")]
        static bool MeatHookSpell_RearrangeEnemies(MeatHookSpell __instance, Transform _enemy_transform)
        {
            Enemy enemy = _enemy_transform?.GetComponent<Enemy>();
            if (enemy == null)
            {
                return false;
            }

            //Abort if the enemy is dead
            if (!__instance.app.model.enemies.Contains(enemy) || !enemy.alive || enemy.health <= 0f)
            {
                return false;
            }

            //Abort if used against Peeper Eyes
            if (enemy.enemyName == EnemyName.PeepEye || enemy.enemyName == EnemyName.TaintedPeepEye)
            {
                return false;
            }

            //Prevent effect from considering the hit enemy itself
            __instance.app.controller.ClearOwner(enemy);
            return true;
        }
        //Patch: Fixes Mirror enemies not dying (bug caused by above patch)
        [HarmonyPrefix, HarmonyPatch(typeof(MirrorHauntEnemy), nameof(MirrorHauntEnemy.Respawn))]
        static bool MirrorHauntEnemy_Respawn(MirrorHauntEnemy __instance)
        {
            //Abort if the enemy is dead
            if (!__instance.app.model.enemies.Contains(__instance) || !__instance.alive || __instance.health <= 0f)
            {
                return false;
            }

            return true;
        }
        //Patch: Fixes Quake attacking dead enemies (bug caused by above patch)
        [HarmonyPrefix, HarmonyPatch(typeof(QuakeSpell), "DropRock")]
        static void QuakeSpell_DropRock(QuakeSpell __instance)
        {
            List<Enemy> enemiesToHit = (List<Enemy>)AccessTools.Field(typeof(QuakeSpell), "enemies_to_hit").GetValue(__instance);

            if (enemiesToHit.Count > 0)
            {
                Enemy enemyToHit = enemiesToHit[0];

                if (enemyToHit != null)
                {
                    if (!__instance.app.model.enemies.Contains(enemyToHit))
                    {
                        List<Enemy> enemiesToHitReplacement = enemiesToHit;
                        enemiesToHitReplacement[0] = null;
                        AccessTools.Field(typeof(QuakeSpell), "enemies_to_hit").SetValue(__instance, enemiesToHitReplacement);
                    }
                }
            }
        }
        //Patch: Fixes enemies not getting knocked back
        [HarmonyPostfix, HarmonyPatch(typeof(Enemy), nameof(Enemy.Knockback))]
        static void Enemy_Knockback(Enemy __instance)
        {
            if (__instance is not CaddyBoss)
            {
                AccessTools.Field(typeof(Enemy), "knockbackHappened").SetValue(__instance, false);
            }
        }

        private static readonly string spawnDustKey = "spawnDustKey";

        //Patch: Fixes Mirror dust
        [HarmonyPrefix, HarmonyPatch(typeof(MirrorHauntEnemy), nameof(MirrorHauntEnemy.Init))]
        static void MirrorHauntEnemy_Init(MirrorHauntEnemy __instance)
        {
            //Only spawn dust on Init if the room is starting
            if (__instance.app.model.bumboEvent.ToString() == "RoomStartEvent")
            {
                //Set flag to enable spawning dust
                ObjectDataStorage.StoreData<bool>(__instance.gameObject, spawnDustKey, true);

                __instance.SpawnDust();
            }
            //Set flag to disable spawning dust
            ObjectDataStorage.StoreData<bool>(__instance.gameObject, spawnDustKey, false);
        }
        //Changes MirrorHauntEnemy_Relocate to spawn dust after the Mirror has moved instead of beforehand, which would spawn the dust at the wrong location
        [HarmonyPostfix, HarmonyPatch(typeof(MirrorHauntEnemy), nameof(MirrorHauntEnemy.Relocate))]
        static void MirrorHauntEnemy_Relocate(MirrorHauntEnemy __instance)
        {
            //Set flag to enable spawning dust
            ObjectDataStorage.StoreData<bool>(__instance.gameObject, spawnDustKey, true);
            //Spawn dust
            __instance.SpawnDust();
            //Set flag to disable spawning dust
            ObjectDataStorage.StoreData<bool>(__instance.gameObject, spawnDustKey, false);
        }

        //Fixes Host erroneously ending events
        [HarmonyPrefix, HarmonyPatch(typeof(HostEnemy), "Close")]
        static bool HostEnemy_Close(HostEnemy __instance)
        {
            bool closed = (bool)AccessTools.Field(typeof(HostEnemy), "closed").GetValue(__instance);

            //Modify fields
            AccessTools.Field(typeof(HostEnemy), "closed").SetValue(__instance, true);
            __instance.noFlyOver = false;
            __instance.turnsMade++;

            //Only animate if closing
            if (!closed)
            {
                //Reset animation
                DOTween.Kill(__instance.tweenID, false);
                __instance.enemySprites.transform.localScale = Vector3.one;

                //Animate
                DOTween.Sequence().Append(ShortcutExtensions.DOScale(__instance.enemySprites.transform, new Vector3(0.5714f, 1.75f, 1f), __instance.AttackAnimTime(0.1f)).SetEase(Ease.OutQuad)).Append(ShortcutExtensions.DOScale(__instance.enemySprites.transform, new Vector3(1.75f, 0.5714f, 1f), __instance.AttackAnimTime(0.1f))).AppendCallback(delegate
                {
                    __instance.AnimateIdle();
                }).SetId(__instance.tweenID);
            }

            return false;
        }

        //Prevents Host from attempting to open while beneath a flying enemy 
        [HarmonyPostfix, HarmonyPatch(typeof(Enemy), nameof(Enemy.WillSkip))]
        static void Enemy_WillSkip(Enemy __instance, ref bool __result)
        {
            if (__instance is HostEnemy)
            {
                HostEnemy hostEnemy = __instance as HostEnemy;

                //Do not attempt to open if a flying enemy is above
                if (hostEnemy.IsClosed() && hostEnemy.app.model.aiModel.battlefieldPositions[hostEnemy.app.model.aiModel.battlefieldPositionIndex[hostEnemy.battlefieldPosition.x, hostEnemy.battlefieldPosition.y]].owner_air != null)
                {
                    //Skip turn
                    __result = true;
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