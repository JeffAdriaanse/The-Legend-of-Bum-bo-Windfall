using HarmonyLib;
using PathologicalGames;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

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

        public static Vector3 BattlefieldDefaultEnemyPosition(Position battlefieldPosition)
        {
            return new Vector3(battlefieldPosition.x - 1.25f, 0f, -(battlefieldPosition.y * 0.975f + 0.6f));
        }

        /// <summary>
        /// Estimates enemy position.
        /// </summary>
        public static Vector3 EnemyTransformPosition(Enemy enemy)
        {
            float enemyTypeHeightModifier = enemy.enemyType == Enemy.EnemyType.Flying ? 1f : 0f;
            return enemy.transform.position + new Vector3(0f, 0.33f + enemyTypeHeightModifier, 0f);
        }

        /// <summary>
        /// Searches for a file of the given search pattern in the current mod directory.
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        /// <returns>The file path to the first valid file found, or null if no valid file is found.</returns>
        public static string FindFileInCurrentDirectory(string searchPattern)
        {
            string path = Directory.GetCurrentDirectory();
            string[] filePaths = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);

            if (filePaths.Length > 0) return filePaths[0];
            return null;
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

        //Finds all valid enemy positions on the battlefield adjacent to the provided enemy position
        public static List<BattlefieldPosition> AdjacentBattlefieldPositions(AIModel aiModel, BattlefieldPosition battlefieldPosition, bool includeDiagonal, bool includeHorizontal = true, bool includeVertical = true)
        {
            List<BattlefieldPosition> battlefieldPositions = new List<BattlefieldPosition>();

            if (aiModel == null || battlefieldPosition == null)
            {
                return battlefieldPositions;
            }

            //Search all potential positions around the battlefield position
            for (int xIterator = battlefieldPosition.x - 1; xIterator < battlefieldPosition.x + 2; xIterator++)
            {
                for (int yIterator = battlefieldPosition.y - 1; yIterator < battlefieldPosition.y + 2; yIterator++)
                {
                    //Exclude the provided position
                    if (xIterator == battlefieldPosition.x && yIterator == battlefieldPosition.y)
                    {
                        continue;
                    }

                    //Exclude positions horizontally outside of battlefield 
                    if (xIterator < 0 || xIterator > 2)
                    {
                        continue;
                    }

                    //Exclude positions vertically outside of battlefield 
                    if (yIterator < 0 || yIterator > 2)
                    {
                        continue;
                    }

                    //When diagonals are not included, exclude poisitions that are not in the same row or lane
                    if (!includeDiagonal)
                    {
                        if (xIterator != battlefieldPosition.x && yIterator != battlefieldPosition.y)
                        {
                            continue;
                        }
                    }

                    //When horizontals are not included, exclude poisitions that are in the same row
                    if (!includeHorizontal)
                    {
                        if (yIterator == battlefieldPosition.y)
                        {
                            continue;
                        }
                    }

                    //When verticals are not included, exclude poisitions that are in the same lane
                    if (!includeVertical)
                    {
                        if (xIterator == battlefieldPosition.x)
                        {
                            continue;
                        }
                    }

                    //Add battlefield positions
                    battlefieldPositions.Add(aiModel.battlefieldPositions[aiModel.battlefieldPositionIndex[xIterator, yIterator]]);
                }
            }

            return battlefieldPositions;
        }

        //Returns a valid enemy in the given battlefield position. Returns null if no enemy is found or if the enemy is not alive.
        public static Enemy GetEnemyByBattlefieldPosition(BattlefieldPosition battlefieldPosition, bool ground, bool living)
        {
            GameObject adjacentEnemyObject;

            if (ground)
            {
                adjacentEnemyObject = battlefieldPosition.owner_ground;
            }
            else
            {
                adjacentEnemyObject = battlefieldPosition.owner_air;
            }

            if (adjacentEnemyObject == null)
            {
                return null;
            }

            Enemy localAdjacentEnemy = adjacentEnemyObject.GetComponent<Enemy>();
            if (localAdjacentEnemy == null)
            {
                return null;
            }

            if (living)
            {
                if (!localAdjacentEnemy.alive && localAdjacentEnemy.enemyName != EnemyName.Shit)
                {
                    return null;
                }
            }

            return localAdjacentEnemy;
        }

        /// <summary>
        /// Returns the world space transform equivalent of an internal battlefield position. Operates using a simplified version of the positioning logic in <see cref="Enemy.setPosition(int, int, float)"/>.
        /// </summary>
        /// <param name="battlefieldPosition">The internal battlefield coordinates.</param>
        /// <returns>The world space transform position of the given battlefield position.</returns>
        /// <remarks>The y position of the returned world space battlefield position will always be 0, corresponding to the floor of the battlefield.
        /// <br/>The y axis of the internal battlefield position corresponds to the z axis of the global transform position due to axis discrepancy between the two systems.</remarks>
        public static Vector3 WorldSpaceBattlefieldPosition(Vector2 battlefieldPosition)
        {
            float transformPositionX = (battlefieldPosition.x - 1f) * 1.25f;
            float transformPositionZ = -0.6f - (battlefieldPosition.y * 0.975f);
            return new Vector3(transformPositionX, 0f, transformPositionZ);
        }

        /// <summary>
        /// Sets the initial scale of a ButtonHoverAnimation.
        /// </summary>
        /// <param name="buttonHoverAnimation">The ButtonHoverAnimation.</param>
        /// <param name="initialScale">The scale.</param>
        public static void SetHoverInitialScale(ButtonHoverAnimation buttonHoverAnimation, Vector3 initialScale)
        {
            if (buttonHoverAnimation == null) return;
            AccessTools.Field(typeof(ButtonHoverAnimation), "initialScale").SetValue(buttonHoverAnimation, initialScale);
        }

        /// <summary>
        /// Randomly unprimes Enemies on the battlefield.
        /// </summary>
        /// <param name="count">The number of Enemies to unprime. If this is negative, all Enemies will be unprimed.</param>
        public static void UnprimeEnemies(int count)
        {
            //List all primed Enemies
            List<Enemy> primedEnemies = new List<Enemy>();
            for (int enemyIterator = 0; enemyIterator < app.model.enemies.Count; enemyIterator++)
            {
                Enemy enemy = app.model.enemies[enemyIterator];
                if (enemy.primed)
                {
                    primedEnemies.Add(enemy);
                }
            }

            //Unprime all Enemies?
            bool unprimeAllEnemies = false;
            if (count < 0 || count >= primedEnemies.Count)
            {
                unprimeAllEnemies = true;
                count = primedEnemies.Count;
            }

            for (int unprimeCounter = 0; unprimeCounter < count; unprimeCounter++)
            {
                if (primedEnemies.Count < 1) return; //Failsafe

                Enemy primedEnemy;
                if (unprimeAllEnemies) primedEnemy = primedEnemies[0]; //Choose the first Enemy
                else primedEnemy = primedEnemies[UnityEngine.Random.Range(0, primedEnemies.Count)]; //Choose a random Enemy

                primedEnemies.Remove(primedEnemy);
                if (primedEnemy == null) continue; //Failsafe

                //Unprime the Enemy
                primedEnemy.Unprime(true);
                if (primedEnemy.boogerCounter == 0)
                {
                    primedEnemy.AnimateIdle();
                }
                else
                {
                    primedEnemy.AnimateBoogered();
                }
            }
        }

        /// <summary>
        /// Rerolls the player's mana.
        /// </summary>
        public static void RerollMana()
        {
            short[] randomMana = new short[6];

            //Get total current mana
            int totalMana = 0;
            for (short manaIterator = 0; manaIterator < 6; manaIterator++) totalMana += app.model.mana[manaIterator];

            //Drain all mana
            app.model.mana = new short[6];

            List<ManaType> manaTypes = new List<ManaType>
            {
                ManaType.Bone,
                ManaType.Booger,
                ManaType.Pee,
                ManaType.Poop,
                ManaType.Tooth
            };

            while (totalMana > 0 && manaTypes.Count > 0)
            {
                int randomManaIndex = UnityEngine.Random.Range(0, manaTypes.Count);
                int randomManaType = (int)manaTypes[randomManaIndex];
                randomMana[randomManaType] += 1;
                totalMana--;

                //Avoid overfilling mana
                if (randomMana[(int)manaTypes[randomManaIndex]] == 9) manaTypes.RemoveAt(randomManaIndex);
            }

            app.controller.UpdateMana(randomMana, false);
        }

        /// <summary>
        /// Replaces visuals of the given GameObject with the given replacements, and adjusts the object's transforms according to the given vectors.
        /// </summary>
        public static void Reskin(GameObject gameObject, Mesh mesh, Material material, Texture2D texture2D, Vector3 localPosition, Vector3 localRotation, Vector3 localScale)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter != null && mesh != null) meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null && material != null) meshRenderer.material = material;

            if (meshRenderer != null && meshRenderer.material != null && texture2D != null) meshRenderer.material.mainTexture = texture2D;

            if (localPosition != null) gameObject.transform.localPosition = localPosition;
            if (localRotation != null) gameObject.transform.localRotation = Quaternion.Euler(localRotation);
            if (localScale != null) gameObject.transform.localScale = localScale;
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