using DG.Tweening;
using HarmonyLib;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    class WindfallTooltip : MonoBehaviour
    {
        public enum Anchor
        {
            Center,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left,
            TopLeft,
        }

        public bool displayAtMouse;
        public Vector3 displayPosition;
        public Anchor displayAnchor;
        public string displayDescription;

        public bool active;

        public bool displayGamepad;

        public static UnityEngine.Color enemyHoverTintColor = new UnityEngine.Color(0.6f, 0.6f, 0.6f);

        public void UpdateDisplayType(bool gamepad)
        {
            displayGamepad = gamepad;
            UpdateDisplayData();
        }

        public void UpdateDisplayData()
        {
            active = true;

            BumboModifier bumboModifier = gameObject.GetComponent<BumboModifier>();
            if (bumboModifier != null)
            {
                if (!bumboModifier.Expanded())
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bumboModifier.TooltipPosition();
                    displayAnchor = Anchor.Right;
                }

                displayDescription = bumboModifier.Description();
                return;
            }

            BumboModifierTemporary bumboModifierTemporary = gameObject.GetComponent<BumboModifierTemporary>();
            if (bumboModifierTemporary != null)
            {
                if (!bumboModifierTemporary.bumboModifier.Expanded())
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bumboModifierTemporary.bumboModifier.TooltipPosition();
                    displayAnchor = Anchor.Right;
                }

                displayDescription = LocalizationModifier.GetLanguageText(bumboModifierTemporary.description, "Indicators");
                return;
            }

            BumboModifierStacking bumboModifierStacking = gameObject.GetComponent<BumboModifierStacking>();
            if (bumboModifierStacking != null)
            {
                if (!bumboModifierStacking.bumboModifier.Expanded())
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bumboModifierStacking.bumboModifier.TooltipPosition();
                    displayAnchor = Anchor.Right;
                }

                displayDescription = bumboModifierStacking.bumboModifier.StackingDescription();
                return;
            }

            SpellView spellView = gameObject.GetComponent<SpellView>();
            if (spellView != null)
            {
                SpellElement spell = spellView.SpellObject;
                if (spell == null || spell.spellName == SpellName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = spellView.transform.position + new Vector3(-0.43f, 0f, 0f);
                    displayAnchor = Anchor.Left;
                }

                displayDescription = string.Empty;
                if (WindfallHelper.app.model.spellModel.spellKA.TryGetValue(spell.spellName, out string spellKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetLanguageText(spellKA, "Spells") + "</u>\n" + WindfallTooltipDescriptions.SpellDescriptionWithValues(spell);
                }
                return;
            }

            SpellViewIndicator spellViewIndicator = gameObject.GetComponent<SpellViewIndicator>();
            if (spellViewIndicator != null)
            {
                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = spellViewIndicator.transform.position + new Vector3(-0.02f, 0.05f, 0f);
                    displayAnchor = Anchor.Top;
                }

                displayDescription = spellViewIndicator.TooltipDescription();
            }

            SpellPickup spellPickup = gameObject.GetComponent<SpellPickup>();
            if (spellPickup != null)
            {
                SpellElement spell = spellPickup.spell;
                if (spell == null || spell.spellName == SpellName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = !displayGamepad;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = spellPickup.transform.Find("Spell Pickup Object").Find("Spell_Holder").GetComponent<MeshRenderer>().bounds.center - spellPickup.transform.right * 0.3f;
                    displayAnchor = Anchor.Right;
                }

                displayDescription = string.Empty;
                if (WindfallHelper.app.model.spellModel.spellKA.TryGetValue(spell.spellName, out string spellKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetLanguageText(spellKA, "Spells") + "</u>\n" + WindfallTooltipDescriptions.SpellDescriptionWithValues(spell);
                }

                return;
            }

            TrinketView trinketView = gameObject.GetComponent<TrinketView>();
            if (trinketView != null)
            {
                if (trinketView.trinketIndex >= WindfallHelper.app.model.characterSheet.trinkets.Count)
                {
                    active = false;
                    return;
                }

                TrinketElement trinket = WindfallHelper.app.controller.GetTrinket(trinketView.trinketIndex);
                if (trinket == null || trinket.trinketName == TrinketName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = trinketView.transform.position + new Vector3(-0.074f, 0.14f, 0f);
                    displayAnchor = Anchor.Top;
                }

                displayDescription = string.Empty;
                if (WindfallHelper.app.model.trinketModel.trinketKA.TryGetValue(trinket.trinketName, out string trinketKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetLanguageText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
                }
                return;
            }

            if (gameObject.transform.parent != null)
            {
                TrinketView trinketViewParent = gameObject.transform.parent.GetComponent<TrinketView>();
                if (gameObject.name == "GlitchVisualObject" && trinketViewParent != null && trinketViewParent.trinketIndex < WindfallHelper.app.model.characterSheet.trinkets.Count)
                {
                    TrinketElement trinket = WindfallHelper.app.model.characterSheet.trinkets[trinketViewParent.trinketIndex];
                    if (trinket == null || trinket.trinketName != TrinketName.Glitch)
                    {
                        active = false;
                        return;
                    }

                    displayAtMouse = false;

                    if (displayAtMouse)
                    {
                        displayPosition = Vector3.zero;
                        displayAnchor = Anchor.TopRight;
                    }
                    else
                    {
                        displayPosition = trinketViewParent.transform.position + new Vector3(-0.074f, 0.14f, 0f);
                        displayAnchor = Anchor.Top;
                    }

                    displayDescription = string.Empty;
                    if (WindfallHelper.app.model.trinketModel.trinketKA.TryGetValue(trinket.trinketName, out string trinketKA))
                    {
                        displayDescription = "<u>" + LocalizationModifier.GetLanguageText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
                    }
                    return;
                }
            }

            TrinketPickupView trinketPickupView = gameObject.GetComponent<TrinketPickupView>();
            if (trinketPickupView != null)
            {
                TrinketElement trinket = trinketPickupView.trinket;
                if (trinket == null || trinket.trinketName == TrinketName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = !displayGamepad;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = trinketPickupView.transform.Find("Trinket_Pickup").GetComponent<MeshRenderer>().bounds.center - trinketPickupView.transform.right * 0.3f;
                    displayAnchor = Anchor.Right;
                }

                displayDescription = string.Empty;
                if (WindfallHelper.app.model.trinketModel.trinketKA.TryGetValue(trinket.trinketName, out string trinketKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetLanguageText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
                }

                return;
            }

            BumboFacesController bumboFacesController = gameObject.GetComponent<BumboFacesController>();
            if (bumboFacesController != null)
            {
                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bumboFacesController.transform.position + new Vector3(0.08f, 0f, 0f);
                    displayAnchor = Anchor.Right;
                }

                displayDescription = string.Empty;

                string bumboName = string.Empty;
                string bumboDescription = string.Empty;

                CharacterSheet.BumboType bumboType = WindfallHelper.app.model.characterSheet.bumboType;
                if (WindfallTooltipDescriptions.BumboNames.TryGetValue(bumboType, out string name)) bumboName = LocalizationModifier.GetLanguageText(name, "Characters");
                if (WindfallTooltipDescriptions.BumboDescriptions.TryGetValue(bumboType, out string description)) bumboDescription = LocalizationModifier.GetLanguageText(description, "Characters");

                displayDescription = "<u>" + bumboName + "</u>\n" + bumboDescription;
                return;
            }

            //Try to find enemy component
            Enemy enemy = gameObject.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = ObjectDataStorage.GetData<Enemy>(gameObject, EntityChanges.colliderEnemyKey);
            }
            if (enemy != null)
            {
                if (enemy is BygoneBoss && !enemy.alive)
                {
                    active = false;
                    return;
                }

                displayAtMouse = !displayGamepad;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = enemy.transform.position - enemy.transform.right * 0.3f;
                    displayAnchor = Anchor.Right;
                }

                //Movement
                string movesText = "\nActions: " + enemy.turns.ToString();

                //Damage
                string damageText = "\nDamage: ";
                int damage = 1;
                if (enemy.berserk || enemy.brieflyBerserk == 1)
                {
                    damage++;
                }
                if (enemy.championType == Enemy.ChampionType.FullHeart)
                {
                    damage++;
                }
                switch (damage)
                {
                    default:
                        damageText += "1/2 heart";
                        break;
                    case 2:
                        damageText += "1 heart";
                        break;
                    case 3:
                        damageText += "1 + 1/2 heart";
                        break;
                }

                //Enemy names
                string localizationCategory = enemy is Boss ? "Bosses" : "Enemies";
                string enemyNameText = LocalizationModifier.GetLanguageText(WindfallTooltipDescriptions.EnemyDisplayName(enemy), localizationCategory);

                //Resistances
                string resitanceText = string.Empty;
                switch (enemy.attackImmunity)
                {
                    case Enemy.AttackImmunity.ReducePuzzleDamage:
                        resitanceText = "\nResists puzzle damage";
                        break;
                    case Enemy.AttackImmunity.ReduceSpellDamage:
                        resitanceText = "\nResists spell damage";
                        break;
                }

                //Invincibility
                string invincibilityText = WindfallTooltipDescriptions.EnemyIsInvincible(enemy) ? "\nInvulnerable" : string.Empty;

                //Damage reduction
                string damageReductionText = WindfallTooltipDescriptions.EnemyDamageReductionWithValues(enemy);

                //Blocking
                string blockText = WindfallTooltipDescriptions.EnemyIsBlocking(enemy) ? "\nBlocks the next hit" : string.Empty;

                //Enemy descriptions
                string descriptionText = WindfallTooltipDescriptions.EnemyDisplayDescription(enemy);

                //Omit irrelevant tooltip information
                if (!enemy.alive)
                {
                    if (enemy is not StonyEnemy)
                    {
                        movesText = string.Empty;
                        damageText = string.Empty;
                    }
                }
                if (WindfallTooltipDescriptions.NonAttackingEnemies.Contains(enemy.enemyName) || enemy.gameObject.name.Contains("Tainted Shy Gal Mimic 2"))
                {
                    damageText = string.Empty;
                }

                //Tint enemy
                ObjectTinter objectTinter = enemy.objectTinter;
                if (objectTinter != null)
                {
                    if (!(bool)AccessTools.Field(typeof(ObjectTinter), "tinted").GetValue(objectTinter))
                    {
                        objectTinter.Tint(enemyHoverTintColor);
                    }
                }

                //Output description
                displayDescription = "<u>" + enemyNameText + "</u>" + movesText + damageText + invincibilityText + resitanceText + damageReductionText + blockText + descriptionText;
                return;
            }
        }
    }

    static class WindfallTooltipController
    {
        private enum DefaultTooltipMode
        {
            Enabled,
            Disabled,
            Override,
        }

        private static GameObject tooltip;
        private static bool tooltipShowing = true;

        private static Transform anchor;

        private static TextMeshPro hiddenLabel;
        private static List<TextMeshPro> labels;

        private static readonly float SCALE_SMALL = 0.85f;
        private static readonly float SCALE_MEDIUM = 1.0f;
        private static readonly float SCALE_LARGE = 1.15f;

        public static void UpdateTooltips()
        {
            if (WindfallHelper.app?.view?.GUICamera?.cam == null)
            {
                return;
            }

            //Abort if tooltips are disabled
            int tooltipSize = WindfallPersistentDataController.LoadData().tooltipSize;
            if (tooltipSize == -2)
            {
                if (tooltip != null)
                {
                    ShowTooltip(false, false);
                }
                return;
            }

            //GUICamera
            Ray GUIray = WindfallHelper.app.view.GUICamera.cam.ScreenPointToRay(Input.mousePosition);

            Ray MainRay;
            if (WindfallHelper.app.view.gamblingView == null)
            {
                //Main Camera
                MainRay = WindfallHelper.app.view.mainCamera.cam.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                //Gambling Camera
                MainRay = WindfallHelper.app.view.gamblingView.gamblingCameraView.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            }

            //Get closest tooltip object under mouse position
            WindfallTooltip tooltipToShow = GetMouseTooltip(GUIray, MainRay);

            bool gamepadTooltip = false;
            //If there is no mouse tooltip, attempt to get a gamepad tooltip
            if (tooltipToShow == null)
            {
                tooltipToShow = GetGamepadTooltip();
                gamepadTooltip = true;
            }

            if (tooltipToShow != null)
            {
                //Update tooltip display type
                tooltipToShow.UpdateDisplayType(gamepadTooltip);
            }

            if (tooltipToShow != null && tooltipToShow.displayAtMouse)
            {
                tooltipToShow.displayPosition = GUIray.GetPoint(1f);
            }

            DisplayTooltip(tooltipToShow);

            ClearEnemyTints(tooltipToShow);

            DisplayGridView(tooltipToShow);
        }

        private static WindfallTooltip GetMouseTooltip(Ray GUIray, Ray MainRay)
        {
            RaycastHit[] GUIhits = Physics.RaycastAll(GUIray);
            RaycastHit[] MainHits = Physics.RaycastAll(MainRay);

            RaycastHit[] AllHits = new RaycastHit[GUIhits.Length + MainHits.Length];
            GUIhits.CopyTo(AllHits, 0);
            MainHits.CopyTo(AllHits, GUIhits.Length);

            WindfallTooltip closestTooltip = null;
            float closestTooltipDistance = 0f;
            bool closestTooltipGUI = false;

            for (int hitIterator = 0; hitIterator < AllHits.Length; hitIterator++)
            {
                RaycastHit hit = AllHits[hitIterator];

                //Do not count hits that mix GUICamera with a non-GUI collider, or vice versa
                bool GUIHit = hitIterator < GUIhits.Length;
                bool GUILayer = hit.collider.gameObject.layer == 5;
                if (GUIHit != GUILayer)
                {
                    continue;
                }

                WindfallTooltip windfallTooltip = hit.collider.GetComponent<WindfallTooltip>();

                if (windfallTooltip != null)
                {
                    windfallTooltip.UpdateDisplayData();

                    //Verify that the tooltip is active
                    if (!windfallTooltip.active)
                    {
                        continue;
                    }

                    //Verify that the tooltip is the closest tooltip
                    if (hit.distance > closestTooltipDistance && closestTooltipDistance != 0f)
                    {
                        continue;
                    }

                    //Prioritize GUI tooltips
                    if (!GUIHit && closestTooltipGUI)
                    {
                        continue;
                    }

                    closestTooltip = windfallTooltip;
                    closestTooltipDistance = hit.distance;
                    closestTooltipGUI = GUIHit;
                }
            }
            return closestTooltip;
        }

        private static WindfallTooltip GetGamepadTooltip()
        {
            //Access gamepad objects and add them to the list
            List<GameObject> gamepadObjects = new List<GameObject>();

            //GamepadSpellSelector
            if (WindfallHelper.GamepadSpellSelector != null)
            {
                //Access m_Selectables
                object m_Selectables = typeof(GamepadSpellSelector).GetField("m_Selectables", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(WindfallHelper.GamepadSpellSelector);
                if (m_Selectables != null && m_Selectables is IEnumerable)
                {
                    foreach (object selectable in (m_Selectables as IEnumerable))
                    {
                        if (selectable == null)
                        {
                            continue;
                        }

                        Type selectableType = selectable.GetType();
                        SpellView m_Spell = (SpellView)selectableType.GetField("m_Spell", BindingFlags.Public | BindingFlags.Instance).GetValue(selectable);
                        TrinketView m_Trinket = (TrinketView)selectableType.GetField("m_Trinket", BindingFlags.Public | BindingFlags.Instance).GetValue(selectable);

                        if (m_Spell != null)
                        {
                            gamepadObjects.Add(m_Spell.gameObject);
                            continue;
                        }

                        if (m_Trinket != null)
                        {
                            gamepadObjects.Add(m_Trinket.gameObject);
                        }
                    }
                }
            }

            //GamepadTreasureRoomController
            if (WindfallHelper.GamepadTreasureRoomController != null)
            {
                //Access m_Selections
                object m_Selections_object = AccessTools.Field(typeof(GamepadTreasureRoomController), "m_Selections").GetValue(WindfallHelper.GamepadTreasureRoomController);

                List<MonoBehaviour> m_Selections = new List<MonoBehaviour>();
                if (m_Selections_object != null && m_Selections_object is List<MonoBehaviour>)
                {
                    m_Selections = m_Selections_object as List<MonoBehaviour>;
                }

                foreach (MonoBehaviour monoBehaviour in m_Selections)
                {
                    if (monoBehaviour != null)
                    {
                        gamepadObjects.Add(monoBehaviour.gameObject);
                    }
                }
            }

            //GamepadBossRoomController
            if (WindfallHelper.GamepadBossRoomController != null)
            {
                //Access m_Selections
                object m_Selections_object = AccessTools.Field(typeof(GamepadTreasureRoomController), "m_Selections").GetValue(WindfallHelper.GamepadBossRoomController);
                List<MonoBehaviour> m_Selections = new List<MonoBehaviour>();
                if (m_Selections_object != null && m_Selections_object is List<MonoBehaviour>)
                {
                    m_Selections = m_Selections_object as List<MonoBehaviour>;
                }

                foreach (MonoBehaviour monoBehaviour in m_Selections)
                {
                    if (monoBehaviour != null)
                    {
                        gamepadObjects.Add(monoBehaviour.gameObject);
                    }
                }
            }

            //GamepadGamblingController
            if (WindfallHelper.GamepadGamblingController != null)
            {
                //Access m_ShopItems
                object m_ShopItems_object = AccessTools.Field(typeof(GamepadGamblingController), "m_ShopItems").GetValue(WindfallHelper.GamepadGamblingController);
                List<MonoBehaviour> m_ShopItems = new List<MonoBehaviour>();
                if (m_ShopItems_object != null && m_ShopItems_object is List<MonoBehaviour>)
                {
                    m_ShopItems = m_ShopItems_object as List<MonoBehaviour>;
                }

                foreach (MonoBehaviour monoBehaviour in m_ShopItems)
                {
                    if (monoBehaviour != null)
                    {
                        gamepadObjects.Add(monoBehaviour.gameObject);
                    }
                }
            }

            //Find the first selected gamepad object and display the return the associated tooltip
            foreach (GameObject gamepadObject in gamepadObjects)
            {
                if (gamepadObject == null)
                {
                    continue;
                }

                WindfallTooltip windfallTooltip = gamepadObject.GetComponent<WindfallTooltip>();
                if (windfallTooltip == null)
                {
                    continue;
                }

                bool selected = false;

                SpellView spellView = gamepadObject.GetComponent<SpellView>();
                if (spellView != null && spellView.gamepadSelectionObject != null && spellView.gamepadSelectionObject.activeSelf)
                {
                    selected = true;
                }

                TrinketView trinketView = gamepadObject.GetComponent<TrinketView>();
                if (trinketView != null && trinketView.gamepadSelectionObject != null && trinketView.gamepadSelectionObject.activeSelf)
                {
                    selected = true;
                }

                SpellPickup spellPickup = gamepadObject.GetComponent<SpellPickup>();
                if (spellPickup != null && spellPickup.selectionArrow != null && spellPickup.selectionArrow.activeSelf)
                {
                    selected = true;
                }

                TrinketPickupView trinketPickupView = gamepadObject.GetComponent<TrinketPickupView>();
                if (trinketPickupView != null && trinketPickupView.selectionArrow != null && trinketPickupView.selectionArrow.activeSelf)
                {
                    selected = true;
                }

                if (selected)
                {
                    windfallTooltip.UpdateDisplayData();

                    //Verify that the tooltip is active
                    if (!windfallTooltip.active)
                    {
                        continue;
                    }

                    return windfallTooltip;
                }
            }

            return null;
        }

        private static void DisplayTooltip(WindfallTooltip windfallTooltip)
        {
            if (WindfallHelper.app == null)
            {
                return;
            }

            if (tooltip == null)
            {
                tooltip = CreateTooltip();

                if (tooltip == null)
                {
                    return;
                }
            }

            if (windfallTooltip == null)
            {
                ShowTooltip(false, true);
                return;
            }

            ResizeTooltipAndSetLabelText(windfallTooltip);

            //Hud Camera
            Camera hudCamera = WindfallHelper.app.view.GUICamera.cam;
            Vector3 hudCameraPosition = hudCamera.transform.position;
            Vector3 hudCameraForward = hudCamera.transform.forward;

            //Place tooltip display pane at a set distance from the hud camera
            Plane tooltipDisplayPlane = new Plane(hudCameraForward, hudCameraPosition + (hudCameraForward * 0.6f));

            //Get target display position
            Vector3 targetDisplayPosition = windfallTooltip.displayPosition;

            //Get hud display direction
            Vector3 hudDisplayDirection = (targetDisplayPosition - hudCameraPosition).normalized;

            //Target display direction
            Vector3 tagetDisplayDirection = hudDisplayDirection;

            Vector3 cameraOffset = Vector3.zero;

            //Use main camera to determine tooltip object direction if the tooltip object is not part of the hud and the tooltip is not displaying at the mouse
            if (windfallTooltip.gameObject.layer != 5 && !windfallTooltip.displayAtMouse)
            {
                //Main Camera
                Camera mainCamera;
                if (WindfallHelper.app.view.gamblingView == null)
                {
                    //Main Camera
                    mainCamera = WindfallHelper.app.view.mainCamera.cam;
                }
                else
                {
                    //Gambling Camera
                    mainCamera = WindfallHelper.app.view.gamblingView.gamblingCameraView.GetComponent<Camera>();
                }
                cameraOffset = mainCamera.GetComponent<CameraView>().perspective == CameraView.PerspectiveType.Full ? new Vector3(0.2f, 0f, 0f) : Vector3.zero;

                Vector3 mainTargetLocal = mainCamera.transform.InverseTransformPoint(targetDisplayPosition);
                Vector3 hudTargetGlobal = hudCamera.transform.TransformPoint(mainTargetLocal);

                tagetDisplayDirection = (hudTargetGlobal - hudCameraPosition).normalized;
            }

            //Cast a ray through to the target position and place the tooltip at the intersection point on the plane
            Ray targetDisplayRay = new Ray(hudCameraPosition, tagetDisplayDirection);
            if (tooltipDisplayPlane.Raycast(targetDisplayRay, out float enter))
            {
                //Move tooltip
                tooltip.transform.position = targetDisplayRay.GetPoint(enter) + cameraOffset;

                //Apply anchor offset
                anchor.localPosition = AnchorOffset(windfallTooltip);

                //Constrain tooltip to camera view
                MeshRenderer toolipBack = ActiveTooltipBack();
                if (toolipBack != null)
                {
                    ConstrainTooltipToCamera(hudCamera, toolipBack, tooltipDisplayPlane);
                }

                ShowTooltip(true, true);
            }
            else
            {
                ShowTooltip(false, false);
            }
        }

        private static Sequence showTooltipAnimation;
        private static readonly float SHOW_TOOLTIP_TWEEN_DURATION = 0.12f;
        private static void ShowTooltip(bool show, bool animate)
        {
            if (tooltip == null || tooltip.transform == null) return;

            if (show == tooltipShowing) return;
            tooltipShowing = show;

            if (showTooltipAnimation != null && showTooltipAnimation.IsPlaying())
            {
                showTooltipAnimation.Kill(false);
            }

            if (show) tooltip.SetActive(true);

            Vector3 scale = show ? TooltipScale() : new Vector3(0f, 0f, 0f);

            showTooltipAnimation = DOTween.Sequence();
            showTooltipAnimation.Append(tooltip.transform.DOScale(scale, SHOW_TOOLTIP_TWEEN_DURATION).SetEase(Ease.InOutQuad));

            if (!show) showTooltipAnimation.AppendCallback(delegate { tooltip.SetActive(false); });
        }

        private static MeshRenderer ActiveTooltipBack()
        {
            foreach (MeshRenderer meshRenderer in tooltip.GetComponentsInChildren<MeshRenderer>(false))
            {
                if (meshRenderer.gameObject.name.Contains("Tooltip"))
                {
                    return meshRenderer;
                }
            }

            return null;
        }

        //Constrains tooltip position to camera view
        private static void ConstrainTooltipToCamera(Camera camera, MeshRenderer meshRenderer, Plane tooltipDisplayPlane)
        {
            //Calculate the global positions of the sides of the tooltip back
            List<Vector3> boundsSides = RenderedMeshSidePositionsGlobal(meshRenderer, true, false, true);

            Vector2 pixelOffset = new Vector2();

            //Find the tooltip sides that are furthest from the center of the screen
            for (int sideIterator = 0; sideIterator < boundsSides.Count; sideIterator++)
            {
                Vector3 sideScreenPoint3D = camera.WorldToScreenPoint(boundsSides[sideIterator]);
                Vector2 sideScreenPoint = new Vector2(sideScreenPoint3D.x, sideScreenPoint3D.y);

                Vector2 sidePixelOffset = PixelOffsetIntoCameraView(camera, sideScreenPoint);

                //Track the horizontal offset needed to move the furthest horizontal edge into the camera view
                if (Math.Abs(sidePixelOffset.x) > Math.Abs(pixelOffset.x))
                {
                    pixelOffset.x = sidePixelOffset.x;
                }

                //Track the vetical offset needed to move the furthest vetical edge into the camera view
                if (Math.Abs(sidePixelOffset.y) > Math.Abs(pixelOffset.y))
                {
                    pixelOffset.y = sidePixelOffset.y;
                }
            }

            //Abort if the tooltip is within the camera view already and does not need to be moved
            if (pixelOffset.x == 0 && pixelOffset.y == 0)
            {
                return;
            }

            Vector3 tooltipScreenPoint = camera.WorldToScreenPoint(tooltip.transform.position);
            tooltipScreenPoint.x += pixelOffset.x;
            tooltipScreenPoint.y += pixelOffset.y;

            Ray ray = camera.ScreenPointToRay(new Vector2(tooltipScreenPoint.x, tooltipScreenPoint.y));
            if (tooltipDisplayPlane.Raycast(ray, out float enter))
            {
                tooltip.transform.position = ray.GetPoint(enter);
            }
        }

        //Returns an array containing the global positions of the center of all six faces of the object's local rendered mesh bounds
        private static List<Vector3> RenderedMeshSidePositionsGlobal(MeshRenderer meshRenderer, bool includeX = true, bool includeY = true, bool includeZ = true)
        {
            MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null) return null;

            Mesh mesh = meshFilter.mesh;
            if (mesh == null) return null;

            Bounds meshBounds = mesh.bounds;

            List<Vector3> boundsSides = new List<Vector3>();
            if (includeX)
            {
                boundsSides.Add(meshRenderer.transform.TransformPoint(meshBounds.center + new Vector3(meshBounds.extents.x, 0f, 0f)));
                boundsSides.Add(meshRenderer.transform.TransformPoint(meshBounds.center + new Vector3(-meshBounds.extents.x, 0f, 0f)));
            }
            if (includeY)
            {
                boundsSides.Add(meshRenderer.transform.TransformPoint(meshBounds.center + new Vector3(0f, meshBounds.extents.y, 0f)));
                boundsSides.Add(meshRenderer.transform.TransformPoint(meshBounds.center + new Vector3(0f, -meshBounds.extents.y, 0f)));
            }
            if (includeZ)
            {
                boundsSides.Add(meshRenderer.transform.TransformPoint(meshBounds.center + new Vector3(0f, 0f, meshBounds.extents.z)));
                boundsSides.Add(meshRenderer.transform.TransformPoint(meshBounds.center + new Vector3(0f, 0f, -meshBounds.extents.z)));
            }

            return boundsSides;
        }

        //Returns pixel offset needed to move a screen point into the camera view
        private static Vector2 PixelOffsetIntoCameraView(Camera camera, Vector2 screenPoint)
        {
            Vector2 pixelOffset = new Vector2();

            if (screenPoint.x < 0)
            {
                pixelOffset.x = -screenPoint.x;
            }
            else if (screenPoint.x > camera.pixelWidth)
            {
                pixelOffset.x = -(screenPoint.x - camera.pixelWidth);
            }

            if (screenPoint.y < 0)
            {
                pixelOffset.y = -screenPoint.y;
            }
            else if (screenPoint.y > camera.pixelHeight)
            {
                pixelOffset.y = -(screenPoint.y - camera.pixelHeight);
            }

            return pixelOffset;
        }

        private static void ResizeTooltipAndSetLabelText(WindfallTooltip windfallTooltip)
        {
            if (labels == null || labels.Count < 1)
            {
                return;
            }
            if (hiddenLabel == null)
            {
                return;
            }

            int linecount = -1;
            if (windfallTooltip != null && windfallTooltip.displayDescription != null)
            {
                //Remove underline unless the language is English
                if (LocalizationManager.CurrentLanguage != "English") windfallTooltip.displayDescription = windfallTooltip.displayDescription.Replace("<u>", "").Replace("</u>", "");

                hiddenLabel.SetText(windfallTooltip.displayDescription);
                hiddenLabel.ForceMeshUpdate();
                TMP_TextInfo textInfo = hiddenLabel.textInfo;
                linecount = textInfo.lineCount;
            }

            for (int labelCounter = 0; labelCounter < labels.Count; labelCounter++)
            {
                GameObject tooltipBack = labels[labelCounter].transform.parent.gameObject;

                bool active = false;
                if (linecount >= 0 && (tooltipBack.name.Contains(linecount.ToString()) || linecount >= 5 && tooltipBack.name.Contains("5")))
                {
                    active = true;
                }
                tooltipBack.SetActive(active);
                MeshRenderer meshRenderer = tooltipBack.GetComponent<MeshRenderer>();
                meshRenderer.enabled = active;

                labels[labelCounter].SetText(hiddenLabel.text);
            }
        }

        private static readonly float anchorOffsetDistance = 0.1f;
        private static Vector3 AnchorOffset(WindfallTooltip windfallTooltip)
        {
            Vector3 offset;

            GameObject tooltipBack = null;
            foreach (TextMeshPro textMeshPro in labels)
            {
                GameObject currentTooltipBack = textMeshPro.transform.parent.gameObject;
                if (currentTooltipBack.activeSelf)
                {
                    tooltipBack = currentTooltipBack;
                }
            }

            MeshRenderer meshRenderer = tooltipBack?.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
            {
                return Vector3.zero;
            }

            float width;
            float height;
            if (meshRenderer != null)
            {
                width = meshRenderer.bounds.size.x * 0.46f;
                height = meshRenderer.bounds.size.y * 0.54f;
            }
            else
            {
                width = anchorOffsetDistance;
                height = anchorOffsetDistance;
            }

            switch (windfallTooltip.displayAnchor)
            {
                case WindfallTooltip.Anchor.Top:
                    offset = new Vector3(0f, height, 0f);
                    break;
                case WindfallTooltip.Anchor.TopRight:
                    offset = new Vector3(width, height, 0f);
                    break;
                case WindfallTooltip.Anchor.Right:
                    offset = new Vector3(width, 0f, 0f);
                    break;
                case WindfallTooltip.Anchor.BottomRight:
                    offset = new Vector3(width, -height, 0f);
                    break;
                case WindfallTooltip.Anchor.Bottom:
                    offset = new Vector3(0f, -height, 0f);
                    break;
                case WindfallTooltip.Anchor.BottomLeft:
                    offset = new Vector3(-width, -height, 0f);
                    break;
                case WindfallTooltip.Anchor.Left:
                    offset = new Vector3(-width, 0f, 0f);
                    break;
                case WindfallTooltip.Anchor.TopLeft:
                    offset = new Vector3(-width, height, 0f);
                    break;
                default:
                    offset = Vector3.zero;
                    break;
            }

            return offset;
        }

        private static readonly string tooltipPath = "Tooltip Base";
        private static GameObject CreateTooltip()
        {
            if (WindfallHelper.app == null)
            {
                return null;
            }

            if (Windfall.assetBundle == null || !Windfall.assetBundle.Contains(tooltipPath))
            {
                return null;
            }

            Transform tooltipTransform = WindfallHelper.ResetShader(UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>(tooltipPath), WindfallHelper.app.view.GUICamera.transform.Find("HUD")).transform);

            anchor = tooltipTransform.Find("Anchor");

            hiddenLabel = anchor.Find("Hidden Label").GetComponent<TextMeshPro>();
            LocalizationModifier.ChangeFont(null, hiddenLabel, WindfallHelper.GetEdmundMcmillenFont());
            WindfallHelper.LocalizeObject(hiddenLabel.gameObject, null);

            labels = tooltipTransform.GetComponentsInChildren<TextMeshPro>(true).ToList();
            if (labels.Contains(hiddenLabel)) labels.Remove(hiddenLabel);

            foreach (TextMeshPro textMeshPro in labels)
            {
                LocalizationModifier.ChangeFont(null, textMeshPro, WindfallHelper.GetEdmundMcmillenFont());
                WindfallHelper.LocalizeObject(textMeshPro.gameObject, null);
            }

            ShowTooltip(false, false);

            return tooltipTransform.gameObject;
        }

        private static Vector3 TooltipScale()
        {
            //Scale tooltips according to user settings
            int tooltipSize = WindfallPersistentDataController.LoadData().tooltipSize;
            float tooltipScale = SCALE_SMALL;
            switch (tooltipSize)
            {
                case -1:
                    tooltipScale = SCALE_SMALL;
                    break;
                case 0:
                    tooltipScale = SCALE_MEDIUM;
                    break;
                case 1:
                    tooltipScale = SCALE_LARGE;
                    break;
            }

            return new Vector3(tooltipScale, tooltipScale, tooltipScale * 0.5f);
        }

        private static void ClearEnemyTints(WindfallTooltip tooltipToShow)
        {
            if (WindfallHelper.app?.model?.enemies == null)
            {
                return;
            }

            foreach (Enemy enemy in WindfallHelper.app.model.enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                ObjectTinter objectTinter = enemy.objectTinter;
                if (objectTinter == null)
                {
                    continue;
                }
                if (objectTinter.tintColor != WindfallTooltip.enemyHoverTintColor)
                {
                    continue;
                }

                if (tooltipToShow != null)
                {
                    Enemy tooltipEnemy = tooltipToShow.gameObject.GetComponent<Enemy>();
                    if (tooltipEnemy == null)
                    {
                        tooltipEnemy = ObjectDataStorage.GetData<Enemy>(tooltipToShow.gameObject, EntityChanges.colliderEnemyKey);
                    }

                    if (tooltipEnemy != null && enemy == tooltipEnemy)
                    {
                        continue;
                    }
                }

                objectTinter.NoTint();
            }
        }

        private static void DisplayGridView(WindfallTooltip tooltipToShow)
        {
            //Show battlefield grid
            List<Vector2Int> enemyBattlefieldPositionsVector = new List<Vector2Int>();

            if (tooltipToShow != null)
            {
                Enemy tooltipEnemy = tooltipToShow.gameObject.GetComponent<Enemy>();
                if (tooltipEnemy == null)
                {
                    tooltipEnemy = ObjectDataStorage.GetData<Enemy>(tooltipToShow.gameObject, EntityChanges.colliderEnemyKey);
                }

                if (tooltipEnemy != null)
                {
                    List<BattlefieldPosition> enemyBattlefieldPositions = new List<BattlefieldPosition>();
                    BattlefieldPosition enemyBattlefieldPosition = WindfallHelper.app.model.aiModel.battlefieldPositions[WindfallHelper.app.model.aiModel.battlefieldPositionIndex[tooltipEnemy.position.x, tooltipEnemy.position.y]];
                    enemyBattlefieldPositions.Add(enemyBattlefieldPosition);
                    if (tooltipEnemy.enemyWidth == 3)
                    {
                        enemyBattlefieldPositions.AddRange(WindfallHelper.AdjacentBattlefieldPositions(WindfallHelper.app.model.aiModel, enemyBattlefieldPosition, false, true, false));
                    }

                    foreach (BattlefieldPosition battlefieldPosition in enemyBattlefieldPositions)
                    {
                        enemyBattlefieldPositionsVector.Add(new Vector2Int(battlefieldPosition.x, battlefieldPosition.y));
                    }
                }
            }

            BattlefieldGridView.ShowGrid(enemyBattlefieldPositionsVector);
        }
    }

    public static class WindfallTooltipDescriptions
    {

        public static Dictionary<CharacterSheet.BumboType, string> BumboNames
        {
            get
            {
                return new Dictionary<CharacterSheet.BumboType, string>
                {
                    { CharacterSheet.BumboType.TheBrave, "BRAVE_NAME" },
                    { CharacterSheet.BumboType.TheNimble, "NIMBLE_NAME" },
                    { CharacterSheet.BumboType.TheStout, "STOUT_NAME" },
                    { CharacterSheet.BumboType.TheWeird, "WEIRD_NAME" },
                    { CharacterSheet.BumboType.TheDead, "DEAD_NAME" },
                    { CharacterSheet.BumboType.TheLost, "LOST_NAME" },
                    { CharacterSheet.BumboType.Eden, "EMPTY_NAME" },
                    { (CharacterSheet.BumboType) 10, "WISE_NAME" },
                };
            }
        }

        public static readonly string WISE_DESCRIPTION = "WISE_TOOLTIP_DESCRIPTION";
        public static Dictionary<CharacterSheet.BumboType, string> BumboDescriptions
        {
            get
            {
                return new Dictionary<CharacterSheet.BumboType, string>
                {
                    { CharacterSheet.BumboType.TheBrave, "BRAVE_TOOLTIP_DESCRIPTION" },
                    { CharacterSheet.BumboType.TheNimble, "NIMBLE_TOOLTIP_DESCRIPTION" },
                    { CharacterSheet.BumboType.TheStout, "STOUT_TOOLTIP_DESCRIPTION" },
                    { CharacterSheet.BumboType.TheWeird, "WEIRD_TOOLTIP_DESCRIPTION" },
                    { CharacterSheet.BumboType.TheDead, "DEAD_TOOLTIP_DESCRIPTION" },
                    { CharacterSheet.BumboType.TheLost, "LOST_TOOLTIP_DESCRIPTION" },
                    { CharacterSheet.BumboType.Eden, "EMPTY_TOOLTIP_DESCRIPTION" },
                    { (CharacterSheet.BumboType) 10, WISE_DESCRIPTION },
                };
            }
        }

        public static string SpellDescriptionWithValues(SpellElement spell)
        {
            string tooltipDescriptionTerm = spell.Name;
            int lastUnderscoreIndex = tooltipDescriptionTerm.LastIndexOf("_");
            tooltipDescriptionTerm = tooltipDescriptionTerm.Substring(0, lastUnderscoreIndex);
            tooltipDescriptionTerm = tooltipDescriptionTerm + "_TOOLTIP_DESCRIPTION";

            string value = LocalizationModifier.GetLanguageText(tooltipDescriptionTerm, "Spells");

            if (value != string.Empty)
            {
                CharacterSheet characterSheet = WindfallHelper.app?.model?.characterSheet;
                switch (spell.spellName)
                {
                    case (SpellName)1000:
                        if (spell is PlasmaBallSpell plasmaBallSpell)
                        {
                            value = value.Replace("[spread]", plasmaBallSpell.ChainDistance().ToString());
                        }
                        break;
                    case SpellName.BrownBelt:
                        if (characterSheet != null)
                        {
                            value = value.Replace("[damage]", characterSheet.getItemDamage().ToString());
                        }
                        break;
                    case SpellName.D10:
                        string grounded = string.Empty;
                        string flying = string.Empty;
                        if (characterSheet != null)
                        {
                            switch (characterSheet.currentFloor)
                            {
                                case 2:
                                    grounded = LocalizationModifier.GetLanguageText("D10_TOOLTIP_GROUNDED_2", "Spells");
                                    flying = LocalizationModifier.GetLanguageText("D10_TOOLTIP_FLYING_2", "Spells");
                                    break;
                                case 3:
                                    grounded = LocalizationModifier.GetLanguageText("D10_TOOLTIP_GROUNDED_3", "Spells");
                                    flying = LocalizationModifier.GetLanguageText("D10_TOOLTIP_FLYING_3", "Spells");
                                    break;
                                default:
                                    grounded = LocalizationModifier.GetLanguageText("D10_TOOLTIP_GROUNDED_1", "Spells");
                                    flying = LocalizationModifier.GetLanguageText("D10_TOOLTIP_FLYING_1", "Spells");
                                    break;
                            }
                        }
                        value = value.Replace("[grounded]", grounded);
                        value = value.Replace("[flying]", flying);
                        break;
                    case SpellName.Euthanasia:
                        value = value.Replace("[damage]", "5");
                        break;
                    case SpellName.ExorcismKit:
                        string healing = "1";
                        value = value.Replace("[healing]", healing);
                        break;
                    case SpellName.RockFriends:
                        if (characterSheet != null)
                        {
                            int itemDamage = characterSheet.getItemDamage() + 1;
                            value = value.Replace("[count]", itemDamage.ToString());

                            //Handle English pluralization
                            if (value == "1" && LocalizationManager.CurrentLanguage == "English") value = value.Replace("rocks on random enemies, each", "rock on a random enemy,");
                        }
                        break;
                }

                value = value.Replace("[damage]", spell.Damage().ToString());
                value = value.Replace("[stacking]", CollectibleChanges.PercentSpellEffectStackingCap(spell.spellName).ToString());
                return value;
            }
            return string.Empty;
        }

        public static string TrinketDescriptionWithValues(TrinketElement trinket)
        {
            string tooltipDescriptionTerm = trinket.Name;
            int lastUnderscoreIndex = tooltipDescriptionTerm.LastIndexOf("_");
            tooltipDescriptionTerm = tooltipDescriptionTerm.Substring(0, lastUnderscoreIndex);
            tooltipDescriptionTerm = tooltipDescriptionTerm + "_TOOLTIP_DESCRIPTION";

            string value = LocalizationModifier.GetLanguageText(tooltipDescriptionTerm, "Trinkets");

            if (value != null) return value;
            return string.Empty;
        }

        public static string EnemyDisplayName(Enemy enemy)
        {
            if (enemy == null) { return string.Empty; }

            //Get boss
            Boss boss = null;
            if (enemy is Boss) boss = enemy as Boss;

            //Enemy names
            string enemyNameText = string.Empty;
            if (enemyNameText == string.Empty)
            {
                //Enemy names from name
                if (EnemyDisplayNamesByEnemyName.TryGetValue(enemy.enemyName, out string enemyNameFromName)) enemyNameText = enemyNameFromName;
            }
            if (enemyNameText == string.Empty)
            {
                //Boss names from name
                if (boss != null && BossDisplayNamesByBossName.TryGetValue((enemy as Boss).bossName, out string bossNameFromName)) enemyNameText = bossNameFromName;
            }
            if (enemyNameText == string.Empty)
            {
                //Enemy and Boss names from type
                if (EnemyDisplayNamesByType.TryGetValue(enemy.GetType(), out string enemyNameFromType)) enemyNameText = enemyNameFromType;
            }

            //Get Flipper
            if (enemy is FlipperEnemy) enemyNameText = enemy.attackImmunity == Enemy.AttackImmunity.ReducePuzzleDamage ? "NIB_NAME" : "JIB_NAME";

            //Get Bygone Ghost
            if (enemy.gameObject.name.Contains("Bygone Ghost")) enemyNameText = "BYGONE_GHOST_NAME";

            return enemyNameText;
        }
        private static Dictionary<EnemyName, string> EnemyDisplayNamesByEnemyName
        {
            get
            {
                return new Dictionary<EnemyName, string>
                {
                    { EnemyName.Arsemouth, "TALL_BOY_NAME" },
                    { EnemyName.BlackBlobby, "BLACK_BLOBBY_NAME" },
                    { EnemyName.Blib, "BLIB_NAME" },
                    { EnemyName.BlueBoney, "SKULLY_B_NAME" },
                    { EnemyName.BoomFly, "BOOM_FLY_NAME" },
                    { EnemyName.Burfer, "BURFER_NAME" },
                    { EnemyName.Butthead, "SQUAT_NAME" },
                    { EnemyName.CornyDip, "CORN_DIP_NAME" },
                    { EnemyName.Curser, "CURSER_NAME" },
                    { EnemyName.DigDig, "DIG_DIG_NAME" },
                    { EnemyName.Dip, "DIP_NAME" },
                    { EnemyName.Flipper, "FLIPPER_NAME" },//Missing
                    { EnemyName.FloatingCultist, "FLOATER_NAME" },
                    { EnemyName.Fly, "FLY_NAME" },
                    { EnemyName.Greedling, "GREEDLING_NAME" },
                    { EnemyName.GreenBlib, "GREEN_BLIB_NAME" },
                    { EnemyName.GreenBlobby, "GREEN_BLOBBY_NAME" },
                    { EnemyName.Hanger, "KEEPER_NAME" },
                    { EnemyName.Hopper, "LEAPER_NAME" },
                    { EnemyName.Host, "HOST_NAME" },
                    //{ EnemyName.Imposter, "IMPOSTER_NAME" },
                    { EnemyName.Isaacs, "ISAAC_NAME" },
                    { EnemyName.Larry, "LARRY_NAME" },
                    { EnemyName.Leechling, "SUCK_NAME" },
                    { EnemyName.Longit, "LONGITS_NAME" },
                    { EnemyName.ManaWisp, "MANA_WISP_NAME" },
                    { EnemyName.MaskedImposter, "MASK_NAME" },
                    { EnemyName.MeatGolem, "MEAT_GOLUM_NAME" },
                    { EnemyName.MegaPoofer, "MEGA_POOFER_NAME" },
                    { EnemyName.MirrorHauntLeft, "MIRROR_NAME" },
                    { EnemyName.MirrorHauntRight, "MIRROR_NAME" },
                    { EnemyName.PeepEye, "PEEPER_EYE_NAME" },
                    { EnemyName.Poofer, "POOFER_NAME" },
                    { EnemyName.Pooter, "POOTER_NAME" },
                    { EnemyName.PurpleBoney, "SKULLY_P_NAME" },
                    { EnemyName.RedBlobby, "RED_BLOBBY_NAME" },
                    { EnemyName.RedCultist, "RED_FLOATER_NAME" },
                    { EnemyName.Screecher, "SCREECHER_NAME" },
                    { EnemyName.Shit, "POOP_NAME" },
                    { EnemyName.Spookie, "SPOOKIE_NAME" },
                    { EnemyName.Stone, "ROCK_NAME" },
                    { EnemyName.Stony, "STONY_NAME" },
                    { EnemyName.Sucker, "SUCKER_NAME" },
                    { EnemyName.Tader, "DADDY_TATO_NAME" },
                    { EnemyName.Tado, "TATO_KID_NAME" },
                    { EnemyName.TaintedPeepEye, "TAINTED_PEEPER_EYE_NAME" },
                    { EnemyName.Tutorial, "KEEPER_NAME" },
                    { EnemyName.WalkingCultist, "CULTIST_NAME" },
                    { EnemyName.WillOWisp, "WHISP_NAME" },
                };
            }
        }
        private static Dictionary<BossName, string> BossDisplayNamesByBossName
        {
            get
            {
                return new Dictionary<BossName, string>
                {
                    { BossName.Bygone, "BYGONE_BODY_NAME" },
                    { BossName.Duke, "DUKE_NAME" },
                    { BossName.Dusk, "DUSK_NAME" },
                    { BossName.Gibs, "GIBS_NAME" },
                    { BossName.Gizzarda, "GIZZARDA_NAME" },
                    { BossName.Loaf, "LOAF_NAME" },
                    { BossName.Peeper, "PEEPER_NAME" },
                    { BossName.Pyre, "PYRE_NAME" },
                    { BossName.Sangre, "SANGRE_NAME" },
                    { BossName.ShyGal, "SHY_GALS_NAME" },
                    { BossName.TaintedDusk, "TAINTED_DUSK_NAME" },
                    { BossName.TaintedPeeper, "TAINTED_PEEPER_NAME" },
                    { BossName.TaintedShyGal, "TAINTED_SHY_GALS_NAME" },
                };
            }
        }
        private static Dictionary<Type, string> EnemyDisplayNamesByType
        {
            get
            {
                return new Dictionary<Type, string>
                {
                    //Enemies
                    { typeof(ArsemouthEnemy), "TALL_BOY_NAME" },
                    { typeof(BlackBlobbyEnemy), "BLACK_BLOBBY_NAME" },
                    { typeof(BlibEnemy), "BLIB_NAME" },
                    { typeof(BlueBoneyEnemy), "SKULLY_B_NAME" },
                    { typeof(BoomFlyEnemy), "BOOM_FLY_NAME" },
                    { typeof(BurferEnemy), "BURFER_NAME" },
                    { typeof(ButtheadEnemy), "SQUAT_NAME" },
                    //CornyDip
                    { typeof(CurserEnemy), "CURSER_NAME" },
                    { typeof(DigDigEnemy), "DIG_DIG_NAME" },
                    { typeof(DipEnemy), "DIP_NAME" },
                    { typeof(FlipperEnemy), "Flipper" },
                    { typeof(FloatingCultistEnemy), "FLOATER_NAME" },
                    { typeof(FlyEnemy), "FLY_NAME" },
                    { typeof(GreedlingEnemy), "GREEDLING_NAME" },
                    //GreenBlib
                    { typeof(GreenBlobbyEnemy), "GREEN_BLOBBY_NAME" },
                    { typeof(HangerEnemy), "KEEPER_NAME" },
                    { typeof(HopperEnemy), "LEAPER_NAME" },
                    { typeof(HostEnemy), "HOST_NAME" },
                    { typeof(ImposterEnemy), "IMPOSTER_NAME" },
                    { typeof(IsaacsEnemy), "ISAAC_NAME" },
                    { typeof(LarryEnemy), "LARRY_NAME" },
                    { typeof(LeecherEnemy), "SUCK_NAME" },
                    { typeof(LongitEnemy), "LONGITS_NAME" },
                    { typeof(ManaWispEnemy), "MANA_WISP_NAME" },
                    { typeof(MaskedImposterEnemy), "MASK_NAME" },
                    { typeof(MeatGolemEnemy), "MEAT_GOLUM_NAME" },
                    { typeof(MegaPooferEnemy), "MEGA_POOFER_NAME" },
                    { typeof(MirrorHauntEnemy), "MIRROR_NAME" },
                    { typeof(PeepEyeEnemy), "PEEPER_EYE_NAME" },
                    { typeof(PooferEnemy), "POOFER_NAME" },
                    { typeof(PooterEnemy), "POOTER_NAME" },
                    { typeof(PurpleBoneyEnemy), "SKULLY_P_NAME" },
                    { typeof(RedBlobbyEnemy), "RED_BLOBBY_NAME" },
                    { typeof(RedCultistEnemy), "RED_FLOATER_NAME" },
                    { typeof(ScreecherEnemy), "SCREECHER_NAME" },
                    { typeof(ShitEnemy), "POOP_NAME" },
                    { typeof(SpookieEnemy), "SPOOKIE_NAME" },
                    { typeof(StoneEnemy), "ROCK_NAME" },
                    { typeof(StonyEnemy), "STONY_NAME" },
                    { typeof(SuckerEnemy), "SUCKER_NAME" },
                    { typeof(TaderEnemy), "DADDY_TATO_NAME" },
                    { typeof(TadoEnemy), "TATO_KID_NAME" },
                    //TaintedPeepEye
                    { typeof(TutorialEnemy), "KEEPER_NAME" },
                    { typeof(WalkingCultistEnemy), "CULTIST_NAME" },
                    { typeof(WilloWispEnemy), "WHISP_NAME" },

                    //Bosses
                    { typeof(BygoneBoss), "BYGONE_BODY_NAME" },
                    { typeof(BygoneGhostBoss), "BYGONE_GHOST_NAME" },
                    { typeof(DukeBoss), "DUKE_NAME" },
                    { typeof(DuskBoss), "DUSK_NAME" },
                    { typeof(GibsBoss), "GIBS_NAME" },
                    { typeof(GizzardaBoss), "GIZZARDA_NAME" },
                    { typeof(LoafBoss), "LOAF_NAME" },
                    { typeof(PeepsBoss), "PEEPER_NAME" },
                    { typeof(PyreBoss), "PYRE_NAME" },
                    { typeof(CaddyBoss), "SANGRE_NAME" },
                    { typeof(ShyGalBoss), "SHY_GALS_NAME" },
                    { typeof(TaintedDuskBoss), "TAINTED_DUSK_NAME" },
                    //TaintedPeeper
                    //TaintedShyGal
                };
            }
        }

        public static string EnemyDisplayDescription(Enemy enemy)
        {
            string localizationCategory = enemy is Boss ? "Bosses" : "Enemies";
            if (EnemyDescriptions.TryGetValue(EnemyDisplayName(enemy), out string value)) { return "\n" + LocalizationModifier.GetLanguageText(value, localizationCategory); }
            return string.Empty;
        }
        private static Dictionary<string, string> EnemyDescriptions
        {
            get
            {
                return new Dictionary<string, string>
                {
                    //Enemies
                    { "BLACK_BLOBBY_NAME", "BLACK_BLOBBY_ABILITY" },
                    { "BOOM_FLY_NAME", "BOOM_FLY_ABILITY" },
                    { "CULTIST_NAME", "CULTIST_ABILITY" },
                    { "DADDY_TATO_NAME", "DADDY_TATO_ABILITY" },
                    { "DIG_DIG_NAME", "DIG_DIG_ABILITY" },
                    { "GREEDLING_NAME", "GREEDLING_ABILITY" },
                    { "ISAAC_NAME", "ISAAC_ABILITY" },
                    { "JIB_NAME", "JIB_ABILITY" },
                    { "LARRY_NAME", "LARRY_ABILITY" },
                    { "LONGITS_NAME", "LONGITS_ABILITY" },
                    { "MANA_WISP_NAME", "MANA_WISP_ABILITY" },
                    { "MEAT_GOLUM_NAME", "MEAT_GOLUM_ABILITY" },
                    { "MEGA_POOFER_NAME", "MEGA_POOFER_ABILITY" },
                    { "NIB_NAME", "NIB_ABILITY" },
                    { "POOFER_NAME", "POOFER_ABILITY" },
                    { "RED_FLOATER_NAME", "RED_FLOATER_ABILITY" },
                    { "SPOOKIE_NAME", "SPOOKIE_ABILITY" },
                    { "SUCKER_NAME", "SUCKER_ABILITY" },
                    { "TATO_KID_NAME", "TATO_KID_ABILITY" },

                    //Bosses
                    { "BYGONE_BODY_NAME", "BYGONE_BODY_ABILITY" },
                    { "BYGONE_GHOST_NAME", "BYGONE_GHOST_ABILITY" },
                    { "DUSK_NAME", "DUSK_ABILITY" },
                    { "GIBS_NAME", "GIBS_ABILITY" },
                    { "GIZZARDA_NAME", "GIZZARDA_ABILITY" },
                    { "LOAF_NAME", "LOAF_ABILITY" },
                    { "PYRE_NAME", "PYRE_ABILITY" },
                    { "TAINTED_PEEPER_NAME", "TAINTED_PEEPER_ABILITY" },
                    { "TAINTED_DUSK_NAME", "TAINTED_DUSK_ABILITY" },
                };
            }
        }

        public static string EnemyDamageReductionWithValues(Enemy enemy)
        {
            if (DamageReductionEnemies.Contains(enemy.GetType()))
            {
                int damageReduction = 1;

                if (enemy is DukeBoss)
                {
                    DukeBoss dukeBoss = enemy as DukeBoss;
                    DukeBoss.DukeSize dukeSize = (DukeBoss.DukeSize)AccessTools.Field(typeof(DukeBoss), "dukeSize").GetValue(dukeBoss);

                    if (dukeSize == DukeBoss.DukeSize.Large) damageReduction = 1;
                    else if (dukeSize == DukeBoss.DukeSize.Medium) damageReduction = 2;
                    else return string.Empty;
                }

                string damageReductionWithValue = LocalizationModifier.GetLanguageText("DAMAGE_REDUCTION_ABILITY", "Enemies");
                damageReductionWithValue = damageReductionWithValue.Replace("[damage]", damageReduction.ToString());
                return "\n" + damageReductionWithValue;
            }
            return string.Empty;
        }

        public static List<Type> DamageReductionEnemies
        {
            get
            {
                return new List<Type>
                {
                    //Enemies
                    typeof(SpookieEnemy),

                    //Bosses
                    typeof(BygoneGhostBoss),
                    typeof(DukeBoss),
                };
            }
        }

        public static List<EnemyName> NonAttackingEnemies
        {
            get
            {
                return new List<EnemyName>
                {
                    EnemyName.Arsemouth,
                    EnemyName.Curser,
                    EnemyName.FloatingCultist,
                    EnemyName.TaintedPeepEye,
                    EnemyName.Screecher,
                    EnemyName.Sucker,
                };
            }
        }

        public static bool EnemyIsBlocking(Enemy enemy)
        {
            bool block = false;

            //Blobby passive ability
            if (enemy is BlackBlobbyEnemy)
            {
                BlackBlobbyEnemy blackBlobbyEnemy = (BlackBlobbyEnemy)enemy;
                if (blackBlobbyEnemy.healthState == BlackBlobbyEnemy.HealthState.full) { block = true; }
            }
            if (enemy is GreenBlobbyEnemy)
            {
                GreenBlobbyEnemy greenBlobbyEnemy = (GreenBlobbyEnemy)enemy;
                if (greenBlobbyEnemy.healthState == GreenBlobbyEnemy.HealthState.full) { block = true; }
            }
            if (enemy is RedBlobbyEnemy)
            {
                RedBlobbyEnemy redBlobbyEnemy = (RedBlobbyEnemy)enemy;
                if (redBlobbyEnemy.healthState == RedBlobbyEnemy.HealthState.full) { block = true; }
            }

            //Shy gal mask
            if (enemy is ShyGalBoss)
            {
                ShyGalBoss shyGalBoss = (ShyGalBoss)enemy;
                if (!(bool)AccessTools.Field(typeof(ShyGalBoss), "is_dumb").GetValue(shyGalBoss) && (shyGalBoss.isMasked || !shyGalBoss.boss)) { block = true; }
            }

            //Red bubble shield
            if (WindfallHelper.app.model.aiModel.battlefieldEffects[WindfallHelper.app.model.aiModel.battlefieldPositionIndex[enemy.position.x, enemy.position.y]].effect == BattlefieldEffect.Effect.Shield)
            {
                block = true;
            }

            return block;
        }

        public static bool EnemyIsInvincible(Enemy enemy)
        {
            if (enemy.attackImmunity == Enemy.AttackImmunity.SuperAttack)
            {
                return true;
            }

            bool invincible = false;
            if (PotentiallyInvincibleEnemies.Contains(EnemyDisplayName(enemy)))
            {
                invincible = true;

                //Host skull
                if (enemy is HostEnemy)
                {
                    HostEnemy hostEnemy = (HostEnemy)enemy;
                    if (!hostEnemy.IsClosed()) { invincible = false; }
                }

                //Sangre chest
                if (enemy is CaddyBoss)
                {
                    CaddyBoss caddyBoss = (CaddyBoss)enemy;
                    if (caddyBoss.IsChestOpen()) { invincible = false; }
                }
            }

            return invincible;
        }
        private static List<string> PotentiallyInvincibleEnemies
        {
            get
            {
                return new List<string>
                {
                    "MANA_WISP_NAME",
                    "HOST_NAME",
                    "PEEPER_EYE_NAME",
                    "SANGRE_NAME",
                    "STONY_NAME",
                    "TAINTED_PEEPER_EYE_NAME",
                };
            }
        }
    }
}
