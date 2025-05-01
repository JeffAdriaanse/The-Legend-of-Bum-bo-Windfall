using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using The_Legend_of_Bum_bo_Windfall.scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class WindfallOptionsMenu
    {
        private static GameObject menuViewReference;
        private static GameObject windfallOptionsMenu;

        private static bool balanceChanges;
        private static bool antiAliasing;
        private static bool motionBlur;

        private static int tooltipSize = 1;

        private static Sprite toggleActive;
        private static Sprite toggleInactive;

        public static void SetUpWindfallOptionsMenu(GameObject menuView, bool pauseMenu)
        {
            AssetBundle assets = Windfall.assetBundle;

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

            //Get font asset
            TMP_FontAsset edmundmcmillen_regular = WindfallHelper.GetEdmundMcmillenFont();

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

                GameObject header = windfallOptionsMenu.transform.Find("Header").gameObject;
                GameObject balanceChanges = windfallOptionsMenu.transform.Find("Balance Changes").gameObject;
                GameObject antialiasing = windfallOptionsMenu.transform.Find("Antialiasing").gameObject;
                GameObject motionBlur = windfallOptionsMenu.transform.Find("Motion Blur").gameObject;
                GameObject tooltips = windfallOptionsMenu.transform.Find("Tooltips").gameObject;
                GameObject tooltipsSize = tooltips.transform.Find("Size").gameObject;
                GameObject syncAchievements = windfallOptionsMenu.transform.Find("Sync Achievements").gameObject;
                GameObject save = windfallOptionsMenu.transform.Find("Save").gameObject;
                GameObject cancel = windfallOptionsMenu.transform.Find("Cancel").gameObject;

                //Localize header
                WindfallHelper.LocalizeObject(header, "Menu/WINDFALL_OPTIONS");

                //Localize tooltips
                WindfallHelper.LocalizeObject(tooltips, "Menu/TOOLTIPS");

                //Initialize buttons
                InitializeButton(balanceChanges, ToggleBalanceChanges, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(antialiasing, ToggleAntiAliasing, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(motionBlur, ToggleMotionBlur, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(tooltipsSize, CycleTooltipSize, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(syncAchievements, SyncAchievements, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(save, SaveWindfallOptions, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
                InitializeButton(cancel, CloseWindfallOptionsMenu, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);

                //Localize buttons
                WindfallHelper.LocalizeObject(balanceChanges, "Menu/BALANCE_CHANGES");
                WindfallHelper.LocalizeObject(antialiasing, "Menu/ANTI_ALIASING");
                WindfallHelper.LocalizeObject(motionBlur, "Menu/MOTION_BLUR");
                WindfallHelper.LocalizeObject(tooltipsSize, null);
                WindfallHelper.LocalizeObject(syncAchievements, "Menu/SYNC_ACHIEVEMENTS");
                WindfallHelper.LocalizeObject(save, "Menu/OPTIONS_SAVE");
                WindfallHelper.LocalizeObject(cancel, "Menu/OPTIONS_CANCEL");
            }
            GamepadMenuController gamepadMenuController = windfallOptionsMenu.AddComponent<GamepadMenuController>();

            WindfallHelper.UpdateGamepadMenuButtons(gamepadMenuController, windfallOptionsMenu.transform.Find("Cancel")?.gameObject);

            HotkeysMenu.CreateHotkeysMenu(menuView);
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
            UpdateButtons();
        }
        private static void ToggleAntiAliasing()
        {
            antiAliasing = !antiAliasing;
            UpdateButtons();
        }
        private static void ToggleMotionBlur()
        {
            motionBlur = !motionBlur;
            UpdateButtons();
        }
        private static void CycleTooltipSize()
        {
            tooltipSize++;
            if (tooltipSize > 1)
            {
                tooltipSize = -2;
            }
            UpdateButtons();
        }
        private static void UpdateButtons()
        {
            UpdateToggle(windfallOptionsMenu?.transform.Find("Balance Changes")?.Find("Toggle")?.gameObject, balanceChanges);
            UpdateToggle(windfallOptionsMenu?.transform.Find("Antialiasing")?.Find("Toggle")?.gameObject, antiAliasing);
            UpdateToggle(windfallOptionsMenu?.transform.Find("Motion Blur")?.Find("Toggle")?.gameObject, motionBlur);

            Localize tooltipSizeLocalize = windfallOptionsMenu?.transform.Find("Tooltips")?.Find("Size")?.GetComponent<Localize>();
            if (tooltipSizeLocalize != null)
            {
                switch (tooltipSize)
                {
                    case -2:
                        Localization.SetKey(tooltipSizeLocalize, eI2Category.Menu, "DISABLED");
                        break;
                    case -1:
                        Localization.SetKey(tooltipSizeLocalize, eI2Category.Menu, "SMALL");
                        break;
                    case 0:
                        Localization.SetKey(tooltipSizeLocalize, eI2Category.Menu, "MEDIUM");
                        break;
                    case 1:
                        Localization.SetKey(tooltipSizeLocalize, eI2Category.Menu, "LARGE");
                        break;
                }
            }
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
            windfallPersistentData.tooltipSize = tooltipSize;
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
            tooltipSize = windfallPersistentData.tooltipSize;

            UpdateButtons();
        }
    }
}
