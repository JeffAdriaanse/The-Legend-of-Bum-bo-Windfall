using HarmonyLib;
using I2.Loc;
using PathologicalGames;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class WindfallHelper
    {
        //Vanilla static references

        //Static reference to vanilla BumboApplication
        private static BumboApplication bumboApplication;
        public static BumboApplication app
        {
            get
            {
                if (bumboApplication == null) bumboApplication = GameObject.FindObjectOfType<BumboApplication>();
                return bumboApplication;
            }
            set { bumboApplication = value; }
        }
        public static void GetApp(BumboApplication _app)
        {
            if (_app != null) app = _app;
        }

        //Static reference to vanilla GamepadSpellSelector
        private static GamepadSpellSelector gamepadSpellSelector;
        public static GamepadSpellSelector GamepadSpellSelector
        {
            get
            {
                if (gamepadSpellSelector == null) gamepadSpellSelector = WindfallHelper.app?.view?.GUICamera?.GetComponent<GamepadSpellSelector>();
                if (gamepadSpellSelector == null) gamepadSpellSelector = GameObject.FindObjectOfType<GamepadSpellSelector>();
                return gamepadSpellSelector;
            }
        }

        //Static reference to vanilla GamepadTreasureRoomController
        private static GamepadTreasureRoomController gamepadTreasureRoomController;
        public static GamepadTreasureRoomController GamepadTreasureRoomController
        {
            get
            {
                if (gamepadTreasureRoomController == null) gamepadTreasureRoomController = PoolManager.Pools["Spells"]?.GetComponent<GamepadTreasureRoomController>();
                if (gamepadTreasureRoomController == null) gamepadTreasureRoomController = GameObject.FindObjectOfType<GamepadTreasureRoomController>();
                return gamepadTreasureRoomController;
            }
        }

        //Static reference to vanilla GamepadBossRoomController
        private static GamepadTreasureRoomController gamepadBossRoomController;
        public static GamepadTreasureRoomController GamepadBossRoomController
        {
            get
            {
                if (gamepadBossRoomController == null)
                {
                    GameObject[] bossRewardParents = WindfallHelper.app?.view?.bossRewardParents;
                    if (bossRewardParents != null && bossRewardParents[0] != null) gamepadBossRoomController = bossRewardParents[0]?.transform.parent?.GetComponent<GamepadTreasureRoomController>();
                }

                if (gamepadBossRoomController == null) gamepadBossRoomController = GameObject.FindObjectOfType<GamepadTreasureRoomController>();
                return gamepadBossRoomController;
            }
        }

        //Static reference to vanilla GamepadGamblingController
        private static GamepadGamblingController gamepadGamblingController;
        public static GamepadGamblingController GamepadGamblingController
        {
            get
            {
                if (gamepadGamblingController == null) gamepadGamblingController = GameObject.FindObjectOfType<GamepadGamblingController>();
                return gamepadGamblingController;
            }
        }

        //Windfall static references

        //Static reference to Windfall BattlefieldGridViewController
        private static BattlefieldGridViewController battlefieldGridViewController;
        public static BattlefieldGridViewController BattlefieldGridViewController
        {
            get
            {
                if (battlefieldGridViewController == null) battlefieldGridViewController = GameObject.FindObjectOfType<BattlefieldGridViewController>();
                if (battlefieldGridViewController == null)
                {
                    battlefieldGridViewController = CreateWindfallController<BattlefieldGridViewController>();
                    battlefieldGridViewController.InitializeGrid();
                }
                return battlefieldGridViewController;
            }
            set { battlefieldGridViewController = value; }
        }

        //Static reference to Windfall EnabledSpellsController
        private static EnabledSpellsController enabledSpellsController;
        public static EnabledSpellsController EnabledSpellsController
        {
            get
            {
                if (enabledSpellsController == null) enabledSpellsController = GameObject.FindObjectOfType<EnabledSpellsController>();
                if (enabledSpellsController == null) enabledSpellsController = CreateWindfallController<EnabledSpellsController>();
                return enabledSpellsController;
            }
            set { enabledSpellsController = value; }
        }

        //Static reference to Windfall BlockGroupController
        private static BlockGroupController blockGroupController;
        public static BlockGroupController BlockGroupController
        {
            get
            {
                if (blockGroupController == null) blockGroupController = GameObject.FindObjectOfType<BlockGroupController>();
                if (blockGroupController == null) blockGroupController = CreateWindfallController<BlockGroupController>();
                return blockGroupController;
            }
            set { blockGroupController = value; }
        }

        //Static reference to Windfall BumboModifierIndicationController
        private static BumboModifierIndicationController bumboModifierIndicationController;
        public static BumboModifierIndicationController BumboModifierIndicationController
        {
            get
            {
                if (bumboModifierIndicationController == null) bumboModifierIndicationController = GameObject.FindObjectOfType<BumboModifierIndicationController>();
                if (bumboModifierIndicationController == null) bumboModifierIndicationController = CreateWindfallController<BumboModifierIndicationController>();
                return bumboModifierIndicationController;
            }
            set { bumboModifierIndicationController = value; }
        }

        //Static reference to Windfall WindfallTooltipController
        private static WindfallTooltipController windfallTooltipController;
        public static WindfallTooltipController WindfallTooltipController
        {
            get
            {
                if (windfallTooltipController == null) windfallTooltipController = GameObject.FindObjectOfType<WindfallTooltipController>();
                if (windfallTooltipController == null) windfallTooltipController = CreateWindfallController<WindfallTooltipController>();
                return windfallTooltipController;
            }
            set { windfallTooltipController = value; }
        }

        //Static reference to Windfall SpellViewIndicationController
        private static SpellViewIndicationController spellViewIndicationController;
        public static SpellViewIndicationController SpellViewIndicationController
        {
            get
            {
                if (spellViewIndicationController == null) spellViewIndicationController = GameObject.FindObjectOfType<SpellViewIndicationController>();
                if (spellViewIndicationController == null) spellViewIndicationController = CreateWindfallController<SpellViewIndicationController>();
                return spellViewIndicationController;
            }
            set { spellViewIndicationController = value; }
        }

        //Static reference to Windfall ModifySpellHoverPreviewController
        private static ModifySpellHoverPreviewController modifySpellHoverPreviewController;
        public static ModifySpellHoverPreviewController ModifySpellHoverPreviewController
        {
            get
            {
                if (modifySpellHoverPreviewController == null) modifySpellHoverPreviewController = GameObject.FindObjectOfType<ModifySpellHoverPreviewController>();
                if (modifySpellHoverPreviewController == null) modifySpellHoverPreviewController = CreateWindfallController<ModifySpellHoverPreviewController>();
                return modifySpellHoverPreviewController;
            }
            set { modifySpellHoverPreviewController = value; }
        }


        /// <summary>
        ///Creates a GameObject child of BumboController with the given MonoBehaviour component type attached as a new instance. Returns the created MonoBehaviour component.
        /// </summary>
        private static T CreateWindfallController<T>() where T : MonoBehaviour
        {
            BumboController bumboController = app.controller;
            if (bumboController == null) return null;

            GameObject gameObject = GameObject.Instantiate(new GameObject(), bumboController.transform);
            T controller = gameObject.AddComponent<T>();
            gameObject.name = controller.GetType().ToString();
            return controller;
        }

        /// <summary>
        ///Given a battlefield position, returns the default enemy worldspace position of that battlefield position
        /// </summary>
        public static Vector3 BattlefieldDefaultEnemyPosition(Position battlefieldPosition)
        {
            return new Vector3(battlefieldPosition.x - 1.25f, 0f, -(battlefieldPosition.y * 0.975f + 0.6f));
        }

        /// <summary>
        /// Returns the approximate worldspace position of the given enemy.
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

        private readonly static string edFontPath = "EdmundMcMillen-Regular SDF";
        private static TMP_FontAsset edFont;
        public static TMP_FontAsset GetEdmundMcmillenFont()
        {
            if (edFont == null && Windfall.assetBundle != null && Windfall.assetBundle.Contains(edFontPath)) edFont = Windfall.assetBundle.LoadAsset<TMP_FontAsset>(edFontPath);
            return edFont;
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

        /// <summary>
        /// Updates the GamepadMenuController to have references to relevant menu buttons. Recursively searches for buttons in children Transforms down to the given depth value.
        /// </summary>
        public static void UpdateGamepadMenuButtons(GamepadMenuController gamepadMenuController, GameObject cancelButton, int childOptionButtonDepth)
        {
            if (gamepadMenuController == null) return;

            //Search for children with GamepadMenuOptionSelection
            List<GameObject> newOptions = ChildrenWithComponent<GamepadMenuOptionSelection>(gamepadMenuController.transform, childOptionButtonDepth);
            if (newOptions.Count > 0) gamepadMenuController.m_Buttons = newOptions.ToArray();

            //Add cancel button
            if (cancelButton != null) gamepadMenuController.m_CancelButton = cancelButton;
        }

        /// <summary>
        /// Recursively searches for all children of the given Transform that have given component, up to the given maximum depth. Returns a list containing the child GameObjects.
        /// </summary>
        private static List<GameObject> ChildrenWithComponent<T>(Transform parent, int maximumDepth, int currentDepth = 0)
        {
            List<GameObject> childrenWithComponent = new List<GameObject>();
            if (currentDepth >= maximumDepth) return childrenWithComponent;

            for (int childCounter = 0; childCounter < parent.childCount; childCounter++)
            {
                Transform child = parent.transform.GetChild(childCounter);
                if (child.gameObject.activeSelf && child.GetComponent<T>() != null) childrenWithComponent.Add(child.gameObject);
                childrenWithComponent.AddRange(ChildrenWithComponent<T>(child, maximumDepth, currentDepth + 1));
            }
            return childrenWithComponent;
        }

        static Shader defaultShader;
        static Shader DefaultShader
        {
            get
            {
                if (defaultShader == null) defaultShader = Shader.Find("Standard");
                return defaultShader;
            }
        }
        public static Transform ResetShader(Transform transform)
        {
            if (transform == null) return null;

            foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>())
            {
                if (meshRenderer != null && !meshRenderer.GetComponent<TextMeshPro>())
                {
                    if (meshRenderer?.material?.shader != null && DefaultShader != null) meshRenderer.material.shader = DefaultShader;
                    meshRenderer.material.shaderKeywords = new string[] { "_GLOSSYREFLECTIONS_OFF", "_SPECULARHIGHLIGHTS_OFF" };
                }
            }
            return transform;
        }

        //Finds all valid enemy positions on the battlefield adjacent to the provided enemy position
        public static List<BattlefieldPosition> AdjacentBattlefieldPositions(AIModel aiModel, BattlefieldPosition battlefieldPosition, bool includeDiagonal, bool includeHorizontal = true, bool includeVertical = true)
        {
            List<BattlefieldPosition> battlefieldPositions = new List<BattlefieldPosition>();

            if (aiModel == null || battlefieldPosition == null) return battlefieldPositions;

            //Search all potential positions around the battlefield position
            for (int xIterator = battlefieldPosition.x - 1; xIterator < battlefieldPosition.x + 2; xIterator++)
            {
                for (int yIterator = battlefieldPosition.y - 1; yIterator < battlefieldPosition.y + 2; yIterator++)
                {
                    //Exclude the provided position
                    if (xIterator == battlefieldPosition.x && yIterator == battlefieldPosition.y) continue;

                    //Exclude positions horizontally outside of battlefield 
                    if (xIterator < 0 || xIterator > 2) continue;

                    //Exclude positions vertically outside of battlefield 
                    if (yIterator < 0 || yIterator > 2) continue;

                    //When diagonals are not included, exclude positions that are not in the same row or lane
                    if (!includeDiagonal)
                    {
                        if (xIterator != battlefieldPosition.x && yIterator != battlefieldPosition.y) continue;
                    }

                    //When horizontals are not included, exclude positions that are in the same row
                    if (!includeHorizontal)
                    {
                        if (yIterator == battlefieldPosition.y) continue;
                    }

                    //When verticals are not included, exclude positions that are in the same lane
                    if (!includeVertical)
                    {
                        if (xIterator == battlefieldPosition.x) continue;
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

            if (ground) adjacentEnemyObject = battlefieldPosition.owner_ground;
            else adjacentEnemyObject = battlefieldPosition.owner_air;

            if (adjacentEnemyObject == null) return null;

            Enemy localAdjacentEnemy = adjacentEnemyObject.GetComponent<Enemy>();
            if (localAdjacentEnemy == null) return null;
            if (living)
            {
                if (!localAdjacentEnemy.alive && localAdjacentEnemy.enemyName != EnemyName.Shit) return null;
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
                if (enemy.primed) primedEnemies.Add(enemy);
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
                if (primedEnemy.boogerCounter == 0) primedEnemy.AnimateIdle();
                else primedEnemy.AnimateBoogered();
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
        /// Grants Bum-bo mana of random colors.
        /// </summary>
        /// <param name="amount">The amount of mana for Bum-bo to gain.</param>
        /// <param name="spreadOutManaColors">Whether to attempt to spread out mana evenly across all five colors.</param>
        /// <param name="avoidGoingOverMaximum">Whether to attempt to avoid giving Bum-bo mana over the maximum of 9 for each color.</param>
        public static void AddRandomMana(int amount, bool spreadOutManaColors = false, bool avoidGoingOverMaximum = true)
        {
            short[] manaToAdd = new short[6];

            List<ManaType> manaColors = new List<ManaType>
            {
                ManaType.Bone,
                ManaType.Booger,
                ManaType.Pee,
                ManaType.Poop,
                ManaType.Tooth
            };

            for (int manaGainCounter = 0; manaGainCounter < amount; manaGainCounter++)
            {
                //If mana colors are not being spread out, refresh the mana colors list
                if (!spreadOutManaColors || manaColors.Count == 0)
                {
                    manaColors = new List<ManaType>
                    {
                        ManaType.Bone,
                        ManaType.Booger,
                        ManaType.Pee,
                        ManaType.Poop,
                        ManaType.Tooth
                    };
                }

                short[] mana = app.model.mana;

                if (avoidGoingOverMaximum)
                {
                    //Avoid mana types that Bum-bo already has 9 of
                    for (int manaTypeCounter = 0; manaTypeCounter < mana.Length; manaTypeCounter++)
                    {
                        if (mana[manaTypeCounter] >= 9) manaColors.Remove((ManaType)manaTypeCounter);
                    }
                }

                //Failsafe
                if (manaColors.Count == 0)
                {
                    manaColors = new List<ManaType>
                    {
                        ManaType.Bone,
                        ManaType.Booger,
                        ManaType.Pee,
                        ManaType.Poop,
                        ManaType.Tooth
                    };
                }

                //Choose a random mana to add
                int manaIndex = UnityEngine.Random.Range(0, manaColors.Count);
                manaColors.RemoveAt(manaIndex);
                manaToAdd[manaIndex]++;
            }

            app.controller.UpdateMana(manaToAdd, false);
            app.controller.ShowManaGain();
        }

        /// <summary>
        /// Replaces visuals of the given GameObject with the given replacements, and adjusts the object's transforms according to the given vectors.
        /// </summary>
        public static void Reskin(GameObject gameObject, Mesh mesh, Material material, Texture2D texture2D, bool resetShader = true)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter != null && mesh != null) meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (material != null) meshRenderer.material = material;
                if (meshRenderer.material != null && texture2D != null) meshRenderer.material.mainTexture = texture2D;
                if (resetShader && meshRenderer.material != null) meshRenderer.material.shader = DefaultShader;
            }
        }

        /// <summary>
        /// Adjusts the given GameObject's transforms according to the given vectors.
        /// </summary>
        public static void ReTransform(GameObject gameObject, Vector3 localPosition, Vector3 localRotation, Vector3 localScale, string omitTransform)
        {
            if (omitTransform == null || !omitTransform.Contains("position")) gameObject.transform.localPosition = localPosition;
            if (omitTransform == null || !omitTransform.Contains("rotation")) gameObject.transform.localRotation = Quaternion.Euler(localRotation);
            if (omitTransform == null || !omitTransform.Contains("scale")) gameObject.transform.localScale = localScale;
        }

        /// <summary>
        /// Adds localization objects to the given GameObject and applies the given localization term.
        /// </summary>
        /// <param name="gameObject">The GameObject to localize.</param>
        /// <param name="term">The localization term.</param>
        /// <param name="localizationFontOverrides">A list of localization font overrides to apply.</param>
        public static void LocalizeObject(GameObject gameObject, string term, List<LocalizationFontOverrides.OverrideEntry> localizationFontOverrides = null)
        {
            Localize localize = gameObject.AddComponent<Localize>();
            if (term != null && term != string.Empty) localize.Term = term;
            localize.SecondaryTerm = "FONT_FACE_MAIN";

            if (localizationFontOverrides != null && localizationFontOverrides.Count > 0)
            {
                LocalizationFontOverrides localizationFontOverridesComponent = gameObject.AddComponent<LocalizationFontOverrides>();
                localizationFontOverridesComponent.m_Overrides = new List<LocalizationFontOverrides.OverrideEntry>(localizationFontOverrides);
            }
        }

        /// <summary>
        /// Initializes button functionality of the given GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to localize.</param>
        /// <param name="term">The localization term.</param>
        /// <param name="localizationFontOverrides">A list of localization font overrides to apply.</param>
        public static void InitializeButton(GameObject buttonObject, UnityAction unityAction, TMP_FontAsset font, GamepadMenuOptionSelection.eInjectDots eInjectDots)
        {
            ButtonHoverAnimation buttonHoverAnimation = buttonObject.AddComponent<ButtonHoverAnimation>();
            buttonHoverAnimation.hoverSoundFx = SoundsView.eSound.Menu_ItemHover;
            buttonHoverAnimation.clickSoundFx = SoundsView.eSound.Menu_ItemSelect;

            Button buttonComponent = buttonObject.GetComponent<Button>();
            if (buttonComponent == null) buttonComponent = buttonObject.AddComponent<Button>();
            buttonComponent.onClick.AddListener(unityAction);

            GamepadMenuOptionSelection gamepadMenuOptionSelection = buttonObject.AddComponent<GamepadMenuOptionSelection>();
            gamepadMenuOptionSelection.m_InjectDots = eInjectDots;
            gamepadMenuOptionSelection.m_SelectionObjects = new GameObject[0];

            TextMeshProUGUI textMeshProUGUI = buttonObject.GetComponent<TextMeshProUGUI>();
            LocalizationModifier.ChangeFont(textMeshProUGUI, null, font);
        }

        /// <summary>
        /// Creates a duplicate of the given SpellElement. Note that data stored with <see cref="ObjectDataStorage"/> will also be copied over.
        /// </summary>
        /// <param name="spell">The SpellElement to copy.</param>
        /// <returns>The duplicate SpellElement.</returns>
        public static SpellElement CopySpell(SpellElement spell)
        {
            if (spell == null) return null;

            SpellName spellName = spell.spellName;
            SpellElement duplicateSpell = WindfallHelper.app.model.spellModel.spells[spellName];
            if (spell.Cost != null) duplicateSpell.Cost = (short[])spell.Cost.Clone();
            if (spell.CostModifier != null) duplicateSpell.CostModifier = (short[])spell.CostModifier.Clone();
            duplicateSpell.CostOverride = spell.CostOverride;
            duplicateSpell.charge = spell.charge;
            duplicateSpell.requiredCharge = spell.requiredCharge;
            duplicateSpell.chargeEveryRound = spell.chargeEveryRound;
            duplicateSpell.usedInRound = spell.usedInRound;
            duplicateSpell.baseDamage = spell.baseDamage;
            duplicateSpell.UsedInRoom = spell.UsedInRoom;
            duplicateSpell.setCost = spell.setCost;

            ObjectDataStorage.CopyData(spell, duplicateSpell);

            return duplicateSpell;
        }

        /// <summary>
        /// Determines whether the first SpellElement is the same as the second SpellElement. Note that data stored with <see cref="ObjectDataStorage"/> will not be copied over.
        /// </summary>
        /// <param name="firstSpell">The SpellElement to copy.</param>
        /// <param name="secondSpell">The SpellElement to copy.</param>
        /// <param name="lenient">Whether to allow some of the most variable pieces of data to differ without the SpellElements being considered unequal.</param>
        /// <returns>Whether the first SpellElement is the same as the second SpellElement, or false if either SpellElement is null or either SpellElement Cost is null.</returns>
        public static bool CompareSpells(SpellElement firstSpell, SpellElement secondSpell, bool lenient = false)
        {
            if (firstSpell == null || secondSpell == null) return false;

            bool equal = false;

            bool lenientEqual = firstSpell.GetType().Equals(secondSpell.GetType())
                && firstSpell.spellName.Equals(secondSpell.spellName)

                && firstSpell.Cost != null
                && secondSpell.Cost != null
                && firstSpell.Cost.SequenceEqual(secondSpell.Cost)

                && firstSpell.CostOverride == secondSpell.CostOverride
                && firstSpell.requiredCharge == secondSpell.requiredCharge
                && firstSpell.chargeEveryRound == secondSpell.chargeEveryRound
                && firstSpell.baseDamage == secondSpell.baseDamage;

            bool strictEqual = firstSpell.charge == secondSpell.charge
                && firstSpell.usedInRound == secondSpell.usedInRound
                && firstSpell.UsedInRoom == secondSpell.UsedInRoom
                && firstSpell.setCost == secondSpell.setCost;

            if (lenient) equal = lenientEqual;
            else equal = lenientEqual && strictEqual;

            return equal;
        }

        /// <summary>
        /// Returns the total mana cost of the given spell.
        /// </summary>
        /// <param name="spellElement">The SpellElement to find the total mana cost of.</param>
        /// <param name="includeCostModifier">Whether to include the spell's cost modifier.</param>
        /// <returns>The total mana cost of the given spell.</returns>
        public static int SpellTotalManaCost(SpellElement spellElement, bool includeCostModifier)
        {
            int spellTotalManaCost = 0;
            for (int i = 0; i < spellElement.Cost.Length; i++) spellTotalManaCost += spellElement.Cost[i];
            if (includeCostModifier)
            {
                for (int i = 0; i < spellElement.CostModifier.Length; i++) spellTotalManaCost += spellElement.Cost[i];
            }
            return spellTotalManaCost;
        }

        /// <summary>
        /// Updates the spell active visuals of the given SpellView according to the ready state of the given SpellElement.
        /// </summary>
        /// <param name="spellView">The SpellView to update the visuals of.</param>
        /// <param name="spellElement">The SpellElement to check the ready state of.</param>
        public static void UpdateSpellViewActiveVisuals(SpellView spellView, SpellElement spellElement)
        {
            bool ready = spellElement.IsReady();

            //Spell icon
            spellView.SetActive(ready);

            //Spell container
            if (ready) spellView.spellContainer.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, -0.5f));
            else spellView.spellContainer.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, -0.25f));

            //Spell category
            GameObject spellCategoryObject = null;
            switch (spellElement.Category)
            {
                case SpellElement.SpellCategory.Defense:
                    spellCategoryObject = spellView.spellTypeDefense;
                    break;
                case SpellElement.SpellCategory.Puzzle:
                    spellCategoryObject = spellView.spellTypePuzzle;
                    break;
                case SpellElement.SpellCategory.Use:
                    spellCategoryObject = spellView.spellTypeItem;
                    break;
                case SpellElement.SpellCategory.Other:
                    spellCategoryObject = spellView.spellTypeSpecial;
                    break;
                case SpellElement.SpellCategory.Attack:
                    spellCategoryObject = spellView.spellTypeAttack;
                    break;
            }

            if (spellCategoryObject != null)
            {
                if (ready) spellCategoryObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                else spellCategoryObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
            }
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