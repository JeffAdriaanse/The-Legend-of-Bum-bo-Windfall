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
            float closestEnemyDistance = 99f;
            Enemy closestHitEnemy = null;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                Enemy enemy = hit.collider.GetComponent<Enemy>();
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

            Color tintColor = new Color(0.5f, 0.5f, 0.5f);

            if (closestHitEnemy != null && (__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent"))
            {
                closestHitEnemy.objectTinter.Tint(tintColor);

                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane + 0.8f;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
                __instance.app.view.toolTip.Show(closestHitEnemy.enemyName.ToString() + "\nMoves: " + closestHitEnemy.turns, ToolTip.Anchor.BottomLeft);
                __instance.app.view.toolTip.transform.position = worldPosition;
                __instance.app.view.toolTip.transform.rotation = Quaternion.Euler(51f, 180f, 0);
            }

            foreach (Enemy enemy in __instance.app.model.enemies)
            {
                if (enemy != closestHitEnemy && enemy.objectTinter.tintColor == tintColor)
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
