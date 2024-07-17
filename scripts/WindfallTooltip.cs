using DG.Tweening;
using HarmonyLib;
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

                displayDescription = bumboModifierTemporary.description;
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
                    displayDescription = "<u>" + LocalizationModifier.GetEnglishText(spellKA, "Spells") + "</u>\n" + WindfallTooltipDescriptions.SpellDescriptionWithValues(spell);
                }
                return;
            }

            SpellViewIndicator spellViewIndicator = gameObject.GetComponent<SpellViewIndicator>();
            if (spellViewIndicator != null)
            {

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
                    displayDescription = "<u>" + LocalizationModifier.GetEnglishText(spellKA, "Spells") + "</u>\n" + WindfallTooltipDescriptions.SpellDescriptionWithValues(spell);
                }

                return;
            }

            TrinketView trinketView = gameObject.GetComponent<TrinketView>();
            if (trinketView != null && trinketView.trinketIndex < WindfallHelper.app.model.characterSheet.trinkets.Count)
            {
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
                    displayDescription = "<u>" + LocalizationModifier.GetEnglishText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
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
                        displayDescription = "<u>" + LocalizationModifier.GetEnglishText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
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
                    displayDescription = "<u>" + LocalizationModifier.GetEnglishText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
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
                if (WindfallTooltipDescriptions.BumboNames.TryGetValue(bumboType, out string name)) bumboName = name;
                if (WindfallTooltipDescriptions.BumboDescriptions.TryGetValue(bumboType, out string description)) bumboDescription = description;

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
                string enemyNameText = WindfallTooltipDescriptions.EnemyDisplayName(enemy);

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

            ResizeTooltip(windfallTooltip);

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

        private static void ResizeTooltip(WindfallTooltip windfallTooltip)
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

            labels = tooltipTransform.GetComponentsInChildren<TextMeshPro>(true).ToList();
            if (labels.Contains(hiddenLabel))
            {
                labels.Remove(hiddenLabel);
            }

            foreach (TextMeshPro textMeshPro in labels)
            {
                LocalizationModifier.ChangeFont(null, textMeshPro, WindfallHelper.GetEdmundMcmillenFont());
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
                    { CharacterSheet.BumboType.TheBrave, "Bum-bo the Brave" },
                    { CharacterSheet.BumboType.TheNimble, "Bum-bo the Nimble" },
                    { CharacterSheet.BumboType.TheStout, "Bum-bo the Stout" },
                    { CharacterSheet.BumboType.TheWeird, "Bum-bo the Weird" },
                    { CharacterSheet.BumboType.TheDead, "Bum-bo the Dead" },
                    { CharacterSheet.BumboType.TheLost, "Bum-bo the Lost" },
                    { CharacterSheet.BumboType.Eden, "Bum-bo the Empty" },
                    { (CharacterSheet.BumboType) 10, "Bum-bo the Wise" },
                };
            }
        }

        public static readonly string WISE_DESCRIPTION = "Turns tiles wild after moving them";
        public static Dictionary<CharacterSheet.BumboType, string> BumboDescriptions
        {
            get
            {
                return new Dictionary<CharacterSheet.BumboType, string>
                {
                    { CharacterSheet.BumboType.TheBrave, "Gains 1 spell damage and 1 puzzle damage while at or below 2 red health. Increases to 2 spell damage and 2 puzzle damage while at or below 1 red health" },
                    { CharacterSheet.BumboType.TheNimble, "Gains 1 mana of each color upon hitting an enemy with a puzzle attack" },
                    { CharacterSheet.BumboType.TheStout, "Gains extra mana from tile combos: 7 mana from 4-tile combos and 9 mana from bigger combos. Loses all mana at the start of each turn" },
                    { CharacterSheet.BumboType.TheWeird, "Gains 1 movement upon killing an enemy" },
                    { CharacterSheet.BumboType.TheDead, "Gains 2 mana of each color at the start of each room. Rerolls spell mana costs upon activation" },
                    { CharacterSheet.BumboType.TheLost, "Cannot gain health past 1/2 heart. Ghost tiles appear on the puzzle board" },
                    { CharacterSheet.BumboType.Eden, "Starts with random stats. Rerolls each spell into another spell of the same type at the start of each room" },
                    { (CharacterSheet.BumboType) 10, WISE_DESCRIPTION },
                };
            }
        }

        public static string SpellDescriptionWithValues(SpellElement spell)
        {
            if (SpellDescriptions.TryGetValue(spell.spellName, out string value))
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
                        string grounded = String.Empty;
                        string flying = String.Empty;
                        if (characterSheet != null)
                        {
                            switch (characterSheet.currentFloor)
                            {
                                case 2:
                                    grounded = "Masks";
                                    flying = "Longits or Boom Flies";
                                    break;
                                case 3:
                                    grounded = "Skully B.s or Cultists";
                                    flying = "Whisps";
                                    break;
                                default:
                                    grounded = "Dips or Tato Kids";
                                    flying = "Flies";
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
                        string healing = WindfallPersistentDataController.LoadData().implementBalanceChanges ? "1" : "2";
                        value = value.Replace("[healing]", healing);
                        break;
                    case SpellName.LooseChange:
                        string coins = WindfallPersistentDataController.LoadData().implementBalanceChanges ? "4 coins" : "1 coin";
                        value = value.Replace("[coins]", coins);
                        break;
                    case SpellName.RockFriends:
                        if (characterSheet != null)
                        {
                            int itemDamage = characterSheet.getItemDamage() + 1;
                            value = value.Replace("[count]", itemDamage.ToString());
                            value = value.Replace("[target]", itemDamage == 1 ? "rock on a random enemy," : "rocks on random enemies, each");
                        }
                        break;
                }

                value = value.Replace("[damage]", spell.Damage().ToString());
                value = value.Replace("[stacking]", CollectibleChanges.PercentSpellEffectStackingCap(spell.spellName).ToString());
                return value;
            }
            return string.Empty;
        }

        public static Dictionary<SpellName, string> SpellDescriptions
        {
            get
            {
                return new Dictionary<SpellName, string>
                {
                    { (SpellName)1000, "Attacks an enemy for [damage] spell damage, chaining to nearby enemies up to [spread] additional times" },
                    { (SpellName)1001, "Enlarges a random tile, doubling its value in tile combos" },
                    { (SpellName)1002, "Enlarges the selected tile, doubling its value in tile combos" },
                    { SpellName.Addy, "Raises spell damage and puzzle damage by 1 for the current turn" },
                    { SpellName.AttackFly, "Attacks for [damage] spell damage, repeating in the same lane for 1 damage each turn" },
                    { SpellName.Backstabber, "Attacks for [damage] spell damage to the furthest enemy. Always crits primed enemies" },
                    { SpellName.BarbedWire, "Raises retaliatory damage dealt to attacking enemies by 1 for the current room, up to [stacking]" },
                    { SpellName.BeckoningFinger, "Pulls a random enemy to the front row and poisons it" },
                    { SpellName.BeeButt, "Attacks for [damage] spell damage, poisoning the enemy" },
                    { SpellName.BigRock, "Attacks for [damage] spell damage to the furthest enemy, plus 1 splash damage to adjacent spaces" },
                    { SpellName.BigSlurp, "Grants 2 movement" },
                    { SpellName.BlackCandle, "Destroys all curse tiles" },
                    { SpellName.BlackD12, "Rerolls the selected column of tiles" },
                    { SpellName.BlenderBlade, "Destroys the selected tile and the 4 tiles next to it" },
                    { SpellName.BlindRage, "For the current room, raises spell damage and puzzle damage by 1, but multiplies all damage recieved" },
                    { SpellName.BloodRights, "Grants 1 mana of each color, but also randomly places <nobr>1-2</nobr> curse tiles" },
                    { SpellName.BorfBucket, "Attacks for [damage] spell damage, plus 1 splash damage to adjacent spaces" },
                    { SpellName.Box, "Grants 1 mana of each color" },
                    { SpellName.Brimstone, "Attacks for [damage] spell damage to all enemies in the selected lane" },
                    { SpellName.BrownBelt, "Blocks the next hit and counters for [damage] spell damage" },
                    { SpellName.BumboShake, "Shuffles the puzzle board" },
                    { SpellName.BumboSmash, "Attacks for [damage] spell damage" },
                    { SpellName.ButterBean, "Knocks back all enemies" },
                    { SpellName.BuzzDown, "Moves the selected tile column downward by one" },
                    { SpellName.BuzzRight, "Moves the selected tile row rightward by one" },
                    { SpellName.BuzzUp, "Moves the selected tile column upward by one" },
                    { SpellName.CatHeart, "Randomly places a heart tile" },
                    { SpellName.CatPaw, "Drains a red heart and converts it into a soul heart" },
                    { SpellName.Chaos, "Randomly places a wild tile and a curse tile" },
                    { SpellName.CoinRoll, "Grants 1 coin" },
                    { SpellName.ConverterBrown, "Grants 2 brown mana" },
                    { SpellName.ConverterGreen, "Grants 2 green mana" },
                    { SpellName.ConverterGrey, "Grants 2 grey mana" },
                    { SpellName.ConverterWhite, "Grants 2 white mana" },
                    { SpellName.ConverterYellow, "Grants 2 yellow mana" },
                    { SpellName.CraftPaper, "Transforms into a copy of the selected spell" },
                    { SpellName.CrazyStraw, "Destroys the selected tile and grants 3 mana of its color" },
                    { SpellName.CursedRainbow, "Randomly places 3 curse tiles, and 4 wild tiles in the 'next' row" },
                    { SpellName.D10, "Rerolls all grounded enemies into [grounded]. Rerolls all flying enemies into [flying]. Enemy types change each floor." },
                    { SpellName.D20, "Rerolls mana and the puzzle board, grants a coin and a soul heart, and unprimes all enemies" },
                    { SpellName.D4, "Shuffles the puzzle board" },
                    { SpellName.D6, "Rerolls the selected spell" },
                    { SpellName.D8, "Rerolls mana" },
                    { SpellName.DarkLotus, "Grants 3 random mana" },
                    { SpellName.DeadDove, "Destroys the selected tile and all tiles above it" },
                    { SpellName.DogTooth, "Attacks for [damage] spell damage, healing 1/2 heart if it hits an enemy" },
                    { SpellName.Ecoli, "Attacks in the selected lane, transforming the enemy into a Poop, Dip, or Squat" },
                    { SpellName.Eraser, "Destroys all tiles of the selected type" },
                    { SpellName.Euthanasia, "Retaliates for [damage] spell damage to the next attacking enemy" },
                    { SpellName.ExorcismKit, "Attacks a random enemy for [damage] spell damage and heals all other enemies for [healing] health" },
                    { SpellName.FishHook, "Attacks for [damage] spell damage, granting 1 random mana if it hits an enemy" },
                    { SpellName.FlashBulb, "Flashes all enemies, giving each a 50% chance of being blinded" },
                    { SpellName.Flip, "Rerolls the selected tile" },
                    { SpellName.Flush, "Attacks for [damage] spell damage to all enemies and removes all Poops" },
                    { SpellName.GoldenTick, "Reduces mana costs by 40% for the current room and fully charges all other spells" },
                    { SpellName.HairBall, "Attacks for [damage] spell damage, splashing enemies behind for 1 damage" },
                    { SpellName.HatPin, "Attacks for [damage] spell damage to all enemies in the row" },
                    { SpellName.Juiced, "Grants 1 movement" },
                    { SpellName.KrampusCross, "Destroys the selected row and column of tiles" },
                    { SpellName.Lard, "Heals 1 red heart, but reduces movement at the start of the next turn by 1" },
                    { SpellName.LeakyBattery, "Attacks for [damage] spell damage to all enemies" },
                    { SpellName.Lemon, "Attacks for [damage] spell damage, blinding the enemy" },
                    { SpellName.Libra, "Averages current mana between all 5 colors" },
                    { SpellName.LilRock, "Attacks for [damage] spell damage to the furthest enemy" },
                    { SpellName.LithiumBattery, "Grants 2 movement" },
                    { SpellName.LooseChange, "Grants [coins] upon taking damage during the next enemy phase" },
                    { SpellName.LuckyFoot, "Raises luck by 1 for the room" },
                    { SpellName.Magic8Ball, "Randomly places a wild tile in the 'next' row" },
                    { SpellName.MagicMarker, "Randomly places <nobr>2-3</nobr> copies of the selected tile" },
                    { SpellName.Mallot, "Destroys the selected tile and places 2 copies beside it" },
                    { SpellName.MamaFoot, "Attacks for [damage] spell damage to all enemies, but hurts for 1/2 heart" },
                    { SpellName.MamaShoe, "Attacks for [damage] spell damage to all grounded enemies" },
                    { SpellName.MeatHook, "Attacks for [damage] spell damage to to the furthest enemy, pulling it to the front row" },
                    { SpellName.MegaBattery, "Grants 3 movement and <nobr>2-3</nobr> random mana" },
                    { SpellName.MegaBean, "Knocks back all enemies in the front row and poisons all flying enemies" },
                    { SpellName.Melatonin, "Unprimes all enemies" },
                    { SpellName.Metronome, "Grants the effect of a random spell" },
                    { SpellName.MirrorMirror, "Horizontally inverts the selected row of tiles" },
                    { SpellName.MissingPiece, "Raises puzzle damage by 1 for the current room" },
                    { SpellName.MomsLipstick, "Turns the selected tile into a heart tile" },
                    { SpellName.MomsPad, "Blinds an enemy" },
                    { SpellName.MsBang, "Destroys the selected tile and all 8 surrounding tiles" },
                    { SpellName.Mushroom, "Raises spell damage and puzzle damage by 1 for the current room and heals 1/2 heart" },
                    { SpellName.NailBoard, "Attacks for [damage] spell damage to all enemies in the front row" },
                    { SpellName.NavyBean, "Destroys the selected column of tiles" },
                    { SpellName.Needle, "Attacks for [damage] spell damage, increasing its damage by 1 for the current room if it hits an enemy" },
                    { SpellName.Number1, "Attacks for [damage] spell damage, granting 1 movement if it hits an enemy" },
                    { SpellName.OldPillow, "Blocks the next attack" },
                    { SpellName.OrangeBelt, "Raises retaliatory damage dealt to attacking enemies by 1 for the current turn, up to [stacking]" },
                    { SpellName.PaperStraw, "Grants mana for each copy of the most common tile in its own color" },
                    { SpellName.Pause, "Skips the next enemy phase" },
                    { SpellName.Peace, "Randomly unprimes an enemy" },
                    { SpellName.Pentagram, "Raises spell damage by 1 for the current room" },
                    { SpellName.Pepper, "Boogers and knocks back an enemy" },
                    { SpellName.PintoBean, "Knocks back all enemies in the front row" },
                    { SpellName.Pliers, "Attacks for [damage] spell damage, granting 1 grey mana and randomly placing a tooth tile if it hits an enemy" },
                    { SpellName.PotatoMasher, "Destroys the selected tile and randomly places a copy of it" },
                    { SpellName.PrayerCard, "Grants 1/2 soul heart" },
                    { SpellName.PriceTag, "Destroys the selected spell and grants <nobr>10-20</nobr> coins" },
                    { SpellName.PuzzleFlick, "Destroys all tiles of the selected type, then attacks for spell damage equal to half the tiles destroyed" },
                    { SpellName.Quake, "Attacks all grounded enemies that are not below a flying enemy for 1 spell damage. Destroys all obstacles and attacks all spaces for 1 damage per obstacle destroyed" },
                    { SpellName.RainbowFinger, "Turns the selected tile into a wild tile" },
                    { SpellName.RainbowFlag, "Randomly places 3 wild tiles" },
                    { SpellName.RedD12, "Rerolls the selected row of tiles" },
                    { SpellName.Refresh, "Adds 1 charge to a random spell" },
                    { SpellName.Rock, "Attacks for [damage] spell damage to the furthest enemy" },
                    { SpellName.RockFriends, "Drops [count] [target] dealing [damage] spell damage" },
                    { SpellName.RoidRage, "Grants 100% crit chance for the next attack" },
                    { SpellName.RottenMeat, "Heals 1/2 heart, but randomly obscures 4 tiles" },
                    { SpellName.RubberBat, "Attacks for [damage] spell damage to all enemies in the front row and knocks them back" },
                    { SpellName.SilverChip, "Increases coins gained from the clearing the current room by <nobr>1-3</nobr>" },
                    { SpellName.Skewer, "Destroys the selected row of tiles" },
                    { SpellName.SleightOfHand, "Reduces the mana cost of all other spells by 25% for the current room" },
                    { SpellName.SmokeMachine, "Grants 50% dodge chance for the current turn" },
                    { SpellName.Snack, "Heals 1/2 heart" },
                    { SpellName.SnotRocket, "Boogers all enemies in the selected lane" },
                    { SpellName.Stick, "Attacks for [damage] spell damage, knocking the enemy back" },
                    { SpellName.StopWatch, "Prevents enemies from taking more than 1 action for the current turn" },
                    { SpellName.Teleport, "Skips the current room" },
                    { SpellName.TheNegative, "Attacks for [damage] spell damage to all enemies in the selected lane" },
                    { SpellName.ThePoop, "Places a poop barrier in the selected lane" },
                    { SpellName.TheRelic, "Grants 1 soul heart" },
                    { SpellName.TheVirus, "Poisons all attacking enemies for the current turn" },
                    { SpellName.TimeWalker, "Grants 3 movement" },
                    { SpellName.TinyDice, "Rerolls all tiles of the selected type" },
                    { SpellName.Toothpick, "Destroys the selected tile and grants 1 mana of its color" },
                    { SpellName.TracePaper, "Randomly grants the effect of another owned spell" },
                    { SpellName.TrapDoor, "Skips the current chapter and grants 10 coins" },
                    { SpellName.TrashLid, "Blocks the next 2 attacks" },
                    { SpellName.TwentyLbsWeight, "Destroys all tiles in the top 3 rows" },
                    { SpellName.TwentyTwenty, "Duplicates the next tile combo effect" },
                    { SpellName.WatchBattery, "Grants 1 movement" },
                    { SpellName.WhiteBelt, "For the current turn, negates enemy curses and mana drain, and limits their damage to 1/2 heart" },
                    { SpellName.WoodenNickel, "Grants <nobr>1-2</nobr> coins" },
                    { SpellName.WoodenSpoon, "Grants 1 movement immediately and at the start of each turn" },
                    { SpellName.YellowBelt, "Raises dodge chance by 5% for the current room, up to [stacking]%" },
                    { SpellName.YumHeart, "Heals 1 heart" },
                };
            }
        }

        public static string TrinketDescriptionWithValues(TrinketElement trinket)
        {
            if (TrinketDescriptions.TryGetValue(trinket.trinketName, out string value))
            {
                return value;
            }
            return string.Empty;
        }

        public static Dictionary<TrinketName, string> TrinketDescriptions
        {
            get
            {
                return new Dictionary<TrinketName, string>
                {
                    { TrinketName.ChargePrick, "Reduces the selected spell's recharge time by 1" },
                    { TrinketName.DamagePrick, "Raises the selected spell's damage by 1" },
                    { TrinketName.ManaPrick, "Reduces the selected spell's mana cost by 25%" },
                    { TrinketName.RandomPrick, "Rerolls the selected spell" },
                    { TrinketName.ShufflePrick, "Rerolls the selected spell's mana cost" },
                    { TrinketName.AAABattery, "Grants a 10% chance to gain 1 movement at the start of each turn" },
                    { TrinketName.AABattery, "Grants a 25% chance to gain 1 movement upon killing an enemy" },
                    { TrinketName.Artery, "Grants a 10% chance to heal 1/2 heart upon killing an enemy" },
                    { TrinketName.BagOJuice, "Grants 1 mana of each color upon taking damage during the enemy phase" },
                    { TrinketName.BagOSucking, "Grants a 50% chance to gain 3 random mana upon dealing spell damage" },
                    { TrinketName.BagOTrash, "Rerolls spell mana costs upon activation" },
                    { TrinketName.BlackCandle, "Negates all damage recieved from curse tiles" },
                    { TrinketName.BlackMagic, "Deals 3 puzzle damage to all enemies upon matching a curse tile combo" },
                    { TrinketName.BloodBag, "Raises spell damage and puzzle damage by 1 for the current room upon taking damage during the enemy phase" },
                    { TrinketName.BloodyBattery, "Grants a 15% chance to gain 1 movement upon damaging an enemy" },
                    { TrinketName.BoneSpur, "Raises the damage of bone attacks by 1" },
                    { TrinketName.Boom, "Deals 6 damage to all enemies" },
                    { TrinketName.Breakfast, "Heals 1/2 heart at the start of each room" },
                    { TrinketName.BrownTick, "Reduces the selected spell's recharge time by 1" },
                    { TrinketName.ButterBean, "Knocks back all enemies at the start of each room" },
                    { TrinketName.CashGrab, "Increases coins gained from clearing each room by 1" },
                    { TrinketName.ChickenBone, "Grants 1 white mana at the start of each turn" },
                    { TrinketName.Clover, "Raises luck by 1" },
                    { TrinketName.CoinPurse, "Increases coins gained from clearing each room by 1" },
                    { TrinketName.ColostomyBag, "Places a poop pile in a random lane at the start of each room" },
                    { TrinketName.Curio, "Multiplies the effects of some trinkets" },
                    { TrinketName.CurvedHorn, "At the start of each turn, grants a 33% chance to raise spell damage by 1 for the current turn" },
                    { TrinketName.Death, "Hurts all <nobr>non-boss</nobr> enemies for damage equal to their current health. Deals 3 damage to bosses" },
                    { TrinketName.Dinner, "Heals all hearts" },
                    { TrinketName.DrakulaTeeth, "Grants a 10% chance to heal 1/2 heart upon damaging an enemy" },
                    { TrinketName.EggBeater, "Rerolls mana upon killing an enemy" },
                    { TrinketName.Experimental, "Raises a random stat by 1 and randomly places a curse tile on the puzzle board" },
                    { TrinketName.FalseTeeth, "Grants 1 grey mana at the start of each turn" },
                    { TrinketName.Feather, "At the start of each turn or when taking damage from a <nobr>non-enemy</nobr> source, if only 1/2 red health remains, chooses a random unavailable spell and allows it to be used once for free" },
                    { TrinketName.Fracture, "Randomly throws a bone at the start of each room" },
                    { TrinketName.Glitch, "Transforms into a random trinket at the start of each room" },
                    { TrinketName.Goober, "Randomly throws a booger at the start of each room" },
                    { TrinketName.HeartLocket, "Randomly places <nobr>2-4 heart</nobr> tiles at the start of each room" },
                    { TrinketName.HolyMantle, "Grants a shield that negates one hit of damage in each room" },
                    { TrinketName.Hoof, "Grants 1 movement at the start of each room" },
                    { TrinketName.IBS, "Increases the size of placed poop piles by 1, but not above 3" },
                    { TrinketName.LotusPetal, "Grants 3 mana of each color" },
                    { TrinketName.Magnet, "Grants a 33% chance to gain 2 extra coins upon clearing a room" },
                    { TrinketName.ModelingClay, "Randomly chooses another owned trinket and becomes a copy of it" },
                    { TrinketName.MomsPhoto, "Grants a 25% chance to apply blindness upon hitting an enemy" },
                    { TrinketName.MysteriousBag, "Grants a 25% chance to splash 1 spell damage to all adjacent spaces upon damaging an enemy" },
                    { TrinketName.MysticMarble, "Raises the damage of tooth attacks by 1" },
                    { TrinketName.NineVolt, "Grants a 25% chance to charge a random spell by 1 upon using a spell" },
                    { TrinketName.Norovirus, "Causes poop barriers to retaliate for 1 damage when attacked" },
                    { TrinketName.NoseGoblin, "Randomly places <nobr>2-4</nobr> booger tiles at the start of each room" },
                    { TrinketName.OldTooth, "Randomly places <nobr>2-4</nobr> tooth tiles at the start of each room" },
                    { TrinketName.OneUp, "Grants an extra life upon taking fatal damage, restoring all starting health" },
                    { TrinketName.PinkBow, "Grants a soul heart at the end of each chapter" },
                    { TrinketName.PinkEye, "Grants a 25% chance to apply poison upon hitting an enemy" },
                    { TrinketName.Pinky, "Grants a 33% chance to randomly place a wild tile upon killing an enemy" },
                    { TrinketName.Plunger, "Randomly places <nobr>2-4</nobr> poop tiles at the start of each room" },
                    { TrinketName.PuzzlePiece, "Upon matching a tile combo, grants 1 mana of each color for each wild tile used" },
                    { TrinketName.RainbowBag, "Rerolls each spell into another spell of the same type at the start of each room" },
                    { TrinketName.RainbowTick, "Reduces a spell's mana cost by 25%" },
                    { TrinketName.RatHeart, "Grants a 25% chance to gain 1/2 soul heart at the start of each room" },
                    { TrinketName.RatTail, "Raises dodge chance by 10%" },
                    { TrinketName.Rib, "Randomly places <nobr>2-4</nobr> bone tiles at the start of each room" },
                    { TrinketName.Sample, "Randomly places <nobr>2-4</nobr> pee tiles at the start of each room" },
                    { TrinketName.SantaSangre, "Grants a 10% chance to gain 1/2 soul heart upon killing an enemy" },
                    { TrinketName.SharpNail, "Grants a 10% chance to deal 1 spell damage to enemies when they move" },
                    { TrinketName.SilverSkull, "Grants 1 random mana upon killing an enemy" },
                    { TrinketName.SinusInfection, "Causes boogers to deal 1 puzzle damage upon hitting an enemy" },
                    { TrinketName.SmallBox, "Grants 3 random mana at the start of each room" },
                    { TrinketName.SoulBag, "Grants a 25% chance to gain 1 movement upon killing an enemy" },
                    { TrinketName.SteamSale, "Reduces the price of needles, hearts, and trinkets in the Wooden Nickel by 2" },
                    { TrinketName.StemCell, "Heals 1/2 heart upon clearing a room" },
                    { TrinketName.StrayBarb, "Grants a 50% chance to retaliate for 1 spell damage against attacking enemies" },
                    { TrinketName.SuperBall, "Grants a 25% chance to apply knockback upon hitting an enemy" },
                    { TrinketName.SwallowedPenny, "Grants a 25% chance to gain 1 coin upon taking damage during the enemy phase" },
                    { TrinketName.Target, "Raises crit chance by 15%" },
                    { TrinketName.TheDevil, "Raises spell damage and puzzle damage by 1 for the current room" },
                    { TrinketName.TheFool, "Restarts the current chapter. Drains soul health and red health, leaving 1 heart remaining" },
                    { TrinketName.Hierophant, "Grants 2 soul hearts" },
                    { TrinketName.TheStars, "Skips to the next treasure room or the Wooden Nickel, whichever is closer" },
                    { TrinketName.Thermos, "Fully charges all spells and heals 1 heart" },
                    { TrinketName.ThreeDollarBill, "Randomly places a wild tile at the start of each room" },
                    { TrinketName.ToiletSeat, "Grants 1 yellow mana at the start of each turn" },
                    { TrinketName.Turdy, "Grants 1 brown mana at the start of each turn" },
                    { TrinketName.TurtleShell, "Limits total damage taken from enemies each enemy phase to 1 heart" },
                    { TrinketName.Tweezers, "Grants a 50% chance to gain 1 random mana upon damaging an enemy" },
                    { TrinketName.UsedTissue, "Grants 1 green mana at the start of each turn" },
                    { TrinketName.WetDiaper, "Grants a 25% chance to gain 1 movement upon receiving movement from any other source" },
                    { TrinketName.WhiteCandle, "Randomly destroys a curse tile at the start of each turn" },
                };
            }
        }

        public static string EnemyDisplayName(Enemy enemy)
        {
            if (enemy == null) { return string.Empty; }

            //Get boss
            Boss boss = null;
            if (enemy is Boss) { boss = enemy as Boss; }

            //Enemy names
            string enemyNameText = string.Empty;
            if (enemyNameText == string.Empty)
            {
                //Enemy names from name
                if (EnemyDisplayNamesByEnemyName.TryGetValue(enemy.enemyName, out string enemyNameFromName))
                {
                    enemyNameText = enemyNameFromName;
                }
            }
            if (enemyNameText == string.Empty)
            {
                //Boss names from name
                if (boss != null && BossDisplayNamesByBossName.TryGetValue((enemy as Boss).bossName, out string bossNameFromName))
                {
                    enemyNameText = bossNameFromName;
                }
            }
            if (enemyNameText == string.Empty)
            {
                //Enemy and Boss names from type
                if (EnemyDisplayNamesByType.TryGetValue(enemy.GetType(), out string enemyNameFromType))
                {
                    enemyNameText = enemyNameFromType;
                }
            }

            //Get Flipper
            if (enemy is FlipperEnemy)
            {
                enemyNameText = enemy.attackImmunity == Enemy.AttackImmunity.ReducePuzzleDamage ? "Nib" : "Jib";
            }

            //Get Bygone Ghost
            if (enemy.gameObject.name.Contains("Bygone Ghost"))
            {
                enemyNameText = "Bygone Ghost";
            }

            return enemyNameText;
        }
        private static Dictionary<EnemyName, string> EnemyDisplayNamesByEnemyName
        {
            get
            {
                return new Dictionary<EnemyName, string>
                {
                    { EnemyName.Arsemouth, "Tall Boy" },
                    { EnemyName.BlackBlobby, "Black Blobby" },
                    { EnemyName.Blib, "Blib" },
                    { EnemyName.BlueBoney, "Skully B." },
                    { EnemyName.BoomFly, "Boom Fly" },
                    { EnemyName.Burfer, "Burfer" },
                    { EnemyName.Butthead, "Squat" },
                    { EnemyName.CornyDip, "Corn Dip" },
                    { EnemyName.Curser, "Curser" },
                    { EnemyName.DigDig, "Dig Dig" },
                    { EnemyName.Dip, "Dip" },
                    { EnemyName.Flipper, "Flipper" },
                    { EnemyName.FloatingCultist, "Floater" },
                    { EnemyName.Fly, "Fly" },
                    { EnemyName.Greedling, "Greedling" },
                    { EnemyName.GreenBlib, "Green Blib" },
                    { EnemyName.GreenBlobby, "Green Blobby" },
                    { EnemyName.Hanger, "Keeper" },
                    { EnemyName.Hopper, "Leaper" },
                    { EnemyName.Host, "Host" },
                    //{ EnemyName.Imposter, "Imposter" },
                    { EnemyName.Isaacs, "Isaac" },
                    { EnemyName.Larry, "Larry" },
                    { EnemyName.Leechling, "Suck" },
                    { EnemyName.Longit, "Longits" },
                    { EnemyName.ManaWisp, "Mana Wisp" },
                    { EnemyName.MaskedImposter, "Mask" },
                    { EnemyName.MeatGolem, "Meat Golum" },
                    { EnemyName.MegaPoofer, "Mega Poofer" },
                    { EnemyName.MirrorHauntLeft, "Mirror" },
                    { EnemyName.MirrorHauntRight, "Mirror" },
                    { EnemyName.PeepEye, "Peeper Eye" },
                    { EnemyName.Poofer, "Poofer" },
                    { EnemyName.Pooter, "Pooter" },
                    { EnemyName.PurpleBoney, "Skully P." },
                    { EnemyName.RedBlobby, "Red Blobby" },
                    { EnemyName.RedCultist, "Red Floater" },
                    { EnemyName.Screecher, "Screecher" },
                    { EnemyName.Shit, "Poop" },
                    { EnemyName.Spookie, "Spookie" },
                    { EnemyName.Stone, "Rock" },
                    { EnemyName.Stony, "Stony" },
                    { EnemyName.Sucker, "Sucker" },
                    { EnemyName.Tader, "Daddy Tato" },
                    { EnemyName.Tado, "Tato Kid" },
                    { EnemyName.TaintedPeepEye, "Tainted Peeper Eye" },
                    { EnemyName.Tutorial, "Keeper" },
                    { EnemyName.WalkingCultist, "Cultist" },
                    { EnemyName.WillOWisp, "Whisp" },
                };
            }
        }
        private static Dictionary<BossName, string> BossDisplayNamesByBossName
        {
            get
            {
                return new Dictionary<BossName, string>
                {
                    { BossName.Bygone, "Bygone" },
                    { BossName.Duke, "The Duke" },
                    { BossName.Dusk, "Dusk" },
                    { BossName.Gibs, "Gibs" },
                    { BossName.Gizzarda, "Gizzarda" },
                    { BossName.Loaf, "Loaf" },
                    { BossName.Peeper, "Peeper" },
                    { BossName.Pyre, "Pyre" },
                    { BossName.Sangre, "Sangre" },
                    { BossName.ShyGal, "Shy Gal" },
                    { BossName.TaintedDusk, "Tainted Dusk" },
                    { BossName.TaintedPeeper, "Tainted Peeper" },
                    { BossName.TaintedShyGal, "Tainted Shy Gal" },
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
                    { typeof(ArsemouthEnemy), "Tall Boy" },
                    { typeof(BlackBlobbyEnemy), "Black Blobby" },
                    { typeof(BlibEnemy), "Blib" },
                    { typeof(BlueBoneyEnemy), "Skully B." },
                    { typeof(BoomFlyEnemy), "BoomFly" },
                    { typeof(BurferEnemy), "Burfer" },
                    { typeof(ButtheadEnemy), "Squat" },
                    //CornyDip
                    { typeof(CurserEnemy), "Curser" },
                    { typeof(DigDigEnemy), "Dig Dig" },
                    { typeof(DipEnemy), "Dip" },
                    { typeof(FlipperEnemy), "Flipper" },
                    { typeof(FloatingCultistEnemy), "Floater" },
                    { typeof(FlyEnemy), "Fly" },
                    { typeof(GreedlingEnemy), "Greedling" },
                    //GreenBlib
                    { typeof(GreenBlobbyEnemy), "Green Blobby" },
                    { typeof(HangerEnemy), "Keeper" },
                    { typeof(HopperEnemy), "Leaper" },
                    { typeof(HostEnemy), "Host" },
                    { typeof(ImposterEnemy), "Imposter" },
                    { typeof(IsaacsEnemy), "Isaac" },
                    { typeof(LarryEnemy), "Larry" },
                    { typeof(LeecherEnemy), "Suck" },
                    { typeof(LongitEnemy), "Longits" },
                    { typeof(ManaWispEnemy), "Mana Wisp" },
                    { typeof(MaskedImposterEnemy), "Mask" },
                    { typeof(MeatGolemEnemy), "Meat Golum" },
                    { typeof(MegaPooferEnemy), "Mega Poofer" },
                    { typeof(MirrorHauntEnemy), "Mirror" },
                    { typeof(PeepEyeEnemy), "Peeper Eye" },
                    { typeof(PooferEnemy), "Poofer" },
                    { typeof(PooterEnemy), "Pooter" },
                    { typeof(PurpleBoneyEnemy), "Skully P." },
                    { typeof(RedBlobbyEnemy), "Red Blobby" },
                    { typeof(RedCultistEnemy), "Red Floater" },
                    { typeof(ScreecherEnemy), "Screecher" },
                    { typeof(ShitEnemy), "Poop" },
                    { typeof(SpookieEnemy), "Spookie" },
                    { typeof(StoneEnemy), "Rock" },
                    { typeof(StonyEnemy), "Stony" },
                    { typeof(SuckerEnemy), "Sucker" },
                    { typeof(TaderEnemy), "Daddy Tato" },
                    { typeof(TadoEnemy), "Tato Kid" },
                    //TaintedPeepEye
                    { typeof(TutorialEnemy), "Keeper" },
                    { typeof(WalkingCultistEnemy), "Cultist" },
                    { typeof(WilloWispEnemy), "Whisp" },

                    //Bosses
                    { typeof(BygoneBoss), "Bygone" },
                    { typeof(BygoneGhostBoss), "Bygone" },
                    { typeof(DukeBoss), "The Duke" },
                    { typeof(DuskBoss), "Dusk" },
                    { typeof(GibsBoss), "Gibs" },
                    { typeof(GizzardaBoss), "Gizzarda" },
                    { typeof(LoafBoss), "Loaf" },
                    { typeof(PeepsBoss), "Peeper" },
                    { typeof(PyreBoss), "Pyre" },
                    { typeof(CaddyBoss), "Sangre" },
                    { typeof(ShyGalBoss), "Shy Gal" },
                    { typeof(TaintedDuskBoss), "Tainted Dusk" },
                    //TaintedPeeper
                    //TaintedShyGal
                };
            }
        }

        public static string EnemyDisplayDescription(Enemy enemy)
        {
            if (EnemyDescriptions.TryGetValue(EnemyDisplayName(enemy), out string value)) { return "\n" + value; }
            return string.Empty;
        }
        private static Dictionary<string, string> EnemyDescriptions
        {
            get
            {
                return new Dictionary<string, string>
                {
                    //Enemies
                    { "Black Blobby", "Drains mana when hurt" },
                    { "Boom Fly", "Explodes on death" },
                    { "Cultist", "Places a curse tile on hit" },
                    { "Daddy Tato", "Shuffles the puzzle board on hit" },
                    { "Dig Dig", "Dies when all Dig Digs are hiding" },
                    { "Greedling", "Steals a coin on hit" },
                    { "Isaac", "Saps movement on death" },
                    { "Jib", "Flips into a Nib when hurt" },
                    { "Larry", "Creates a gas cloud when hurt" },
                    { "Longits", "Curls when hit or boogered" },
                    { "Mana Wisp", "Extinguishes when a tile combo of its color is matched" },
                    { "Meat Golum", "Saps movement on hit" },
                    { "Mega Poofer", "Explodes on death, healing nearby enemies by 2" },
                    { "Nib", "Flips into a Jib when hurt" },
                    { "Poofer", "Explodes on death, healing nearby enemies by 2" },
                    { "Red Floater", "Fires two projectiles when attacking" },
                    { "Spookie", "Places a curse tile when hurt" },
                    { "Sucker", "Reduces mana gain by 1" },
                    { "Tato Kid", "Spawns a Leaper on death" },

                    //Bosses
                    { "Bygone", "Spawns a Fly and obscures two tiles when hurt" },
                    { "Bygone Ghost", "Places a curse tile when hurt" },
                    { "Dusk", "Moves backwards and disables a spell after taking 4 damage in a single turn" },
                    { "Gibs", "Spawns a Green Blib when hurt" },
                    { "Gizzarda", "Flips when hurt" },
                    { "Loaf", "Spawns Dips when hurt" },
                    { "Pyre", "Extinguishes when a tile combo of its color is matched" },
                    { "Tainted Peeper", "Fires two projectiles when attacking\nSpawns a Blib after taking 3 damage" },
                    { "Tainted Dusk", "Moves backwards after taking 4 damage in a single turn" },
                };
            }
        }

        public static string EnemyDamageReductionWithValues(Enemy enemy)
        {
            if (EnemyDamageReductionByType.TryGetValue(enemy.GetType(), out string value))
            {
                if (enemy is DukeBoss)
                {
                    DukeBoss dukeBoss = enemy as DukeBoss;
                    DukeBoss.DukeSize dukeSize = (DukeBoss.DukeSize)AccessTools.Field(typeof(DukeBoss), "dukeSize").GetValue(dukeBoss);

                    int damageReduction;
                    if (dukeSize == DukeBoss.DukeSize.Large)
                    {
                        damageReduction = 1;
                    }
                    else if (dukeSize == DukeBoss.DukeSize.Medium)
                    {
                        damageReduction = 2;
                    }
                    else
                    {
                        return string.Empty;
                    }

                    value = value.Replace("[damage]", damageReduction.ToString());
                }
                return value;
            }
            return string.Empty;
        }
        public static Dictionary<Type, string> EnemyDamageReductionByType
        {
            get
            {
                return new Dictionary<Type, string>
                {
                    //Enemies
                    { typeof(SpookieEnemy), "\nLimits incoming damage to 1" },

                    //Bosses
                    { typeof(BygoneGhostBoss), "\nLimits incoming damage to 1" },
                    { typeof(DukeBoss), "\nLimits incoming damage to [damage]" },
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
                    "Mana Wisp",
                    "Host",
                    "Peeper Eye",
                    "Sangre",
                    "Stony",
                    "Tainted Peeper Eye",
                };
            }
        }
    }
}
