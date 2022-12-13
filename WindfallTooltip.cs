using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public void UpdateDisplayData()
        {
            active = true;

            BumboModifier bumboModifier = gameObject.GetComponent<BumboModifier>();
            if (bumboModifier != null && bumboModifier.Expanded())
            {
                displayAtMouse = false;
                displayPosition = bumboModifier.TooltipPosition();
                displayAnchor = Anchor.Left;
                displayDescription = bumboModifier.Description();
                return;
            }

            active = false;
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
        private static GameObject tooltipBack;
        private static TextMeshPro textMeshPro;

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

            Camera hudCamera = app.view.GUICamera.cam;
            Vector3 cameraPosition = hudCamera.transform.position;
            Vector3 targetdisplayPosition = windfallTooltip.displayAtMouse ? windfallTooltip.displayPosition : hudCamera.transform.TransformPoint(windfallTooltip.displayPosition);

            Plane plane = new Plane(hudCamera.transform.forward, cameraPosition + (hudCamera.transform.forward * 0.8f));

            Ray ray = new Ray(cameraPosition, (targetdisplayPosition - cameraPosition).normalized);

            if (plane.Raycast(ray, out float enter))
            {
                tooltip.transform.position = ray.GetPoint(enter);
                anchor.localPosition = AnchorOffset(windfallTooltip);
            }

            if (textMeshPro != null)
            {
                textMeshPro.text = windfallTooltip.displayDescription;
            }
        }

        private static readonly float anchorOffsetDistance = 0.1f;
        private static Vector3 AnchorOffset(WindfallTooltip windfallTooltip)
        {
            Vector3 offset;

            MeshRenderer meshRenderer = tooltipBack.GetComponent<MeshRenderer>();

            float width;
            float height;
            if (meshRenderer != null)
            {
                width = meshRenderer.bounds.size.x * 4.8f;
                height = meshRenderer.bounds.size.y * 6;
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
            tooltip = tooltipTransform.gameObject;

            tooltipTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            anchor = tooltipTransform.Find("Anchor");
            tooltipBack = anchor.Find("Tooltip").gameObject;
            textMeshPro = tooltipTransform.GetComponentInChildren<TextMeshPro>();
            LocalizationModifier.ChangeFont(null, textMeshPro, LocalizationModifier.edFont);

            return tooltip;
        }
    }
}
