using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

namespace The_Legend_of_Bum_bo_Windfall
{
    class InterfaceFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(InterfaceFixes));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying interface related bug fixes");
        }

        //Patch: Mana drain type notifications no longer overlap when multiple are spawned at the same time; they now layer behind each other instead
        [HarmonyPostfix, HarmonyPatch(typeof(ManaDrainView), "HudAppear")]
        static void ManaDrainView_HudAppear(ManaDrainView __instance)
        {
            ManaDrainView[] currentNotifications = UnityEngine.Object.FindObjectsOfType<ManaDrainView>();
            int offsetCounter = 0;
            for (int notificationIndex = 0; notificationIndex < currentNotifications.Length; notificationIndex++)
            {
                //Exclude self (prevents infinite for loop)
                if (__instance != currentNotifications[notificationIndex])
                {
                    Vector3 thisNotificationPosition = __instance.transform.position;
                    Vector3 currentNotificationPosition = currentNotifications[notificationIndex].transform.position;
                    float notificationOffset = 0.02f;
                    float overlapFidelity = notificationOffset / 3;
                    //Check whether notification is too close on each axis
                    if (currentNotificationPosition.x - overlapFidelity <= thisNotificationPosition.x && thisNotificationPosition.x <= currentNotificationPosition.x + overlapFidelity
                        && currentNotificationPosition.y - overlapFidelity <= thisNotificationPosition.y && thisNotificationPosition.y <= currentNotificationPosition.y + overlapFidelity
                        && currentNotificationPosition.z - overlapFidelity <= thisNotificationPosition.z && thisNotificationPosition.z <= currentNotificationPosition.z + overlapFidelity)
                    {
                        //Move notification back and up slightly
                        __instance.transform.Translate(0f, notificationOffset, notificationOffset);
                        //Increase offset counter
                        offsetCounter++;
                        //Restart loop
                        notificationIndex = 0;
                    }
                }
            }
            if (offsetCounter > 0)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Offsetting position of newly spawned ManaDrainView object; object was offset " + offsetCounter + (offsetCounter == 1 ? "time" : "times"));
            }
        }

        //Patch: Fixes a bug that caused invincible enemies to make the boss health bar display their healh amount when healed; the invincible enemy will now not be healed instead
        //Patch also causes the healing 'gulp' sound to not play for the invincible enemy
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "AddHealth")]
        static bool Enemy_AddHealth(Enemy __instance)
        {
            if (__instance.max_health == Enemy.HealthType.INVINCIBLE)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Aborting healing of " + __instance.enemyName + "; enemy is invincible");
                return false;
            }
            return true;
        }

        //Patch: Fixes enemy healing icons appearing at the back of the lane instead of in front of the enemy being healed
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "Init")]
        static bool Enemy_Init(Enemy __instance)
        {
            if (__instance.indicatorPosition == Vector3.zero)
            {
                __instance.indicatorPosition = new Vector3(0, __instance.enemyType != Enemy.EnemyType.Ground ? 1.25f : 0.5f, -0.5f);
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Correcting " + __instance.enemyName + " healing icon spawn location");
            }
            return true;
        }

        //Patch: Fixes z-fighting between enemies and their spawn dust; dust will now never spawn at the same z coordinate as the enemy's position
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "Dust")]
        static bool Enemy_Dust(Enemy __instance, ref float _x_offset, ref float _y_offset, ref float _z_offset)
        {
            if (_z_offset == 0.2f)
            {
                _z_offset += 0.1f;
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Offsetting position of " + __instance.enemyName + " spawn dust to avoid z-fighting");
            }
            return true;
        }

        //Patch: Fixes holding 'r' to restart while in a treasure room causing the game to softlock
        [HarmonyPrefix, HarmonyPatch(typeof(TreasureChosenEvent), "Execute")]
        static bool TreasureChosenEvent_Execute(TreasureChosenEvent __instance)
        {
            if (__instance.app.controller.loadingController != null)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Aborting treasure chosen event; game is loading");
                return false;
            }
            return true;
        }

        //Patch: Fixes champion enemies creating dust twice when spawning
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "SpawnDust")]
        static bool Enemy_SpawnDust(Enemy __instance)
        {
            if (__instance.championType == Enemy.ChampionType.NotAChampion)
            {
                return true;
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Aborting dust spawn of " + __instance.enemyName + "; enemy is a champion");
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

        //***************************************************
        //*************Trinket pickup use display************
        //***************************************************
        //These patches fix the number of uses displayed on trinket pickups always appearing as one, even if the trinket has multiple uses

        //Patch: Changes displayed number of uses of trinket pickups in treasure rooms
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketPickupView), "SetTrinket", new Type[] { typeof(TrinketName), typeof(TrinketModel) })]
        static void TrinketPickupView_SetTrinket(TrinketPickupView __instance)
        {
            if (__instance.trinket.Category == TrinketElement.TrinketCategory.Use)
            {
                __instance.trinketUses.text = __instance.trinket.uses.ToString();
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Updating trinket pickup use display");
            }
        }

        //Patch: Changes displayed number of uses of trinket pickups in the Wooden Nickel
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketPickupView), "SetTrinket", new Type[] { typeof(TrinketName), typeof(int) })]
        static void TrinketPickupView_SetTrinket_Shop(TrinketPickupView __instance)
        {
            if (__instance.trinket.Category == TrinketElement.TrinketCategory.Use)
            {
                __instance.trinketUses.text = __instance.trinket.uses.ToString();
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Updating shop trinket pickup use display");
            }
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Disables all pickups when a spell pickup is clicked
        [HarmonyPostfix, HarmonyPatch(typeof(SpellPickup), "OnMouseDown")]
        static void SpellPickup_OnMouseDown(SpellPickup __instance)
        {
            __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>().SetClickable(false);
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Disabling treasure room pickups on spell pickup click");
        }

        //Patch: Disables the end turn sign collider when it spawns, preventing it from being clicked while above the play area
        [HarmonyPostfix, HarmonyPatch(typeof(EndTurnView), "Start")]
        static void EndTurn_Start(EndTurnView __instance)
        {
            __instance.GetComponent<BoxCollider>().enabled = false;
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Disabling end turn sign collider on start");
        }

        //Patch: Fixes a bug causing the 'options' sub-menu to not open in the Wooden Nickel
        [HarmonyPrefix, HarmonyPatch(typeof(PauseButtonView), "Options")]
        static bool PauseButtonView_Options(PauseButtonView __instance)
        {
            if (__instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent")
            {
                __instance.app.view.optionsController.Open(__instance.app.view.pauseItems, __instance.app.controller.gamblingController.levelMusicView.GetComponent<AudioSource>());
                Console.WriteLine("[Bum-bo Update Mod] Correcting loacation of audiosource retrieved by options menu; menu was opened in the Wooden Nickel");
                return false;
            }
            return true;
        }
    }
}