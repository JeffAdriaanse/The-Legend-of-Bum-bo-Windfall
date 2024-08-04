using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    class OtherChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(OtherChanges));
            PatchAchievementsUnlock();
        }

        //Patch: Fixes exiting to menu incorrectly deleting certain gameObjects
        //Also rebalances Loose Change coin gain
        public static readonly int looseChangeCoinGain = 4;
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "OnNotification")]
        static bool BumboController_OnNotification(BumboController __instance, string _event_path)
        {
            if (_event_path == "gameover.exittomenu")
            {
                DOTween.KillAll(false);
                LoadingController loadingController2 = LoadingController.Get();
                loadingController2.gameObject.SetActive(true);
                List<GameObject> list = new List<GameObject>();
                list.AddRange(GameObject.FindGameObjectsWithTag("Loading"));
                if (__instance.app.model.characterSheet.currentFloor == 0 && __instance.app.controller.tutorialController != null)
                {
                    __instance.app.model.progression.completedTutorial = true;
                    ProgressionController.SaveProgression(__instance.app.model.progression);
                }
                loadingController2.gameObject.SetActive(true);
                GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
                for (int i = array.Length - 1; i >= 0; i--)
                {
                    if (array[i].name != "Timer" && array[i].name != "[DOTween]" && array[i] != PlatformManagerTemplate<PlatformManagerSteam>.Instance.gameObject && array[i] != InputManager.Instance.gameObject && array[i].name != "SteamManager" && array[i].name != "BepInEx_Manager" && array[i].name != "BepInEx_ThreadingHelper" && list.IndexOf(array[i]) == -1)
                    {
                        UnityEngine.Object.Destroy(array[i].gameObject);
                    }
                }
                loadingController2.loadScene("titlescreen2", true);

                return false;
            }

            //Modify amount of coins granted by Loose Change
            if (_event_path == "bumbo.hurt" && __instance.app.model.characterSheet.bumboRoundModifiers.coinForHurt)
            {
                int coins = WindfallPersistentDataController.LoadData().implementBalanceChanges ? looseChangeCoinGain : 1;
                __instance.ModifyCoins(coins);
                return false;
            }

            return true;
        }

        //Patch: Heart tiles no longer appear naturally when playing as Bum-bo the Lost
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), "nextBlock")]
        static bool Puzzle_nextBlock(Puzzle __instance, ref int ___heartCounter)
        {
            if (!WindfallPersistentDataController.LoadData().implementBalanceChanges)
            {
                return true;
            }

            if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheLost)
            {
                ___heartCounter = -1;
            }
            return true;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketController), "StartingBlocks")]
        static void TrinketController_StartingBlocks(TrinketController __instance, ref int[] __result)
        {
            if (!WindfallPersistentDataController.LoadData().implementBalanceChanges)
            {
                return;
            }

            if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheLost)
            {
                if (__result[1] > 0)
                {
                    __result[1]--;
                }
            }
        }

        //Patch: Fixes a bug in CharacterSheet addSoulHearts logic that permitted granting soul health past the maximum of six total hearts
        [HarmonyPrefix, HarmonyPatch(typeof(CharacterSheet), "addSoulHearts")]
        static bool CharacterSheet_addSoulHearts(CharacterSheet __instance, float _amount)
        {
            float amount = Mathf.Clamp(_amount, 0f, 6f - __instance.bumboBaseInfo.hitPoints - __instance.soulHearts);
            __instance.soulHearts += amount;
            if (__instance.soulHearts > 6f)
            {
                __instance.soulHearts = 6f;
            }
            return false;
        }

        //Patch: Fixes a bug in CharacterSheet addHitPoints logic that permitted granting soul health past the maximum of six total hearts
        [HarmonyPostfix, HarmonyPatch(typeof(CharacterSheet), "addHitPoints")]
        static void CharacterSheet_addHitPoints(CharacterSheet __instance)
        {
            while (__instance.bumboBaseInfo.hitPoints + __instance.soulHearts > 6f && __instance.app.model.characterSheet.soulHearts > 0f)
            {
                __instance.soulHearts -= 0.5f;
            }
        }

        //Patch: Fixes a bug in the Mega Boner tile combo logic; it no longer incorrectly breaks out of the for statements that look for enemies
        [HarmonyPatch(typeof(BoneMegaAttackEvent), "AttackEnemy")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);

            Label endLabel = il.DefineLabel();

            bool firstBreak = true;
            int firstBreakIndex = -1;
            for (int codeIndex = 0; codeIndex < code.Count - 1; codeIndex++)
            {
                if (code[codeIndex].opcode == OpCodes.Br && code[codeIndex - 1].opcode == OpCodes.Callvirt && code[codeIndex - 2].opcode == OpCodes.Callvirt)
                {
                    if (firstBreak)
                    {
                        firstBreakIndex = codeIndex;
                        firstBreak = false;
                    }
                    else
                    {
                        code[codeIndex].opcode = OpCodes.Nop;
                        code[codeIndex].labels.Add(endLabel);

                        code[firstBreakIndex] = new CodeInstruction(OpCodes.Br, endLabel);
                        return code;
                    }
                }
            }
            return code;
        }

        //Patch: Fixes Mega Chomper Tile Combo sometimes trying to damage enemies that are already dead, which can cause a softlock
        [HarmonyPrefix, HarmonyPatch(typeof(ToothMegaAttackEvent), "AttackEnemy")]
        static bool ToothMegaAttackEvent_AttackEnemy(ToothMegaAttackEvent __instance)
        {
            List<Enemy> list = new List<Enemy>();
            list.AddRange(__instance.app.model.enemies);
            short num = 0;
            while (num < list.Count)
            {
                if (list[num] != null && (list[num].alive || list[num].isPoop))
                {
                    list[num].Hurt(__instance.app.model.characterSheet.getPuzzleDamage() + 3, Enemy.AttackImmunity.ReducePuzzleDamage, null, -1);
                }
                num += 1;
            }
            return false;
        }

        //Patch: Fixes Mega Boogie tile combo sometimes not boogering enemies when used in conjunction with Sinus Infection (it would modify the Enemies list while looping through it)
        [HarmonyPrefix, HarmonyPatch(typeof(BoogerMegaAttackEvent), "AttackEnemy")]
        static bool BoogerMegaAttackEvent_AttackEnemy(BoogerMegaAttackEvent __instance)
        {
            List<Enemy> list = new List<Enemy>();
            list.AddRange(__instance.app.model.enemies);
            short num = 0;
            while (num < list.Count)
            {
                if (list[num] != null && list[num].alive)
                {
                    list[num].Booger(2);
                }
                num += 1;
            }
            return false;
        }

        //Patch: Changes floor room generation
        [HarmonyPostfix, HarmonyPatch(typeof(MapCreationController), "CreateMap")]
        static void MapCreationController_CreateMap(MapCreationController __instance)
        {
            //Don't generate new map if player is in the tutorial or if a saved map is loading
            if (__instance.app.model.characterSheet.currentFloor > 0 && !__instance.app.controller.savedStateController.IsLoading())
            {
                List<MapRoom.RoomType> rooms = new List<MapRoom.RoomType>()
                {
                    MapRoom.RoomType.Start,
                    MapRoom.RoomType.Treasure,
                    MapRoom.RoomType.EnemyEncounter,
                    MapRoom.RoomType.EnemyEncounter,
                    MapRoom.RoomType.Treasure,
                    MapRoom.RoomType.Boss
                };

                List<MapRoom.Direction> roomDirections = new List<MapRoom.Direction>();
                for (int roomCounter = 0; roomCounter < rooms.Count - 1; roomCounter++)
                {
                    //Player will never go south (backwards)
                    List<MapRoom.Direction> possibleDirections = new List<MapRoom.Direction>()
                    {
                        MapRoom.Direction.N,
                        MapRoom.Direction.E,
                        MapRoom.Direction.W
                    };

                    //Don't go back to previous room
                    if (roomCounter > 0)
                    {
                        if (roomDirections[roomCounter - 1] == MapRoom.Direction.E)
                        {
                            possibleDirections.Remove(MapRoom.Direction.W);
                        }
                        else if (roomDirections[roomCounter - 1] == MapRoom.Direction.W)
                        {
                            possibleDirections.Remove(MapRoom.Direction.E);
                        }
                    }

                    //Add random direction
                    roomDirections.Add(possibleDirections[UnityEngine.Random.Range(0, possibleDirections.Count)]);
                }

                //Map size will fit all possible maps
                Vector2Int mapSize = new Vector2Int(((rooms.Count - 1) * 2) + 1, rooms.Count);
                __instance.app.model.mapModel.rooms = new MapRoom[mapSize.x, mapSize.y];

                //Set map size
                __instance.app.model.mapModel.mapSize = mapSize;

                //Reset map
                AccessTools.Method(typeof(MapCreationController), "FillMapWithClosedRooms").Invoke(__instance, null);

                //Start at middle bottom of map
                Vector2Int startRoomPosition = new Vector2Int(rooms.Count - 1, 0);
                //Track current room position
                Vector2Int currentPosition = new Vector2Int(startRoomPosition.x, startRoomPosition.y);
                //Track number of treasure rooms
                int treasureRoomCount = 0;

                //Generate map
                for (int roomCounter = 0; roomCounter < rooms.Count; roomCounter++)
                {
                    MapRoom currentRoom = __instance.app.model.mapModel.rooms[currentPosition.x, currentPosition.y];
                    currentRoom.roomType = rooms[roomCounter];
                    currentRoom.cleared = false;

                    //Set start room
                    if (roomCounter == 0)
                    {
                        __instance.app.model.mapModel.currentRoom = currentRoom;
                    }

                    //Add room difficulty
                    if (currentRoom.roomType == MapRoom.RoomType.Start || currentRoom.roomType == MapRoom.RoomType.EnemyEncounter)
                    {
                        currentRoom.difficulty = roomCounter == 0 ? 1 : 2;
                    }

                    //Set treasure room number and type
                    if (currentRoom.roomType == MapRoom.RoomType.Treasure)
                    {
                        treasureRoomCount++;
                        currentRoom.treasureNo = treasureRoomCount;

                        if (__instance.app.model.characterSheet.currentFloor == 1)
                        {
                            currentRoom.treasureRoomType = MapRoom.TreasureRoomType.Spell;
                        }
                        else if (__instance.app.model.characterSheet.currentFloor == 2)
                        {
                            currentRoom.treasureRoomType = MapRoom.TreasureRoomType.Trinket;
                        }
                        else
                        {
                            currentRoom.treasureRoomType = MapRoom.TreasureRoomType.Default;
                        }
                    }

                    //Add entrance door
                    if (roomCounter > 0 && roomDirections.Count > 0)
                    {
                        MapRoom.Direction previousRoomDirection = roomDirections[roomCounter - 1];
                        if (previousRoomDirection == MapRoom.Direction.E)
                        {
                            currentRoom.AddDoor(MapRoom.Direction.W);
                        }
                        else if (previousRoomDirection == MapRoom.Direction.W)
                        {
                            currentRoom.AddDoor(MapRoom.Direction.E);
                        }
                    }

                    if (currentRoom.roomType != MapRoom.RoomType.Boss)
                    {
                        //Add exit door
                        currentRoom.AddDoor(roomDirections[roomCounter]);

                        //Add room direction
                        currentRoom.exitDirection = roomDirections[roomCounter];

                        //Update current position
                        if (roomDirections[roomCounter] == MapRoom.Direction.N)
                        {
                            currentPosition.y++;
                        }
                        else
                        {
                            currentPosition.x += roomDirections[roomCounter] == MapRoom.Direction.E ? 1 : -1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Increments Bum-bo the Wise win progression
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.FinishFloor))]
        static void BumboController_FinishFloor(BumboController __instance)
        {
            CharacterSheet characterSheet = __instance.app.model.characterSheet;
            if (characterSheet.currentFloor == 5 && characterSheet.bumboType == (CharacterSheet.BumboType)10)
            {
                WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
                windfallPersistentData.wiseWins++;
                if (characterSheet.coins >= 45) windfallPersistentData.wiseMoneyWins++;
                WindfallPersistentDataController.SaveData(windfallPersistentData);
            }
        }

        /// <summary>
        /// Grants Windfall unlocks
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(BumboUnlockController), "Start")]
        static void BumboUnlockController_Start(BumboUnlockController __instance)
        {
            List<int> unlocks = (List<int>)AccessTools.Field(typeof(BumboUnlockController), "unlocks").GetValue(__instance);
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();

            //Bum-bo the Wise: Beat The Basement
            if (!windfallPersistentData.unlocks[0] && __instance.progress.wins > 0)
            {
                windfallPersistentData.unlocks[0] = true;
                if (__instance.progress.wins == 1) unlocks.Add(45); //Do not show Bum-bo the Wise unlock unless this is the first win
            }

            //Plasma Ball: Win once as Bum-bo the Wise
            if (!windfallPersistentData.unlocks[1] && windfallPersistentData.wiseWins > 0)
            {
                windfallPersistentData.unlocks[1] = true;
                unlocks.Add(46);
            }

            //Magnifying Glass: Win twice as Bum-bo the Wise
            if (!windfallPersistentData.unlocks[2] && windfallPersistentData.wiseWins > 1)
            {
                windfallPersistentData.unlocks[2] = true;
                unlocks.Add(47);
            }

            //Compost Bag: Win as Bum-bo the Wise with 45+ coins
            if (!windfallPersistentData.unlocks[3] && windfallPersistentData.wiseMoneyWins > 1)
            {
                windfallPersistentData.unlocks[3] = true;
                unlocks.Add(48);
            }

            AccessTools.Field(typeof(BumboUnlockController), "unlocks").SetValue(__instance, unlocks);
            WindfallPersistentDataController.SaveData(windfallPersistentData);
        }

        /// <summary>
        /// Imports new unlock page materials and text keys
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(BumboUnlockController), "Start")]
        static void BumboUnlockController_Start_UnlockImage(BumboUnlockController __instance)
        {
            UnlockImageView unlockImageView = __instance.app.view.unlockImageView;

            //Unlock page materials
            List<Material> unlockMaterials = new List<Material>();
            unlockMaterials.AddRange(unlockImageView.unlockMaterials);

            Material newMaterial = new Material(unlockMaterials[unlockMaterials.Count - 1]); //Copy last material
            newMaterial.mainTexture = Windfall.assetBundle.LoadAsset<Texture2D>("Unlocks 1"); //Change to new texture
            unlockMaterials.Add(newMaterial);

            unlockImageView.unlockMaterials = unlockMaterials.ToArray();

            //Text keys
            List<string> unlockKeys = new List<string>();

            for (int unlockKeyCounter = 0; unlockKeyCounter <= 48; unlockKeyCounter++)
            {
                string unlockKey = string.Empty;
                if (unlockKeyCounter <= 40) unlockKey = unlockImageView.unlockKeys[unlockKeyCounter];
                else
                {
                    switch (unlockKeyCounter)
                    {
                        case 45:
                            unlockKey = "BUMBO_THE_WISE";
                            break;
                        case 46:
                            unlockKey = "PLASMA_BALL";
                            break;
                        case 47:
                            unlockKey = "MAGNIFYING_GLASS";
                            break;
                        case 48:
                            unlockKey = "COMPOST_BAG";
                            break;
                    }
                }

                unlockKeys.Add(unlockKey);
            }

            unlockImageView.unlockKeys = unlockKeys.ToArray();
        }

        //Patch: Resets mod progression when progress is deleted
        [HarmonyPostfix, HarmonyPatch(typeof(TitleController), nameof(TitleController.DeleteProgress))]
        static void TitleController_DeleteProgress()
        {
            WindfallPersistentDataController.ResetProgression();
        }

        /// <summary>
        /// Fixes jackpot achievements according to the current platform
        /// </summary>
        private static void PatchAchievementsUnlock()
        {
            if (Windfall.achievementsSteam)
            {
                var mOriginal = AccessTools.Method(typeof(AchievementsSteam), "Unlock");
                var mPrefix = AccessTools.Method(typeof(OtherChanges), nameof(AchievementsSteam_Unlock));
                if (mOriginal != null && mPrefix != null)
                {
                    Windfall.harmony.Patch(mOriginal, new HarmonyMethod(mPrefix));
                }
            }

            if (Windfall.achievementsGOG)
            {
                var mOriginal = AccessTools.Method(typeof(AchievementsGOG), "Unlock");
                var mPrefix = AccessTools.Method(typeof(OtherChanges), nameof(AchievementsGOG_Unlock));
                if (mOriginal != null && mPrefix != null)
                {
                    Windfall.harmony.Patch(mOriginal, new HarmonyMethod(mPrefix));
                }
            }

            if (Windfall.achievementsEGS)
            {
                var mOriginal = AccessTools.Method(typeof(AchievementsEGS), "Unlock");
                var mPrefix = AccessTools.Method(typeof(OtherChanges), nameof(AchievementsEGS_Unlock));
                if (mOriginal != null && mPrefix != null)
                {
                    Windfall.harmony.Patch(mOriginal, new HarmonyMethod(mPrefix));
                }
            }
        }

        //Patch: Fixes jackpot unlocks triggering the wrong achievements (Steam)
        static void AchievementsSteam_Unlock(AchievementsSteam __instance, ref Achievements.eAchievement Achievement)
        {
            //Change achievement unlock index when using BumboUnlockController
            BumboUnlockController bumboUnlockController = GameObject.FindObjectOfType<BumboUnlockController>();
            if (bumboUnlockController != null)
            {
                if ((int)Achievement >= (int)Achievements.eAchievement.GOLDEN_GOD)
                {
                    Achievement -= 21;
                }
            }
        }

        //Patch: Fixes jackpot unlocks triggering the wrong achievements (GOG)
        static void AchievementsGOG_Unlock(AchievementsGOG __instance, ref Achievements.eAchievement Achievement)
        {
            //Change achievement unlock index when using BumboUnlockController
            BumboUnlockController bumboUnlockController = GameObject.FindObjectOfType<BumboUnlockController>();
            if (bumboUnlockController != null)
            {
                if ((int)Achievement >= (int)Achievements.eAchievement.GOLDEN_GOD)
                {
                    Achievement -= 21;
                }
            }
        }

        //Patch: Fixes jackpot unlocks triggering the wrong achievements (EGS)
        static void AchievementsEGS_Unlock(AchievementsEGS __instance, ref Achievements.eAchievement Achievement)
        {
            //Change achievement unlock index when using BumboUnlockController
            BumboUnlockController bumboUnlockController = GameObject.FindObjectOfType<BumboUnlockController>();
            if (bumboUnlockController != null)
            {
                if ((int)Achievement >= (int)Achievements.eAchievement.GOLDEN_GOD)
                {
                    Achievement -= 21;
                }
            }
        }
    }
}