using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace The_Legend_of_Bum_bo_Windfall
{
    class WindfallOptionsMenu : MonoBehaviour
    {
        private GameObject menuViewReference;

        private bool balanceChanges;
        private bool antiAliasing;
        private bool motionBlur;

        private int tooltipSize = 0;
        private int gameSpeed = 0;

        private GameObject antialiasingContainer;
        private GameObject antialiasingContainerSettingText;
        private GameObject motionBlurContainer;
        private GameObject motionBlurContainerSettingText;
        private GameObject tooltipsContainer;
        private GameObject tooltipsContainerSettingText;
        private GameObject gameSpeedContainer;
        private GameObject gameSpeedContainerSettingText;

        public void SetUpWindfallOptionsMenu(bool pauseMenu)
        {
            GameObject menuView = transform.parent.gameObject;
            AssetBundle assets = Windfall.assetBundle;

            ReorganizeVanillaOptionsMenu(menuView);

            //Set up Windfall menu
            gameObject.SetActive(false);
            transform.SetSiblingIndex(Mathf.Max(0, transform.parent.childCount - 2));

            //Set up hotkeys menu
            GameObject hotkeysMenu = UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>("Hotkeys Menu"), menuView.transform);
            HotkeysMenu hotkeysMenuComponent = hotkeysMenu.AddComponent<HotkeysMenu>();
            hotkeysMenuComponent.SetUpHotkeysMenu(menuView);

            //Set up Windfall credits menu
            GameObject windfallCreditsMenu = UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>("Credits Menu"), menuView.transform);
            WindfallCreditsMenu windfallCreditsMenuComponent = windfallCreditsMenu.AddComponent<WindfallCreditsMenu>();
            windfallCreditsMenuComponent.SetUpWindfallCreditsMenu(menuView);

            //Get font asset
            TMP_FontAsset edmundmcmillen_regular = WindfallHelper.GetEdmundMcmillenFont();

            RectTransform windfallMenuRect;
            windfallMenuRect = GetComponent<RectTransform>();
            //windfallMenuRect.anchoredPosition = new Vector2(-3.6, 55.4);
            //windfallMenuRect.localRotation = Quaternion.Euler(graphicsMenuRect.localRotation.eulerAngles.x, graphicsMenuRect.localRotation.eulerAngles.y, 351);

            GameObject header = transform.Find("Header").gameObject;
            GameObject antialiasing = transform.Find("Antialiasing").gameObject;
            antialiasingContainer = antialiasing.transform.Find("Antialiasing Container").gameObject;
            antialiasingContainerSettingText = antialiasingContainer.transform.Find("Setting Text").gameObject;
            GameObject motionBlur = transform.Find("Motion Blur").gameObject;
            motionBlurContainer = motionBlur.transform.Find("Motion Blur Container").gameObject;
            motionBlurContainerSettingText = motionBlurContainer.transform.Find("Setting Text").gameObject;
            GameObject tooltips = transform.Find("Tooltips").gameObject;
            tooltipsContainer = tooltips.transform.Find("Tooltips Container").gameObject;
            tooltipsContainerSettingText = tooltipsContainer.transform.Find("Setting Text").gameObject;
            GameObject gameSpeed = transform.Find("Game Speed").gameObject;
            gameSpeedContainer = gameSpeed.transform.Find("Game Speed Container").gameObject;
            gameSpeedContainerSettingText = gameSpeedContainer.transform.Find("Setting Text").gameObject;
            GameObject hotkeys = transform.Find("Hotkeys").gameObject;
            GameObject windfallCredits = transform.Find("Windfall Credits").gameObject;
            GameObject syncAchievements = transform.Find("Sync Achievements").gameObject;
            GameObject save = transform.Find("Save").gameObject;
            GameObject cancel = transform.Find("Cancel").gameObject;


            //Initialize buttons
            WindfallHelper.InitializeValueList(antialiasingContainer, antialiasingContainerSettingText, CycleAntiAliasing, edmundmcmillen_regular);
            WindfallHelper.InitializeValueList(motionBlurContainer, motionBlurContainerSettingText, CycleMotionBlur, edmundmcmillen_regular);
            WindfallHelper.InitializeValueList(tooltipsContainer, tooltipsContainerSettingText, CycleTooltipSize, edmundmcmillen_regular);
            WindfallHelper.InitializeValueList(gameSpeedContainer, gameSpeedContainerSettingText, CycleGameSpeed, edmundmcmillen_regular);
            WindfallHelper.InitializeButton(hotkeys, hotkeysMenuComponent.OpenHotkeysMenu, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(windfallCredits, windfallCreditsMenuComponent.OpenWindfallCreditsMenu, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(syncAchievements, SyncAchievements, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(save, SaveWindfallOptions, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(cancel, CloseWindfallOptionsMenu, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);

            //Localize text
            WindfallHelper.LocalizeObject(header, "Menu/WINDFALL_OPTIONS");
            WindfallHelper.LocalizeObject(antialiasing, "Menu/ANTI_ALIASING");
            WindfallHelper.LocalizeObject(antialiasingContainerSettingText, null);
            WindfallHelper.LocalizeObject(motionBlur, "Menu/MOTION_BLUR");
            WindfallHelper.LocalizeObject(motionBlurContainerSettingText, null);
            WindfallHelper.LocalizeObject(tooltips, "Menu/TOOLTIPS");
            WindfallHelper.LocalizeObject(tooltipsContainerSettingText, null);
            WindfallHelper.LocalizeObject(gameSpeed, "Menu/GAME_SPEED");
            WindfallHelper.LocalizeObject(gameSpeedContainerSettingText, null);
            WindfallHelper.LocalizeObject(hotkeys, "Menu/HOTKEYS");
            WindfallHelper.LocalizeObject(windfallCredits, "Menu/WINDFALL_CREDITS");
            WindfallHelper.LocalizeObject(syncAchievements, "Menu/SYNC_ACHIEVEMENTS");
            WindfallHelper.LocalizeObject(save, "Menu/OPTIONS_SAVE");
            WindfallHelper.LocalizeObject(cancel, "Menu/OPTIONS_CANCEL");

            //Keyboard/gamepad control functionality
            GamepadMenuController gamepadMenuController = gameObject.AddComponent<GamepadMenuController>();
            WindfallHelper.UpdateGamepadMenuButtons(gamepadMenuController, transform.Find("Cancel")?.gameObject, 2);
        }

        public void ReorganizeVanillaOptionsMenu(GameObject menuView)
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
                    if (optionsMenuPcTransform.childCount - childCounter < 3) yOffset = -10; //Move save/cancel down instead of up
                    else yOffset = 20;
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
                WindfallHelper.UpdateGamepadMenuButtons(optionsMenuPcGamepadMenuController, null, 2);
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
        private void CycleAntiAliasing(int cycle)
        {
            antiAliasing = !antiAliasing;
            UpdateButtons();
        }
        private void CycleMotionBlur(int cycle)
        {
            motionBlur = !motionBlur;
            UpdateButtons();
        }
        private void CycleTooltipSize(int cycle)
        {
            //-2: Disabled
            //-1: Small
            //0: Medium
            //1: Large
            tooltipSize += cycle;
            if (tooltipSize > 1) tooltipSize = -2;
            if (tooltipSize < -2) tooltipSize = 1;
            UpdateButtons();
        }
        private void CycleGameSpeed(int cycle)
        {
            //0: 1.0x
            //1: 1.5x
            //2: 2.0x
            //3: 2.5x
            //4: 3.0x
            gameSpeed += cycle;
            if (gameSpeed > 4) gameSpeed = 0;
            if (gameSpeed < 0) gameSpeed = 4;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            Localize antialiasingLocalize = antialiasingContainerSettingText.GetComponent<Localize>();
            if (antialiasingLocalize != null)
            {
                string term = string.Empty;
                switch (antiAliasing)
                {
                    case true:
                        term = "OPTIONS_ON";
                        break;
                    default:
                        term = "OPTIONS_OFF";
                        break;
                }
                if (term != string.Empty) Localization.SetKey(antialiasingLocalize, eI2Category.Menu, term);
            }

            Localize motionBlurLocalize = motionBlurContainerSettingText.GetComponent<Localize>();
            if (motionBlurLocalize != null)
            {
                string term = string.Empty;
                switch (motionBlur)
                {
                    case true:
                        term = "OPTIONS_ON";
                        break;
                    default:
                        term = "OPTIONS_OFF";
                        break;
                }
                if (term != string.Empty) Localization.SetKey(motionBlurLocalize, eI2Category.Menu, term);
            }

            Localize tooltipSizeLocalize = tooltipsContainerSettingText.GetComponent<Localize>();
            if (tooltipSizeLocalize != null)
            {
                string term = string.Empty;
                switch (tooltipSize)
                {
                    case -2:
                        term = "DISABLED";
                        break;
                    case -1:
                        term = "SMALL";
                        break;
                    case 0:
                        term = "MEDIUM";
                        break;
                    case 1:
                        term = "LARGE";
                        break;
                }
                if (term != string.Empty) Localization.SetKey(tooltipSizeLocalize, eI2Category.Menu, term);
            }

            Localize gameSpeedLocalize = gameSpeedContainerSettingText.GetComponent<Localize>();
            if (gameSpeedLocalize != null)
            {
                string term = string.Empty;
                switch (gameSpeed)
                {
                    case 0:
                        term = "1.0X";
                        break;
                    case 1:
                        term = "1.5X";
                        break;
                    case 2:
                        term = "2.0X";
                        break;
                    case 3:
                        term = "2.5X";
                        break;
                    case 4:
                        term = "3.0X";
                        break;
                }
                if (term != string.Empty) Localization.SetKey(gameSpeedLocalize, eI2Category.Menu, term);
            }
        }

        private void SyncAchievements()
        {
            Progression progression = ProgressionController.LoadProgression();

            for (int unlockCounter = 0; unlockCounter < progression.unlocks.Length; unlockCounter++)
            {
                if (progression.unlocks[unlockCounter]) Achievements.Instance.Unlock((Achievements.eAchievement)unlockCounter);
            }
        }

        public void OpenWindfallOptionsMenu()
        {
            if (menuViewReference == null) return;

            menuViewReference.transform.Find("Options Menu PC")?.gameObject.SetActive(false);
            gameObject.SetActive(true);

            LoadWindfallOptions();
        }
        public void CloseWindfallOptionsMenu()
        {
            if (menuViewReference == null) return;

            gameObject.SetActive(false);
            menuViewReference.transform.Find("Options Menu PC")?.gameObject.SetActive(true);
        }

        private void SaveWindfallOptions()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            windfallPersistentData.implementBalanceChanges = balanceChanges;
            windfallPersistentData.antiAliasing = antiAliasing;
            windfallPersistentData.motionBlur = motionBlur;
            windfallPersistentData.tooltipSize = tooltipSize;
            windfallPersistentData.gameSpeed = gameSpeed;
            WindfallPersistentDataController.SaveData(windfallPersistentData);

            GraphicsModifier.UpdateCameras();
            WindfallHelper.GameSpeedController?.UpdateGameSpeed();

            CloseWindfallOptionsMenu();
        }

        private void LoadWindfallOptions()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            balanceChanges = windfallPersistentData.implementBalanceChanges;
            antiAliasing = windfallPersistentData.antiAliasing;
            motionBlur = windfallPersistentData.motionBlur;
            tooltipSize = windfallPersistentData.tooltipSize;
            gameSpeed = windfallPersistentData.gameSpeed;

            UpdateButtons();
        }
    }
}

public class WindfallValueListView : BumboValueListView
{
    private Action<int> cycleAction;
    public void SetAction(Action<int> cycleAction)
    {
        this.cycleAction = cycleAction;
    }

    public override void CycleLeft()
    {
        cycleAction(-1);
    }

    public override void CycleRight()
    {
        cycleAction(1);
    }
}