using DG.Tweening;
using HarmonyLib;
using MonoMod.Cil;
using PathologicalGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace The_Legend_of_Bum_bo_Windfall
{
    class InterfaceContent
    {
        //Patch: Get app
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init_GetApp(BumboController __instance)
        {
            WindfallHelper.GetApp(__instance.app);
        }

        //Patch: Add trinket glitches on GUISide Awake
        [HarmonyPostfix, HarmonyPatch(typeof(GUISide), "Awake")]
        static void GUISide_Awake(GUISide __instance)
        {
            GameObject[] trinkets = __instance.trinkets;
            CreateTrinketGlitches(trinkets);
        }
        public static GameObject[] trinketGlitches = new GameObject[4];
        public static void CreateTrinketGlitches(GameObject[] trinkets)
        {
            AssetBundle assets = Windfall.assetBundle;

            for (int trinketCounter = 0; trinketCounter < trinketGlitches.Length; trinketCounter++)
            {
                if (trinketCounter < trinkets.Length)
                {
                    trinketGlitches[trinketCounter] = GameObject.Instantiate(assets.LoadAsset<GameObject>("GlitchVisualObject"), trinkets[trinketCounter].transform.position, trinkets[trinketCounter].transform.rotation, trinkets[trinketCounter].transform);
                    trinketGlitches[trinketCounter].layer = 5;
                    trinketGlitches[trinketCounter].transform.localPosition = new Vector3(-0.035f, 0.01f, -0.025f);
                    trinketGlitches[trinketCounter].transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                    trinketGlitches[trinketCounter].transform.localRotation = Quaternion.Euler(new Vector3(90f, 180f, 0f));
                    trinketGlitches[trinketCounter].name = "GlitchVisualObject";
                    trinketGlitches[trinketCounter].AddComponent<WindfallTooltip>();
                    trinketGlitches[trinketCounter].SetActive(false);
                }
            }
        }
        //Patch: Activates trinket glitches only when trinket is fake
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "UpdateTrinkets")]
        static void BumboController_UpdateTrinkets(BumboController __instance)
        {
            for (short num = 0; num < 4; num += 1)
            {
                if (trinketGlitches[num] != null)
                {
                    if ((int)num < __instance.app.model.characterSheet.trinkets.Count)
                    {
                        trinketGlitches[num].SetActive(__instance.app.model.trinketIsFake[num]);
                    }
                    else
                    {
                        trinketGlitches[num].SetActive(false);
                    }
                }
            }
        }

        //Patch: Adds SpellModifySpellEvent cancel logic
        [HarmonyPatch(typeof(EventsController), nameof(EventsController.OnNotification))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EventsController_OnNotification_SpellModifySpellEvent(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var method = AccessTools.Method(typeof(InterfaceContent), nameof(EventsController_OnNotification_SpellModifySpellEvent_Cancel));

            var codes = new List<CodeInstruction>(instructions);
            var matcher = new CodeMatcher(codes, generator);

            // Match IL pattern
            matcher.MatchForward(false, new CodeMatch[] { new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(TrinketController), nameof(TrinketController.RerollSpellCost))), })
            .ThrowIfInvalid("TrinketController.RerollSpellCost pattern not found")
            .MatchBack(false, new CodeMatch[] { new CodeMatch(OpCodes.Brfalse) })
            .ThrowIfInvalid("OpCodes.Brfalse pattern not found")
            .Advance(1)
            .Insert(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, method)
            });

            return matcher.Instructions();
        }
        private static void EventsController_OnNotification_SpellModifySpellEvent_Cancel()
        {
            if (WindfallHelper.app.model.bumboEvent.GetType().ToString() == "SpellModifySpellEvent")
            {
                //Fixed canceling actions that modify your spells causing both the puzzle board and the spell menu to be selectable at the same time when using gamepad or keyboard controls
                WindfallHelper.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);

                //Add cancel functionality for use trinkets
                CollectibleChanges.currentTrinket = null;

                //Reset spell enabled states
                WindfallHelper.EnabledSpellsController?.ResetState();
            }
        }

        //Patch: Adds spellViewUsed null check for RerollSpellCost trinkets
        [HarmonyPatch(typeof(EventsController), nameof(EventsController.OnNotification))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EventsController_OnNotification_RerollSpellCost(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var method = AccessTools.Method(typeof(InterfaceContent), nameof(EventsController_OnNotification_RerollSpellCost_SpellViewUsed));

            var codes = new List<CodeInstruction>(instructions);
            var matcher = new CodeMatcher(codes, generator);

            // Match IL pattern
            matcher.MatchForward(false, new CodeMatch[] { new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(TrinketController), nameof(TrinketController.RerollSpellCost))), })
            .ThrowIfInvalid("TrinketController.RerollSpellCost pattern not found")
            .Advance(1)
            .Insert(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, method),
                new CodeInstruction(OpCodes.And)
            });

            return matcher.Instructions();
        }
        private static bool EventsController_OnNotification_RerollSpellCost_SpellViewUsed()
        {
            return WindfallHelper.app.model.spellViewUsed != null;
        }

        //Patch: Enables boss room pickups
        [HarmonyPatch(typeof(EventsController), nameof(EventsController.OnNotification))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EventsController_OnNotification_BossRewards(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var method = AccessTools.Method(typeof(InterfaceContent), nameof(EventsController_OnNotification_BossRewards_Enable));

            var codes = new List<CodeInstruction>(instructions);
            var matcher = new CodeMatcher(codes, generator);

            // Match IL pattern
            matcher.MatchForward(false, new CodeMatch[] { new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TweenSettingsExtensions), nameof(TweenSettingsExtensions.Join))), })
            .ThrowIfInvalid("TweenSettingsExtensions.Join pattern not found")
            .Advance(1)
            .Insert(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, method),
            });

            return matcher.Instructions();
        }
        private static void EventsController_OnNotification_BossRewards_Enable(Sequence s)
        {
            if (WindfallHelper.app.view.gamblingView == null)
            {
                //Enable boss room pickups
                s.AppendCallback(delegate
                {
                    GameObject[] bossRewardParents = WindfallHelper.app.view.bossRewardParents;
                    if (bossRewardParents != null && bossRewardParents.Length > 1)
                    {
                        bossRewardParents[0]?.transform.GetChild(0)?.GetComponent<TrinketPickupView>()?.SetClickable(true);
                        bossRewardParents[1]?.transform.GetChild(0)?.GetComponent<TrinketPickupView>()?.SetClickable(true);
                    }
                });
            }
        }

        //Patch: Fixes camera moving incorrectly when canceling taking a trinket from a treasure room
        [HarmonyPatch(typeof(EventsController), nameof(EventsController.OnNotification))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EventsController_OnNotification_TrinketReplaceCancel(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var method = AccessTools.Method(typeof(InterfaceContent), nameof(EventsController_OnNotification_TrinketReplaceCancel_Condition));

            var codes = new List<CodeInstruction>(instructions);
            var matcher = new CodeMatcher(codes, generator);

            // Match IL pattern
            matcher.MatchForward(false, new CodeMatch[] { new CodeMatch(OpCodes.Ldstr, "TrinketReplaceEvent"), new CodeMatch(OpCodes.Call), new CodeMatch(OpCodes.Brfalse) })
            .ThrowIfInvalid("Ldstr TrinketReplaceEvent pattern not found")
            .Advance(2)
            .Insert(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, method),
                new CodeInstruction(OpCodes.And),
            });

            return matcher.Instructions();
        }
        private static bool EventsController_OnNotification_TrinketReplaceCancel_Condition()
        {
            return (WindfallHelper.app.view.gamblingView == null && WindfallHelper.app.model.mapModel?.currentRoom?.roomType == MapRoom.RoomType.Boss) || (WindfallHelper.app.view.gamblingView != null && WindfallHelper.app.model.gamblingModel?.cameraAt == 0);
        }

        //Patch: Allows cancel button to be pressed while in a treasure room after canceling taking a trinket
        [HarmonyPrefix, HarmonyPatch(typeof(CancelView), "OnMouseDown")]
        static bool CancelView_OnMouseDown(CancelView __instance)
        {
            if (__instance.animationSequence == null || !__instance.animationSequence.IsActive() || __instance.animationSequence.IsComplete())
            {
                if (__instance.app.view.boxes.treasureRoom != null && __instance.app.view.boxes.treasureRoom.gameObject.activeSelf && __instance.app.model.bumboEvent.GetType() != typeof(TreasureSpellReplaceEvent) && __instance.app.model.bumboEvent.GetType() != typeof(TrinketReplaceEvent) && __instance.app.model.bumboEvent.GetType() != typeof(TreasureStartEvent) && __instance.app.model.bumboEvent.GetType() != typeof(TrinketChosenToReplaceEvent))
                {
                    return false;
                }
                __instance.app.Notify("cancel.spell", __instance, Array.Empty<object>());
                __instance.Hide();
            }
            return false;
        }

        //Patch: Increases cup game coin rewards
        //Invokes CupGameResultEvent_Execute_CoinReward() and sets coin_result to the returned value
        [HarmonyPatch(typeof(CupGameResultEvent), nameof(CupGameResultEvent.Execute))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CupGameResultEvent_Execute_CoinReward_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var method = AccessTools.Method(typeof(InterfaceContent), nameof(CupGameResultEvent_Execute_CoinReward));

            var codes = new List<CodeInstruction>(instructions);
            var matcher = new CodeMatcher(codes, generator);

            // Look for the IL pattern
            matcher.MatchForward(false, new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldc_R4, 0f),
                new CodeMatch(OpCodes.Ldc_R4, 1f),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range), new Type[] { typeof(float), typeof(float) })),
                new CodeMatch(OpCodes.Ldc_R4, 0.5f),
            });
            matcher.ThrowIfInvalid("Random.Range pattern not found");
            int startIndex = matcher.Pos;

            //Find Stfld
            matcher.MatchForward(true, new CodeMatch(OpCodes.Stfld));
            matcher.ThrowIfInvalid("coin_result stfld not found");
            int endIndex = matcher.Pos;

            //Insert new coin calculation
            for (int i = startIndex; i < endIndex; i++)
            {
                if (i == startIndex) codes[i] = new CodeInstruction(OpCodes.Call, method);
                else codes[i] = new CodeInstruction(OpCodes.Nop);
            }

            return codes;
        }
        private static int CupGameResultEvent_Execute_CoinReward()
        {
            //Coin reward
            return UnityEngine.Random.Range(8, 11);
        }

        private static MethodInfo MethodInfo_Random_Range = AccessTools.Method(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range), new Type[] { typeof(int), typeof(int) });
        private static MethodInfo MethodInfo_WheelSpin_Spin_Stats = AccessTools.Method(typeof(InterfaceContent), nameof(WheelSpin_Spin_Stats));

        //Patch: Causes stat wheel to avoid increasing maxed stats
        [HarmonyPatch(typeof(WheelSpin), "Spin")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> WheelSpin_Spin_Stats_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            Type displayClass = typeof(WheelSpin)
            .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
            .FirstOrDefault(t => t.Name == "<>c__DisplayClass6_0");

            FieldInfo rngField = AccessTools.Field(displayClass, "rng");

            codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Call, MethodInfo_Random_Range))
                .ThrowIfInvalid("Could not find call to Random.Range")
                .Advance(2)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, MethodInfo_WheelSpin_Spin_Stats),
                    new CodeInstruction(OpCodes.Stfld, rngField)
                );

            return codeMatcher.Instructions();
        }
        private static int WheelSpin_Spin_Stats()
        {
            //Prevent the stat wheel from increasing maxed stats
            GamblingModel gamblingModel = WindfallHelper.GamblingModel;
            CharacterSheet characterSheet = WindfallHelper.app.model.characterSheet;

            bool hitPointsNotMaxed = characterSheet.bumboBaseInfo.hitPoints < 6f;
            if (characterSheet.bumboType == CharacterSheet.BumboType.TheDead) hitPointsNotMaxed = (characterSheet.soulHearts < 6f);
            bool itemDamageNotMaxed = characterSheet.bumboBaseInfo.itemDamage < 5;
            bool puzzleDamageNotMaxed = characterSheet.bumboBaseInfo.puzzleDamage < 5;
            bool dexterityNotMaxed = characterSheet.bumboBaseInfo.dexterity < 5;
            bool luckNotMaxed = characterSheet.bumboBaseInfo.luck < 5;

            bool anyAllowedWheelSlices = false;
            bool[] allowedWheelSlices = new bool[gamblingModel.wheelSlices.Length];

            //Force coin reward to have a 16.66...% chance to occur
            if (UnityEngine.Random.Range(0f, 1f) < 0.166f)
            {
                for (int i = 0; i < gamblingModel.wheelSlices.Length; i++)
                {
                    if (gamblingModel.wheelSlices[i] == WheelView.WheelSlices.Coin20) return i;
                }
            }

            //Allow non-maxed wheel rewards
            for (int i = 0; i < gamblingModel.wheelSlices.Length; i++)
            {
                if ((gamblingModel.wheelSlices[i] == WheelView.WheelSlices.Health && hitPointsNotMaxed) || (gamblingModel.wheelSlices[i] == WheelView.WheelSlices.ItemDamage && itemDamageNotMaxed) || (gamblingModel.wheelSlices[i] == WheelView.WheelSlices.PuzzleDamage && puzzleDamageNotMaxed) || (gamblingModel.wheelSlices[i] == WheelView.WheelSlices.Dexterity && dexterityNotMaxed) || (gamblingModel.wheelSlices[i] == WheelView.WheelSlices.Luck && luckNotMaxed))
                {
                    allowedWheelSlices[i] = true;
                    anyAllowedWheelSlices = true;
                }
                else allowedWheelSlices[i] = false;
            }
            if (!anyAllowedWheelSlices)
            {
                //Failsafe: Allow all wheel slices if none were allowed
                for (int k = 0; k < allowedWheelSlices.Length; k++) allowedWheelSlices[k] = true;
            }
            List<int> allowedWheelSliceIndices = new List<int>();
            for (int l = 0; l < allowedWheelSlices.Length; l++)
            {
                if (allowedWheelSlices[l]) allowedWheelSliceIndices.Add(l);
            }
            int index = UnityEngine.Random.Range(0, allowedWheelSliceIndices.Count);
            return allowedWheelSliceIndices[index];
        }

        //***************************************************
        //***************Trinket Reward Display**************
        //***************************************************

        //Patch: Creates trinket display when winning a trinket while having no empty trinket slots
        //Invokes CupGameResultEvent_Execute_TrinketReward() after SetEvent(new TrinketReplaceEvent) in the <Execute>b__0_2 delegate
        [HarmonyPatch(typeof(CupGameResultEvent), "<Execute>b__0_2")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CupGameResultEvent_Execute_TrinketReward_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var method = AccessTools.Method(typeof(InterfaceContent), nameof(CupGameResultEvent_Execute_TrinketReward));

            var codes = new List<CodeInstruction>(instructions);
            var matcher = new CodeMatcher(codes);

            // Find callvirt to EventsController.SetEvent
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Callvirt,
                    AccessTools.Method(typeof(EventsController), nameof(EventsController.SetEvent)))
            );

            if (!matcher.IsValid)
                throw new Exception("Failed to find call to SetEvent.");

            // Insert call to MyMethod immediately after SetEvent
            matcher.Advance(1) // move past SetEvent call
                   .Insert(new CodeInstruction(OpCodes.Call, method));

            return matcher.Instructions();
        }
        private static void CupGameResultEvent_Execute_TrinketReward()
        {
            //Create trinket reward display
            CreateTrinketRewardDisplay(WindfallHelper.app);
        }

        static GameObject trinketRewardDisplay;
        static readonly Vector3 trinketRewardDisplayPosition = new Vector3(-1.15f, 0.87f, -5.54f);
        static readonly float trinketRewardDisplayOffsetY = 0.8f;
        static readonly float displayLightIntensity = 1.1f;
        static Sequence trinketRewardDisplaySequence;
        static float displaySequenceDuration = 0.5f;

        public static void CreateTrinketRewardDisplay(BumboApplication app)
        {
            if (trinketRewardDisplay != null)
            {
                return;
            }

            trinketRewardDisplay = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Pickups/Trinket Pickup") as GameObject);
            trinketRewardDisplay.name = "Trinket Display";
            trinketRewardDisplay.transform.position = trinketRewardDisplayPosition;
            trinketRewardDisplay.transform.localRotation = Quaternion.Euler(0f, 135f, 0f);
            trinketRewardDisplay.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            trinketRewardDisplay.GetComponent<TrinketPickupView>().SetTrinket(app.model.gamblingModel.trinketReward, 0);
            trinketRewardDisplay.GetComponent<TrinketPickupView>().SetClickable(false);
            trinketRewardDisplay.SetActive(true);
            trinketRewardDisplay.GetComponent<BoxCollider>().enabled = true;
            GameObject trinketDisplayLight = new GameObject("Trinket Display Light");
            trinketDisplayLight.AddComponent<Light>().intensity = 0;
            trinketDisplayLight.transform.position = new Vector3(-1.1f, 1.1f, -6f);
            trinketDisplayLight.transform.SetParent(trinketRewardDisplay.transform);

            trinketRewardDisplay.transform.position += new Vector3(0, trinketRewardDisplayOffsetY, 0);
            trinketRewardDisplaySequence = DOTween.Sequence();
            trinketRewardDisplaySequence.Append(trinketRewardDisplay.transform.DOLocalMoveY(trinketRewardDisplayPosition.y, displaySequenceDuration).SetEase(Ease.InOutQuad));
            trinketRewardDisplaySequence.Join(trinketDisplayLight.GetComponent<Light>()?.DOIntensity(displayLightIntensity, displaySequenceDuration));
        }

        public static void RemoveTrinketRewardDisplay()
        {
            if (trinketRewardDisplaySequence != null)
            {
                trinketRewardDisplaySequence.Kill(true);
            }

            if (trinketRewardDisplay != null)
            {
                trinketRewardDisplaySequence = DOTween.Sequence();
                trinketRewardDisplaySequence.Append(trinketRewardDisplay.transform.DOLocalMoveY(trinketRewardDisplayPosition.y + trinketRewardDisplayOffsetY, displaySequenceDuration).SetEase(Ease.InOutQuad));
                trinketRewardDisplaySequence.Join(trinketRewardDisplay.transform.GetComponentInChildren<Light>()?.DOIntensity(0, displaySequenceDuration));
                trinketRewardDisplaySequence.AppendCallback(delegate { UnityEngine.Object.Destroy(trinketRewardDisplay); });
            }
        }

        //Patch: Removes cancel button when trinket is replaced
        //Patch also removes trinket display from the cup game
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketChosenToReplaceEvent), "Execute")]
        static bool TrinketChosenToReplaceEvent_Execute(TrinketChosenToReplaceEvent __instance, int ___trinketIndex)
        {
            if (___trinketIndex >= 0)
            {
                if (__instance.app.view.gamblingView != null)
                {
                    __instance.app.model.characterSheet.trinkets[___trinketIndex] = __instance.app.model.trinketModel.trinkets[__instance.app.model.gamblingModel.trinketReward];
                }
                else
                {
                    __instance.app.model.characterSheet.trinkets[___trinketIndex] = __instance.app.model.trinketModel.trinkets[__instance.app.model.trinketReward];
                }
                __instance.app.controller.UpdateTrinkets();
            }
            if (!__instance.app.model.iOS)
            {
                __instance.app.controller.HideGUI();
            }
            if (!__instance.app.model.iOS)
            {
                __instance.app.view.mainCamera.transitionToPerspective(CameraView.PerspectiveType.Full, 0.5f);
            }

            //Hide cancel button
            __instance.app.view.GUICamera.GetComponent<GUISide>().cancelView.Hide();
            if (__instance.app.view.gamblingView != null)
            {
                __instance.app.view.GUICamera.GetComponent<GUISide>().expandGUIView.Show();

                //Remove trinket reward display
                RemoveTrinketRewardDisplay();

                __instance.app.controller.gamblingController.shop.UpdatePrices();
                __instance.app.view.gamblingView.gamblingCameraView.ShowArrows();
                __instance.app.view.gamblingView.shopClerkView.TipHat();
                __instance.app.controller.HideNotifications(false);

                __instance.app.view.gamblingView.cupClerkView.AnimateIdle();
                if (__instance.app.model.gamblingModel.cameraAt == 0)
                {
                    __instance.app.controller.gamblingController.cupGamble.ReadyGame();
                }
                TweenExtensions.Kill(__instance.app.model.gamblingModel.lightAnimation, false);
                __instance.app.model.gamblingModel.lightAnimation = DOTween.Sequence();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Join(TweenSettingsExtensions.Join(TweenSettingsExtensions.Append(__instance.app.model.gamblingModel.lightAnimation, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOIntensity(__instance.app.view.gamblingView.fillLight, 1f, 0.25f), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOIntensity(__instance.app.view.gamblingView.keyLight, 1f, 0.25f), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOIntensity(__instance.app.view.gamblingView.resultLight, 0f, 0.25f), Ease.InOutQuad)), delegate ()
                {
                    __instance.End();
                });
                return false;
            }
            __instance.End();

            return false;
        }

        //***************************************************
        //***************************************************
        //***************************************************

        //***************************************************
        //*****************Cutscene Menu********************
        //***************************************************

        //Patch: Add cutscene menu button to main menu
        //Also disables cutscenes that haven't been unlocked
        //Also adds coins and logo to title box
        //Also adds win streak counter to main menu
        //Also applies graphics settings to title camera and character select camera
        //Also adds Windfall options menu to title
        [HarmonyPostfix, HarmonyPatch(typeof(TitleController), "Start")]
        static void TitleController_Start(TitleController __instance)
        {
            GameObject cutscenesButton = UnityEngine.Object.Instantiate(__instance.debugMenu.transform.Find("Cutscenes").gameObject, __instance.mainMenu.transform);
            cutscenesButton.GetComponent<RectTransform>().SetSiblingIndex(4);

            WindfallHelper.LocalizeObject(cutscenesButton, "Menu/CUTSCENES");

            GamepadMenuOptionSelection cutscenesOptionSelection = cutscenesButton.AddComponent<GamepadMenuOptionSelection>();
            if (cutscenesOptionSelection != null)
            {
                cutscenesOptionSelection.m_SelectionObjects = new GameObject[0];
                cutscenesOptionSelection.m_InjectDots = GamepadMenuOptionSelection.eInjectDots.Both;
            }

            TextMeshProUGUI cutscenesTextMeshPro = cutscenesOptionSelection.GetComponent<TextMeshProUGUI>();
            if (cutscenesTextMeshPro != null)
            {
                cutscenesTextMeshPro.enableWordWrapping = false;
            }

            ButtonHoverAnimation cutscenesButtonHover = cutscenesButton.GetComponent<ButtonHoverAnimation>();
            if (cutscenesButtonHover != null)
            {
                cutscenesButtonHover.hoverTextColor = Color.black;
            }

            GamepadMenuController gamepadMenuController = cutscenesButton.transform.parent.GetComponent<GamepadMenuController>();

            WindfallHelper.UpdateGamepadMenuButtons(gamepadMenuController, null, 2);

            //Get references to cutscene menu buttons
            Transform cutsceneMenuTransform = __instance.cutsceneMenu.transform;
            GameObject miniCreditsRoll = cutsceneMenuTransform.Find("Mini Credits Roll").gameObject;
            GameObject nimbleEnding = cutsceneMenuTransform.Find("Nimble Ending").gameObject;
            GameObject stoutEnding = cutsceneMenuTransform.Find("Stout Ending").gameObject;
            GameObject weirdEnding = cutsceneMenuTransform.Find("Weird Ending").gameObject;
            GameObject basementEnding = cutsceneMenuTransform.Find("Ending Basement").gameObject;
            GameObject momEnding = cutsceneMenuTransform.Find("Ending Mom").gameObject;
            GameObject creditsRoll = cutsceneMenuTransform.Find("Credits Roll").gameObject;
            GameObject finalEnding = cutsceneMenuTransform.Find("Ending Final").gameObject;
            GameObject back = cutsceneMenuTransform.Find("Back").gameObject;

            //Add localization to cutscene menu buttons
            WindfallHelper.LocalizeObject(miniCreditsRoll, "Menu/MINI_CREDITS_ROLL");
            WindfallHelper.LocalizeObject(nimbleEnding, "Menu/NIMBLE_ENDING");
            WindfallHelper.LocalizeObject(stoutEnding, "Menu/STOUT_ENDING");
            WindfallHelper.LocalizeObject(weirdEnding, "Menu/WEIRD_ENDING");
            WindfallHelper.LocalizeObject(basementEnding, "Menu/BASEMENT_ENDING");
            WindfallHelper.LocalizeObject(momEnding, "Menu/MOM_ENDING");
            WindfallHelper.LocalizeObject(creditsRoll, "Menu/CREDITS_ROLL");
            WindfallHelper.LocalizeObject(finalEnding, "Menu/FINAL_ENDING");
            WindfallHelper.LocalizeObject(back, "Menu/GO_BACK");

            //Recenter main menu
            RectTransform mainMenuRectTransform = __instance.mainMenu.GetComponent<RectTransform>();
            if (mainMenuRectTransform != null)
            {
                mainMenuRectTransform.anchoredPosition = new Vector2(mainMenuRectTransform.anchoredPosition.x, mainMenuRectTransform.anchoredPosition.y + 22);
            }

            //Reorder endings
            weirdEnding.transform.SetSiblingIndex(4);
            creditsRoll.transform.SetSiblingIndex(6);

            //Disable endings based on game progress
            Progression progression = ProgressionController.LoadProgression();
            if (!progression.unlocks[0])
            {
                //Disable Mini Credits
                miniCreditsRoll.GetComponent<Button>().interactable = false;

                //Disable Nimble ending
                nimbleEnding.GetComponent<Button>().interactable = false;
            }
            if (!progression.unlocks[1])
            {
                //Disable Stout ending
                stoutEnding.GetComponent<Button>().interactable = false;
            }
            if (!progression.unlocks[2])
            {
                //Disable Weird ending
                weirdEnding.GetComponent<Button>().interactable = false;
            }
            if (!progression.unlocks[5])
            {
                //Disable Basement ending
                basementEnding.GetComponent<Button>().interactable = false;
            }
            if (progression.wins < 1)
            {
                //Disable Mom ending
                momEnding.GetComponent<Button>().interactable = false;
                //Disable Credits Roll
                creditsRoll.GetComponent<Button>().interactable = false;
            }

            bool removeFinalEnding = false;
            for (int i = 0; i < 31; i++)
            {
                if (!progression.unlocks[i]) removeFinalEnding = true;
            }
            if (removeFinalEnding)
            {
                //Disable Final ending
                finalEnding.GetComponent<Button>().interactable = false;
            }

            //Fix cutscene menu gamepad controls and visuals
            GamepadMenuController cutsceneMenuGamepadMenuController = __instance.cutsceneMenu.GetComponent<GamepadMenuController>();
            List<GameObject> newCutsceneMenuButtons = new List<GameObject>();
            for (int childCounter = 0; childCounter < __instance.cutsceneMenu.transform.childCount; childCounter++)
            {
                Transform child = __instance.cutsceneMenu.transform.GetChild(childCounter);

                GamepadMenuOptionSelection cutscenesButtonOptionSelection = child.gameObject.AddComponent<GamepadMenuOptionSelection>();
                if (cutscenesButtonOptionSelection != null)
                {
                    cutscenesButtonOptionSelection.m_SelectionObjects = new GameObject[0];
                    cutscenesButtonOptionSelection.m_InjectDots = GamepadMenuOptionSelection.eInjectDots.Both;
                }

                ButtonHoverAnimation buttonHoverAnimation = child.GetComponent<ButtonHoverAnimation>();
                if (buttonHoverAnimation != null)
                {
                    buttonHoverAnimation.hoverTextColor = Color.black;
                }

                newCutsceneMenuButtons.Add(child.gameObject);
            }
            if (newCutsceneMenuButtons.Count > 0 && cutsceneMenuGamepadMenuController != null)
            {
                cutsceneMenuGamepadMenuController.m_Buttons = newCutsceneMenuButtons.ToArray();
            }

            //Title box coins
            AssetBundle assets = Windfall.assetBundle;

            var CoinsMesh = assets.LoadAsset<Mesh>("Coins_Model_V3");
            var CoinsTexture = assets.LoadAsset<Texture2D>("Title_Coins_Texture_V2");

            GameObject coins = new GameObject();
            MeshFilter meshFilter = coins.AddComponent<MeshFilter>();
            meshFilter.mesh = CoinsMesh;
            MeshRenderer meshRenderer = coins.AddComponent<MeshRenderer>();
            meshRenderer.material.mainTexture = CoinsTexture;
            Transform planeTransform = __instance.box.transform.Find("Plane").transform;
            coins.transform.position = planeTransform.position + new Vector3(0f, 0.37f, 0f);
            coins.transform.rotation = planeTransform.rotation;
            coins.transform.Rotate(new Vector3(0, 0, 57f));
            coins.transform.localScale = Vector3.Scale(planeTransform.localScale, new Vector3(0.2f, 0.2f, 0.1f));
            coins.transform.SetParent(planeTransform);

            //Title Windfall Logo
            var LogoMesh = assets.LoadAsset<Mesh>("Windfall_Logo");
            var LogoTexture = assets.LoadAsset<Texture2D>("Windfall_Logo_Image");

            GameObject logo = new GameObject();
            MeshFilter meshFilter2 = logo.AddComponent<MeshFilter>();
            meshFilter2.mesh = LogoMesh;
            MeshRenderer meshRenderer2 = logo.AddComponent<MeshRenderer>();
            meshRenderer2.material.mainTexture = LogoTexture;
            Transform bumboTransform = GameObject.Find("Logo").transform.Find("Bum-bo");
            logo.transform.position = new Vector3(0.05f, 1.2f, -2.19f);
            logo.transform.rotation = Quaternion.Euler(274.2f, 181.4f, 1.51f);
            logo.transform.localScale = new Vector3(180f, 2.62f, 60f);
            logo.transform.SetParent(bumboTransform);

            Transform tagline = bumboTransform.Find("bum-bo_tagline");
            tagline.position = new Vector3(-0.08f, 0.17f, -1.87f);

            //Win streak counter
            WinStreakCounter.CreateWinStreakCounter(__instance);

            //Apply graphics to cameras
            GraphicsModifier.ApplyGraphicsToCamera(__instance.titleCamera.GetComponent<Camera>());
            GraphicsModifier.ApplyGraphicsToCamera(__instance.chooseCamera.GetComponent<Camera>());

            //Set up Windfall options menu
            GameObject windfallOptionsMenu = UnityEngine.Object.Instantiate(assets.LoadAsset<GameObject>("Windfall Menu"), __instance.menuObject.transform);
            WindfallOptionsMenu windfallOptionsMenuComponent = windfallOptionsMenu.AddComponent<WindfallOptionsMenu>();
            windfallOptionsMenuComponent.SetUpWindfallOptionsMenu(false);
        }

        //Patch: Adds Windfall options menu to pause screen
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init_Graphics(BumboController __instance)
        {
            AssetBundle assets = Windfall.assetBundle;

            //Set up windfall options
            GameObject windfallOptionsMenu = UnityEngine.Object.Instantiate(assets.LoadAsset<GameObject>("Windfall Menu"), __instance.app.view.menuView.transform);
            WindfallOptionsMenu windfallOptionsMenuComponent = windfallOptionsMenu.AddComponent<WindfallOptionsMenu>();
            windfallOptionsMenuComponent.SetUpWindfallOptionsMenu(true);
        }

        //Patch: Hide coins and logo from title box on input
        [HarmonyPrefix, HarmonyPatch(typeof(TitleController), "Update")]
        static bool TitleController_Update(TitleController __instance, ref bool ___loading)
        {
            if (__instance.unlockAllCharacters)
            {
                __instance.unlockAllCharacters = false;
                __instance.UnlockAllCharacters();
            }
            if (__instance.unlockEverything)
            {
                __instance.unlockEverything = false;
                Progression progression = new Progression();
                for (int i = 0; i < 43; i++)
                {
                    progression.unlocks[i] = true;
                }
                ProgressionController.SaveProgression(progression);
            }
            if (__instance.turnOnDebugKey && (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F1)))
            {
                __instance.OpenDebugMenu();
            }
            else if ((Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.touchCount > 0) && !___loading)
            {
                ___loading = true;
                GameObject gameObject = GameObject.Find("title_box").gameObject;
                gameObject.GetComponent<Animator>().SetTrigger("OpenBox");
                SoundsView.Instance.PlaySound(SoundsView.eSound.TitleScreenFade, SoundsView.eAudioSlot.Default, false);
                Sequence sequence = DOTween.Sequence();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Insert(TweenSettingsExtensions.Append(sequence, TweenSettingsExtensions.SetDelay<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveY(gameObject.transform, (-5.5f) * 1.5f, (0.3f) * 1.5f, false), Ease.InQuad), 0.33333334f)), 0.33333334f, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveY(GameObject.Find("Logo").transform, (6.5f) * 1.5f, (0.3f) * 1.5f, false), Ease.InQuad)), 0.5f), delegate ()
                {
                    __instance.menuObject.SetActive(true);
                }), delegate ()
                {
                    SoundsView.Instance.PlaySound(SoundsView.eSound.Menu_Appear, SoundsView.eAudioSlot.Default, false);
                });
            }
            return false;
        }

        //Patch: Update cutscene menu when progress is deleted
        //Also resets win streak counter when deleting game progress
        [HarmonyPostfix, HarmonyPatch(typeof(TitleController), "DeleteProgress")]
        static void TitleController_DeleteProgress(TitleController __instance)
        {
            //Disable Mini Credits
            __instance.menuObject.transform.Find("Cutscene Menu").Find("Mini Credits Roll").GetComponent<Button>().interactable = false;
            //Disable Nimble ending
            __instance.menuObject.transform.Find("Cutscene Menu").Find("Nimble Ending").GetComponent<Button>().interactable = false;
            //Disable Stout ending
            __instance.menuObject.transform.Find("Cutscene Menu").Find("Stout Ending").GetComponent<Button>().interactable = false;
            //Disable Weird ending
            __instance.menuObject.transform.Find("Cutscene Menu").Find("Weird Ending").GetComponent<Button>().interactable = false;
            //Disable Basement ending
            __instance.menuObject.transform.Find("Cutscene Menu").Find("Ending Basement").GetComponent<Button>().interactable = false;
            //Disable Mom ending
            __instance.menuObject.transform.Find("Cutscene Menu").Find("Ending Mom").GetComponent<Button>().interactable = false;
            //Disable Credits Roll
            __instance.menuObject.transform.Find("Cutscene Menu").Find("Credits Roll").GetComponent<Button>().interactable = false;
            //Disable Final ending
            __instance.menuObject.transform.Find("Cutscene Menu").Find("Ending Final").GetComponent<Button>().interactable = false;

            //Reset win streak counter
            WinStreakCounter.ResetStreak(false);
        }

        //Patch: Cutscene menu from main menu
        [HarmonyPrefix, HarmonyPatch(typeof(TitleController), "OpenCutsceneMenu")]
        static bool TitleController_OpenCutsceneMenu(TitleController __instance)
        {
            __instance.cutsceneMenu.SetActive(true);
            __instance.mainMenu.SetActive(false);
            return false;
        }

        //Patch: Main menu from cutscene menu
        [HarmonyPrefix, HarmonyPatch(typeof(TitleController), "CloseCutsceneMenu")]
        static bool TitleController_CloseCutsceneMenu(TitleController __instance)
        {
            __instance.cutsceneMenu.SetActive(false);
            __instance.mainMenu.SetActive(true);
            return false;
        }

        static bool RewatchingCutscene = false;
        //Patch: Tracks whether player is rewatching a cutscene from the main menu
        [HarmonyPostfix, HarmonyPatch(typeof(TitleController), "PlayCutscene")]
        static void TitleController_PlayCutscene(TitleController __instance)
        {
            RewatchingCutscene = true;
        }

        //Patch: Disables achievements when rewatching cutscenes from the main menu
        //Jackpot ending plays after Mom cutscene; odds of displaying Jackpot win/lose are equal
        [HarmonyPrefix, HarmonyPatch(typeof(BumboUnlockController), "Start")]
        static bool BumboUnlockController_Start(BumboUnlockController __instance, ref List<int> ___unlocks)
        {
            if (!RewatchingCutscene)
            {
                return true;
            }
            RewatchingCutscene = false;
            __instance.skipUnlock = true;
            __instance.forceUnlock = false;

            if (__instance.app.view.badUnlock != null)
            {
                if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                {
                    __instance.app.view.introPaperView.musicAudio = __instance.badMusic;
                    __instance.app.view.introPaperView.video.clip = __instance.app.view.badUnlock;
                    __instance.app.view.goodEnding = false;
                }
                else
                {
                    __instance.app.view.goodEnding = true;
                }
            }
            return true;
        }

        //***************************************************
        //***************************************************
        //***************************************************

        //***************************************************
        //*********************Map Menu**********************
        //***************************************************

        //Patch: Add map menu buttonclick effect
        [HarmonyPrefix, HarmonyPatch(typeof(MenuButtonView), "OnMouseDown")]
        static bool MenuButtonView_OnMouseDown(MenuButtonView __instance, bool ___clickable)
        {
            if ((__instance.name == ("Map Menu Button") || __instance.name == "Gambling Map Menu Button") && ___clickable)
            {
                MapMenu.OpenMapMenu();
                return false;
            }
            return true;
        }

        //Patch: Set up map menu
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init_MapMenu(BumboController __instance)
        {
            MapMenu.CreateMapMenu(__instance);
        }

        //Patch: Create gambling map menu button
        [HarmonyPostfix, HarmonyPatch(typeof(GamblingController), "Start")]
        static void GamblingController_Init(GamblingController __instance)
        {
            MapMenu.CreateGamblingMapMenuButton();
        }

        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Apply custom graphics to game cameras
        [HarmonyPostfix, HarmonyPatch(typeof(CameraView), "Awake")]
        static void CameraView_Awake(CameraView __instance)
        {
            Camera camera = __instance.GetComponent<Camera>();
            if (camera != null)
            {
                //Only apply depth of field if not GUI camera
                GraphicsModifier.ApplyGraphicsToCamera(camera);
            }
        }

        //Patch: Applies custom graphics to unlock camera
        [HarmonyPostfix, HarmonyPatch(typeof(BumboUnlockController), "Start")]
        static void BumboUnlockController_Start(BumboUnlockController __instance)
        {
            Camera camera = __instance.app.view.unlockCameraView.transform.GetChild(0).GetComponent<Camera>();
            if (camera != null)
            {
                GraphicsModifier.ApplyGraphicsToCamera(camera);
            }
        }

        //Patch: Applies custom graphics to mom ending camera
        [HarmonyPostfix, HarmonyPatch(typeof(MomEndingController), "Start")]
        static void MomEndingController_Start(MomEndingController __instance)
        {
            Camera camera = __instance.app.view.unlockCameraView.transform.GetChild(0).GetComponent<Camera>();
            if (camera != null)
            {
                GraphicsModifier.ApplyGraphicsToCamera(camera);
            }
        }

        //Patch: Update win streak on game win
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "FinishFloor")]
        static void BumboController_FinishFloor(BumboController __instance)
        {
            if (__instance.app.model.characterSheet.currentFloor == 5)
            {
                WinStreakCounter.GameWon();
            }
        }
        //Patch: Prevents the win streak from erroneously incrementing when changing floors using the debug menu while in The Basement
        [HarmonyPrefix, HarmonyPatch(typeof(DebugController), "ChangeFloor")]
        static void DebugController_ChangeFloor(DebugController __instance)
        {
            ref int currentFloor = ref __instance.app.model.characterSheet.currentFloor;
            if (currentFloor == 4)
            {
                currentFloor--;
            }
        }

        //Patch: Update win streak on game loss
        [HarmonyPostfix, HarmonyPatch(typeof(GameOverEvent), "Execute")]
        static void GameOverEvent_Execute(GameOverEvent __instance)
        {
            if (WindfallHelper.ChaptersUnlocked(__instance.app.model.progression) == 4)
            {
                WinStreakCounter.GameLost();
            }
        }

        //Patch: Reset win streak on new game (if a saved game is being overwritten)
        [HarmonyPrefix, HarmonyPatch(typeof(FloorStartEvent), "Execute")]
        static bool FloorStartEvent_Execute(FloorStartEvent __instance)
        {
            if (UnityEngine.Object.FindObjectOfType<CharacterSheet>() == null && SavedStateController.HasSavedState() && (TitleController.startMode == TitleController.StartMode.NewGame || TitleController.startMode == TitleController.StartMode.Nothing))
            {
                WinStreakCounter.ResetStreak(true);
            }
            return true;
        }

        private static readonly float CharacterSelectIndicatorLocalScaleMin = 0.1f;
        private static readonly float CharacterSelectIndicatorLocalScaleMax = CharacterSelectIndicatorLocalScaleMin * 1.15f;

        private static readonly Vector3 LocalRotation = new Vector3(270f, 0f, 0f);
        private static readonly Vector3 BravePosition = new Vector3(0.2151f, 0.5791f, -10.8167f);
        private static readonly Vector3 NimblePosition = new Vector3(0.2515f, 0.5791f, -10.8167f);
        private static readonly Vector3 StoutPosition = new Vector3(0.3417f, 0.5576f, -10.8167f);
        private static readonly Vector3 WeirdPosition = new Vector3(0.3235f, 0.554f, -10.8167f);
        private static readonly Vector3 LeftNavigationPosition = new Vector3(0.9427f, 0.4104f, -11.1155f);
        private static readonly Vector3 RightNavigationPosition = new Vector3(-0.9427f, 0.4104f, -11.1155f);

        //Patch: Adds character select indicator
        [HarmonyPostfix, HarmonyPatch(typeof(SelectCharacterView), "Start")]
        static void SelectCharacterView_Start(SelectCharacterView __instance)
        {
            GameObject characterSelectIndicatorObject = GameObject.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>("SpellViewIndicator"), __instance.transform.parent);
            characterSelectIndicatorObject.name = "Character Select Indicator";
            WindfallHelper.Reskin(characterSelectIndicatorObject, null, null, Windfall.assetBundle.LoadAsset<Texture2D>("Character Select Indicator"));
            WindfallHelper.ReTransform(characterSelectIndicatorObject, Vector3.zero, LocalRotation, new Vector3(CharacterSelectIndicatorLocalScaleMin, CharacterSelectIndicatorLocalScaleMin, CharacterSelectIndicatorLocalScaleMin), string.Empty);
            characterSelectIndicatorObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            characterSelectIndicatorObject.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(0.56f, 0.76f);
            characterSelectIndicatorObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.24f, 1.24f);

            UpdateCharacterSelectIndicator(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SelectCharacterView), nameof(SelectCharacterView.UpdateCarousel))]
        static void SelectCharacterView_UpdateCarousel(SelectCharacterView __instance)
        {
            UpdateCharacterSelectIndicator(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SelectCharacterView), nameof(SelectCharacterView.UpdatePlayable))]
        static void SelectCharacterView_UpdatePlayable(SelectCharacterView __instance)
        {
            UpdateCharacterSelectIndicator(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChooseBumbo), nameof(ChooseBumbo.ConfirmSelection))]
        static void ChooseBumbo_ConfirmSelection(ChooseBumbo __instance, bool __result)
        {
            if (!__result) return;
            GameObject characterSelectIndicatorObject = __instance.selectCharacterView.transform.parent.Find("Character Select Indicator")?.gameObject;
            if (characterSelectIndicatorObject == null) return;

            Tween localscaleTween = null;

            List<Tween> t = DOTween.TweensByTarget(characterSelectIndicatorObject.transform);
            if (t != null)
            {
                foreach (Tween tween in t)
                {
                    if (tween.id is string && (string)tween.id == "localScale") localscaleTween = tween;
                }
            }

            if (localscaleTween != null) localscaleTween.Kill();
            characterSelectIndicatorObject.transform.DOScale(Vector3.zero, 0.3f).SetId("scaleToZero").OnComplete(delegate { UnityEngine.Object.Destroy(characterSelectIndicatorObject); });
        }

        private static void UpdateCharacterSelectIndicator(SelectCharacterView selectCharacterView)
        {
            GameObject characterSelectIndicatorObject = selectCharacterView.transform.parent.Find("Character Select Indicator")?.gameObject;
            if (characterSelectIndicatorObject == null) return;
            if (selectCharacterView.progression != null)
            {
                CharacterSheet.BumboType suggestedBumbo = CharacterSheet.BumboType.Random;

                Vector3 indicatorPosition = Vector3.zero;

                if (!selectCharacterView.progression.Unlocked(Unlocks.BumboTheNimble))
                {
                    //Suggest playing as Bum-bo the Brave
                    suggestedBumbo = CharacterSheet.BumboType.TheBrave;
                    indicatorPosition = BravePosition;
                }

                if (selectCharacterView.progression.Unlocked(Unlocks.BumboTheNimble) && !selectCharacterView.progression.Unlocked(Unlocks.BumboTheStout))
                {
                    //Suggest playing as Bum-bo the Nimble
                    suggestedBumbo = CharacterSheet.BumboType.TheNimble;
                    indicatorPosition = NimblePosition;
                }

                if (selectCharacterView.progression.Unlocked(Unlocks.BumboTheStout) && !selectCharacterView.progression.Unlocked(Unlocks.BumboTheWeird))
                {
                    //Suggest playing as Bum-bo the Stout
                    suggestedBumbo = CharacterSheet.BumboType.TheStout;
                    indicatorPosition = StoutPosition;
                }

                if (selectCharacterView.progression.Unlocked(Unlocks.BumboTheWeird) && !selectCharacterView.progression.Unlocked(Unlocks.TheBasement))
                {
                    //Suggest playing as Bum-bo the Weird
                    suggestedBumbo = CharacterSheet.BumboType.TheWeird;
                    indicatorPosition = WeirdPosition;
                }

                if (suggestedBumbo != CharacterSheet.BumboType.Random)
                {
                    characterSelectIndicatorObject.SetActive(true);

                    List<CharacterSheet.BumboType> bumboTypes = (List<CharacterSheet.BumboType>)AccessTools.Field(typeof(SelectCharacterView), "bumboTypes").GetValue(selectCharacterView);
                    int index = (int)AccessTools.Field(typeof(SelectCharacterView), "index").GetValue(selectCharacterView);
                    CharacterSheet.BumboType currentBumbo = bumboTypes[index];

                    int distance = selectCharacterView.GetBumboTypeDistance(currentBumbo, suggestedBumbo);
                    if (Math.Abs(distance) > (bumboTypes.Count / 2)) distance -= (bumboTypes.Count * Math.Sign(distance));

                    if (distance < 0) indicatorPosition = LeftNavigationPosition;
                    else if (distance > 0) indicatorPosition = RightNavigationPosition;

                    Tween localmoveTween = null;
                    Tween localscaleTween = null;

                    List<Tween> t = DOTween.TweensByTarget(characterSelectIndicatorObject.transform);
                    if (t != null)
                    {
                        foreach (Tween tween in t)
                        {
                            if (tween.id is string && (string)tween.id == "scaleToZero") return;
                            if (tween.id is string && (string)tween.id == "localScale") localscaleTween = tween;
                            if (tween.id is string && (string)tween.id == "localMove") localmoveTween = tween;
                        }
                    }

                    if (localscaleTween == null) characterSelectIndicatorObject.transform.DOScale(CharacterSelectIndicatorLocalScaleMax, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetId("localScale");

                    if (localmoveTween != null) localmoveTween.Kill();
                    characterSelectIndicatorObject.transform.DOLocalMove(indicatorPosition, 0.5f).SetEase(Ease.OutQuad).SetId("localMove");
                    return;
                }
            }

            List<Tween> tweens = DOTween.TweensByTarget(characterSelectIndicatorObject.transform);
            if (tweens != null) foreach (Tween tween in tweens) tween.Kill();
            characterSelectIndicatorObject.SetActive(false);
        }
    }

    [HarmonyPatch]
    class GamepadSpellSelector_Selectable_Constructor_Patch
    {
        //Patch must be in its own class for HarmonyTargetMethod to work
        [HarmonyTargetMethod]
        static MethodBase SelectableConstrutor()
        {
            //To access stuff from a private class, HarmonyTargetMethod must be used to get the class type and method/constructor
            return AccessTools.Constructor(AccessTools.Inner(typeof(GamepadSpellSelector), "Selectable"), new Type[] { typeof(SpellView), typeof(int), typeof(GamepadSpellSelector) });
        }
        //Patch: Fixes empty spell slots being selectable using gamepad/keyboard controls after using Price Tag to remove a spell
        [HarmonyPrefix]
        static void GamepadSpellSelector_Selectable_Constructor(ref SpellView Spell)
        {
            //GamepadSpellSelector reads spell.IsActive property when determining whether the spell is selectable; the property isn't properly updated when the spell is removed, so this patch makes the spell unselectable if it's null
            if (Spell.SpellObject == null)
            {
                AccessTools.Property(typeof(SpellView), "isActive").SetValue(Spell, false);
            }
        }
    }
}