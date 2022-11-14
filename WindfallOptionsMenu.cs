using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using I2.Loc;
using static UnityStandardAssets.ImageEffects.BloomOptimized;
using UnityStandardAssets.ImageEffects;
using ScionEngine;
using UnityEngine.Assertions;
using UnityEngine.TextCore;
using System.Linq;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class WindfallOptionsMenu
    {
        private static GameObject menuViewReference;
        private static GameObject windfallOptionsMenu;

        private static bool balanceChanges;
        private static bool antiAliasing;
        private static bool motionBlur;

        private static Sprite toggleActive;
        private static Sprite toggleInactive;

        public static void SetUpWindfallOptionsMenu(GameObject menuView, bool pauseMenu)
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            //Add sprites
            toggleActive = assets.LoadAsset<Sprite>("UI Toggle Active Thick");
            toggleInactive = assets.LoadAsset<Sprite>("UI Toggle Inactive Thick");

            ReorganizeVanillaOptionsMenu(menuView);
        }

        public static void ReorganizeVanillaOptionsMenu(GameObject menuView)
        {
            menuViewReference = menuView;

            Transform optionsMenuPcTransform = menuView?.transform.Find("Options Menu PC");

            if (optionsMenuPcTransform != null)
            {
                //Reorganize vanilla options menu
                for (int childCounter = 0; childCounter < optionsMenuPcTransform.childCount; childCounter++)
                {
                    RectTransform rectTransform = optionsMenuPcTransform.GetChild(childCounter)?.GetComponent<RectTransform>();

                    float yOffset;
                    if (optionsMenuPcTransform.childCount - childCounter < 3) //Move save/cancel down instead of up
                    {
                        yOffset = -10;
                    }
                    else
                    {
                        yOffset = 20;
                    }
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + yOffset);
                }
            }

            //Add Windfall options button to the vanilla options menu
            Transform graphicsOptionsButtonTransform = optionsMenuPcTransform?.Find("Graphics Options");

            GameObject windfallOptionsButtonObject = null;
            if (graphicsOptionsButtonTransform != null)
            {
                windfallOptionsButtonObject = UnityEngine.Object.Instantiate(graphicsOptionsButtonTransform.gameObject, graphicsOptionsButtonTransform.parent);
            }

            int graphicsOptionsButtonIndex = graphicsOptionsButtonTransform.GetSiblingIndex();

            if (graphicsOptionsButtonIndex + 1 < optionsMenuPcTransform.childCount)
            {
                windfallOptionsButtonObject.transform.SetSiblingIndex(graphicsOptionsButtonIndex + 1);
            }

            RectTransform windfallOptionsRectTransform;
            Localize windfallOptionsLocalize;
            LocalizationFontOverrides windfallOptionsLocalization;
            TextMeshProUGUI windfallOptionsTextMeshPro;
            Button windfallOptionsButton;
            if (windfallOptionsButtonObject != null)
            {
                windfallOptionsRectTransform = windfallOptionsButtonObject.GetComponent<RectTransform>();
                windfallOptionsLocalize = windfallOptionsButtonObject.GetComponent<Localize>();
                windfallOptionsLocalization = windfallOptionsButtonObject.GetComponent<LocalizationFontOverrides>();
                windfallOptionsTextMeshPro = windfallOptionsButtonObject.GetComponent<TextMeshProUGUI>();
                windfallOptionsButton = windfallOptionsButtonObject.GetComponent<Button>();

                if (windfallOptionsRectTransform != null)
                {
                    windfallOptionsRectTransform.anchoredPosition = new Vector2(windfallOptionsRectTransform.anchoredPosition.x, windfallOptionsRectTransform.anchoredPosition.y - 35);
                }

                if (windfallOptionsLocalize != null)
                {
                    Localization.SetKey(windfallOptionsLocalize, eI2Category.Menu, "WINDFALL_OPTIONS");
                }

                //if (windfallOptionsLocalization != null)
                //{
                //    windfallOptionsLocalization.enabled = false;
                //    UnityEngine.Object.Destroy(windfallOptionsLocalization);
                //}

                //if (windfallOptionsTextMeshPro != null)
                //{
                //    AssetBundle assets = Windfall.assetBundle;
                //    TMP_FontAsset fontAsset = assets.LoadAsset<TMP_FontAsset>("TMP_EdFont SDF");
                //    if (fontAsset != null)
                //    {
                //        windfallOptionsTextMeshPro.font = fontAsset;
                //    }
                //}

                if (windfallOptionsButton != null)
                {
                    windfallOptionsButton.onClick = new Button.ButtonClickedEvent();
                    windfallOptionsButton.onClick.AddListener(OpenWindfallOptionsMenu);
                }
            }

            GamepadMenuController optionsMenuPcGamepadMenuController = optionsMenuPcTransform.GetComponent<GamepadMenuController>();

            if (optionsMenuPcGamepadMenuController != null)
            {
                WindfallHelper.UpdateGamepadMenuButtons(optionsMenuPcGamepadMenuController, null);
            }

            //Fix Music/SFX labels acting as buttons
            GameObject musicLabel = optionsMenuPcTransform?.Find("MusicLabel").gameObject;
            ButtonHoverAnimation musicLabelButtonHoverAnimation = musicLabel?.GetComponent<ButtonHoverAnimation>();
            if (musicLabelButtonHoverAnimation != null)
            {
                musicLabelButtonHoverAnimation.hoverSoundFx = SoundsView.eSound.NoSound;
                musicLabelButtonHoverAnimation.clickSoundFx = SoundsView.eSound.NoSound;
                musicLabelButtonHoverAnimation.scaleAmount = 1;
            }

            GameObject sfxLabel = optionsMenuPcTransform?.Find("SFXLabel").gameObject;
            ButtonHoverAnimation sfxLabelButtonHoverAnimation = sfxLabel?.GetComponent<ButtonHoverAnimation>();
            if (sfxLabelButtonHoverAnimation != null)
            {
                sfxLabelButtonHoverAnimation.hoverSoundFx = SoundsView.eSound.NoSound;
                sfxLabelButtonHoverAnimation.clickSoundFx = SoundsView.eSound.NoSound;
                sfxLabelButtonHoverAnimation.scaleAmount = 1;
            }

            List<GameObject> buttons = optionsMenuPcGamepadMenuController?.m_Buttons.ToList();
            if (optionsMenuPcGamepadMenuController != null)
            {
                buttons.RemoveAll(button => button == musicLabel || button == sfxLabel);
                optionsMenuPcGamepadMenuController.m_Buttons = buttons.ToArray();
            }
        }

        public static void CreateWindfallOptionsMenu(GameObject menuView)
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            //Create windfall menu
            windfallOptionsMenu = UnityEngine.Object.Instantiate(assets.LoadAsset<GameObject>("Windfall Menu"), menuView.transform);
            windfallOptionsMenu.SetActive(false);
            if (windfallOptionsMenu.transform.parent.childCount > 0)
            {
                windfallOptionsMenu.transform.SetSiblingIndex(windfallOptionsMenu.transform.parent.childCount - 2);
            }

            RectTransform windfallMenuRect;
            if (windfallOptionsMenu != null)
            {
                windfallMenuRect = windfallOptionsMenu.GetComponent<RectTransform>();
                //windfallMenuRect.anchoredPosition = new Vector2(-3.6, 55.4);
                //windfallMenuRect.localRotation = Quaternion.Euler(graphicsMenuRect.localRotation.eulerAngles.x, graphicsMenuRect.localRotation.eulerAngles.y, 351);

                //Initialize buttons
                InitializeButton(windfallOptionsMenu.transform.Find("Balance Changes").gameObject, ToggleBalanceChanges, LocalizationModifier.edFont, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(windfallOptionsMenu.transform.Find("Antialiasing").gameObject, ToggleAntiAliasing, LocalizationModifier.edFont, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(windfallOptionsMenu.transform.Find("Motion Blur").gameObject, ToggleMotionBlur, LocalizationModifier.edFont, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(windfallOptionsMenu.transform.Find("Sync Achievements").gameObject, SyncAchievements, LocalizationModifier.edFont, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(windfallOptionsMenu.transform.Find("Save").gameObject, SaveWindfallOptions, LocalizationModifier.edFont, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(windfallOptionsMenu.transform.Find("Cancel").gameObject, CloseWindfallOptionsMenu, LocalizationModifier.edFont, GamepadMenuOptionSelection.eInjectDots.Both);

                //Change header font
                LocalizationModifier.ChangeFont(windfallOptionsMenu.transform.Find("Header").gameObject.GetComponent<TextMeshProUGUI>(), null, LocalizationModifier.edFont);
            }
            GamepadMenuController gamepadMenuController = windfallOptionsMenu.AddComponent<GamepadMenuController>();

            WindfallHelper.UpdateGamepadMenuButtons(gamepadMenuController, windfallOptionsMenu.transform.Find("Cancel")?.gameObject);
        }

        private static void InitializeButton(GameObject buttonObject, UnityAction unityAction, TMP_FontAsset font, GamepadMenuOptionSelection.eInjectDots eInjectDots)
        {
            ButtonHoverAnimation buttonHoverAnimation = buttonObject.AddComponent<ButtonHoverAnimation>();
            buttonHoverAnimation.hoverSoundFx = SoundsView.eSound.Menu_ItemHover;
            buttonHoverAnimation.clickSoundFx = SoundsView.eSound.Menu_ItemSelect;

            Button buttonComponent = buttonObject.GetComponent<Button>();
            if (buttonComponent != null && unityAction != null)
            {
                buttonComponent.onClick.AddListener(unityAction);
            }

            GamepadMenuOptionSelection gamepadMenuOptionSelection = buttonObject.AddComponent<GamepadMenuOptionSelection>();
            gamepadMenuOptionSelection.m_InjectDots = eInjectDots;
            gamepadMenuOptionSelection.m_SelectionObjects = new GameObject[0];

            TextMeshProUGUI textMeshProUGUI = buttonObject.GetComponent<TextMeshProUGUI>();
            LocalizationModifier.ChangeFont(textMeshProUGUI, null, font);
        }

        private static void ToggleBalanceChanges()
        {
            balanceChanges = !balanceChanges;
            UpdateToggles();
        }
        private static void ToggleAntiAliasing()
        {
            antiAliasing = !antiAliasing;
            UpdateToggles();
        }
        private static void ToggleMotionBlur()
        {
            motionBlur = !motionBlur;
            UpdateToggles();
        }
        private static void UpdateToggles()
        {
            UpdateToggle(windfallOptionsMenu?.transform.Find("Balance Changes")?.Find("Toggle")?.gameObject, balanceChanges);
            UpdateToggle(windfallOptionsMenu?.transform.Find("Antialiasing")?.Find("Toggle")?.gameObject, antiAliasing);
            UpdateToggle(windfallOptionsMenu?.transform.Find("Motion Blur")?.Find("Toggle")?.gameObject, motionBlur);
        }
        private static void UpdateToggle(GameObject toggleObject, bool active)
        {
            if (toggleObject != null)
            {
                Image toggleImage = toggleObject.GetComponent<Image>();
                if (toggleImage != null) toggleImage.sprite = active ? toggleActive : toggleInactive;
            }
        }

        private static void SyncAchievements()
        {
            Progression progression = ProgressionController.LoadProgression();

            for (int unlockCounter = 0; unlockCounter < progression.unlocks.Length; unlockCounter++)
            {
                if (progression.unlocks[unlockCounter])
                {
                    Achievements.Instance.Unlock((Achievements.eAchievement)unlockCounter);
                }
            }
        }

        public static void OpenWindfallOptionsMenu()
        {
            if (menuViewReference == null)
            {
                return;
            }

            menuViewReference.transform.Find("Options Menu PC")?.gameObject.SetActive(false);
            windfallOptionsMenu.SetActive(true);

            LoadWindfallOptions();
        }
        public static void CloseWindfallOptionsMenu()
        {
            if (menuViewReference == null)
            {
                return;
            }

            windfallOptionsMenu.SetActive(false);
            menuViewReference.transform.Find("Options Menu PC")?.gameObject.SetActive(true);
        }

        static void SaveWindfallOptions()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            windfallPersistentData.implementBalanceChanges = balanceChanges;
            windfallPersistentData.antiAliasing = antiAliasing;
            windfallPersistentData.motionBlur = motionBlur;
            WindfallPersistentDataController.SaveData(windfallPersistentData);

            GraphicsModifier.UpdateCameras();

            CloseWindfallOptionsMenu();
        }

        private static void LoadWindfallOptions()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            balanceChanges = windfallPersistentData.implementBalanceChanges;
            antiAliasing = windfallPersistentData.antiAliasing;
            motionBlur = windfallPersistentData.motionBlur;

            UpdateToggles();
        }
    }
}
