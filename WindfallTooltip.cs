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
                ResizeTooltip();
            }
        }

        private static readonly List<String> tooltipPaths = new List<string>()
        {
            "Tooltip Size 1",
            "Tooltip Size 2",
            "Tooltip Size 3",
            "Tooltip Size 4",
            "Tooltip Size 5",
        };

        private static Dictionary<Mesh, Texture2D> tooltipSizes;

        private static void ResizeTooltip()
        {
            if (tooltipSizes == null)
            {
                tooltipSizes = new Dictionary<Mesh, Texture2D>();

                foreach (string path in tooltipPaths)
                {
                    if (Windfall.assetBundle.Contains(path))
                    {
                        tooltipSizes.Add(Windfall.assetBundle.LoadAsset<Mesh>(path), Windfall.assetBundle.LoadAsset<Texture2D>(path));
                    }
                }
            }

            Bounds textBounds = textMeshPro.textBounds;
            float padding = 1.6f;
            float targetHeight = textBounds.extents.y * padding;

            Mesh tooltipMesh = null;
            Texture2D tooltipTexture = null;
            float tooltipMeshHeight = 0;

            Mesh largestMesh = null;
            Texture2D largestTexture = null;
            float largestHeight = 0;

            foreach (Mesh mesh in tooltipSizes.Keys)
            {
                float heightMultiplier = 13.6f;
                float height = (mesh.bounds.extents.x * heightMultiplier) + (0.168f * heightMultiplier);
                if (height > targetHeight && (tooltipMeshHeight == 0 || height < tooltipMeshHeight))
                {
                    tooltipMeshHeight = height;
                    tooltipMesh = mesh;

                    if (tooltipSizes.TryGetValue(mesh, out Texture2D texture2D))
                    {
                        tooltipTexture = texture2D;
                    }
                }

                float meshHeight = mesh.bounds.extents.x;
                if (largestHeight == 0 || largestHeight > meshHeight)
                {
                    largestHeight = meshHeight;
                    largestMesh = mesh;

                    if (tooltipSizes.TryGetValue(mesh, out Texture2D texture2D))
                    {
                        largestTexture = texture2D;
                    }
                }
            }

            if (tooltipMesh == null || tooltipTexture == null)
            {
                tooltipMesh = largestMesh;
                tooltipTexture = largestTexture;
            }

            tooltipBack.GetComponent<MeshFilter>().mesh = tooltipMesh;
            tooltipBack.GetComponent<MeshRenderer>().material.mainTexture = tooltipTexture;

            tooltipBack.transform.localPosition = Vector3.zero;
        }

        private static readonly float anchorOffsetDistance = 0.1f;
        private static Vector3 AnchorOffset(WindfallTooltip windfallTooltip)
        {
            Vector3 offset;

            MeshRenderer meshRenderer = tooltipBack?.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
            {
                return Vector3.zero;
            }

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
