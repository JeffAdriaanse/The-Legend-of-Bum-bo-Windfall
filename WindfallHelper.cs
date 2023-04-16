using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Linq;
using PathologicalGames;
using System;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class WindfallHelper
    {
        private static BumboApplication bumboApplication;
        public static BumboApplication app
        {
            get
            {
                if (bumboApplication == null)
                {
                    bumboApplication = GameObject.FindObjectOfType<BumboApplication>();
                }

                return bumboApplication;
            }
            set
            {
                bumboApplication = value;
            }
        }
        public static void GetApp(BumboApplication _app)
        {
            if (_app != null)
            {
                app = _app;
            }
        }

        private static GamepadSpellSelector gamepadSpellSelector;
        public static GamepadSpellSelector GamepadSpellSelector
        {
            get
            {
                if (gamepadSpellSelector == null)
                {
                    gamepadSpellSelector = WindfallHelper.app?.view?.GUICamera?.GetComponent<GamepadSpellSelector>();
                }

                if (gamepadSpellSelector == null)
                {
                    gamepadSpellSelector = GameObject.FindObjectOfType<GamepadSpellSelector>();
                }

                return gamepadSpellSelector;
            }
        }

        private static GamepadTreasureRoomController gamepadTreasureRoomController;
        public static GamepadTreasureRoomController GamepadTreasureRoomController
        {
            get
            {
                if (gamepadTreasureRoomController == null)
                {
                    gamepadTreasureRoomController = PoolManager.Pools["Spells"]?.GetComponent<GamepadTreasureRoomController>();
                }

                if (gamepadTreasureRoomController == null)
                {
                    gamepadTreasureRoomController = GameObject.FindObjectOfType<GamepadTreasureRoomController>();
                }

                return gamepadTreasureRoomController;
            }
        }

        private static GamepadTreasureRoomController gamepadBossRoomController;
        public static GamepadTreasureRoomController GamepadBossRoomController
        {
            get
            {
                if (gamepadBossRoomController == null)
                {
                    GameObject[] bossRewardParents = WindfallHelper.app?.view?.bossRewardParents;
                    if (bossRewardParents != null && bossRewardParents[0] != null)
                    {
                        gamepadBossRoomController = bossRewardParents[0]?.transform.parent?.GetComponent<GamepadTreasureRoomController>();
                    }
                }

                if (gamepadBossRoomController == null)
                {
                    gamepadBossRoomController = GameObject.FindObjectOfType<GamepadTreasureRoomController>();
                }

                return gamepadBossRoomController;
            }
        }

        private static GamepadGamblingController gamepadGamblingController;
        public static GamepadGamblingController GamepadGamblingController
        {
            get
            {
                if (gamepadGamblingController == null)
                {
                    gamepadGamblingController = GameObject.FindObjectOfType<GamepadGamblingController>();
                }

                return gamepadGamblingController;
            }
        }

        private readonly static string edmundmcmillen_font_path = "Edmundmcmillen-regular SDF";
        private static TMP_FontAsset edmundmcmillen_regular;
        public static TMP_FontAsset GetEdmundMcmillenFont()
        {
            if (edmundmcmillen_regular == null && Windfall.assetBundle != null && Windfall.assetBundle.Contains(edmundmcmillen_font_path))
            {
                edmundmcmillen_regular = Windfall.assetBundle.LoadAsset<TMP_FontAsset>("Edmundmcmillen-regular SDF");
            }

            return edmundmcmillen_regular;
        }

        public static int ChaptersUnlocked(Progression progression)
        {
            int numberOfChapters;
            if (!progression.unlocks[0]) numberOfChapters = 1;
            else if (!progression.unlocks[1]) numberOfChapters = 2;
            else if (!progression.unlocks[2]) numberOfChapters = 3;
            else numberOfChapters = 4;
            return numberOfChapters;
        }

        //Method goes two children deep when searching for buttons
        public static void UpdateGamepadMenuButtons(GamepadMenuController gamepadMenuController, GameObject cancelButton)
        {
            if (gamepadMenuController == null)
            {
                return;
            }

            List<GameObject> newOptions = new List<GameObject>();

            //Search for children with GamepadMenuOptionSelection
            for (int childCounter = 0; childCounter < gamepadMenuController.transform.childCount; childCounter++)
            {
                Transform childTransform = gamepadMenuController.transform.GetChild(childCounter);

                if (childTransform.gameObject.activeSelf && childTransform.GetComponent<GamepadMenuOptionSelection>() != null)
                {
                    newOptions.Add(childTransform.gameObject);
                }

                //Search for sub-children with GamepadMenuOptionSelection
                for (int subChildCounter = 0; subChildCounter < childTransform.childCount; subChildCounter++)
                {
                    Transform subChildTransform = childTransform.GetChild(subChildCounter);

                    if (subChildTransform.gameObject.activeSelf && subChildTransform.GetComponent<GamepadMenuOptionSelection>() != null)
                    {
                        newOptions.Add(subChildTransform.gameObject);
                    }
                }
            }

            if (newOptions.Count > 0)
            {
                gamepadMenuController.m_Buttons = newOptions.ToArray();
            }

            //Add cancel button
            if (cancelButton != null)
            {
                gamepadMenuController.m_CancelButton = cancelButton;
            }
        }

        static Shader defaultShader;
        public static Transform ResetShader(Transform transform)
        {
            if (transform == null)
            {
                return null;
            }

            foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>())
            {
                if (meshRenderer != null && !meshRenderer.GetComponent<TextMeshPro>())
                {
                    if (defaultShader == null)
                    {
                        defaultShader = Shader.Find("Standard");
                    }

                    if (meshRenderer?.material?.shader != null && defaultShader != null)
                    {
                        meshRenderer.material.shader = defaultShader;
                    }

                    meshRenderer.material.shaderKeywords = new string[] { "_GLOSSYREFLECTIONS_OFF", "_SPECULARHIGHLIGHTS_OFF" };
                }
            }

            return transform;
        }
    }

    //Disables unwanted notifiactions immediately after they are created
    public static class NotificationRemoval
    {
        public enum NotificationType
        {
            MANA_DRAIN,
            MANA_GAIN,
            DAMAGE_UP,
            LOSE_MOVE,
            DOOMED,
        }

        static Dictionary<NotificationType, string> NotificationNames = new Dictionary<NotificationType, string>()
        {
            { NotificationType.MANA_DRAIN, "Mana_Drain" },
            { NotificationType.MANA_GAIN, "Mana Gain" },
            { NotificationType.DAMAGE_UP, "damage up" },
            { NotificationType.LOSE_MOVE, "Lose_Move" },
            { NotificationType.DOOMED, "Doomed" },
        };
        public static void RemoveNewestNotification(GUISide guiSide, NotificationType notificationType)
        {
            GameObject notificationToDisable = null;
            int largestNotificationIndex = -1;
            for (int childCounter = 0; childCounter < guiSide.transform.childCount; childCounter++)
            {
                //Loop through all children of GUISide object
                Transform child = guiSide.transform.GetChild(childCounter);

                if (child.gameObject.activeSelf && childCounter > largestNotificationIndex && NotificationNames.TryGetValue(notificationType, out string name))
                {
                    if (child.name.Contains(name))
                    {
                        //Locate the lowest active notification of the given type
                        notificationToDisable = child.gameObject;
                        largestNotificationIndex = childCounter;
                    }
                }
            }
            //Disable notification
            if (notificationToDisable != null)
            {
                notificationToDisable.SetActive(false);
            }
        }
    }
}