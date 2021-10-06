using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using TMPro;
using DG.Tweening;

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
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Offsetting position of newly spawned ManaDrainView object; object was offset " + offsetCounter + (offsetCounter == 1 ? " time" : " times"));
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
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Correcting location of audiosource retrieved by options menu; menu was opened in the Wooden Nickel");
                return false;
            }
            return true;
        }

        //Patch: Fixed toggle spell menu arrow persisting after leaving a treasure room if the treasure room was exited in a certain way
        [HarmonyPostfix, HarmonyPatch(typeof(RoomStartEvent), "Execute")]
        static void RoomStartEvent_Execute(RoomStartEvent __instance)
        {
            __instance.app.view.GUICamera.GetComponent<GUISide>().expandGUIView.Hide();
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Hiding expandGUIView on room start");
        }

        //Patch: Fixed spell HUD clipping into the camera during boss VS screens if the spell HUD was open when exiting the treasure room
        [HarmonyPatch(typeof(GUISide), "HideHud")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count - 1; i++)
            {
                //Checking for IL code
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == -0.5f)
                {
                    //Change operands
                    code[i].operand = -1f;
                    break;
                }
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing spell HUD from clipping into the camera");
            return code;
        }

        //Patch: Fixed Bum-bo's popsicle stick clipping into the camera view during the Mega Chomper tile combo animation
        [HarmonyPostfix, HarmonyPatch(typeof(ToothMegaAttackEvent), "Execute")]
        static void ToothMegaAttackEvent_Execute(ToothMegaAttackEvent __instance)
        {
            GameObject popsicle = null;
            for (int childIndex = 0; childIndex < __instance.app.view.bumboThrow.transform.childCount; childIndex++)
            {
                if (__instance.app.view.bumboThrow.transform.GetChild(childIndex).name == "Popsicle")
                {
                    popsicle = __instance.app.view.bumboThrow.transform.GetChild(childIndex).gameObject;
                }
            }
            
            if (popsicle != null)
            {
                popsicle.SetActive(false);
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Hiding Bum-bo popsicle stick during Mega Chomper tile combo animation");
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(2f);
                sequence.AppendCallback(delegate ()
                {
                    popsicle.SetActive(true);
                });
            }
        }

        //Patch: Fixes rooms staying darkened after being cleared during ChanceToCastSpellEvent
        [HarmonyPostfix, HarmonyPatch(typeof(RoomEndEvent), "Execute")]
        static void RoomEndEvent_Execute(RoomEndEvent __instance)
        {
            if (!__instance.app.model.defaultLightOn)
            {
                __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().ChangeLight(EnemyRoomView.RoomLightScheme.Default, 0.2f, true);
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing to default light scheme on room clear");
        }

        //Patch: Fixes boss room darkening when opening the spell menu while choosing a trinket if the boss was defeated during ChanceToCastSpellEvent
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "ShowEndTurnSign")]
        static bool BumboController_ShowEndTurnSign(BumboController __instance)
        {
            if (__instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent")
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Aborting ShowEndTurnSign; current event is BossDyingEvent");
                return false;
            }
            return true;
        }

        //Patch: Fixes spell ready notifications appearing before the SideGUI has expanded
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "SetActiveSpells")]
        static bool BumboController_SetActiveSpells(BumboController __instance)
        {
            int num = 0;
            short num2 = 0;
            while ((int)num2 < __instance.app.model.characterSheet.spells.Count)
            {
                bool flag = __instance.app.model.characterSheet.spells[(int)num2].IsReady();
                if (__instance.app.model.characterSheet.spells[(int)num2].IsChargeable)
                {
                    __instance.app.view.spells[(int)num2].spellMana1.transform.GetChild(0).GetComponent<TextMeshPro>().text = __instance.app.model.characterSheet.spells[(int)num2].charge + " / " + __instance.app.model.characterSheet.spells[(int)num2].requiredCharge;
                }
                if (flag)
                {
                    num++;
                    if (__instance.app.view.spells[(int)num2].spellContainer.GetComponent<MeshRenderer>().material.GetTextureOffset("_MainTex") != new Vector2(0f, -0.5f))
                    {
                        __instance.app.view.spells[(int)num2].spellContainer.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, -0.5f));
                        if (__instance.app.view.sideGUI.activeSelf)
                        {
                            __instance.app.view.spellReadyView.spellReady[(int)num2].SetActive(true);
                            DOTween.Kill("spellready" + num2, false);
                            __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition = new Vector3(0.5f, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.y, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.z);
                            Sequence sequence = DOTween.Sequence();
                            TweenSettingsExtensions.SetId<Sequence>(sequence, "spellready" + num2);
                            TweenSettingsExtensions.Append(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOLocalMove(__instance.app.view.spellReadyView.spellReady[(int)num2].transform, new Vector3(0f, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.y, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.z), 1f, false), Ease.OutBounce));
                            TweenSettingsExtensions.OnComplete<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(sequence, 0.5f), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOLocalMove(__instance.app.view.spellReadyView.spellReady[(int)num2].transform, new Vector3(0.5f, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.y, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.z), 0.5f, false), Ease.InQuad)), delegate ()
                            {
                                __instance.HideAllSpellReady();
                            });
                        }
                        else
                        {
                            Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing spell ready notification from appearing; SideGUI is not expanded");
                        }
                    }
                }
                else
                {
                    __instance.app.model.spellJustNowReady[(int)num2] = false;
                    __instance.app.view.spells[(int)num2].spellContainer.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, -0.25f));
                }
                __instance.app.view.spells[(int)num2].SetActive(flag);
                switch (__instance.app.model.characterSheet.spells[(int)num2].Category)
                {
                    case SpellElement.SpellCategory.Defense:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypeDefense.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].defenseObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            if (!__instance.app.model.spellJustNowReady[(int)num2])
                            {
                                __instance.app.model.spellJustNowReady[(int)num2] = true;
                                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.DefenseSpellReady, SoundsView.eAudioSlot.Default, false);
                            }
                            __instance.app.view.spells[(int)num2].spellTypeDefense.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].defenseObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            }
                        }
                        break;
                    case SpellElement.SpellCategory.Puzzle:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypePuzzle.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].puzzleObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            if (!__instance.app.model.spellJustNowReady[(int)num2])
                            {
                                __instance.app.model.spellJustNowReady[(int)num2] = true;
                                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.PuzzleSpellReady, SoundsView.eAudioSlot.Default, false);
                            }
                            __instance.app.view.spells[(int)num2].spellTypePuzzle.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].puzzleObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        break;
                    case SpellElement.SpellCategory.Use:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypeItem.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].itemObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            __instance.app.view.spells[(int)num2].spellTypeItem.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].itemObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        break;
                    case SpellElement.SpellCategory.Other:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypeSpecial.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].specialObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            __instance.app.view.spells[(int)num2].spellTypeSpecial.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].specialObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        break;
                    default:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypeAttack.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].attackObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            if (!__instance.app.model.spellJustNowReady[(int)num2])
                            {
                                __instance.app.model.spellJustNowReady[(int)num2] = true;
                                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.AttackSpellReady, SoundsView.eAudioSlot.Default, false);
                            }
                            __instance.app.view.spells[(int)num2].spellTypeAttack.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].attackObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            }
                        }
                        break;
                }
                num2 += 1;
            }
            return false;
        }

        //Patch: Fixes the opposing navigation arrow remaining clickable while the character select screen is still rotating
        [HarmonyPrefix, HarmonyPatch(typeof(RotateCharacters), "setClick")]
        static bool RotateCharacters_setClick(bool _enabled)
        {
            RotateCharacters[] navArrows = UnityEngine.Object.FindObjectsOfType<RotateCharacters>();
            for (int i = 0; i < navArrows.Length; i++)
            {
                navArrows[i].GetComponent<BoxCollider>().enabled = _enabled;
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Disabling opposing navigation arrow on RotateCharacters setClick");
            return false;
        }

        //Patch: Fixes back button staying clickable after selecting a character
        [HarmonyPostfix, HarmonyPatch(typeof(ChooseBumbo), "OnMouseDown")]
        static void ChooseBumbo_OnMouseDown(ChooseBumbo __instance)
        {
            GameObject.Find("BackButton").GetComponent<SelectCharacterBackButton>().active = false;
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Making back button unclickable after selecting a character");
        }

        //Patch: Fixes the pause menu button remaining clickable while the pause menu is already open
        [HarmonyPrefix, HarmonyPatch(typeof(MenuButtonView), "Update")]
        static bool MenuButtonView_Update(MenuButtonView __instance, bool ___showing)
        {
            if (!__instance.app.view.menuView.activeSelf && (__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent"))
            {
                if (!___showing)
                {
                    __instance.Show();
                    return false;
                }
            }
            else if (___showing)
            {
                __instance.Hide();
            }
            return false;
        }

        //Patch: Fixes spell/trinket inventory hover sounds playing while the pause menu is open
        [HarmonyPostfix, HarmonyPatch(typeof(ButtonHoverAnimation), "CheckEnabled")]
        static void ButtonHoverAnimation_CheckEnabled(ButtonHoverAnimation __instance, ref bool __result, SpellView ___spellView, TrinketView ___trinketView)
        {
            if (___spellView != null)
            {
                if (__instance.app.model.paused)
                {
                    __result = false;
                }
            }
            else if (___trinketView != null)
            {
                if (__instance.app.model.paused)
                {
                    __result = false;
                }
            }
        }

        //Patch: Fixes the pause menu opening sound playing when attempting to open the pause menu using hotkeys while it is already open
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "Update")]
        static bool BumboController_Update(BumboController __instance, ref float ___restartTime)
        {
            if (Input.GetKey(KeyCode.R) && __instance.app.model.characterSheet != null && __instance.app.model.characterSheet.currentFloor > 0)
            {
                ___restartTime += Time.deltaTime;
                if (___restartTime >= 2f)
                {
                    ___restartTime = 0f;
                    __instance.app.Notify("gameover.retry", __instance, new object[0]);
                }
            }
            else
            {
                ___restartTime = 0f;
            }
            if (Input.GetKeyDown(KeyCode.Escape) && (__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent") && !__instance.app.view.menuView.activeSelf)
            {
                __instance.app.model.paused = true;
                __instance.app.view.menuView.SetActive(true);
                SoundsView.Instance.PlaySound(SoundsView.eSound.Menu_Appear, SoundsView.eAudioSlot.Default, false);
            }
            if (Input.GetKeyDown(KeyCode.Return) && __instance.app.view.menuView.activeSelf)
            {
                __instance.app.view.menuView.SetActive(false);
                SoundsView.Instance.PlaySound(SoundsView.eSound.Menu_Close, SoundsView.eAudioSlot.Default, false);
                __instance.app.model.paused = false;
            }
            return false;
        }

        //***************************************************
        //**************Shop Trinket Pickups*****************
        //***************************************************
        //These patches prevent trinket/needle pickups from disappearing after quickly canceling a trinket/needle purchase

        [HarmonyPrefix, HarmonyPatch(typeof(TrinketPickupView), "HopAndDisappear")]
        static bool TrinketPickupView_HopAndDisappear(TrinketPickupView __instance)
        {
            Sequence sequence = DOTween.Sequence();
            TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.Insert(TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, new Vector3(1.25f, 0.8f, 1.25f), 0.15f), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, new Vector3(0.8f, 1.25f, 0.8f), 0.15f), Ease.InOutQuad)), 0.225f, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMove(__instance.transform, __instance.transform.position + new Vector3(0f, 0.5f, 0f), 0.15f, false), Ease.OutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, Vector3.one, 0.15f), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMove(__instance.transform, __instance.transform.position + new Vector3(0f, 0f, 0f), 0.15f, false), Ease.InQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, new Vector3(1.5f, 0f, 1.5f), 0.075f), Ease.OutQuad)), delegate ()
            {
                __instance.gameObject.SetActive(false);
            });
            if (__instance.app.view.gamblingView != null)
            {
                sequence.SetId("TrinketPickup");
            }
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TrinketPickupView), "Reappear")]
        static bool TrinketPickupView_Reappear(TrinketPickupView __instance)
        {
            if (__instance.app.view.gamblingView != null)
            {
                DOTween.Kill("TrinketPickup", true);
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemPriceView), "Hide")]
        static bool ItemPriceView_Hide(ItemPriceView __instance)
        {
            Sequence sequence = DOTween.Sequence();
            TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.Insert(sequence, 0.3f, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, new Vector3(1.25f, 0.8f, 1.25f), 0.15f), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, new Vector3(0.8f, 1.25f, 0.8f), 0.15f), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, new Vector3(1.5f, 0f, 1.5f), 0.075f), Ease.OutQuad));
            if (__instance.app.view.gamblingView != null)
            {
                sequence.SetId("ItemPrice");
            }
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemPriceView), "Show")]
        static bool ItemPriceView_Show(ItemPriceView __instance)
        {
            if (__instance.app.view.gamblingView != null)
            {
                DOTween.Kill("ItemPrice", true);
            }
            return true;
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Fixes spell UI not appearing when quickly buying a trinket/needle after canceling a purchase, which could cause a softlock when buying a trinket
        //Patch also causes the shop clerk to wave when a trinket is purchased
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketPickupView), "OnMouseDown")]
        static bool TrinketPickupView_OnMouseDown(TrinketPickupView __instance)
        {
            if (!__instance.app.model.paused)
            {
                if (__instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent" && __instance.app.model.characterSheet.trinkets.Count < 4)
                {
                    __instance.Acquire();
                    __instance.app.controller.eventsController.EndEvent();
                    return false;
                }
                if (__instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent")
                {
                    __instance.app.model.trinketReward = __instance.trinket.trinketName;
                    __instance.app.controller.eventsController.SetEvent(new TrinketReplaceEvent());
                    return false;
                }
                if (__instance.clickable)
                {
                    if (__instance.app.model.bumboEvent.GetType().ToString() == "TreasureStartEvent")
                    {
                        __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>().SetClickable(false);
                        if (__instance.app.model.characterSheet.trinkets.Count < 4)
                        {
                            __instance.Acquire();
                            return false;
                        }
                        __instance.app.model.trinketReward = __instance.trinket.trinketName;
                        __instance.app.controller.eventsController.SetEvent(new TrinketReplaceEvent());
                        return false;
                    }
                    else if (__instance.app.view.gamblingView != null && __instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent" && !__instance.app.view.sideGUI.activeSelf)
                    {
                        if (__instance.app.model.characterSheet.coins >= __instance.AdjustedPrice && (__instance.app.model.characterSheet.trinkets.Count < 4 || __instance.trinket.Category == TrinketElement.TrinketCategory.Prick))
                        {
                            if (__instance.app.model.gamblingModel.cameraAt != 2)
                            {
                                return false;
                            }
                            __instance.app.view.gamblingView.shopClerkView.Wave();
                            __instance.app.view.gamblingView.shopRegisterView.Pay(__instance.AdjustedPrice);
                            __instance.app.controller.ModifyCoins(-__instance.AdjustedPrice);
                            __instance.Acquire();
                        }
                        if (__instance.app.model.bumboEvent.GetType().ToString() != "SpellModifyEvent" && __instance.app.model.characterSheet.coins >= __instance.AdjustedPrice && __instance.app.model.characterSheet.trinkets.Count >= 4)
                        {
                            if (__instance.app.view.gamblingView != null && __instance.app.model.bumboEvent.GetType().ToString() != "TreasureStartEvent")
                            {
                                if (__instance.app.model.gamblingModel.cameraAt != 2)
                                {
                                    return false;
                                }
                                __instance.app.view.gamblingView.shopClerkView.Wave();
                                __instance.app.view.gamblingView.shopRegisterView.Pay(__instance.AdjustedPrice);
                                __instance.app.model.gamblingModel.trinketReward = __instance.trinket.trinketName;
                                __instance.app.controller.gamblingController.selectedShopIdx = __instance.shopIndex;
                                __instance.HopAndDisappear();
                                SoundsView.Instance.PlaySound(SoundsView.eSound.Trinket_Sign_Away, __instance.transform.position, SoundsView.eAudioSlot.Default, false);
                            }
                            __instance.app.controller.ModifyCoins(-__instance.AdjustedPrice);
                            if (__instance.app.view.gamblingView == null)
                            {
                                __instance.app.model.trinketReward = __instance.trinket.trinketName;
                            }
                            __instance.app.controller.eventsController.SetEvent(new TrinketReplaceEvent());
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        //***************************************************
        //************Shop Nav Arrow Clickability************
        //***************************************************
        //These patches disable shop navigation arrows when they are disappearing

        [HarmonyPostfix, HarmonyPatch(typeof(GamblingNavigation), "Hide")]
        static void GamblingNavigation_Hide(GamblingNavigation __instance)
        {
            __instance.MakeClickable(false);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GamblingNavigation), "Show")]
        static void GamblingNavigation_Show(GamblingNavigation __instance)
        {
            __instance.MakeClickable(true);
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Fixes shop navigation arrows not appearing after quickly replacing a trinket while the trinket replacement UI is still opening, potentially resulting in a softlock
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketView), "OnMouseDown")]
        static bool TrinketView_OnMouseDown(TrinketView __instance)
        {
            if (DOTween.IsTweening("ShowingGUI", false))
            {
                return false;
            }
            return true;
        }

        //Patch: Adapts EventsController OnNotification to allow for cancel button functionality when replacing a trinket
        [HarmonyPrefix, HarmonyPatch(typeof(EventsController), "OnNotification")]
        static bool EventsController_OnNotification(EventsController __instance, string _event_path, object _target, params object[] _data)
        {
            //Fixing cancel button in treasure rooms during BumboEvent
            if (_event_path == "cancel.spell" && __instance.app.model.bumboEvent.GetType().ToString() == "BumboEvent" && __instance.app.view.boxes.treasureRoom != null && __instance.app.view.boxes.treasureRoom.gameObject.activeSelf)
            {
                DOTween.KillAll(true);
                __instance.SetEvent(new TreasureChosenEvent());
                return false;
            }
            if (_event_path == "cancel.spell" && (__instance.app.model.bumboEvent.GetType().ToString() == "TreasureStartEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "GrabBossRewardEvent"))
            {
                __instance.EndEvent();
                return false;
            }
            if (_event_path == "cancel.spell" && __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceEvent" && __instance.app.view.gamblingView != null)
            {
                if (__instance.app.model.gamblingModel.cameraAt == 0)
                {
                    __instance.app.controller.eventsController.SetEvent(new TrinketChosenToReplaceEvent(-1));
                    return false;
                }
                __instance.app.controller.HideNotifications(false);
                __instance.app.controller.gamblingController.CancelPickup();
                __instance.SetEvent(new SpellModifyDelayEvent(false));
                return false;
            }
            else
            {
                if (_event_path == "cancel.spell" && __instance.app.view.gamblingView != null)
                {
                    __instance.app.controller.HideNotifications(false);
                    __instance.app.controller.gamblingController.CancelPickup();
                    __instance.SetEvent(new SpellModifyDelayEvent(false));
                }
                else if (_event_path == "cancel.spell" && __instance.app.view.boxes.treasureRoom != null && __instance.app.view.boxes.treasureRoom.gameObject.activeSelf)
                {
                    __instance.app.controller.HideNotifications(false);
                    __instance.SetEvent(new BumboEvent());
                    if (!__instance.app.model.iOS)
                    {
                        __instance.app.controller.HideGUI();
                    }
                    if (!__instance.app.model.iOS)
                    {
                        __instance.app.view.mainCamera.transitionToPerspective(CameraView.PerspectiveType.Full, 0.5f);
                    }
                    __instance.app.view.GUICamera.GetComponent<GUISide>().expandGUIView.Show();
                    __instance.app.view.GUICamera.GetComponent<GUISide>().cancelView.Hide();
                    __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>().cancelView.Show(new Vector3(0.6f, -0.526f, -2.337f), true);
                    Sequence sequence = DOTween.Sequence();
                    TweenSettingsExtensions.Append(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMove(__instance.app.view.mainCameraView.transform, new Vector3(0f, 1f, -4.29f), 1f, false), Ease.InOutQuad));
                    TweenSettingsExtensions.Join(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(__instance.app.view.mainCameraView.transform, new Vector3(8.2f, 1.33f, 0f), 1f, 0), Ease.InOutQuad));
                    TweenSettingsExtensions.AppendCallback(sequence, delegate ()
                    {
                        __instance.SetEvent(new TreasureStartEvent());
                    });
                }
                else if (_event_path == "cancel.spell" && __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceEvent")
                {
                    __instance.SetEvent(new BumboEvent());
                    __instance.app.controller.HideNotifications(false);
                    if (!__instance.app.model.iOS)
                    {
                        __instance.app.controller.HideGUI();
                    }
                    __instance.app.view.GUICamera.GetComponent<GUISide>().expandGUIView.Show();
                    __instance.app.view.GUICamera.GetComponent<GUISide>().cancelView.Hide();
                    __instance.app.view.bossCancelView.Show(new Vector3(-0.011f, 0.94f, -2.004f), true);
                    __instance.app.view.mainCamera.transitionToPerspective(CameraView.PerspectiveType.Full, 0.5f);
                    __instance.SetEvent(new BossDyingEvent());
                }
                else if (_event_path == "cancel.spell")
                {
                    if (__instance.app.controller.trinketController.RerollSpellCost())
                    {
                        SpellView spellViewUsed = __instance.app.model.spellViewUsed;
                        int spellIndex = spellViewUsed.spellIndex;
                        if (!__instance.app.model.characterSheet.spells[spellIndex].IsChargeable)
                        {
                            __instance.app.model.characterSheet.spells[spellIndex].Cost = __instance.app.model.spellUsedCost;
                            __instance.app.controller.SetSpell(spellViewUsed.spellIndex, __instance.app.model.characterSheet.spells[spellIndex]);
                        }
                    }
                    __instance.app.view.clickableColumnViews[1].TurnOffLights();
                    __instance.app.controller.RemoveTint();
                    __instance.app.view.bowlingArrowView.Hide();
                    __instance.app.controller.HideNotifications(false);
                    short num = 0;
                    while ((int)num < __instance.app.view.clickableColumnViews.Length)
                    {
                        __instance.app.view.clickableColumnViews[(int)num].gameObject.SetActive(false);
                        num += 1;
                    }
                    if (__instance.app.model.costRefundOverride)
                    {
                        __instance.app.model.costRefundOverride = false;
                        __instance.app.controller.UpdateMana(__instance.app.model.costRefundAmount, false);
                    }
                    else
                    {
                        __instance.app.controller.UpdateMana(__instance.app.model.spellModel.currentSpell.Cost, false);
                        __instance.app.model.spellModel.currentSpell.FullCharge();
                    }
                    float transition_time = 1f * __instance.app.model.enemyAnimationSpeed;
                    __instance.app.model.spellModel.currentSpell = null;
                    __instance.app.model.spellModel.spellQueued = false;
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().ChangeLight(EnemyRoomView.RoomLightScheme.Default, transition_time, true);
                    __instance.SetEvent(new ResetCameraEvent());
                }
                if (_event_path == "acquire.needle" && ((GameObject)_target).GetComponent<NeedleView>().price <= __instance.app.model.characterSheet.coins)
                {
                    __instance.app.controller.ModifyCoins(-((GameObject)_target).GetComponent<NeedleView>().price);
                    __instance.app.controller.needleController.UseNeedle(((GameObject)_target).GetComponent<NeedleView>().needleAppearance);
                    ((GameObject)_target).GetComponent<NeedleView>().Disappear();
                }
                if (__instance.app.model.bumboEvent.GetType().ToString() == "TreasureStartEvent")
                {
                    __instance.EndEvent();
                    return false;
                }
                if (__instance.app.model.bumboEvent.GetType().ToString() == "ShopStartEvent" && _event_path == "debug.done.shopping")
                {
                    __instance.app.view.boxes.shopRoom.GetComponent<ShopRoomView>().HideShelf();
                    __instance.EndEvent();
                    return false;
                }
                if (_event_path == "end.turn")
                {
                    __instance.EndEvent();
                    return false;
                }
                if ((__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent") && _event_path == "spell.cast")
                {
                    if (__instance.app.model.actionPoints == 0)
                    {
                        __instance.app.controller.HideEndTurnSign();
                    }
                    if (__instance.app.model.iOS)
                    {
                        __instance.app.controller.IOSHideGUI();
                    }
                    __instance.app.model.spellModel.currentSpellIndex = ((SpellView)_target).spellIndex;
                    __instance.app.model.spellModel.currentSpell = ((SpellView)_target).SpellObject;
                    __instance.app.model.spellModel.currentSpell.SetSpellView((SpellView)_target);
                    __instance.app.model.spellModel.currentSpell.CastSpell();
                    return false;
                }
                if (__instance.app.model.bumboEvent.GetType().ToString() != "NavigationEvent" && _event_path == "navigation.move")
                {
                    MonoBehaviour.print("Trying to navigate but we are currently on the event " + __instance.app.model.bumboEvent.GetType().ToString());
                    return false;
                }
                if (__instance.app.model.bumboEvent.GetType().ToString() == "NavigationEvent" && _event_path == "navigation.move")
                {
                    int num2;
                    int num3;
                    switch ((NavigationArrowView.Direction)_data[0])
                    {
                        case NavigationArrowView.Direction.Up:
                            num2 = __instance.app.model.mapModel.currentRoom.x;
                            num3 = __instance.app.model.mapModel.currentRoom.y + 1;
                            goto IL_AFA;
                        case NavigationArrowView.Direction.Down:
                            num2 = __instance.app.model.mapModel.currentRoom.x;
                            num3 = __instance.app.model.mapModel.currentRoom.y - 1;
                            goto IL_AFA;
                        case NavigationArrowView.Direction.Right:
                            num2 = __instance.app.model.mapModel.currentRoom.x + 1;
                            num3 = __instance.app.model.mapModel.currentRoom.y;
                            goto IL_AFA;
                    }
                    num2 = __instance.app.model.mapModel.currentRoom.x - 1;
                    num3 = __instance.app.model.mapModel.currentRoom.y;
                IL_AFA:
                    __instance.app.view.navigation.arrowNorth.SetActive(false);
                    __instance.app.view.navigation.arrowSouth.SetActive(false);
                    __instance.app.view.navigation.arrowEast.SetActive(false);
                    __instance.app.view.navigation.arrowWest.SetActive(false);
                    __instance.app.controller.mapController.SetCurrentRoom(__instance.app.model.mapModel.rooms[num2, num3]);
                    __instance.SetEvent(new MoveToRoomEvent((NavigationArrowView.Direction)_data[0]));
                    return false;
                }
                if (__instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent" && _event_path == "acquire.spell")
                {
                    __instance.EndEvent();
                }
                return false;
            }
        }

        //Patch: Fixes room lights not illuminating properly when quickly transitioning to a choose lane event or enemy turn event when out of moves
        [HarmonyPrefix, HarmonyPatch(typeof(EnemyRoomView), "ChangeLight")]
        static bool EnemyRoomView_ChangeLight(EnemyRoomView __instance, EnemyRoomView.RoomLightScheme _scheme)
        {
            if ((__instance.app.model.bumboEvent.GetType().ToString() == "SelectSpellColumn" || __instance.app.model.bumboEvent.GetType().ToString() == "StartMonsterTurnEvent") && _scheme == EnemyRoomView.RoomLightScheme.EndTurn)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing room light scheme from being changed to EndTurn during SelectSpellColumn or StartMonsterTurnEvent");
                return false;
            }
            return true;
        }

        //Patch: Fixes shop needles/trinkets being unclickable after winning a trinket at the cup game while having no empty trinket slots
        [HarmonyPrefix, HarmonyPatch(typeof(CupGamble), "ReadyGame")]
        static bool CupGamble_ReadyGame(CupGamble __instance)
        {
            if (__instance.app.model.bumboEvent.GetType().ToString() != "CupGameResultEvent")
            {
                __instance.readyToPlay = true;
                __instance.gameObject.GetComponent<BoxCollider>().enabled = true;
                __instance.MakeSkullsUnclickable();
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing Cup Game from erroneously ending current event");
                return false;
            }
            return true;
        }

        //Patch: Fixes Prick Pusher not tipping hat after a trinket is purchased
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketChosenToReplaceEvent), "Execute")]
        static void TrinketChosenToReplaceEvent_Execute(TrinketChosenToReplaceEvent __instance)
        {
            if (__instance.app.view.gamblingView != null && __instance.app.model.gamblingModel.cameraAt == 2)
            {
                __instance.app.view.gamblingView.shopClerkView.TipHat();
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Triggering Prick Pusher tip hat animation on trinket purchase");
            }
        }

        //Patch: Fixes Prick Pusher not idling after canceling a purchase
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModifyDelayEvent), "NextEvent")]
        static void SpellModifyDelayEvent_NextEvent(SpellModifyDelayEvent __instance, bool ___delay)
        {
            if (!___delay)
            {
                __instance.app.view.gamblingView.shopClerkView.Idle();
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Triggering Prick Pusher idle animation on cancel purchase");
        }
    }
}