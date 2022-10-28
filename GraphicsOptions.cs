using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class GraphicsOptions
    {
        private static GameObject graphicsMenu;

        private static Canvas mouseCanvas;

        private static GameObject resolutionDropdown;
        private static GameObject refreshRateDropdown;

        private static List<Vector2> resolutions;
        private static List<int> refreshRates;

        private static bool antiAliasing;
        private static bool depthOfField;
        private static bool motionBlur;
        private static bool fullScreen;

        private static Sprite toggleActive;
        private static Sprite toggleInactive;

        public static void SetUpGraphicsOptions(GameObject menuView, bool pauseMenu)
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

            ////Reorganize options menu
            //VerticalLayoutGroup optionsVerticalLayout = titleController.optionsMenu.GetComponent<VerticalLayoutGroup>();
            //optionsVerticalLayout.enabled = true;
            //optionsVerticalLayout.spacing = 0;

            //RectTransform optionsHeaderRect = titleController.optionsMenu.transform.Find("Image").GetComponent<RectTransform>();
            //optionsHeaderRect.sizeDelta = new Vector2(optionsHeaderRect.sizeDelta.x, 60);

            ////Load graphics options
            //Button optionsMenuButton = titleController.mainMenu.transform.Find("Options").GetComponent<Button>();
            //optionsMenuButton.onClick.AddListener(LoadGraphicsOptions);

            ////Save/cancel layout
            //Transform saveCancel = UnityEngine.Object.Instantiate(titleController.optionsMenu.transform.Find("Image (1)").gameObject, titleController.optionsMenu.transform).transform;
            //saveCancel.SetAsLastSibling();
            //foreach (Transform child in saveCancel)
            //{
            //    UnityEngine.Object.Destroy(child.gameObject);
            //}
            //saveCancel.gameObject.AddComponent<HorizontalLayoutGroup>();
            //saveCancel.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            //saveCancel.localScale = Vector3.one;
            //RectTransform saveCancelRect = saveCancel.GetComponent<RectTransform>();
            //saveCancelRect.sizeDelta = new Vector2(250, saveCancelRect.sizeDelta.y);
            ////Save
            //Transform saveTransform = titleController.optionsMenu.transform.Find("Save");
            //saveTransform.SetParent(saveCancel);
            //saveTransform.localScale = Vector3.one;
            //saveTransform.GetComponent<Button>().onClick.AddListener(SaveGraphicsOptions);
            ////Cancel
            //Transform cancelTransform = titleController.optionsMenu.transform.Find("Cancel");
            //cancelTransform.SetParent(saveCancel);
            //cancelTransform.localScale = Vector3.one;

            //Save graphics options
            Transform saveTransform = menuView.transform.Find("Graphics Menu").transform.Find("Save");

            saveTransform.GetComponent<Button>()?.onClick.AddListener(SaveGraphicsOptions);

            //Load graphics options
            Button optionsMenuButton;
            if (!pauseMenu)
            {
                optionsMenuButton = menuView?.transform.Find("Options Menu PC")?.Find("Graphics Options")?.GetComponent<Button>();
            }
            else
            {
                optionsMenuButton = menuView?.transform.Find("Options Menu PC")?.Find("Graphics Options")?.GetComponent<Button>();
            }

            if (optionsMenuButton != null)
            {
                optionsMenuButton.onClick.AddListener(LoadGraphicsOptions);
            }
        }

        public static void CreateGraphicsMenu(GameObject menuView)
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            //Create graphics menu
            graphicsMenu = UnityEngine.Object.Instantiate(assets.LoadAsset<GameObject>("Graphics Menu"), menuView.transform.Find("Graphics Menu"));
            graphicsMenu.transform.SetSiblingIndex(1);
            RectTransform graphicsMenuRect = graphicsMenu.GetComponent<RectTransform>();
            graphicsMenuRect.anchoredPosition = new Vector2(240, -30);
            graphicsMenuRect.localRotation = Quaternion.Euler(graphicsMenuRect.localRotation.eulerAngles.x, graphicsMenuRect.localRotation.eulerAngles.y, 351);

            //Initialize buttons
            InitializeButton(graphicsMenu.transform.Find("Antialiasing").gameObject, ToggleAntiAliasing);
            InitializeButton(graphicsMenu.transform.Find("Depth of Field").gameObject, ToggleDepthOfField);
            InitializeButton(graphicsMenu.transform.Find("Motion Blur").gameObject, ToggleMotionBlur);
            InitializeButton(graphicsMenu.transform.Find("Full Screen").gameObject, ToggleFullScreen);

            //Initialize resolution dropdown
            resolutions = GetScreenResolutions();

            resolutionDropdown = graphicsMenu.transform.Find("Resolution Holder").Find("Resolution Dropdown Holder").Find("Resolution Dropdown").gameObject;

            List<Dropdown.OptionData> resolutionOptions = new List<Dropdown.OptionData>();
            for (int resolutionCounter = 0; resolutionCounter < resolutions.Count; resolutionCounter++)
            {
                resolutionOptions.Add(new Dropdown.OptionData(resolutions[resolutionCounter].x.ToString() + "x" + resolutions[resolutionCounter].y.ToString()));
            }

            if (resolutionDropdown != null)
            {
                InitializeDropdown(resolutionDropdown, resolutionOptions);
                resolutionDropdown.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { UpdateRefreshRateDropdown(false); });
            }

            //Initialize refresh rate dropdown
            refreshRates = GetRefreshRates();

            refreshRateDropdown = graphicsMenu.transform.Find("Resolution Holder").Find("Resolution Dropdown Holder").Find("Refresh Rate Dropdown").gameObject;

            UpdateRefreshRateDropdown(true);

            //Move mouse
            MoveMouseToNewCanvas(menuView);
        }

        private static void ToggleAntiAliasing()
        {
            antiAliasing = !antiAliasing;
            UpdateGraphicsToggles();
        }
        private static void ToggleDepthOfField()
        {
            depthOfField = !depthOfField;
            UpdateGraphicsToggles();
        }
        private static void ToggleMotionBlur()
        {
            motionBlur = !motionBlur;
            UpdateGraphicsToggles();
        }
        private static void ToggleFullScreen()
        {
            fullScreen = !fullScreen;
            UpdateGraphicsToggles();
        }

        private static void UpdateGraphicsToggles()
        {
            UpdateGraphicsToggle(graphicsMenu?.transform.Find("Antialiasing")?.Find("Toggle")?.gameObject, antiAliasing);
            UpdateGraphicsToggle(graphicsMenu?.transform.Find("Depth of Field")?.Find("Toggle")?.gameObject, depthOfField);
            UpdateGraphicsToggle(graphicsMenu?.transform.Find("Motion Blur")?.Find("Toggle")?.gameObject, motionBlur);
            UpdateGraphicsToggle(graphicsMenu?.transform.Find("Full Screen")?.Find("Toggle")?.gameObject, fullScreen);
        }

        private static void UpdateGraphicsToggle(GameObject toggleObject, bool active)
        {
            if (toggleObject != null)
            {
                Image toggleImage = toggleObject.GetComponent<Image>();
                if (toggleImage != null) toggleImage.sprite = active ? toggleActive : toggleInactive;
            }
        }

        private static void SaveGraphicsOptions()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            windfallPersistentData.antiAliasing = antiAliasing;
            windfallPersistentData.depthOfField = depthOfField;
            windfallPersistentData.motionBlur = motionBlur;
            WindfallPersistentDataController.SaveData(windfallPersistentData);

            ChangeResolution();
            UpdateCameras();
        }

        private static List<Camera> cameras;
        public static void TrackCamera(Camera camera)
        {
            if (cameras == null)
            {
                cameras = new List<Camera>();
            }

            if (camera != null)
            {
                cameras.Add(camera);
            }

            cameras.RemoveAll(delegate (Camera cameraComponent) { return cameraComponent == null; });
        }

        private static void UpdateCameras()
        {
            if (cameras != null)
            {
                foreach (Camera cameraComponent in cameras)
                {
                    if (cameraComponent != null)
                    {
                        GraphicsModifier.ApplyGraphicsToCamera(cameraComponent, false);
                    }
                }
            }
        }

        private static void LoadGraphicsOptions()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            antiAliasing = windfallPersistentData.antiAliasing;
            depthOfField = windfallPersistentData.depthOfField;
            motionBlur = windfallPersistentData.motionBlur;

            fullScreen = Screen.fullScreen;

            if (resolutionDropdown != null)
            {
                int index = -1;
                for (int resolutionCounter = 0; resolutionCounter < resolutions.Count; resolutionCounter++)
                {
                    Vector2 resolution = resolutions[resolutionCounter];
                    if (resolution.x == Screen.width && resolution.y == Screen.height)
                    {
                        index = resolutionCounter;
                    }
                }
                if (index != -1)
                {
                    resolutionDropdown.GetComponent<Dropdown>().value = index;
                }
            }

            if (refreshRateDropdown != null)
            {
                Dropdown refreshRateDropdownComponent = refreshRateDropdown.GetComponent<Dropdown>();
                refreshRateDropdownComponent.value = refreshRateDropdownComponent.options.Count - 1;
            }

            UpdateGraphicsToggles();
        }

        private static void InitializeButton(GameObject buttonObject, UnityAction unityAction)
        {
            ButtonHoverAnimation buttonHoverAnimation = buttonObject.AddComponent<ButtonHoverAnimation>();
            buttonHoverAnimation.hoverSoundFx = SoundsView.eSound.Menu_ItemHover;
            buttonHoverAnimation.clickSoundFx = SoundsView.eSound.Menu_ItemSelect;

            Button buttonComponent = buttonObject.GetComponent<Button>();
            buttonComponent.onClick.AddListener(unityAction);
        }

        private static void InitializeDropdown(GameObject dropdown, List<Dropdown.OptionData> options)
        {
            ButtonHoverAnimation buttonHoverAnimation = dropdown.AddComponent<ButtonHoverAnimation>();
            buttonHoverAnimation.hoverSoundFx = SoundsView.eSound.Menu_ItemHover;
            buttonHoverAnimation.clickSoundFx = SoundsView.eSound.Menu_ItemSelect;

            Text[] dropdownItems = dropdown.transform.Find("Template").GetComponentsInChildren<Text>();
            foreach (Text item in dropdownItems)
            {
                ButtonHoverAnimation itemButtonHoverAnimation = item.transform.parent.gameObject.AddComponent<ButtonHoverAnimation>();
                itemButtonHoverAnimation.hoverSoundFx = SoundsView.eSound.Menu_ItemHover;
                itemButtonHoverAnimation.clickSoundFx = SoundsView.eSound.Menu_ItemSelect;
            }

            UpdateDropdown(dropdown, options);
        }

        private static void UpdateDropdown(GameObject dropdown, List<Dropdown.OptionData> options)
        {
            Dropdown dropdownComponent = dropdown.GetComponent<Dropdown>();
            if (dropdownComponent != null)
            {
                dropdownComponent.options = options;
            }
        }

        private static void ChangeResolution()
        {
            if (resolutionDropdown != null && refreshRateDropdown != null)
            {
                Vector2 resolution = resolutions[resolutionDropdown.GetComponent<Dropdown>().value];
                int refreshRate = refreshRates[refreshRateDropdown.GetComponent<Dropdown>().value];
                Screen.SetResolution((int)resolution.x, (int)resolution.y, fullScreen, refreshRate);
            }
        }

        private static List<Vector2> GetScreenResolutions()
        {
            List<Vector2> screenResolutions = new List<Vector2>();
            for (int resolutionCounter = 0; resolutionCounter < Screen.resolutions.Length; resolutionCounter++)
            {
                Resolution resolution = Screen.resolutions[resolutionCounter];
                Vector2 resolutionDimensions = new Vector2(resolution.width, resolution.height);
                if (!screenResolutions.Contains(resolutionDimensions))
                {
                    screenResolutions.Add(resolutionDimensions);
                }
            }
            return screenResolutions;
        }

        private static List<int> GetRefreshRates()
        {
            List<int> refreshRates = new List<int>();
            for (int resolutionCounter = 0; resolutionCounter < Screen.resolutions.Length; resolutionCounter++)
            {
                Resolution resolution = Screen.resolutions[resolutionCounter];
                Vector2 resolutionDimensions = new Vector2(resolution.width, resolution.height);
                if (resolutionDropdown != null && resolutions[resolutionDropdown.GetComponent<Dropdown>().value] == resolutionDimensions)
                {
                    refreshRates.Add(resolution.refreshRate);
                }
            }
            return refreshRates;
        }

        private static void UpdateRefreshRateDropdown(bool initialize)
        {
            refreshRates = GetRefreshRates();

            List<Dropdown.OptionData> refreshRateOptions = new List<Dropdown.OptionData>();
            for (int refreshRateCounter = 0; refreshRateCounter < refreshRates.Count; refreshRateCounter++)
            {
                refreshRateOptions.Add(new Dropdown.OptionData(refreshRates[refreshRateCounter].ToString() + "hz"));
            }

            if (refreshRateDropdown != null)
            {
                if (initialize)
                {
                    InitializeDropdown(refreshRateDropdown, refreshRateOptions);
                }
                else
                {
                    UpdateDropdown(refreshRateDropdown, refreshRateOptions);
                    if (refreshRateDropdown != null)
                    {
                        Dropdown refreshRateDropdownComponent = refreshRateDropdown.GetComponent<Dropdown>();
                        refreshRateDropdownComponent.value = refreshRateDropdownComponent.options.Count - 1;
                    }
                }
            }
        }

        private static void MoveMouseToNewCanvas(GameObject menuView)
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            GameObject mouse = menuView.transform.Find("Mouse").gameObject;
            GameObject canvasObject = UnityEngine.Object.Instantiate(assets.LoadAsset<GameObject>("Mouse Canvas"), menuView.transform);
            canvasObject.GetComponent<RectTransform>().SetAsLastSibling();

            mouseCanvas = canvasObject.GetComponent<Canvas>();

            mouse.transform.SetParent(mouseCanvas.transform);
            mouse.transform.localScale = new Vector3(1.9f, 2, 1);
        }
    }

    static class GraphicsModifier
    {
        public static void ApplyGraphicsToCamera(Camera camera, bool trackCamera = true)
        {
            if (camera == null)
            {
                return;
            }

            //Load graphics settings
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();

            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            //Only apply depth of field if the camera is the main game camera
            bool mainGameCamera = camera.GetComponent<CameraView>() != null && camera.GetComponent<GUISide>() == null;


            //TEST
            //Unity depth of field effect
            UnityStandardAssets.ImageEffects.DepthOfField unityDepthOfFieldEffect = camera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();

            if (mainGameCamera)
            {
                if (unityDepthOfFieldEffect == null)
                {
                    UnityStandardAssets.ImageEffects.DepthOfField newUnitDepthOfFieldEffect = camera.gameObject.AddComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
                }
            }


            //Depth of field effect
            DepthOfFieldEffect depthOfFieldEffect = camera.gameObject.GetComponent<DepthOfFieldEffect>();

            if (windfallPersistentData.depthOfField && mainGameCamera)
            {
                if (depthOfFieldEffect == null)
                {
                    DepthOfFieldEffect newDepthOfFieldEffect = camera.gameObject.AddComponent<DepthOfFieldEffect>();
                    newDepthOfFieldEffect.dofShader = assets.LoadAsset<Shader>("DepthOfFieldShader");
                }
                else
                {
                    depthOfFieldEffect.enabled = true;
                }
            }
            else if (depthOfFieldEffect != null)
            {
                depthOfFieldEffect.enabled = false;
            }

            //Antialiasing effect
            FXAAEffect fxaaEffect = camera.gameObject.GetComponent<FXAAEffect>();

            if (windfallPersistentData.antiAliasing)
            {
                if (fxaaEffect == null)
                {
                    FXAAEffect newFxaaEffect = camera.gameObject.AddComponent<FXAAEffect>();
                    newFxaaEffect.fxaaShader = assets.LoadAsset<Shader>("FXAA");
                    newFxaaEffect.luminanceSource = FXAAEffect.LuminanceMode.Calculate;
                }
                else
                {
                    fxaaEffect.enabled = true;
                }
            }
            else if (fxaaEffect != null)
            {
                fxaaEffect.enabled = false;
            }

            //Motion blur effect
            AmplifyMotionEffect amplifyMotionEffect = camera.gameObject.GetComponent<AmplifyMotionEffect>();

            if (windfallPersistentData.motionBlur)
            {
                if (amplifyMotionEffect != null)
                {
                    amplifyMotionEffect.enabled = true;
                }
            }
            else if (amplifyMotionEffect != null)
            {
                amplifyMotionEffect.enabled = false;
            }

            if (trackCamera) GraphicsOptions.TrackCamera(camera);
        }
    }
}