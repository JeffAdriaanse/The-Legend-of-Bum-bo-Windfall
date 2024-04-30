using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using DG.Tweening;
using System.Xml;
using System.IO;
using System.Text;
using System.Reflection;
using PathologicalGames;
using System.Collections;
using DG.Tweening.Plugins.Core.PathCore;

namespace The_Legend_of_Bum_bo_Windfall
{
    //Saved state methods are high priority to avoid them being skipped
    //[HarmonyPriority(Priority.First)]
    class SaveChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(SaveChanges));
        }

        //Patch: Fixes copying old saves
        [HarmonyPostfix, HarmonyPatch(typeof(SaveSystemPC), "update_save")]
        static void SaveSystemPC_update_save(SaveSystemPC __instance, string Filename)
        {
            //Only delete saved state, not progression
            if (Filename != "state.sav" && Filename != "state.sav.xml")
            {
                return;
            }

            string newPath = __instance.GetSaveDirectory() + "/" + Filename;
            string oldPath = Application.persistentDataPath + "/" + Filename;

            if (File.Exists(newPath) && File.Exists(oldPath))
            {
                bool duplicateSave = false;

                byte[] newPathBytes = File.ReadAllBytes(newPath);
                byte[] oldPathBytes = File.ReadAllBytes(oldPath);

                if (newPathBytes.Length == oldPathBytes.Length)
                {
                    duplicateSave = true;

                    for (int i = 0; i < newPathBytes.Length; i++)
                    {
                        if (newPathBytes[i] != oldPathBytes[i])
                        {
                            duplicateSave = false;
                            break;
                        }
                    }
                }

                if (duplicateSave)
                {
                    File.Delete(oldPath);
                }
            }
        }

        //Patch: Load start
        [HarmonyPostfix, HarmonyPatch(typeof(SavedStateController), nameof(SavedStateController.LoadStart))]
        static void SavedStateController_LoadStart(SavedStateController __instance)
        {
            WindfallSavedState.LoadStart(__instance.app);
            Console.WriteLine("[The Legend of Bum-bo: Windfall] LoadStart");
        }

        //Patch: Load end
        [HarmonyPostfix, HarmonyPatch(typeof(SavedStateController), nameof(SavedStateController.LoadEnd))]
        static void SavedStateController_LoadEnd()
        {
            WindfallSavedState.LoadEnd();
            Console.WriteLine("[The Legend of Bum-bo: Windfall] LoadEnd");
        }

        //Patch: Loads modified character sheet when loading into the Wooden Nickel
        [HarmonyPrefix, HarmonyPatch(typeof(SavedStateController), nameof(SavedStateController.LoadCharacterSheet))]
        static bool SavedStateController_LoadCharacterSheet_Windfall(SavedStateController __instance)
        {
            if (WindfallSavedState.LoadIntoWoodenNickel(__instance.app))
            {
                WindfallSavedState.LoadCharacterSheet(__instance.app);
                return false;
            }
            return true;
        }

        //Patch: Loads directly to the Wooden Nickel
        [HarmonyPrefix, HarmonyPatch(typeof(FloorStartEvent), nameof(FloorStartEvent.Execute))]
        static bool FloorStartEvent_Execute_Prefix(FloorStartEvent __instance)
        {
            WindfallSavedState.LoadStart(__instance.app);

            Console.WriteLine("[The Legend of Bum-bo: Windfall] FloorStartEvent");

            if (WindfallSavedState.LoadIntoWoodenNickel(__instance.app))
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] FloorStartEvent: LoadIntoWoodenNickel");

                //Abort loading into Wooden Nickel in the case of an incompatible save
                if (!WindfallSavedState.IsLoading())
                {
                    WindfallSavedState.LoadEnd();
                    return true;
                }

                WindfallSavedState.LoadEnd();

                __instance.app.controller.Init();

                //Pause level music
                LevelMusicView levelMusicView = __instance.app.view.levelMusicView;
                if (levelMusicView != null)
                {
                    AudioSource audioSource = levelMusicView.gameObject.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.Pause();
                    }
                }

                __instance.app.controller.savedStateController.LoadEnd();
                __instance.app.controller.FinishFloor();

                LoadingController loadingController = UnityEngine.Object.Instantiate(Resources.Load("Loading") as GameObject).GetComponent<LoadingController>();
                loadingController.loadScene("gambling");
                return false;
            }

            WindfallSavedState.LoadEnd();
            return true;
        }

        //Patch: Saves the shop when GamblingEvent is executed
        [HarmonyPostfix, HarmonyPatch(typeof(BumboEvent), nameof(BumboEvent.Execute))]
        static void BumboEvent_Execute(BumboEvent __instance)
        {
            if (__instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent" && __instance.app.controller.gamblingController != null)
            {
                WindfallSavedState.SaveShop(__instance.app.controller.gamblingController.shop);
            }
        }

        //Patch: Saves the shop when the stat wheel finishes spinning
        [HarmonyPostfix, HarmonyPatch(typeof(WheelSpin), "MakeWheelClickable")]
        static void WheelSpin_MakeWheelClickable(WheelSpin __instance)
        {
            if (__instance.app.controller.gamblingController != null)
            {
                WindfallSavedState.SaveShop(__instance.app.controller.gamblingController.shop);
            }
        }

        //Patch: Saves coins when spending money at the Wooden Nickel
        [HarmonyPostfix, HarmonyPatch(typeof(GamblingController), nameof(GamblingController.ModifyCoins))]
        static void GamblingController_ModifyCoins(GamblingController __instance)
        {
            WindfallSavedState.SaveCoins(__instance.app);
        }

        //Patch: Saves damage taken
        [HarmonyPostfix, HarmonyPatch(typeof(CharacterSheet), nameof(CharacterSheet.RegisterDamage))]
        static void CharacterSheet_RegisterDamage(CharacterSheet __instance, float damage)
        {
            if (damage > 0f)
            {
                WindfallSavedState.SaveDamageTaken(damage);
            }
        }

        //Patch: Loads damage taken
        [HarmonyPostfix, HarmonyPatch(typeof(SavedStateController), nameof(SavedStateController.LoadCharacterSheet))]
        static void SavedStateController_LoadCharacterSheet_DamageTaken(SavedStateController __instance)
        {
            WindfallSavedState.LoadDamageTaken(__instance.app);
        }

        //Patch: Removes navigation arrows and moves camera when loading directly into a treasure room (namely, when reloading a treasure room save)
        [HarmonyPostfix, HarmonyPatch(typeof(FloorStartEvent), nameof(FloorStartEvent.Execute))]
        static void FloorStartEvent_Execute_Treasure_Room(FloorStartEvent __instance)
        {
            if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure)
            {
                __instance.app.view.navigation.arrowNorth.SetActive(false);
                __instance.app.view.navigation.arrowSouth.SetActive(false);
                __instance.app.view.navigation.arrowEast.SetActive(false);
                __instance.app.view.navigation.arrowWest.SetActive(false);

                //Move camera to normal position for treasure room
                //Transform values copied from MoveIntoRoomEvent
                __instance.app.view.mainCameraView.transform.position = new Vector3(0f, 1f, -4.29f);
                __instance.app.view.mainCameraView.transform.eulerAngles = new Vector3(8.2f, 1.33f, 0f);
            }
        }

        //Log mainCamera tweens on MoveIntoRoomEvent
        [HarmonyPostfix, HarmonyPatch(typeof(MoveIntoRoomEvent), nameof(MoveIntoRoomEvent.Execute))]
        static void MoveIntoRoomEvent_Execute(MoveIntoRoomEvent __instance, NavigationArrowView.Direction ___direction)
        {
            List<Tween> gameObjectTweens = DOTween.TweensByTarget(__instance.app.view.mainCamera);
            if (gameObjectTweens != null)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] mainCamera gameObject tweens count: " + gameObjectTweens.Count);
            }

            List<Tween> transformTweens = DOTween.TweensByTarget(__instance.app.view.mainCamera.transform);
            if (transformTweens != null)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] mainCamera transform tweens count: " + transformTweens.Count);
            }
        }

        //Track MoveIntoRoomEvent sequence
        static Sequence moveIntoRoomEventSequence;
        //Patch: Tracks sequence for WrapAround
        [HarmonyPrefix, HarmonyPatch(typeof(MoveIntoRoomEvent), nameof(MoveIntoRoomEvent.WrapAround))]
        static bool MoveIntoRoomEvent_WrapAround(MoveIntoRoomEvent __instance, NavigationArrowView.Direction ___direction)
        {
            __instance.app.view.mainCameraView.transform.position = new Vector3(__instance.app.view.mainCamera.transform.position.x * -1f, __instance.app.view.mainCamera.transform.position.y, __instance.app.view.mainCamera.transform.position.z);
            if (__instance.app.model.mapModel.currentRoom.roomType != MapRoom.RoomType.Treasure)
            {
                Sequence sequence = DOTween.Sequence();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Join(TweenSettingsExtensions.Append(TweenSettingsExtensions.Join(TweenSettingsExtensions.Append(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMove(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position + new Vector3((___direction != NavigationArrowView.Direction.Left) ? 0.333f : -0.333f, 0f, 0f), 0.35f, false), Ease.OutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.rotation.eulerAngles + new Vector3(0f, 0f, (___direction != NavigationArrowView.Direction.Left) ? -3f : 3f), 0.35f, 0), Ease.OutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMove(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position, 0.15f, false), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.rotation.eulerAngles, 0.15f, 0), Ease.InOutQuad)), delegate ()
                {
                    __instance.ResetRoom();
                });

                //Track sequence
                moveIntoRoomEventSequence = sequence;
            }
            else
            {
                __instance.ResetRoom();
            }
            return false;
        }
        //Patch: Tracks sequence for HopInFront
        [HarmonyPrefix, HarmonyPatch(typeof(MoveIntoRoomEvent), nameof(MoveIntoRoomEvent.HopInFront))]
        static bool MoveIntoRoomEvent_HopInFront(MoveIntoRoomEvent __instance, NavigationArrowView.Direction ___direction)
        {
            __instance.app.view.mainCameraView.transform.position = __instance.app.model.cameraNavigationPosition.position + new Vector3(0f, 4.5f, -4f);
            if (__instance.app.model.mapModel.currentRoom.roomType != MapRoom.RoomType.Treasure)
            {
                Sequence sequence = DOTween.Sequence();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.Join(TweenSettingsExtensions.Join(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveZ(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position.z, 0.5f, false), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveY(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position.y, 0.5f, false), Ease.InOutQuad)), 0.05f, delegate ()
                {
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().ChangeLight(EnemyRoomView.RoomLightScheme.Default, 0.1f, true);
                }), delegate ()
                {
                    __instance.ResetRoom();
                });
                //Track sequence
                moveIntoRoomEventSequence = sequence;
            }
            else
            {
                __instance.ResetRoom();
            }
            return false;
        }
        //Patch: Tracks sequence for HopInBack
        [HarmonyPrefix, HarmonyPatch(typeof(MoveIntoRoomEvent), nameof(MoveIntoRoomEvent.HopInBack))]
        static bool MoveIntoRoomEvent_HopInBack(MoveIntoRoomEvent __instance, NavigationArrowView.Direction ___direction)
        {
            __instance.app.view.mainCameraView.transform.position = __instance.app.model.cameraNavigationPosition.position + new Vector3(0f, 4.5f, 6f);
            if (__instance.app.model.mapModel.currentRoom.roomType != MapRoom.RoomType.Treasure)
            {
                Sequence sequence = DOTween.Sequence();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.Join(TweenSettingsExtensions.Join(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveZ(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position.z, 0.5f, false), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveY(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position.y, 0.5f, false), Ease.InOutQuad)), 0.05f, delegate ()
                {
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().ChangeLight(EnemyRoomView.RoomLightScheme.Default, 0.1f, true);
                }), delegate ()
                {
                    __instance.ResetRoom();
                });
                //Track sequence
                moveIntoRoomEventSequence = sequence;
            }
            else
            {
                __instance.ResetRoom();
            }
            return false;
        }

        //Patch: Overrides BumboController StartRoom
        //Saves the game when entering treasure rooms
        //Waits for rooms to be initialized before saving, which ensures that the puzzle board is saved properly
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), nameof(BumboController.StartRoom))]
        static bool BumboController_StartRoom(BumboController __instance)
        {
            __instance.boxController.ChangeRoomBox();
            __instance.app.view.decorationView.ShowDecorations();
            if (__instance.app.model.mapModel.currentRoom.coins != null && __instance.app.model.mapModel.currentRoom.coins.Length > 0)
            {
                short num = 0;
                while ((int)num < __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration.Length)
                {
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration[(int)num].gameObject.SetActive(__instance.app.model.mapModel.currentRoom.coins[(int)num]);
                    num += 1;
                }
            }
            else
            {
                __instance.app.model.mapModel.currentRoom.coins = new bool[__instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration.Length];
                short num2 = 0;
                while ((int)num2 < __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration.Length)
                {
                    __instance.app.model.mapModel.currentRoom.coins[(int)num2] = false;
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration[(int)num2].gameObject.SetActive(false);
                    num2 += 1;
                }
            }
            __instance.app.view.reflectionProbe.RenderProbe();
            short num3 = (short)((__instance.app.view.shitView.shit.Count - 3) / 2);
            short num4 = (short)(__instance.app.view.shitView.shit.Count - (int)num3 - 1);
            __instance.app.view.shitView.availableShit.Clear();
            for (int i = 0; i < __instance.app.view.shitView.shit.Count; i++)
            {
                if (i >= (int)num3 && i <= (int)num4)
                {
                    __instance.app.view.shitView.shit[i].gameObject.SetActive(true);
                    __instance.app.view.shitView.shit[i].Reset();
                    __instance.app.view.shitView.availableShit.Add(__instance.app.view.shitView.shit[i]);
                }
                else
                {
                    __instance.app.view.shitView.shit[i].gameObject.SetActive(false);
                }
            }
            for (short num5 = 0; num5 < 6; num5 += 1)
            {
                __instance.app.model.mana[(int)num5] = 0;
            }
            __instance.SetActiveSpells(true, true);
            __instance.StartRoomWithTrinkets();
            __instance.StartRoundWithTrinkets();
            __instance.app.model.spellModel.spellQueued = false;
            if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure && !__instance.app.model.mapModel.currentRoom.cleared)
            {
                __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>().InitiateRoom();
            }
            else if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Shop && !__instance.app.model.mapModel.currentRoom.cleared)
            {
                __instance.app.view.boxes.shopRoom.GetComponent<ShopRoomView>().shop.Init();
            }

            Console.WriteLine("[The Legend of Bum-bo: Windfall] RoomStart");

            if (__instance.app.controller.savedStateController.IsLoading())
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Is Loading");

                if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Boss)
                {
                    if (__instance.app.model.characterSheet.currentFloor == 4)
                    {
                        __instance.app.view.levelMusicView.PlayFinalBossMusic();
                    }
                    else
                    {
                        __instance.app.view.levelMusicView.PlayBossMusic();
                    }
                }
                __instance.app.controller.savedStateController.LoadEnd();
            }
            else if (__instance.app.model.characterSheet.currentFloor != 0 && (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.EnemyEncounter || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Boss || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Start || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure) && !__instance.app.model.mapModel.currentRoom.cleared)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Isn't Loading");

                if (moveIntoRoomEventSequence != null && moveIntoRoomEventSequence.IsPlaying())
                {
                    //Wait for room to be initialized
                    moveIntoRoomEventSequence.OnComplete(delegate
                    {
                        Console.WriteLine("[The Legend of Bum-bo: Windfall] Waited for room to initialize before saving");
                        __instance.app.controller.savedStateController.Save();
                    });
                }
                else
                {
                    Console.WriteLine("[The Legend of Bum-bo: Windfall] Didn't wait for room to initialize before saving");
                    __instance.app.controller.savedStateController.Save();
                }
            }
            return false;
        }

        //Patch: Creates new empty enemy ground/air layouts if they are null, preventing a NullReferenceException
        //Enemy layouts will be null when entering a boss room after reloading from the preceding treasure room
        [HarmonyPrefix, HarmonyPatch(typeof(SavedStateController), "Save")]
        static bool SavedStateController_Save(SavedStateController __instance)
        {
            //Enemy layout
            if (__instance.app.model.mapModel.currentRoomEnemyLayout == null)
            {
                __instance.app.model.mapModel.currentRoomEnemyLayout = new EnemyLayout();
            }

            //Ground enemies
            if (__instance.app.model.mapModel.currentRoomEnemyLayout.groundEnemies == null)
            {
                __instance.app.model.mapModel.currentRoomEnemyLayout.groundEnemies = new List<List<EnemyName>>();
            }
            //Air enemies
            if (__instance.app.model.mapModel.currentRoomEnemyLayout.airEnemies == null)
            {
                __instance.app.model.mapModel.currentRoomEnemyLayout.airEnemies = new List<List<EnemyName>>();
            }

            return true;
        }

        //Patch: Saves windfall state
        //Must happen after vanilla state is saved for tracking vanilla save data
        [HarmonyPostfix, HarmonyPatch(typeof(SavedStateController), "Save")]
        static void SavedStateController_Save_Postfix(SavedStateController __instance)
        {
            WindfallSavedState.Save(__instance.app);
        }

        //Patch: Loads saved pickups when reloading a save in a treasure room
        [HarmonyPostfix, HarmonyPatch(typeof(TreasureRoom), "InitiateRoom")]
        static void TreasureRoom_InitiateRoom(TreasureRoom __instance, ref List<GameObject> ___pickups)
        {
            if (WindfallSavedState.IsLoading())
            {
                WindfallSavedState.LoadTreasure(__instance, ___pickups);
            }
        }

        //Patch: Loads boss room pickups
        [HarmonyPostfix, HarmonyPatch(typeof(SavedStateController), "LoadBoss")]
        static void SavedStateController_LoadBoss()
        {
            WindfallSavedState.LoadBoss();
        }

        //Patch: Changes boss room pickups to loaded pickups
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "CreateBossRewards")]
        static void BumboController_CreateBossRewards(BumboController __instance)
        {
            if (WindfallSavedState.bossRewards != null)
            {
                __instance.app.view.bossRewardParents[0].transform.GetChild(0).GetComponent<TrinketPickupView>().SetTrinket(WindfallSavedState.bossRewards[0], 0);
                __instance.app.view.bossRewardParents[1].transform.GetChild(0).GetComponent<TrinketPickupView>().SetTrinket(WindfallSavedState.bossRewards[1], 0);
            }
        }

        //Patch: Loads trinket uses
        [HarmonyPostfix, HarmonyPatch(typeof(SavedStateController), "LoadCharacterSheet")]
        static void SavedStateController_LoadCharacterSheet(SavedStateController __instance)
        {
            WindfallSavedState.LoadTrinketUses(__instance.app);
            WindfallSavedState.LoadGlitchedTrinkets(__instance.app);
            __instance.app.controller.UpdateTrinkets();
        }

        //Overrides MakeAChampion method to load champion enemies
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), nameof(BumboController.MakeAChampion))]
        static bool BumboController_MakeAChampion(BumboController __instance)
        {
            if (WindfallSavedState.IsLoading())
            {
                WindfallSavedState.LoadChampionEnemies(__instance.app);
                return false;
            }

            return true;
        }
    }
}
