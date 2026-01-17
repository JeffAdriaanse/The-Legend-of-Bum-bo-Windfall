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
    public class WindfallTooltipController : MonoBehaviour
    {
        private enum DefaultTooltipMode
        {
            Enabled,
            Disabled,
            Override,
        }

        private GameObject tooltip;
        private bool tooltipShowing = true;

        private Transform anchor;

        private TextMeshPro hiddenLabel;
        private List<TextMeshPro> labels;

        private readonly float SCALE_SMALL = 0.85f;
        private readonly float SCALE_MEDIUM = 1.0f;
        private readonly float SCALE_LARGE = 1.15f;

        public void UpdateTooltips()
        {
            if (WindfallHelper.app?.view?.GUICamera?.cam == null) return;

            //Abort if tooltips are disabled
            int tooltipSize = WindfallPersistentDataController.LoadData().tooltipSize;
            if (tooltipSize == -2)
            {
                if (tooltip != null) ShowTooltip(false, false);
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
            ClearEntityTints(tooltipToShow);
            DisplayGridView(tooltipToShow);
        }

        private WindfallTooltip GetMouseTooltip(Ray GUIray, Ray MainRay)
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
                if (GUIHit != GUILayer) continue;

                WindfallTooltip windfallTooltip = hit.collider.GetComponent<WindfallTooltip>();

                if (windfallTooltip != null)
                {
                    windfallTooltip.UpdateDisplayData();

                    //Verify that the tooltip is active
                    if (!windfallTooltip.active) continue;

                    //Verify that the tooltip is the closest tooltip
                    if (hit.distance > closestTooltipDistance && closestTooltipDistance != 0f) continue;

                    //Prioritize GUI tooltips
                    if (!GUIHit && closestTooltipGUI) continue;

                    closestTooltip = windfallTooltip;
                    closestTooltipDistance = hit.distance;
                    closestTooltipGUI = GUIHit;
                }
            }
            return closestTooltip;
        }

        private WindfallTooltip GetGamepadTooltip()
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
                        if (selectable == null) continue;

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
                if (gamepadObject == null) continue;

                WindfallTooltip windfallTooltip = gamepadObject.GetComponent<WindfallTooltip>();
                if (windfallTooltip == null) continue;

                bool selected = false;

                SpellView spellView = gamepadObject.GetComponent<SpellView>();
                if (spellView != null && spellView.gamepadSelectionObject != null && spellView.gamepadSelectionObject.activeSelf) selected = true;

                TrinketView trinketView = gamepadObject.GetComponent<TrinketView>();
                if (trinketView != null && trinketView.gamepadSelectionObject != null && trinketView.gamepadSelectionObject.activeSelf) selected = true;

                SpellPickup spellPickup = gamepadObject.GetComponent<SpellPickup>();
                if (spellPickup != null && spellPickup.selectionArrow != null && spellPickup.selectionArrow.activeSelf) selected = true;

                TrinketPickupView trinketPickupView = gamepadObject.GetComponent<TrinketPickupView>();
                if (trinketPickupView != null && trinketPickupView.selectionArrow != null && trinketPickupView.selectionArrow.activeSelf) selected = true;

                if (selected)
                {
                    windfallTooltip.UpdateDisplayData();

                    //Verify that the tooltip is active
                    if (!windfallTooltip.active) continue;
                    return windfallTooltip;
                }
            }
            return null;
        }

        private void DisplayTooltip(WindfallTooltip windfallTooltip)
        {
            if (WindfallHelper.app == null) return;

            if (tooltip == null)
            {
                tooltip = CreateTooltip();
                if (tooltip == null) return;
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

        private Sequence showTooltipAnimation;
        private readonly float SHOW_TOOLTIP_TWEEN_DURATION = 0.12f;
        private void ShowTooltip(bool show, bool animate)
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

        private MeshRenderer ActiveTooltipBack()
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
        private void ConstrainTooltipToCamera(Camera camera, MeshRenderer meshRenderer, Plane tooltipDisplayPlane)
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
                if (Math.Abs(sidePixelOffset.x) > Math.Abs(pixelOffset.x)) pixelOffset.x = sidePixelOffset.x;

                //Track the vetical offset needed to move the furthest vetical edge into the camera view
                if (Math.Abs(sidePixelOffset.y) > Math.Abs(pixelOffset.y)) pixelOffset.y = sidePixelOffset.y;
            }

            //Abort if the tooltip is within the camera view already and does not need to be moved
            if (pixelOffset.x == 0 && pixelOffset.y == 0) return;

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
        private List<Vector3> RenderedMeshSidePositionsGlobal(MeshRenderer meshRenderer, bool includeX = true, bool includeY = true, bool includeZ = true)
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
        private Vector2 PixelOffsetIntoCameraView(Camera camera, Vector2 screenPoint)
        {
            Vector2 pixelOffset = new Vector2();

            if (screenPoint.x < 0) pixelOffset.x = -screenPoint.x;
            else if (screenPoint.x > camera.pixelWidth) pixelOffset.x = -(screenPoint.x - camera.pixelWidth);


            if (screenPoint.y < 0) pixelOffset.y = -screenPoint.y;
            else if (screenPoint.y > camera.pixelHeight) pixelOffset.y = -(screenPoint.y - camera.pixelHeight);

            return pixelOffset;
        }

        private void ResizeTooltipAndSetLabelText(WindfallTooltip windfallTooltip)
        {
            if (labels == null || labels.Count < 1) return;
            if (hiddenLabel == null) return;

            int linecount = -1;
            if (windfallTooltip != null && windfallTooltip.displayDescription != null)
            {
                //Remove underline if the language is Chinese
                if (LocalizationManager.CurrentLanguage == "Chinese") windfallTooltip.displayDescription = windfallTooltip.displayDescription.Replace("<u>", "").Replace("</u>", "");

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

        private readonly float anchorOffsetDistance = 0.1f;
        private Vector3 AnchorOffset(WindfallTooltip windfallTooltip)
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
            if (meshRenderer == null) return Vector3.zero;

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

        private readonly string tooltipPath = "Tooltip Base";
        private GameObject CreateTooltip()
        {
            if (WindfallHelper.app == null) return null;
            if (Windfall.assetBundle == null || !Windfall.assetBundle.Contains(tooltipPath)) return null;

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

        private Vector3 TooltipScale()
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

        private void ClearEntityTints(WindfallTooltip tooltipToShow)
        {
            List<Enemy> enemies = WindfallHelper.app?.model?.enemies;
            if (enemies != null)
            {
                foreach (Enemy enemy in WindfallHelper.app.model.enemies)
                {
                    if (enemy == null) continue;
                    ObjectTinter objectTinter = enemy.objectTinter;

                    ClearEntityTint(tooltipToShow, enemy, objectTinter);
                }
            }

            List<BattlefieldEffect> battleFieldEffects = WindfallHelper.app?.model?.aiModel?.battlefieldEffects;
            if (battleFieldEffects != null)
            {
                foreach (BattlefieldEffect battleFieldEffect in WindfallHelper.app?.model?.aiModel?.battlefieldEffects)
                {
                    if (battleFieldEffect?.view == null) continue;
                    ObjectTinter objectTinter = battleFieldEffect.view.GetComponent<ObjectTinter>();

                    MonoBehaviour entity = battleFieldEffect.view.GetComponent<BloodShieldEffectView>();
                    if (entity == null) entity = battleFieldEffect.view.GetComponent<FogEffectView>();

                    ClearEntityTint(tooltipToShow, entity, objectTinter);
                }
            }
        }

        private void ClearEntityTint(WindfallTooltip tooltipToShow, MonoBehaviour entity, ObjectTinter objectTinter)
        {
            if (objectTinter == null) return;
            if (objectTinter.tintColor != WindfallTooltip.entityHoverTintColor) return;

            if (tooltipToShow != null)
            {
                MonoBehaviour tooltipBehaviour = ObjectDataStorage.GetData<object>(tooltipToShow.gameObject, EntityChanges.colliderEntityKey) as MonoBehaviour;
                if (tooltipBehaviour == null) tooltipBehaviour = tooltipToShow.gameObject.GetComponent<BloodShieldEffectView>();
                if (tooltipBehaviour == null) tooltipBehaviour = tooltipToShow.gameObject.GetComponent<FogEffectView>();
                if (tooltipBehaviour == null) tooltipBehaviour = tooltipToShow.gameObject.GetComponent<Enemy>();
                if (tooltipBehaviour != null && entity == tooltipBehaviour) return;
            }

            objectTinter.NoTint();
        }

        private void DisplayGridView(WindfallTooltip tooltipToShow)
        {
            //Show battlefield grid
            List<Vector2Int> enemyBattlefieldPositionsVector = new List<Vector2Int>();

            if (tooltipToShow != null)
            {
                Enemy tooltipEnemy = tooltipToShow.gameObject.GetComponent<Enemy>();
                if (tooltipEnemy == null)
                {
                    tooltipEnemy = ObjectDataStorage.GetData<Enemy>(tooltipToShow.gameObject, EntityChanges.colliderEntityKey);
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
            WindfallHelper.BattlefieldGridViewController?.ShowGrid(enemyBattlefieldPositionsVector);
        }
    }

    public class WindfallTooltipPatches()
    {
        //Patch: Update Windfall Tooltip
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Update")]
        static void BumboController_Update(BumboController __instance)
        {
            WindfallHelper.WindfallTooltipController.UpdateTooltips();
        }

        //Patch: Adds tooltips to spellViews
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.SetSpell))]
        static void BumboController_SetSpell(BumboController __instance, int _spell_index)
        {
            SpellView spellView = __instance.app.view.spells[_spell_index];
            WindfallTooltip windfallTooltip = spellView.GetComponent<WindfallTooltip>();
            if (windfallTooltip == null) spellView.gameObject.AddComponent<WindfallTooltip>();
        }

        //Patch: Adds tooltips to spell pickups
        //Also adjusts spell pickup colliders
        [HarmonyPostfix, HarmonyPatch(typeof(SpellPickup), "Start")]
        static void SpellPickup_Start(SpellPickup __instance)
        {
            WindfallTooltip windfallTooltip = __instance.GetComponent<WindfallTooltip>();

            if (windfallTooltip == null)
            {
                __instance.gameObject.AddComponent<WindfallTooltip>();
            }

            //Adjust collider
            BoxCollider boxCollider = __instance.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.size = new Vector3(boxCollider.size.x * 1.32f, boxCollider.size.y, boxCollider.size.z);
            }
        }

        //Patch: Adds tooltips to trinkets
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.UpdateTrinkets))]
        static void BumboController_UpdateTrinkets_Tooltips(BumboController __instance)
        {
            foreach (GameObject trinket in __instance.app.view.GUICamera.GetComponent<GUISide>().trinkets)
            {
                WindfallTooltip windfallTooltip = trinket.GetComponent<WindfallTooltip>();

                if (windfallTooltip == null)
                {
                    trinket.gameObject.AddComponent<WindfallTooltip>();
                }
            }
        }

        //Patch: Adds tooltips to trinket pickups
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketPickupView), "Start")]
        static void TrinketPickupView_Start(TrinketPickupView __instance)
        {
            WindfallTooltip windfallTooltip = __instance.GetComponent<WindfallTooltip>();

            if (windfallTooltip == null)
            {
                __instance.gameObject.AddComponent<WindfallTooltip>();
            }

            //Adjust collider
            BoxCollider boxCollider = __instance.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.size = new Vector3(boxCollider.size.x * 1.1f, boxCollider.size.y * 1.11f, boxCollider.size.z);
                boxCollider.center = new Vector3(boxCollider.center.x, 0.235f, boxCollider.center.z);
            }
        }

        //Patch: Adds tooltips to bum-bo faces
        [HarmonyPostfix, HarmonyPatch(typeof(BumboFacesController), "Start")]
        static void BumboFacesController_Start(BumboFacesController __instance)
        {
            WindfallTooltip windfallTooltip = __instance.GetComponent<WindfallTooltip>();

            if (windfallTooltip == null)
            {
                __instance.gameObject.AddComponent<WindfallTooltip>();
            }
        }

        //Patch: Disables vanilla tooltips
        [HarmonyPrefix, HarmonyPatch(typeof(ToolTip), nameof(ToolTip.Show))]
        static bool ToolTip_Show()
        {
            if (WindfallPersistentDataController.LoadData().tooltipSize != -2)
            {
                return false;
            }

            return true;
        }
    }
}
