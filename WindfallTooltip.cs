using HarmonyLib;
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
                displayAnchor = Anchor.Right;
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
                displayAnchor = Anchor.Right;
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
                displayAnchor = Anchor.Right;
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
                displayPosition = spellView.transform.position + new Vector3(-0.43f, 0f, 0f);
                displayAnchor = Anchor.Left;
                displayDescription = string.Empty;
                if (WindfallHelper.app.model.spellModel.spellKA.TryGetValue(spell.spellName, out string spellKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetEnglishText(spellKA, "Spells") + "</u>\n" + WindfallTooltipDescriptions.SpellDescriptionWithValues(spell);
                }
                return;
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

                displayAtMouse = true;
                displayPosition = Vector3.zero;
                displayAnchor = Anchor.TopRight;
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
                displayPosition = trinketView.transform.position + new Vector3(-0.074f, 0.14f, 0f);
                displayAnchor = Anchor.Top;
                displayDescription = string.Empty;
                if (WindfallHelper.app.model.trinketModel.trinketKA.TryGetValue(trinket.trinketName, out string trinketKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetEnglishText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
                }
                return;
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

                displayAtMouse = true;
                displayPosition = Vector3.zero;
                displayAnchor = Anchor.TopRight;
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
                displayPosition = bumboFacesController.transform.position + new Vector3(0.08f, 0f, 0f);
                displayAnchor = Anchor.Right;
                displayDescription = string.Empty;

                string bumboName = string.Empty;
                string bumboDescription = string.Empty;

                CharacterSheet.BumboType bumboType = WindfallHelper.app.model.characterSheet.bumboType;
                if (WindfallTooltipDescriptions.BumboNames.TryGetValue(bumboType, out string name)) bumboName = name;
                if (WindfallTooltipDescriptions.BumboDescriptions.TryGetValue(bumboType, out string description)) bumboDescription = description;

                displayDescription = "<u>" + bumboName + "</u>\n" + bumboDescription;
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

        private static Transform anchor;

        private static TextMeshPro hiddenLabel;
        private static List<TextMeshPro> labels;

        private static GameObject defaultTooltipObject;
        private static DefaultTooltipMode defaultTooltip = DefaultTooltipMode.Disabled;

        private static readonly float SCALE_SMALL = 0.85f;
        private static readonly float SCALE_MEDIUM = 1.0f;
        private static readonly float SCALE_LARGE = 1.15f;

        public static void UpdateTooltips()
        {
            if (WindfallHelper.app?.view?.GUICamera?.cam == null)
            {
                return;
            }

            //GUICamera
            Ray GUIray = WindfallHelper.app.view.GUICamera.cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] GUIhits = Physics.RaycastAll(GUIray);

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
                tooltipToShow.displayPosition = GUIray.GetPoint(1f);
            }

            DisplayTooltip(tooltipToShow);
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

            int tooltipSize = WindfallPersistentDataController.LoadData().tooltipSize;

            if (windfallTooltip == null || tooltipSize == 0)
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

            float tooltipScale = SCALE_SMALL;
            switch (tooltipSize)
            {
                case 1:
                    tooltipScale = SCALE_SMALL;
                    break;
                case 2:
                    tooltipScale = SCALE_MEDIUM;
                    break;
                case 3:
                    tooltipScale = SCALE_LARGE;
                    break;
            }
            ScaleTooltip(tooltipScale);

            Camera hudCamera = WindfallHelper.app.view.GUICamera.cam;
            Vector3 hudCameraForward = hudCamera.transform.forward;
            Vector3 cameraPosition = hudCamera.transform.position;

            //Place tooltip display pane at a set distance from the camera
            Plane tooltipDisplayPlane = new Plane(hudCameraForward, cameraPosition + (hudCameraForward * 0.6f));

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

            return tooltipTransform.gameObject;
        }

        private static void ScaleTooltip(float scale)
        {
            if (tooltip != null)
            {
                tooltip.transform.Find("Anchor").localScale = new Vector3(scale, scale, scale * 0.5f);
            }
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
                };
            }
        }

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
                    case SpellName.RockFriends:
                        if (characterSheet != null)
                        {
                            int itemDamage = characterSheet.getItemDamage();
                            value = value.Replace("[count]", itemDamage.ToString());
                            value = value.Replace("[target]", itemDamage == 1 ? "enemy" : "enemies");
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
                    { SpellName.Addy, "Raises spell damage and puzzle damage by 1 for the current turn" },
                    { SpellName.AttackFly, "Attacks for [damage] spell damage, repeating in the same lane for 1 damage each turn for the current room" },
                    { SpellName.Backstabber, "Attacks for [damage] spell damage to the furthest enemy. Always crits primed enemies" },
                    { SpellName.BarbedWire, "Raises retaliatory damage dealt to attacking enemies by 1 for the current room, up to [stacking]" },
                    { SpellName.BeckoningFinger, "Pulls a random enemy to the front row and poisons it" },
                    { SpellName.BeeButt, "Attacks for [damage] spell damage, poisoning the enemy" },
                    { SpellName.BigRock, "Attacks for [damage] spell damage to the furthest enemy, plus 1 splash damage to adjacent spaces" },
                    { SpellName.BigSlurp, "Grants 2 movement" },
                    { SpellName.BlackCandle, "Destroys all curse tiles" },
                    { SpellName.BlackD12, "Rerolls a column of tiles" },
                    { SpellName.BlenderBlade, "Destroys a tile and the 4 tiles next to it" },
                    { SpellName.BlindRage, "For the current room, raises spell damage and puzzle damage by 1, but multiplies all damage recieved" },
                    { SpellName.BloodRights, "Grants 1 mana of each color, but also randomly places 1-2 curse tiles" },
                    { SpellName.BorfBucket, "Attacks for [damage] spell damage, plus 1 splash damage to adjacent spaces" },
                    { SpellName.Box, "Grants 1 mana of each color" },
                    { SpellName.Brimstone, "Attacks for [damage] spell damage to all enemies in a lane" },
                    { SpellName.BrownBelt, "Blocks the next hit and counters for [damage] spell damage" },
                    { SpellName.BumboShake, "Shuffles the puzzle board" },
                    { SpellName.BumboSmash, "Attacks for [damage] spell damage" },
                    { SpellName.ButterBean, "Knocks back all enemies" },
                    { SpellName.BuzzDown, "Moves a tile column downwards" },
                    { SpellName.BuzzRight, "Moves a tile row to the right" },
                    { SpellName.BuzzUp, "Moves a tile column upwards" },
                    { SpellName.CatHeart, "Randomly places a heart tile" },
                    { SpellName.CatPaw, "Drains a red heart and converts it into a soul heart" },
                    { SpellName.Chaos, "Randomly places a wild and a curse" },
                    { SpellName.CoinRoll, "Grants 1 coin" },
                    { SpellName.ConverterBrown, "Grants 2 brown mana" },
                    { SpellName.ConverterGreen, "Grants 2 green mana" },
                    { SpellName.ConverterGrey, "Grants 2 grey mana" },
                    { SpellName.ConverterWhite, "Grants 2 white mana" },
                    { SpellName.ConverterYellow, "Grants 2 yellow mana" },
                    { SpellName.CraftPaper, "Transforms into a copy of another spell" },
                    { SpellName.CrazyStraw, "Destroys a tile and grants 3 mana of its color" },
                    { SpellName.CursedRainbow, "Randomly places 3 curse tiles, and 4 wild tiles in the 'next' row" },
                    { SpellName.D10, "Rerolls all grounded enemies into [grounded]. Rerolls all flying enemies into [flying]. Enemy types change each floor." },
                    { SpellName.D20, "Rerolls mana and the puzzle board, grants a coin and a soul heart, and unprimes all enemies" },
                    { SpellName.D4, "Shuffles the puzzle board" },
                    { SpellName.D6, "Rerolls a spell" },
                    { SpellName.D8, "Rerolls mana" },
                    { SpellName.DarkLotus, "Grants 3 random mana" },
                    { SpellName.DeadDove, "Destroys a tile and all tiles above it" },
                    { SpellName.DogTooth, "Attacks for [damage] spell damage, healing 1/2 heart if it hits an enemy" },
                    { SpellName.Ecoli, "Transforms an enemy into a Poop, Dip, or Squat" },
                    { SpellName.Eraser, "Destroys all tiles of the same type" },
                    { SpellName.Euthanasia, "Retaliates for [damage] spell damage to the next attacking enemy" },
                    { SpellName.ExorcismKit, "Attacks a random enemy for [damage] spell damage and heals all other enemies for 1 health" },
                    { SpellName.FishHook, "Attacks for [damage] spell damage, granting 1 random mana if it hits an enemy" },
                    { SpellName.FlashBulb, "Flashes all enemies, granting a 50% chance of blinding them" },
                    { SpellName.Flip, "Rerolls a tile" },
                    { SpellName.Flush, "Attacks for [damage] spell damage to all enemies and removes all Poops" },
                    { SpellName.GoldenTick, "Reduces mana costs by 40% for the current room and charges all other spells" },
                    { SpellName.HairBall, "Attacks for [damage] spell damage, splashing enemies behind for 1 damage" },
                    { SpellName.HatPin, "Attacks for [damage] spell damage to all enemies in the row" },
                    { SpellName.Juiced, "Grants 1 movement" },
                    { SpellName.KrampusCross, "Destroys a row and column of tiles" },
                    { SpellName.Lard, "Heals 1 red heart, but reduces movement at the start of the next turn by 1" },
                    { SpellName.LeakyBattery, "Attacks for [damage] spell damage to all enemies" },
                    { SpellName.Lemon, "Attacks for [damage] spell damage, blinding the enemy" },
                    { SpellName.Libra, "Averages current mana between all 5 colors" },
                    { SpellName.LilRock, "Attacks for [damage] spell damage to the furthest enemy" },
                    { SpellName.LithiumBattery, "Grants 2 movement" },
                    { SpellName.LooseChange, "Grants 4 coins when hit for one turn" },
                    { SpellName.LuckyFoot, "Raises luck by 1 for the room" },
                    { SpellName.Magic8Ball, "Randomly places a wild tile in the 'next' row" },
                    { SpellName.MagicMarker, "Randomly places 2-3 copies of a tile" },
                    { SpellName.Mallot, "Destroys a tile and places 2 copies beside it" },
                    { SpellName.MamaFoot, "Attacks for [damage] spell damage to all enemies, but hurts for 1/2 heart" },
                    { SpellName.MamaShoe, "Attacks for [damage] spell damage to all grounded enemies" },
                    { SpellName.MeatHook, "Attacks for [damage] spell damage to to the furthest enemy, pulling it to the front row" },
                    { SpellName.MegaBattery, "Grants 3 movement and 2-3 random mana" },
                    { SpellName.MegaBean, "Knocks back all enemies in the front row and poisons all flying enemies" },
                    { SpellName.Melatonin, "Unprimes all enemies" },
                    { SpellName.Metronome, "Grants the effect of a random spell" },
                    { SpellName.MirrorMirror, "Horizontally inverts a row of tiles" },
                    { SpellName.MissingPiece, "Raises puzzle damage by 1 for the current room" },
                    { SpellName.MomsLipstick, "Places a heart tile" },
                    { SpellName.MomsPad, "Blinds an enemy" },
                    { SpellName.MsBang, "Destroys a tile and all 8 surrounding tiles" },
                    { SpellName.Mushroom, "Raises spell damage and puzzle damage by 1 for the current room and heals 1/2 heart" },
                    { SpellName.NailBoard, "Attacks for [damage] spell damage to all enemies in the front row" },
                    { SpellName.NavyBean, "Destroys a column of tiles" },
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
                    { SpellName.PotatoMasher, "Destroys a tile and randomly places a copy of it" },
                    { SpellName.PrayerCard, "Grants 1/2 soul heart" },
                    { SpellName.PriceTag, "Destroys a spell and grants 10-20 coins" },
                    { SpellName.PuzzleFlick, "Destroys all tiles of the same type, then attacks for spell damage equal to half the tiles destroyed" },
                    { SpellName.Quake, "Attacks all grounded enemies that are not below a flying enemy for 1 spell damage. Destroys all obstacles and attacks all spaces for 1 damage per obstacle destroyed" },
                    { SpellName.RainbowFinger, "Places a wild tile" },
                    { SpellName.RainbowFlag, "Randomly places 3 wild tiles" },
                    { SpellName.RedD12, "Rerolls a row of tiles" },
                    { SpellName.Refresh, "Randomly adds 1 charge to a spell" },
                    { SpellName.Rock, "Attacks for [damage] spell damage to the furthest enemy" },
                    { SpellName.RockFriends, "Randomly attacks [count] [target] for [damage] spell damage" },
                    { SpellName.RoidRage, "Grants 100% crit chance for the next attack" },
                    { SpellName.RottenMeat, "Heals 1/2 heart, but randomly obscures 4 tiles" },
                    { SpellName.RubberBat, "Attacks for [damage] spell damage to all enemies in the front row and knocks them back" },
                    { SpellName.SilverChip, "Increases coins gained from the clearing the current room by 1-3" },
                    { SpellName.Skewer, "Destroys a row of tiles" },
                    { SpellName.SleightOfHand, "Reduces the mana cost of all other spells by 25% for the current room" },
                    { SpellName.SmokeMachine, "Grants 50% dodge chance for the current turn" },
                    { SpellName.Snack, "Heals 1/2 heart" },
                    { SpellName.SnotRocket, "Boogers all enemies in a lane" },
                    { SpellName.Stick, "Attacks for [damage] spell damage, knocking the enemy back" },
                    { SpellName.StopWatch, "Prevents enemies from taking more than 1 action for the current turn" },
                    { SpellName.Teleport, "Skips the current room" },
                    { SpellName.TheNegative, "Attacks for [damage] spell damage to all enemies in a lane" },
                    { SpellName.ThePoop, "Places a poop barrier" },
                    { SpellName.TheRelic, "Grants 1 soul heart" },
                    { SpellName.TheVirus, "Poisons all attacking enemies for the current turn" },
                    { SpellName.TimeWalker, "Grants 3 movement" },
                    { SpellName.TinyDice, "Rerolls all tiles of the same type" },
                    { SpellName.Toothpick, "Destroys a tile and grants 1 mana of its color" },
                    { SpellName.TracePaper, "Randomly grants the effect of another owned spell" },
                    { SpellName.TrapDoor, "Skips the current chapter and grants 10 coins" },
                    { SpellName.TrashLid, "Blocks the next 2 attacks" },
                    { SpellName.WatchBattery, "Grants 1 movement" },
                    { SpellName.WhiteBelt, "For the current turn, negates enemy curses and mana drain, and limits their damage to 1/2 heart" },
                    { SpellName.WoodenNickel, "Grants 1-2 coins" },
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
                    { TrinketName.ChargePrick, "Reduces an item's recharge time by 1" },
                    { TrinketName.DamagePrick, "Raises a spell's damage by 1" },
                    { TrinketName.ManaPrick, "Reduces a spell's mana cost by 25%" },
                    { TrinketName.RandomPrick, "Rerolls a spell" },
                    { TrinketName.ShufflePrick, "Rerolls a spell's mana cost" },
                    { TrinketName.AAABattery, "Grants a 10% chance to gain 1 movement at the start of each turn" },
                    { TrinketName.AABattery, "Grants a 25% chance to gain 1 movement upon killing an enemy" },
                    { TrinketName.Artery, "Grants a 10% chance to heal 1/2 heart upon killing an enemy" },
                    { TrinketName.BagOJuice, "Grants 1 mana of each color upon taking damage during the enemy phase" },
                    { TrinketName.BagOSucking, "Grants a 50% chance to gain 3 random mana upon dealing spell damage" },
                    { TrinketName.BagOTrash, "Rerolls spell mana costs upon activation" },
                    { TrinketName.BlackCandle, "Negates all damage recieved from curse tiles" },
                    { TrinketName.BlackMagic, "Deals 3 puzzle damage to all enemies upon making a curse tile combo" },
                    { TrinketName.BloodBag, "Raises spell damage and puzzle damage by 1 for the current room upon taking damage during the enemy phase" },
                    { TrinketName.BloodyBattery, "Grants a 15% chance to gain 1 movement upon damaging an enemy" },
                    { TrinketName.BoneSpur, "Raises the damage of bone attacks by 1" },
                    { TrinketName.Boom, "Deals 6 damage to all enemies" },
                    { TrinketName.Breakfast, "Heals 1/2 heart at the start of each room" },
                    { TrinketName.BrownTick, "Reduces an item's recharge time by 1" },
                    { TrinketName.ButterBean, "Knocks back all enemies at the start of each room" },
                    { TrinketName.CashGrab, "Increases coins gained from clearing each room by 1" },
                    { TrinketName.ChickenBone, "Grants 1 white mana at the start of each turn" },
                    { TrinketName.Clover, "Raises luck by 1" },
                    { TrinketName.CoinPurse, "Increases coins gained from clearing each room by 1" },
                    { TrinketName.ColostomyBag, "Randomly places a poop pile at the start of each room" },
                    { TrinketName.Curio, "Multiplies the effects of some trinkets" },
                    { TrinketName.CurvedHorn, "At the start of each turn, grants a 33% chance to raise spell damage by 1 for the current turn" },
                    { TrinketName.Death, "Hurts all non-boss enemies for damage equal to their current health. Deals 3 damage to bosses" },
                    { TrinketName.Dinner, "Heals all hearts" },
                    { TrinketName.DrakulaTeeth, "Grants a 10% chance to heal 1/2 heart upon damaging an enemy" },
                    { TrinketName.EggBeater, "Rerolls mana upon killing an enemy" },
                    { TrinketName.Experimental, "Randomly raises a stat by 1 and places a curse tile on the puzzle board" },
                    { TrinketName.FalseTeeth, "Grants 1 grey mana at the start of each turn" },
                    { TrinketName.Feather, "At the start of each turn or when taking damage from a non-enemy source, if only 1/2 red health remains, randomly chooses an unavailable spell and allows it to be used once for free" },
                    { TrinketName.Fracture, "Randomly throws a bone at the start of each room" },
                    { TrinketName.Glitch, "Transforms into a random trinket at the start of each room" },
                    { TrinketName.Goober, "Randomly throws a booger at the start of each room" },
                    { TrinketName.HeartLocket, "Randomly places 2-4 heart tiles at the start of each room" },
                    { TrinketName.HolyMantle, "Grants a shield that negates one hit of damage in each room" },
                    { TrinketName.Hoof, "Grants 1 movement at the start of each room" },
                    { TrinketName.IBS, "Increases the size of placed poop piles by 1, but not above 3" },
                    { TrinketName.LotusPetal, "Grants 3 mana of each color" },
                    { TrinketName.Magnet, "Grants a 33% chance to increase coins gained from clearing each room by 2" },
                    { TrinketName.ModelingClay, "Randomly chooses another owned trinket and becomes a copy of it" },
                    { TrinketName.MomsPhoto, "Grants a 25% chance to apply blindness upon hitting an enemy" },
                    { TrinketName.MysteriousBag, "Grants a 25% chance to splash 1 spell damage to all adjacent spaces upon damaging an enemy" },
                    { TrinketName.MysticMarble, "Raises the damage of tooth attacks by 1" },
                    { TrinketName.NineVolt, "Grants a 25% chance to randomly charge a spell by 1 upon using a spell" },
                    { TrinketName.Norovirus, "Causes poop barriers to retaliate for 1 damage when attacked" },
                    { TrinketName.NoseGoblin, "Randomly places 2-4 booger tiles at the start of each room" },
                    { TrinketName.OldTooth, "Randomly places 2-4 tooth tiles at the start of each room" },
                    { TrinketName.OneUp, "Grants an extra life upon taking fatal damage, restoring all starting health" },
                    { TrinketName.PinkBow, "Grants a soul heart upon entering the Wooden Nickel" },
                    { TrinketName.PinkEye, "Grants a 25% chance to apply poison upon hitting an enemy" },
                    { TrinketName.Pinky, "Grants a 33% chance to randomly place a wild tile upon killing an enemy" },
                    { TrinketName.Plunger, "Randomly places 2-4 poop tiles at the start of each room" },
                    { TrinketName.PuzzlePiece, "Upon making a tile combo, grants 1 mana of each color for each wild tile used" },
                    { TrinketName.RainbowBag, "Rerolls each spell into another spell of the same type at the start of each room" },
                    { TrinketName.RainbowTick, "Reduces a spell's mana cost by 25%" },
                    { TrinketName.RatHeart, "Grants a 25% chance to gain 1/2 soul heart at the start of each room" },
                    { TrinketName.RatTail, "Raises dodge chance by 10%" },
                    { TrinketName.Rib, "Randomly places 2-4 bone tiles at the start of each room" },
                    { TrinketName.Sample, "Randomly places 2-4 pee tiles at the start of each room" },
                    { TrinketName.SantaSangre, "Grants a 10% chance to gain 1/2 soul heart upon damaging an enemy" },
                    { TrinketName.SharpNail, "Grants a 10% chance to deal 1 spell damage to enemies when they move" },
                    { TrinketName.SilverSkull, "Grants 1 random mana upon killing an enemy" },
                    { TrinketName.SinusInfection, "Causes boogers to deal 1 puzzle damage upon hitting an enemy" },
                    { TrinketName.SmallBox, "Grants 3 random mana" },
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
                    { TrinketName.Thermos, "Charges all Items and heals 1 heart" },
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
    }
}
