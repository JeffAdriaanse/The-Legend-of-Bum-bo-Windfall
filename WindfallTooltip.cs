using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
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

        public WindfallTooltip()
        {
            UpdateDisplayData();
        }

        //void OnMouseEnter()
        //{
        //    //Play Sound
        //}

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
                displayPosition = bumboModifier.TooltipPosition();
                displayAnchor = Anchor.Left;
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
                displayPosition = bumboModifierTemporary.TooltipPosition();
                displayAnchor = Anchor.Left;
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
                displayPosition = bumboModifierStacking.TooltipPosition();
                displayAnchor = Anchor.Left;
                displayDescription = bumboModifierStacking.bumboModifier.StackingDescription();
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

        private static BumboApplication app;
        private static GameObject tooltip;

        private static Transform anchor;

        private static TextMeshPro hiddenLabel;
        private static List<TextMeshPro> labels;

        private static GameObject defaultTooltipObject;
        private static DefaultTooltipMode defaultTooltip = DefaultTooltipMode.Disabled;

        public static BumboApplication GetApp(BumboApplication _app)
        {
            if (app != null)
            {
                return app;
            }

            if (_app != null)
            {
                app = _app;
            }
            else
            {
                app = GameObject.FindObjectOfType<BumboApplication>();
            }

            return app;
        }

        public static void UpdateTooltips()
        {
            if (app?.view?.GUICamera?.cam == null)
            {
                return;
            }

            Ray ray = app.view.GUICamera.cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            WindfallTooltip closestTooltip = null;
            float closestTooltipDistance = 0f;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                WindfallTooltip windfallTooltip = hit.collider.GetComponent<WindfallTooltip>();

                if (windfallTooltip != null)
                {
                    windfallTooltip.UpdateDisplayData();

                    if (windfallTooltip.active && (hit.distance < closestTooltipDistance || closestTooltipDistance == 0f))
                    {
                        closestTooltip = windfallTooltip;
                        closestTooltipDistance = hit.distance;
                    }
                }
            }

            WindfallTooltip tooltipToShow = closestTooltip;

            if (defaultTooltip == DefaultTooltipMode.Override || (tooltipToShow == null && defaultTooltip == DefaultTooltipMode.Enabled))
            {
                if (defaultTooltipObject == null)
                {
                    defaultTooltipObject = new GameObject();
                    tooltipToShow = defaultTooltipObject.AddComponent<WindfallTooltip>();
                }
                else
                {
                    tooltipToShow = defaultTooltipObject.GetComponent<WindfallTooltip>();
                }

                tooltipToShow.displayAtMouse = true;
                tooltipToShow.displayAnchor = WindfallTooltip.Anchor.BottomLeft;
            }

            if (tooltipToShow != null && tooltipToShow.displayAtMouse)
            {
                tooltipToShow.displayPosition = ray.GetPoint(1f);
            }

            DisplayTooltip(tooltipToShow);
        }

        private static void DisplayTooltip(WindfallTooltip windfallTooltip)
        {
            if (app == null)
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
                if (tooltip.activeSelf)
                {
                    tooltip.SetActive(false);
                }
                return;
            }

            if (!tooltip.activeSelf)
            {
                tooltip.SetActive(true);
            }

            ResizeTooltip(windfallTooltip);

            Camera hudCamera = app.view.GUICamera.cam;
            Vector3 hudCameraForward = hudCamera.transform.forward;
            Vector3 cameraPosition = hudCamera.transform.position;

            //Place tooltip display pane at a set distance from the camera
            Plane tooltipDisplayPlane = new Plane(hudCameraForward, cameraPosition + (hudCameraForward * 0.8f));

            //Get target display position
            Vector3 targetdisplayPosition = windfallTooltip.displayPosition;
            Vector3 targetdisplayDirection = (targetdisplayPosition - cameraPosition).normalized;

            //The target display position is slightly too wide when getting the position from world space
            //Consequently, the tooltip display direction must be adjusted to compensate
            if (!windfallTooltip.displayAtMouse)
            {
                //TEST targetdisplayDirection = Vector3.Lerp(hudCameraForward, targetdisplayDirection, 0.93f);
            }

            //Cast a ray through to the target position and place the tooltip at the intersection point on the plane
            Ray targetDisplayRay = new Ray(cameraPosition, targetdisplayDirection);
            if (tooltipDisplayPlane.Raycast(targetDisplayRay, out float enter))
            {
                tooltip.transform.position = targetDisplayRay.GetPoint(enter);
                //Apply anchor offset
                anchor.localPosition = AnchorOffset(windfallTooltip);
            }
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
                if (linecount >= 0 && tooltipBack.name.Contains(linecount.ToString()))
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
                width = meshRenderer.bounds.size.x * 0.48f;
                height = meshRenderer.bounds.size.y * 0.6f;
            }
            else
            {
                width = anchorOffsetDistance;
                height = anchorOffsetDistance;
            }

            switch (windfallTooltip.displayAnchor)
            {
                case WindfallTooltip.Anchor.Top:
                    offset = new Vector3(0f, -height, 0f);
                    break;
                case WindfallTooltip.Anchor.TopRight:
                    offset = new Vector3(-width, -height, 0f);
                    break;
                case WindfallTooltip.Anchor.Right:
                    offset = new Vector3(-width, 0f, 0f);
                    break;
                case WindfallTooltip.Anchor.BottomRight:
                    offset = new Vector3(-width, height, 0f);
                    break;
                case WindfallTooltip.Anchor.Bottom:
                    offset = new Vector3(0f, height, 0f);
                    break;
                case WindfallTooltip.Anchor.BottomLeft:
                    offset = new Vector3(width, height, 0f);
                    break;
                case WindfallTooltip.Anchor.Left:
                    offset = new Vector3(width, 0f, 0f);
                    break;
                case WindfallTooltip.Anchor.TopLeft:
                    offset = new Vector3(width, -height, 0f);
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
            if (app == null)
            {
                return null;
            }

            if (Windfall.assetBundle == null || !Windfall.assetBundle.Contains(tooltipPath))
            {
                return null;
            }

            Transform tooltipTransform = WindfallHelper.ResetShader(UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>(tooltipPath), app.view.GUICamera.transform.Find("HUD")).transform);

            tooltipTransform.localScale = new Vector3(1f, 1f, 1f);

            anchor = tooltipTransform.Find("Anchor");

            hiddenLabel = anchor.Find("Hidden Label").GetComponent<TextMeshPro>();
            LocalizationModifier.ChangeFont(null, hiddenLabel, LocalizationModifier.edFont);

            labels = tooltipTransform.GetComponentsInChildren<TextMeshPro>(true).ToList();
            if (labels.Contains(hiddenLabel))
            {
                labels.Remove(hiddenLabel);
            }

            foreach (TextMeshPro textMeshPro in labels)
            {
                LocalizationModifier.ChangeFont(null, textMeshPro, LocalizationModifier.edFont);
            }


            return tooltipTransform.gameObject;
        }
    }
}
