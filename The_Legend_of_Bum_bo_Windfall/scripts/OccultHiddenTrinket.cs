using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    //Unused
    public class OccultHiddenTrinket : TrinketElement
    {
        public OccultHiddenTrinket()
        {
            base.trinketName = (TrinketName)1000;
            base.Name = "OCCULT_HIDDEN_DESCRIPTION";
            base.IconPosition = new Vector2(0f, 0f);
            base.Category = TrinketCategory.Special;
        }

        private readonly string occultSpiritPrefabPath = "Occult Spirit";

        public void SpawnSpirit(Enemy enemy)
        {
            //Spawn spirits
            int enemyBaseHealth = enemy.initialHealth;
            if (EntityFixes.EnemyBaseHealth.TryGetValue(enemy.enemyName, out int baseHealth)) enemyBaseHealth = baseHealth;

            int spiritsToSpawn = Mathf.FloorToInt(enemyBaseHealth / 3);
            if (spiritsToSpawn <= 0) spiritsToSpawn = 1;

            for (int spiritIterator = 0; spiritIterator < spiritsToSpawn; spiritIterator++)
            {
                //Place prefab
                GameObject occultSpirit = UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>(occultSpiritPrefabPath));
                OccultSpirit occultSpiritComponent = occultSpirit.AddComponent<OccultSpirit>(); //Add OccultSpirit component to prefab
                occultSpiritComponent.AnimateSpawn(enemy);
            }
        }
    }

    public class OccultSpirits
    {
        public static void Awake()
        {
            //Harmony.CreateAndPatchAll(typeof(OccultSpirits));
        }

        public List<OccultSpirit> occultSpirits;

        /// <summary>
        ///Triggers OccultHiddenTrinket effect and passes the Enemy instance that is dying.
        ///Vanilla <see cref="TrinketController.OnKill"/> does not work here because it does not pass the Enemy instance to triggered trinkets.
        ///Note that this triggers before any enemy death logic is performed, contrary to how <see cref="TrinketController.OnKill"/> works.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), nameof(Enemy.timeToDie))]
        static void Enemy_timeToDie(Enemy __instance)
        {
            //Trigger OccultHiddenTrinket in regular trinket slots
            for (int trinketIterator = 0; trinketIterator < __instance.app.model.characterSheet.trinkets.Count; trinketIterator++)
            {
                TrinketElement trinketElement = __instance.app.controller.GetTrinket(trinketIterator);
                if (trinketElement != null && trinketElement is OccultHiddenTrinket)
                {
                    (trinketElement as OccultHiddenTrinket).SpawnSpirit(__instance);
                }
            }

            //Trigger OccultHiddenTrinket in hidden trinket slot
            TrinketElement hiddenTrinketElement = __instance.app.model.characterSheet.hiddenTrinket;
            if (hiddenTrinketElement != null && hiddenTrinketElement is OccultHiddenTrinket)
            {
                (hiddenTrinketElement as OccultHiddenTrinket).SpawnSpirit(__instance);
            }
        }

        /// <summary>
        ///Inserts SpiritAttackEvent just after AttackFlyEvent, which causes Spirits to attack enemies.
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(AttackFlyEvent), nameof(AttackFlyEvent.NextEvent))]
        static void AttackFlyEvent_NextEvent(AttackFlyEvent __instance, ref BumboEvent __result)
        {
            if (__result is NewRoundEvent) __result = new OccultSpiritAttackEvent();
        }
    }

    public class OccultSpirit : MonoBehaviour
    {
        public void AnimateSpawn(Enemy enemy)
        {
            Vector3 enemyPosition = WindfallHelper.EnemyTransformPosition(enemy);
            transform.position = enemyPosition;

            Sequence sequence = DOTween.Sequence();

            Vector3 pathDestination = new Vector3(0f, 2.3f, -5.6f)/*Heart & Map Camera default location*/ + new Vector3(-0.4f, 0f, 0f)/*offset*/;


            //Path point placed in the middle of the trajectory to add an arc to the spirit's movement
            //Arc angle is randomized using a circle
            Vector3 midpoint = Vector3.Lerp(pathDestination, enemyPosition, 0.5f);/*path midpoint*/
            Plane plane = new Plane(pathDestination - enemyPosition, midpoint);

            Vector3[] waypoints = new Vector3[]
            {
                //arcPoint,
                pathDestination,
            };

            Tween path = transform.DOPath(waypoints, 1.5f, PathType.CatmullRom, PathMode.Full3D, 5).SetLookAt(0f, Vector3.up/*spirit flies head first*/);
            sequence.Append(path);
        }
    }

    public class OccultSpiritAttackEvent : BumboEvent
    {
        public override void Execute()
        {
            //Trigger Spirit attacks
            End();
        }

        public override BumboEvent NextEvent()
        {
            return new NewRoundEvent();
        }
    }
}
