using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using PathologicalGames;

namespace The_Legend_of_Bum_bo_Windfall
{
    class InterfaceContent
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(InterfaceContent));
        }

        //Patch: Get app
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init_GetApp(BumboController __instance)
        {
            WindfallHelper.GetApp(__instance.app);
            BumboModifierIndication.GetApp(__instance.app);
            WindfallTooltipController.GetApp(__instance.app);
        }

        //Patch: Update Windfall Tooltip
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Update")]
        static void BumboController_Update(BumboController __instance)
        {
            WindfallTooltipController.UpdateTooltips();
        }

        //Patch: Display bumbo modifiers on RoomStartEvent
        [HarmonyPostfix, HarmonyPatch(typeof(RoomStartEvent), "Execute")]
        static void RoomStartEvent_Execute_ModifierDisplay(RoomStartEvent __instance)
        {
            __instance.app.StartCoroutine(BumboModifierIndication.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on NewRoundEvent
        [HarmonyPostfix, HarmonyPatch(typeof(NewRoundEvent), "Execute")]
        static void NewRoundEvent_Execute_ModifierDisplay(NewRoundEvent __instance)
        {
            __instance.app.StartCoroutine(BumboModifierIndication.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on CastSpell
        [HarmonyPostfix, HarmonyPatch(typeof(SpellElement), "CastSpell")]
        static void SpellElement_CastSpell_ModifierDisplay(SpellElement __instance, bool __result)
        {
            if (__result)
            {
                __instance.app.StartCoroutine(BumboModifierIndication.UpdateModifiersDelayed());
            }
        }
        //Patch: Display bumbo modifiers on Discharge
        [HarmonyPostfix, HarmonyPatch(typeof(SpellElement), "Discharge")]
        static void SpellElement_Discharge_ModifierDisplay(SpellElement __instance, bool __result)
        {
            if (__result)
            {
                __instance.app.StartCoroutine(BumboModifierIndication.UpdateModifiersDelayed());
            }
        }
        //Patch: Display bumbo modifiers on TakeDamage
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "TakeDamage")]
        static void BumboController_TakeDamage_ModifierDisplay(BumboController __instance)
        {
            __instance.app.StartCoroutine(BumboModifierIndication.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on Enemy Hurt
        [HarmonyPostfix, HarmonyPatch(typeof(Enemy), "Hurt")]
        static void Enemy_Hurt_ModifierDisplay(Enemy __instance)
        {
            __instance.app.StartCoroutine(BumboModifierIndication.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on NextComboEvent
        [HarmonyPostfix, HarmonyPatch(typeof(NextComboEvent), "NextEvent")]
        static void NextComboEvent_NextEvent_ModifierDisplay(NextComboEvent __instance)
        {
            __instance.app.StartCoroutine(BumboModifierIndication.UpdateModifiersDelayed());
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
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            var trinketGlitchMesh = assets.LoadAsset<Mesh>("Glitch_Visual_V3");
            var trinketGlitchTexture = assets.LoadAsset<Texture2D>("Glitch Visuals Texture");

            for (int trinketCounter = 0; trinketCounter < trinketGlitches.Length; trinketCounter++)
            {
                if (trinketCounter < trinkets.Length)
                {
                    trinketGlitches[trinketCounter] = GameObject.Instantiate(trinkets[trinketCounter], trinkets[trinketCounter].transform.position, trinkets[trinketCounter].transform.rotation, trinkets[trinketCounter].transform);

                    BoxCollider boxCollider = trinketGlitches[trinketCounter].GetComponent<BoxCollider>();
                    if (boxCollider)
                    {
                        GameObject.Destroy(boxCollider);
                    }

                    TrinketView trinketView = trinketGlitches[trinketCounter].GetComponent<TrinketView>();
                    if (trinketView)
                    {
                        GameObject.Destroy(trinketView);
                    }

                    ButtonHoverAnimation buttonHoverAnimation = trinketGlitches[trinketCounter].GetComponent<ButtonHoverAnimation>();
                    if (buttonHoverAnimation)
                    {
                        GameObject.Destroy(buttonHoverAnimation);
                    }

                    for (int childCounter = 0; childCounter < trinketGlitches[trinketCounter].transform.childCount; childCounter++)
                    {
                        GameObject.Destroy(trinketGlitches[trinketCounter].transform.GetChild(childCounter).gameObject);
                    }

                    MeshFilter meshFilter = trinketGlitches[trinketCounter].GetComponent<MeshFilter>();
                    meshFilter.mesh = trinketGlitchMesh;

                    MeshRenderer meshRenderer = trinketGlitches[trinketCounter].GetComponent<MeshRenderer>();
                    meshRenderer.material.mainTexture = trinketGlitchTexture;

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

        //***************************************************
        //**********Animation Speed Manipulation*************
        //***************************************************

        static float animationSpeed = 1.5f;

        //Patch: Applies animation speed setting to CupGameResultEvent
        //Also creates trinket display when winning a trinket while having no empty trinket slots
        //Also increases coin rewards
        [HarmonyPrefix, HarmonyPatch(typeof(CupGameResultEvent), "Execute")]
        static bool CupGameResultEvent_Execute(CupGameResultEvent __instance)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.timeScale *= animationSpeed; //Change speed of cup game result animation
            float num = UnityEngine.Random.Range(0f, 1f);
            bool flag = false;
            if (num > 0.66f)
            {
                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.SkullGameLose, SoundsView.eAudioSlot.Default, false);
                __instance.app.view.gamblingView.cupClerkView.AnimateDisappointed();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendInterval(sequence, 1f), delegate ()
                {
                    __instance.app.view.soundsView.PlayBumboSound(SoundsView.eSound.BumboHurt, __instance.app.model.characterSheet.bumboType, -1);
                }), 1f), delegate ()
                {
                    __instance.app.controller.gamblingController.cupGamble.PutDownCups();
                });
            }
            else if (num > 0.33f)
            {
                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.SkullGameWin, SoundsView.eAudioSlot.Default, false);
                __instance.app.view.gamblingView.cupClerkView.AnimateHappy();
                GameObject reward = null;
                string notification = string.Empty;
                int num2 = UnityEngine.Random.Range(0, 2);
                if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheLost)
                {
                    num2 = 1;
                }
                if (num2 != 1)
                {
                    reward = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Pickups/Soul Heart Pickup") as GameObject, __instance.app.controller.gamblingController.cupGamble.cups[__instance.app.model.gamblingModel.selectedCup].transform.position, Quaternion.Euler(0.016f, __instance.app.controller.gamblingController.cupGamble.cups[__instance.app.model.gamblingModel.selectedCup].transform.rotation.eulerAngles.y, -0.96f));
                    reward.transform.localPosition = __instance.app.controller.gamblingController.cupGamble.cups[__instance.app.model.gamblingModel.selectedCup].transform.position;
                    reward.transform.GetChild(0).localScale = new Vector3(0.75f, 1f, 0.75f);
                    reward.GetComponent<HeartPickupView>().Init(0, 0);
                    reward.GetComponent<BoxCollider>().enabled = false;
                    reward.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    reward.transform.localScale = new Vector3(1.25f, 0f, 1.25f);
                    notification = "GUI Notifications/WON_SOUL_HEART";
                    Sequence sequence2 = DOTween.Sequence();
                    TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendInterval(sequence2, 1.25f), delegate ()
                    {
                        SoundsView.Instance.PlaySound(SoundsView.eSound.SkullGame_Prize, SoundsView.eAudioSlot.Default, false);
                    }), delegate ()
                    {
                        __instance.app.controller.gamblingController.cupGamble.SpawnHearts(10);
                    });
                    __instance.app.model.characterSheet.addSoulHearts(1f);
                    __instance.app.controller.gamblingController.healthController.UpdateHearts(false);
                }
                else
                {
                    CoinView component = UnityEngine.Object.Instantiate<GameObject>(__instance.app.view.gamblingView.coinView.gameObject, __instance.app.controller.gamblingController.cupGamble.cups[__instance.app.model.gamblingModel.selectedCup].transform.position, Quaternion.Euler(0.016f, __instance.app.controller.gamblingController.cupGamble.cups[__instance.app.model.gamblingModel.selectedCup].transform.rotation.eulerAngles.y, -0.96f)).GetComponent<CoinView>();
                    component.gameObject.SetActive(true);
                    component.InitModel();
                    component.transform.localPosition = __instance.app.controller.gamblingController.cupGamble.cups[__instance.app.model.gamblingModel.selectedCup].transform.position;
                    component.activeCoinModel.transform.localRotation = Quaternion.Euler(90f, 90f, -90f);
                    component.transform.localScale = new Vector3(1.25f, 0f, 1.25f);
                    reward = component.gameObject;

                    int coin_result = UnityEngine.Random.Range(8, 11);

                    if (!WindfallPersistentDataController.LoadData().implementBalanceChanges)
                    {
                        coin_result = UnityEngine.Random.Range(0f, 1f) < 0.5f ? 4 : 6;
                    }

                    __instance.app.view.gamblingView.gamblingBangView.paramsManager.SetParameterValue("AMOUNT", coin_result.ToString(), true);
                    notification = "GUI Notifications/WON_COINS";
                    __instance.app.controller.gamblingController.ModifyCoins(coin_result);
                    Sequence sequence3 = DOTween.Sequence();
                    TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendInterval(sequence3, 1.25f), delegate ()
                    {
                        SoundsView.Instance.PlaySound(SoundsView.eSound.SkullGame_Prize, SoundsView.eAudioSlot.Default, false);
                    }), delegate ()
                    {
                        __instance.app.controller.gamblingController.cupGamble.SpawnCoins(coin_result);
                    });
                }
                TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(sequence, 0.25f), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(reward.transform, Vector3.one, 0.25f), Ease.InOutQuad)), 1f);
                TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(sequence, delegate ()
                {
                    __instance.app.view.soundsView.PlayBumboSound(SoundsView.eSound.BumboHappy, __instance.app.model.characterSheet.bumboType, -1);
                }), delegate ()
                {
                    __instance.app.view.gamblingView.gamblingBangView.Appear(notification);
                }), 2f), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(reward.transform, new Vector3(1.25f, 0f, 1.25f), 0.15f), Ease.InOutQuad)), delegate ()
                {
                    __instance.app.controller.gamblingController.cupGamble.PutDownCups();
                }), delegate ()
                {
                    UnityEngine.Object.Destroy(reward);
                }), 0.35f);
            }
            else
            {
                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.SkullGameWin, SoundsView.eAudioSlot.Default, false);
                __instance.app.view.gamblingView.cupClerkView.AnimateHappy();
                GameObject reward = null;
                string notification = string.Empty;
                reward = __instance.app.controller.gamblingController.cupGamble.SetTrinket(__instance.app.model.gamblingModel.trinketReward, 1);
                reward.transform.localScale = new Vector3(1f, 0f, 1f);
                reward.transform.localPosition = __instance.app.controller.gamblingController.cupGamble.cups[__instance.app.model.gamblingModel.selectedCup].transform.position;
                notification = "GUI Notifications/WON_TRINKET";
                flag = __instance.app.model.characterSheet.trinkets.Count == 4;
                if (!flag)
                {
                    __instance.app.model.characterSheet.trinkets.Add(__instance.app.model.trinketModel.trinkets[__instance.app.model.gamblingModel.trinketReward]);
                    __instance.app.controller.UpdateTrinkets();
                }
                TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(sequence, 0.25f), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(reward.transform, Vector3.one, 0.25f), Ease.InOutQuad)), 1f);
                TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(sequence, delegate ()
                {
                    __instance.app.view.gamblingView.gamblingBangView.Appear(notification);
                }), delegate ()
                {
                    __instance.app.view.soundsView.PlayBumboSound(SoundsView.eSound.BumboVeryHappy, __instance.app.model.characterSheet.bumboType, -1);
                }), 2f), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(reward.transform, new Vector3(1.25f, 0f, 1.25f), 0.15f), Ease.InOutQuad)), delegate ()
                {
                    __instance.app.controller.gamblingController.cupGamble.PutDownCups();
                }), delegate ()
                {
                    UnityEngine.Object.Destroy(reward);
                }), 0.35f);
            }
            if (flag)
            {
                TweenSettingsExtensions.AppendCallback(sequence, delegate ()
                {
                    __instance.app.controller.eventsController.SetEvent(new TrinketReplaceEvent(0, true));

                    //Create trinket reward display
                    CreateTrinketRewardDisplay(__instance.app);
                });
            }
            else
            {
                __instance.app.controller.gamblingController.shop.UpdatePrices();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.AppendCallback(sequence, delegate ()
                {
                    __instance.app.view.gamblingView.cupClerkView.AnimateIdle();
                }), 0.25f), delegate ()
                {
                    __instance.app.view.gamblingView.gamblingCameraView.ShowArrows();
                }), delegate ()
                {
                    __instance.app.controller.gamblingController.cupGamble.ReadyGame();
                });
            }
            return false;
        }

        //Patch: Applies animation speed setting to CupGameEvent
        [HarmonyPrefix, HarmonyPatch(typeof(CupGameEvent), "Execute")]
        static bool CupGameEvent_Execute(CupGameEvent __instance)
        {
            __instance.app.view.gamblingView.gamblingCameraView.HideArrows();
            List<TrinketName> list = new List<TrinketName>();
            list.AddRange(__instance.app.model.trinketModel.validTrinkets.ToArray());
            GameObject reward = null;
            __instance.app.model.gamblingModel.trinketReward = list[UnityEngine.Random.Range(0, list.Count)];
            reward = __instance.app.controller.gamblingController.cupGamble.SetTrinket(__instance.app.model.gamblingModel.trinketReward, 1);
            reward.transform.localScale = new Vector3(1f, 0f, 1f);
            __instance.app.view.gamblingView.cupClerkView.AnimateHappy();
            Sequence sequence = DOTween.Sequence();
            TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(sequence, 0.25f), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(reward.transform, new Vector3(0.8f, 1.25f, 0.8f), 0.25f), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(reward.transform, Vector3.one, 0.125f), Ease.InOutQuad));
            SoundsView.Instance.PlaySound(SoundsView.eSound.SkullGame_Start, SoundsView.eAudioSlot.Default, false);
            Sequence sequence2 = DOTween.Sequence();

            //Change speed of cup game startup animation
            sequence.timeScale *= animationSpeed;
            sequence2.timeScale *= animationSpeed;

            TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(sequence2, __instance.app.controller.gamblingController.cupGamble.cups[1].GetComponent<SkullCup>().LiftCup()), 1.25f);
            float num = TweenExtensions.Duration(sequence2, true);
            TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Insert(TweenSettingsExtensions.Insert(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(sequence2, 0.25f), __instance.app.controller.gamblingController.cupGamble.cups[1].GetComponent<SkullCup>().PutDownCup()), num, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(reward.transform, new Vector3(0.8f, 1.25f, 0.8f), 0.25f), Ease.InOutQuad)), num + 0.25f, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(reward.transform, new Vector3(1.25f, 0f, 1.25f), 0.125f), Ease.InOutQuad)), delegate ()
            {
                UnityEngine.Object.Destroy(reward);
            }), delegate ()
            {
                __instance.app.view.gamblingView.cupClerkView.AnimateShuffle();
            });

            return false;
        }

        //Patch: Applies animation speed setting to CupClerkView AnimateShuffle
        [HarmonyPostfix, HarmonyPatch(typeof(CupClerkView), "AnimateShuffle")]
        static void CupClerkView_Shuffle(CupClerkView __instance, ref Sequence ___animation)
        {
            ___animation.timeScale *= animationSpeed;
        }

        //Patch: Applies animation speed setting to GamblingBangView Disappear
        [HarmonyPrefix, HarmonyPatch(typeof(GamblingBangView), "Disappear")]
        static bool GamblingBangView_Disappear(GamblingBangView __instance)
        {
            SoundsView.Instance.PlaySound(SoundsView.eSound.Splash_Sign_Hide, SoundsView.eAudioSlot.Default, false);
            Sequence sequence = DOTween.Sequence();
            TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(sequence, 1.5f * (1 / animationSpeed)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, new Vector3(0.67f, 1f, 1.5f), 0.2f), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(__instance.transform, new Vector3(1.5f, 1f, 0.1f), 0.1f), Ease.InOutQuad)), delegate ()
            {
                __instance.gameObject.SetActive(false);
            });
            return false;
        }

        //Patch: Applies animation speed setting to stat wheel spin
        //Also causes stat wheel to avoid increasing maxed stats
        [HarmonyPrefix, HarmonyPatch(typeof(WheelSpin), "Spin")]
        static bool WheelSpin_Spin(WheelSpin __instance, bool _pay, ref bool ___isSpinning)
        {
            __instance.controller.ModifyCoins(-15);
            __instance.wheelView.gamblingCameraView.HideArrows();

            //Prevent the stat wheel from increasing maxed stats
            bool flag = __instance.app.model.characterSheet.bumboBaseInfo.hitPoints < 6f;
            if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheDead)
            {
                flag = (__instance.app.model.characterSheet.soulHearts < 6f);
            }
            bool flag2 = __instance.app.model.characterSheet.bumboBaseInfo.itemDamage < 5;
            bool flag3 = __instance.app.model.characterSheet.bumboBaseInfo.puzzleDamage < 5;
            bool flag4 = __instance.app.model.characterSheet.bumboBaseInfo.dexterity < 5;
            bool flag5 = __instance.app.model.characterSheet.bumboBaseInfo.luck < 5;
            bool flag6 = false;
            bool[] array = new bool[__instance.model.wheelSlices.Length];
            int wheelReward;
            if (UnityEngine.Random.Range(0f, 1f) < 0.166f && __instance.app.model.characterSheet.bumboType != CharacterSheet.BumboType.TheStout)
            {
                wheelReward = 0;
            }
            else
            {
                for (int i = 0; i < __instance.model.wheelSlices.Length; i++)
                {
                    if ((__instance.model.wheelSlices[i] == WheelView.WheelSlices.Health && flag) || (__instance.model.wheelSlices[i] == WheelView.WheelSlices.ItemDamage && flag2) || (__instance.model.wheelSlices[i] == WheelView.WheelSlices.PuzzleDamage && flag3) || (__instance.model.wheelSlices[i] == WheelView.WheelSlices.Dexterity && flag4) || (__instance.model.wheelSlices[i] == WheelView.WheelSlices.Luck && flag5))
                    {
                        array[i] = true;
                    }
                    else
                    {
                        array[i] = false;
                    }
                }
                for (int j = 0; j < array.Length; j++)
                {
                    if (array[j])
                    {
                        flag6 = true;
                    }
                }
                if (!flag6)
                {
                    for (int k = 0; k < array.Length; k++)
                    {
                        array[k] = true;
                    }
                }
                List<int> list = new List<int>();
                for (int l = 0; l < array.Length; l++)
                {
                    if (array[l])
                    {
                        list.Add(l);
                    }
                }
                int index = UnityEngine.Random.Range(0, list.Count);
                wheelReward = list[index];
            }
            ___isSpinning = true;
            Sequence sequence = __instance.wheelView.Spin(wheelReward, _pay);

            sequence.timeScale *= animationSpeed; //Change speed of the wheel spin animation

            TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendInterval(sequence, 0.25f), delegate ()
            {
                SoundsView.Instance.PlaySound(SoundsView.eSound.Shop_GambleReward, SoundsView.eAudioSlot.Default, false);
            }), delegate ()
            {
                AccessTools.Method(typeof(WheelSpin), "Reward").Invoke(__instance, new object[] { __instance.model.wheelSlices[wheelReward] });
            });

            return false;
        }

        //Patch: Applies animation speed setting to stat wheel reward
        [HarmonyPrefix, HarmonyPatch(typeof(WheelSpin), "Reward")]
        static bool WheelSpin_Reward(WheelSpin __instance, WheelView.WheelSlices _reward)
        {
            __instance.app.view.soundsView.PlaySound(SoundsView.eSound.WheelSpinResult, __instance.wheelView.transform.position, SoundsView.eAudioSlot.Default, false);
            Sequence sequence = DOTween.Sequence();

            sequence.timeScale *= animationSpeed; //Change speed of wheel reward animation

            TweenSettingsExtensions.Join(TweenSettingsExtensions.Append(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMove(__instance.wheelView.gamblingCameraView.transform, new Vector3(0f, 1f, -5.68f), 1f, false), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(__instance.wheelView.gamblingCameraView.transform, Vector3.zero, 1f, 0), Ease.InOutQuad));
            switch (_reward)
            {
                case WheelView.WheelSlices.Dexterity:
                    if (__instance.app != null)
                    {
                        TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                        {
                            __instance.app.model.characterSheet.addDex(1);
                        });
                    }
                    TweenSettingsExtensions.InsertCallback(sequence, 0.6f, delegate ()
                    {
                        __instance.wheelView.gamblingController.UpdateStats();
                    });
                    TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                    {
                        __instance.wheelView.gamblingBubbleView.Reward(WheelSpin.WheelResult.Dexterity);
                    });
                    __instance.wheelView.Idle();
                    break;
                case WheelView.WheelSlices.PuzzleDamage:
                    if (__instance.app != null)
                    {
                        TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                        {
                            __instance.app.model.characterSheet.addPuzzleDamage(1);
                        });
                    }
                    TweenSettingsExtensions.InsertCallback(sequence, 0.6f, delegate ()
                    {
                        __instance.wheelView.gamblingController.UpdateStats();
                    });
                    TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                    {
                        __instance.wheelView.gamblingBubbleView.Reward(WheelSpin.WheelResult.PuzzlePower);
                    });
                    __instance.wheelView.Idle();
                    break;
                case WheelView.WheelSlices.Luck:
                    if (__instance.app != null)
                    {
                        TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                        {
                            __instance.app.model.characterSheet.addLuck(1);
                        });
                    }
                    TweenSettingsExtensions.InsertCallback(sequence, 0.6f, delegate ()
                    {
                        __instance.wheelView.gamblingController.UpdateStats();
                    });
                    TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                    {
                        __instance.wheelView.gamblingBubbleView.Reward(WheelSpin.WheelResult.Luck);
                    });
                    __instance.wheelView.Idle();
                    break;
                case WheelView.WheelSlices.ItemDamage:
                    if (__instance.app != null)
                    {
                        TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                        {
                            __instance.app.model.characterSheet.addItemDamage(1);
                        });
                    }
                    TweenSettingsExtensions.InsertCallback(sequence, 0.6f, delegate ()
                    {
                        __instance.wheelView.gamblingController.UpdateStats();
                    });
                    TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                    {
                        __instance.wheelView.gamblingBubbleView.Reward(WheelSpin.WheelResult.ItemPower);
                    });
                    __instance.wheelView.Idle();
                    break;
                case WheelView.WheelSlices.Health:
                    if (__instance.app != null)
                    {
                        if (__instance.app.model.characterSheet.bumboBaseInfo.bumboType == CharacterSheet.BumboType.TheDead)
                        {
                            __instance.app.model.characterSheet.addSoulHearts(1f);
                            TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                            {
                                __instance.wheelView.gamblingController.healthController.UpdateHearts(true);
                            });
                        }
                        else
                        {
                            __instance.app.model.characterSheet.addHitPoints(1);
                            float hit_points = __instance.app.model.characterSheet.getHitPoints();
                            __instance.app.model.characterSheet.hitPoints = __instance.app.model.characterSheet.getHitPoints();
                            TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                            {
                                __instance.wheelView.gamblingController.healthController.SetHearts((int)hit_points);
                            });
                        }
                    }
                    TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                    {
                        __instance.wheelView.gamblingController.UpdateStats();
                    });
                    TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                    {
                        __instance.wheelView.gamblingBubbleView.Reward(WheelSpin.WheelResult.Health);
                    });
                    __instance.wheelView.Idle();
                    break;
                default:
                    if (__instance.app != null)
                    {
                        TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                        {
                            __instance.app.model.characterSheet.Spend(-20);
                        });
                    }
                    TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
                    {
                        __instance.wheelView.gamblingBubbleView.CoinReward(20);
                    }), 0.6f, delegate ()
                    {
                        __instance.wheelView.gamblingController.UpdateCoins();
                    }), 0.6f, delegate ()
                    {
                        __instance.SpawnCoins(20);
                    });
                    __instance.wheelView.Idle();
                    break;
            }
            TweenSettingsExtensions.OnComplete<Sequence>(TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.InsertCallback(sequence, 0.5f, delegate ()
            {
                __instance.app.view.soundsView.PlayBumboSound(SoundsView.eSound.BumboLaugh, __instance.app.model.characterSheet.bumboType, -1);
            }), 1.25f, delegate ()
            {
                __instance.wheelView.wheelClerkView.Celebrate();
            }), 3.375f, delegate ()
            {
                __instance.wheelView.gamblingCameraView.ShowArrows();
            }), delegate ()
            {
                AccessTools.Method(typeof(WheelSpin), "MakeWheelClickable").Invoke(__instance, new object[] { });
            });
            return false;
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Fixes canceling spells with temporarily reduced mana costs refunding more mana than they cost
        //Patch also counteracts reduction in mana refund from Sucker enemies
        //Patch: Add cancel functionality for use trinkets
        //Patch: Fixes canceling actions that modify your spells causing both the puzzle board and the spell menu to be selectable at the same time when using gamepad or keyboard controls
        //Patch: Fixes camera moving incorrectly when canceling taking a trinket from a treasure room
        [HarmonyPrefix, HarmonyPatch(typeof(EventsController), "OnNotification")]
        static bool EventsController_OnNotification(EventsController __instance, string _event_path, object _target, params object[] _data)
        {
            if (_event_path == "cancel.spell" && (__instance.app.model.bumboEvent.GetType().ToString() == "TreasureStartEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "GrabBossRewardEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceCancelledEvent"))
            {
                __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);
                __instance.app.view.GUICamera.GetComponent<GUISide>().expandGUIView.Hide();
                if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure)
                {
                    PoolManager.Pools["Spells"].GetComponent<GamepadTreasureRoomController>().Shutdown();
                }
                else if (__instance.app.view.gamblingView == null)
                {
                    __instance.app.view.bossRewardParents[0].transform.parent.GetComponent<GamepadTreasureRoomController>().Shutdown();
                }
                __instance.EndEvent();
                return false;
            }
            //Only perform this trinket replace cancel in boss rooms or at the skull game
            if (_event_path == "cancel.spell" && __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceEvent" && ((__instance.app.view.gamblingView == null && __instance.app.model.mapModel?.currentRoom?.roomType == MapRoom.RoomType.Boss) || (__instance.app.view.gamblingView != null && __instance.app.model.gamblingModel?.cameraAt == 0)))
            {
                TrinketReplaceEvent trinketReplaceEvent = __instance.app.model.bumboEvent as TrinketReplaceEvent;
                __instance.app.controller.HideNotifications(false);
                __instance.SetEvent(new TrinketReplaceCancelledEvent());
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

                Sequence s = DOTween.Sequence();
                s.Append(__instance.app.view.mainCameraView.transform.DOMove(new Vector3(0f, 0.46f, -3.27f), 1f, false).SetEase(Ease.InOutQuad));
                s.Join(__instance.app.view.mainCameraView.transform.DORotate(new Vector3(-1.43f, 0f, 0f), 1f, RotateMode.Fast).SetEase(Ease.InOutQuad));

                if (__instance.app.view.gamblingView == null)
                {
                    //Enable boss room pickups
                    s.AppendCallback(delegate
                    {
                        GameObject[] bossRewardParents = __instance.app.view.bossRewardParents;
                        if (bossRewardParents != null && bossRewardParents.Length > 1)
                        {
                            bossRewardParents[0]?.transform.GetChild(0)?.GetComponent<TrinketPickupView>()?.SetClickable(true);
                            bossRewardParents[1]?.transform.GetChild(0)?.GetComponent<TrinketPickupView>()?.SetClickable(true);
                        }
                    });
                }

                __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);
                if (__instance.app.view.gamblingView == null && __instance.app.model.mapModel.currentRoom.roomType != MapRoom.RoomType.Treasure)
                {
                    __instance.app.view.bossRewardParents[0].transform.parent.GetComponent<GamepadTreasureRoomController>().Initialize(trinketReplaceEvent.index);
                }
                return false;
            }
            if (_event_path == "cancel.spell" && __instance.app.view.gamblingView != null)
            {
                __instance.app.view.GUICamera.GetComponent<GUISide>().expandGUIView.Show();
                __instance.app.controller.HideNotifications(false);
                __instance.app.controller.gamblingController.CancelPickup();
                __instance.SetEvent(new SpellModifyDelayEvent(false));
                __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);
            }
            else if (_event_path == "cancel.spell" && __instance.app.view.boxes.treasureRoom != null && __instance.app.view.boxes.treasureRoom.gameObject.activeSelf)
            {
                if (__instance.app.model.bumboEvent.GetType() == typeof(TreasureSpellReplaceEvent) || __instance.app.model.bumboEvent.GetType() == typeof(TrinketReplaceEvent))
                {
                    int index = 0;
                    if (__instance.app.model.bumboEvent.GetType() == typeof(TreasureSpellReplaceEvent))
                    {
                        index = (__instance.app.model.bumboEvent as TreasureSpellReplaceEvent).index;
                    }
                    else if (__instance.app.model.bumboEvent.GetType() == typeof(TrinketReplaceEvent))
                    {
                        index = (__instance.app.model.bumboEvent as TrinketReplaceEvent).index;
                    }
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
                    __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>().cancelView.Show(new Vector3(0.6f, -0.55f, -2.29f), true);
                    __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);
                    Sequence s2 = DOTween.Sequence();
                    s2.Append(__instance.app.view.mainCameraView.transform.DOMove(new Vector3(0f, 1f, -4.29f), 1f, false).SetEase(Ease.InOutQuad));
                    s2.Join(__instance.app.view.mainCameraView.transform.DORotate(new Vector3(8.2f, 1.33f, 0f), 1f, RotateMode.Fast).SetEase(Ease.InOutQuad));
                    s2.AppendCallback(delegate
                    {
                        __instance.SetEvent(new TreasureStartEvent(index));
                    });
                }
            }
            else if (_event_path == "cancel.spell")
            {
                if (__instance.app.model.bumboEvent.GetType().ToString() == "SpellModifySpellEvent")
                {
                    //Fixed canceling actions that modify your spells causing both the puzzle board and the spell menu to be selectable at the same time when using gamepad or keyboard controls
                    __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);

                    //Add cancel functionality for use trinkets
                    if (CollectibleChanges.currentTrinket != null)
                    {
                        for (int spellCounter2 = 0; spellCounter2 < __instance.app.model.characterSheet.spells.Count; spellCounter2++)
                        {
                            if (CollectibleChanges.enabledSpells[spellCounter2])
                            {
                                __instance.app.view.spells[spellCounter2].EnableSpell();
                            }
                            else
                            {
                                __instance.app.view.spells[spellCounter2].DisableSpell();
                            }
                        }
                        CollectibleChanges.currentTrinket = null;
                    }
                }

                //Null check for spellViewUsed
                if (__instance.app.controller.trinketController.RerollSpellCost() && __instance.app.model.spellViewUsed != null)
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
                //Null check for current spell
                else if (__instance.app.model.spellModel.currentSpell != null)
                {
                    //Mana cost refund
                    //UpdateMana replacement that doesn't factor Sucker mana reduction
                    //Include cost modifier
                    for (int colorCounter = 0; colorCounter < 6; colorCounter++)
                    {
                        short[] mana = __instance.app.model.mana;
                        mana[colorCounter] += (short)(__instance.app.model.spellModel.currentSpell.Cost[colorCounter] + __instance.app.model.spellModel.currentSpell.CostModifier[colorCounter]);
                        if (mana[colorCounter] > 9)
                        {
                            mana[colorCounter] = 9;
                        }
                        if (mana[colorCounter] < 0)
                        {
                            mana[colorCounter] = 0;
                        }
                        __instance.app.view.manaView.setManaText((ManaType)colorCounter, __instance.app.model.mana[(int)colorCounter]);
                    }

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
            if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure)
            {
                //_event_path == "acquire.spell";
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
                        break;
                    case NavigationArrowView.Direction.Down:
                        num2 = __instance.app.model.mapModel.currentRoom.x;
                        num3 = __instance.app.model.mapModel.currentRoom.y - 1;
                        break;
                    default:
                        num2 = __instance.app.model.mapModel.currentRoom.x - 1;
                        num3 = __instance.app.model.mapModel.currentRoom.y;
                        break;
                    case NavigationArrowView.Direction.Right:
                        num2 = __instance.app.model.mapModel.currentRoom.x + 1;
                        num3 = __instance.app.model.mapModel.currentRoom.y;
                        break;
                }
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

        //Patch: Fixes triggering actions that modify your spells using mouse controls causing both the puzzle board and the spell menu to be selectable at the same time using gamepad/keyboard controls
        [HarmonyPrefix, HarmonyPatch(typeof(SpellView), "OnMouseDown")]
        static void SpellView_OnMouseDown_Prefix(SpellView __instance)
        {
            object spell = AccessTools.Field(typeof(SpellView), "spell").GetValue(__instance);
            if (!__instance.app.model.paused && spell is SpellElement && (SpellElement)spell != null && !__instance.disableObject.activeSelf && __instance.app.model.bumboEvent.GetType().ToString() == "SpellModifySpellEvent")
            {
                __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);
            }
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
            trinketRewardDisplay.SetActive(true);
            trinketRewardDisplay.GetComponent<BoxCollider>().enabled = false;
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

        //***************************************************
        //***************************************************
        //***************************************************

        //***************************************************
        //*****************Cutscene Menu********************
        //***************************************************

        //Patch: Add cutscene menu button to main menu
        //Also disables cutscenes that haven't been unlocked
        //Also adds coins to title box
        //Also adds win streak counter to main menu
        //Also applies graphics settings to title camera and character select camera
        [HarmonyPostfix, HarmonyPatch(typeof(TitleController), "Start")]
        static void TitleController_Start(TitleController __instance)
        {
            GameObject cutscenesButton = UnityEngine.Object.Instantiate(__instance.debugMenu.transform.Find("Cutscenes").gameObject, __instance.mainMenu.transform);
            cutscenesButton.GetComponent<RectTransform>().SetSiblingIndex(4);

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

            WindfallHelper.UpdateGamepadMenuButtons(gamepadMenuController, null);

            //Recenter main menu
            RectTransform mainMenuRectTransform = __instance.mainMenu.GetComponent<RectTransform>();
            if (mainMenuRectTransform != null)
            {
                mainMenuRectTransform.anchoredPosition = new Vector2(mainMenuRectTransform.anchoredPosition.x, mainMenuRectTransform.anchoredPosition.y + 22);
            }

            //Reorder endings
            __instance.cutsceneMenu.transform.Find("Weird Ending").SetSiblingIndex(4);
            __instance.cutsceneMenu.transform.Find("Credits Roll").SetSiblingIndex(6);

            //Disable endings based on game progress
            Progression progression = ProgressionController.LoadProgression();
            if (!progression.unlocks[0])
            {
                //Disable Mini Credits
                __instance.cutsceneMenu.transform.Find("Mini Credits Roll").GetComponent<Button>().interactable = false;

                //Disable Nimble ending
                __instance.cutsceneMenu.transform.Find("Nimble Ending").GetComponent<Button>().interactable = false;
            }
            if (!progression.unlocks[1])
            {
                //Disable Stout ending
                __instance.cutsceneMenu.transform.Find("Stout Ending").GetComponent<Button>().interactable = false;
            }
            if (!progression.unlocks[2])
            {
                //Disable Weird ending
                __instance.cutsceneMenu.transform.Find("Weird Ending").GetComponent<Button>().interactable = false;
            }
            if (!progression.unlocks[5])
            {
                //Disable Basement ending
                __instance.cutsceneMenu.transform.Find("Ending Basement").GetComponent<Button>().interactable = false;
            }
            if (progression.wins < 1)
            {
                //Disable Mom ending
                __instance.cutsceneMenu.transform.Find("Ending Mom").GetComponent<Button>().interactable = false;
                //Disable Credits Roll
                __instance.cutsceneMenu.transform.Find("Credits Roll").GetComponent<Button>().interactable = false;
            }

            bool removeFinalEnding = false;
            for (int i = 0; i < 31; i++)
            {
                if (!progression.unlocks[i])
                {
                    removeFinalEnding = true;
                }
            }
            if (removeFinalEnding)
            {
                //Disable Final ending
                __instance.cutsceneMenu.transform.Find("Ending Final").GetComponent<Button>().interactable = false;
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
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

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

            //Set up windfall options
            WindfallOptionsMenu.SetUpWindfallOptionsMenu(__instance.menuObject, false);
            //GraphicsOptions.SetUpGraphicsOptions(__instance.menuObject, false);

            //Create windfall options menu
            WindfallOptionsMenu.CreateWindfallOptionsMenu(__instance.menuObject);
        }

        //Patch: Set up graphics menu
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init_Graphics(BumboController __instance)
        {
            //Set up windfall options
            WindfallOptionsMenu.SetUpWindfallOptionsMenu(__instance.app.view.menuView, true);
            //GraphicsOptions.SetUpGraphicsOptions(__instance.app.view.menuView, true);

            //Create windfall options menu
            WindfallOptionsMenu.CreateWindfallOptionsMenu(__instance.app.view.menuView);
            //GraphicsOptions.CreateGraphicsMenu(__instance.app.view.menuView);
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
    }
}