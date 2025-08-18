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
    class WindfallOptionsMenu : MonoBehaviour
    {
        private GameObject menuViewReference;

        private bool balanceChanges;
        private bool antiAliasing;
        private bool motionBlur;

        private int tooltipSize = 1;

        private Sprite toggleActive;
        private Sprite toggleInactive;

        public void SetUpWindfallOptionsMenu(bool pauseMenu)
        {
            GameObject menuView = transform.parent.gameObject;
            AssetBundle assets = Windfall.assetBundle;

            //Get sprite assets
            toggleActive = assets.LoadAsset<Sprite>("UI Toggle Active Thick");
            toggleInactive = assets.LoadAsset<Sprite>("UI Toggle Inactive Thick");

            ReorganizeVanillaOptionsMenu(menuView);

            //Set up Windfall menu
            gameObject.SetActive(false);
            transform.SetSiblingIndex(Mathf.Max(0, transform.parent.childCount - 2));

            //Set up hotkeys menu
            GameObject hotkeysMenu = UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>("Hotkeys Menu"), menuView.transform);
            HotkeysMenu hotkeysMenuComponent = hotkeysMenu.AddComponent<HotkeysMenu>();
            hotkeysMenuComponent.SetUpHotkeysMenu(menuView);

            //Get font asset
            TMP_FontAsset edmundmcmillen_regular = WindfallHelper.GetEdmundMcmillenFont();

            RectTransform windfallMenuRect;
            windfallMenuRect = GetComponent<RectTransform>();
            //windfallMenuRect.anchoredPosition = new Vector2(-3.6, 55.4);
            //windfallMenuRect.localRotation = Quaternion.Euler(graphicsMenuRect.localRotation.eulerAngles.x, graphicsMenuRect.localRotation.eulerAngles.y, 351);

            GameObject header = transform.Find("Header").gameObject;
            GameObject antialiasing = transform.Find("Antialiasing").gameObject;
            GameObject motionBlur = transform.Find("Motion Blur").gameObject;
            GameObject tooltips = transform.Find("Tooltips").gameObject;
            GameObject tooltipsSize = tooltips.transform.Find("Size").gameObject;
            GameObject hotkeys = transform.Find("Hotkeys").gameObject;
            GameObject syncAchievements = transform.Find("Sync Achievements").gameObject;
            GameObject save = transform.Find("Save").gameObject;
            GameObject cancel = transform.Find("Cancel").gameObject;

            //Localize header
            WindfallHelper.LocalizeObject(header, "Menu/WINDFALL_OPTIONS");

            //Localize tooltips
            WindfallHelper.LocalizeObject(tooltips, "Menu/TOOLTIPS");

            //Initialize buttons
            WindfallHelper.InitializeButton(antialiasing, ToggleAntiAliasing, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(motionBlur, ToggleMotionBlur, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(tooltipsSize, CycleTooltipSize, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(hotkeys, hotkeysMenuComponent.OpenHotkeysMenu, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(syncAchievements, SyncAchievements, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(save, SaveWindfallOptions, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(cancel, CloseWindfallOptionsMenu, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);

            //Localize buttons
            WindfallHelper.LocalizeObject(antialiasing, "Menu/ANTI_ALIASING");
            WindfallHelper.LocalizeObject(motionBlur, "Menu/MOTION_BLUR");
            WindfallHelper.LocalizeObject(tooltipsSize, null);
            WindfallHelper.LocalizeObject(hotkeys, "Menu/HOTKEYS");
            WindfallHelper.LocalizeObject(syncAchievements, "Menu/SYNC_ACHIEVEMENTS");
            WindfallHelper.LocalizeObject(save, "Menu/OPTIONS_SAVE");
            WindfallHelper.LocalizeObject(cancel, "Menu/OPTIONS_CANCEL");

            //Keyboard/gamepad control functionality
            GamepadMenuController gamepadMenuController = gameObject.AddComponent<GamepadMenuController>();
            WindfallHelper.UpdateGamepadMenuButtons(gamepadMenuController, transform.Find("Cancel")?.gameObject);
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

        private void ToggleBalanceChanges()
        {
            balanceChanges = !balanceChanges;
            UpdateButtons();
        }
        private void ToggleAntiAliasing()
        {
            antiAliasing = !antiAliasing;
            UpdateButtons();
        }
        private void ToggleMotionBlur()
        {
            motionBlur = !motionBlur;
            UpdateButtons();
        }
        private void CycleTooltipSize()
        {
            tooltipSize++;
            if (tooltipSize > 1)
            {
                tooltipSize = -2;
            }
            UpdateButtons();
        }
        private void UpdateButtons()
        {
            UpdateToggle(transform.Find("Balance Changes")?.Find("Toggle")?.gameObject, balanceChanges);
            UpdateToggle(transform.Find("Antialiasing")?.Find("Toggle")?.gameObject, antiAliasing);
            UpdateToggle(transform.Find("Motion Blur")?.Find("Toggle")?.gameObject, motionBlur);

            Localize tooltipSizeLocalize = transform.Find("Tooltips")?.Find("Size")?.GetComponent<Localize>();
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
        private void UpdateToggle(GameObject toggleObject, bool active)
        {
            if (toggleObject != null)
            {
                Image toggleImage = toggleObject.GetComponent<Image>();
                if (toggleImage != null) toggleImage.sprite = active ? toggleActive : toggleInactive;
            }
        }

        private void SyncAchievements()
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

        public void OpenWindfallOptionsMenu()
        {
            if (menuViewReference == null)
            {
                return;
            }

            menuViewReference.transform.Find("Options Menu PC")?.gameObject.SetActive(false);
            gameObject.SetActive(true);

            LoadWindfallOptions();
        }
        public void CloseWindfallOptionsMenu()
        {
            if (menuViewReference == null)
            {
                return;
            }

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
            WindfallPersistentDataController.SaveData(windfallPersistentData);

            GraphicsModifier.UpdateCameras();

            CloseWindfallOptionsMenu();
        }

        private void LoadWindfallOptions()
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
