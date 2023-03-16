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

            SpellView spellView = gameObject.GetComponent<SpellView>();
            if (spellView != null)
            {
                SpellElement spellObject = spellView.SpellObject;
                if (spellObject == null || spellObject.spellName == SpellName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;
                displayPosition = spellView.transform.position + new Vector3(-0.42f, 0f, 0f);
                displayAnchor = Anchor.Right;

                displayDescription = string.Empty;
                if (WindfallTooltipDescriptions.SpellDescriptions.TryGetValue(spellObject.spellName, out string value))
                {
                    string damageValueReplacement = "[damage]";
                    if (value.Contains(damageValueReplacement))
                    {
                        value.Replace(damageValueReplacement, spellObject.Damage().ToString());
                    }

                    displayDescription = value;
                }

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

        public static void UpdateTooltips()
        {
            if (WindfallHelper.app?.view?.GUICamera?.cam == null)
            {
                return;
            }

            Ray ray = WindfallHelper.app.view.GUICamera.cam.ScreenPointToRay(Input.mousePosition);
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

            Camera hudCamera = WindfallHelper.app.view.GUICamera.cam;
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
            if (WindfallHelper.app == null)
            {
                return null;
            }

            if (Windfall.assetBundle == null || !Windfall.assetBundle.Contains(tooltipPath))
            {
                return null;
            }

            Transform tooltipTransform = WindfallHelper.ResetShader(UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>(tooltipPath), WindfallHelper.app.view.GUICamera.transform.Find("HUD")).transform);

            tooltipTransform.localScale = new Vector3(1f, 1f, 1f);

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
    }

    public static class WindfallTooltipDescriptions
    {
        public static Dictionary<SpellName, string> SpellDescriptions
        {
            get
            {
                return new Dictionary<SpellName, string>
                {
                    { SpellName.Addy, "Raises spell damage and puzzle damage by 1 for a turn" },
                    { SpellName.AttackFly, "Attacks for [damage] damage, repeating in the same lane for 1 damage each turn" },
                    { SpellName.Backstabber, "Attacks for [damage] damage to the furthest enemy. Always crits primed enemies" },
                    { SpellName.BarbedWire, "Deals [damage] damage to attacking enemies, up to [stacking]" },
                    { SpellName.BeckoningFinger, "Pulls a random enemy to the front row and poisons it" },
                    { SpellName.BeeButt, "Attacks for [damage] damage, poisoning the enemy" },
                    { SpellName.BigRock, "Attacks for [damage] damage to the furthest enemy, plus 1 splash damage to adjacent enemies" },
                    { SpellName.BigSlurp, "Grants 2 movement" },
                    { SpellName.BlackCandle, "Destroys all curse tiles" },
                    { SpellName.BlackD12, "Rerolls a column of tiles" },
                    { SpellName.BlenderBlade, "Destroys a tile and the 4 tiles next to it" },
                    { SpellName.BlindRage, "Raises spell damage and puzzle damage by 1, but increases all damage recieved" },
                    { SpellName.BloodRights, "Grants 1 mana of each color, but also places 1-2 curse tiles" },
                    { SpellName.BorfBucket, "Attacks for [damage] damage, plus 1 splash damage to adjacent enemies" },
                    { SpellName.Box, "Grants 1 mana of each color" },
                    { SpellName.Brimstone, "Attacks for [damage] damage to all enemies in a lane" },
                    { SpellName.BrownBelt, "Blocks the next hit and counters for [damage] damage" },
                    { SpellName.BumboShake, "Shuffles the puzzle board" },
                    { SpellName.BumboSmash, "Attacks for [damage] damage" },
                    { SpellName.ButterBean, "Knocks back all enemies" },
                    { SpellName.BuzzDown, "Moves a tile column downwards" },
                    { SpellName.BuzzRight, "Moves a tile row to the right" },
                    { SpellName.BuzzUp, "Moves a tile column upwards" },
                    { SpellName.CatHeart, "Randomly places a heart tile" },
                    { SpellName.CatPaw, "Deals 1 red heart damage and converts it into a soul heart" },
                    { SpellName.Chaos, "Randomly places a wild and a curse" },
                    { SpellName.CoinRoll, "Grants 1 coin" },
                    { SpellName.ConverterBrown, "Grants 2 brown mana" },
                    { SpellName.ConverterGreen, "Grants 2 green mana" },
                    { SpellName.ConverterGrey, "Grants 2 grey mana" },
                    { SpellName.ConverterWhite, "Grants 2 white mana" },
                    { SpellName.ConverterYellow, "Grants 2 yellow mana" },
                    { SpellName.CraftPaper, "Transforms into a copy of another spell" },
                    { SpellName.CrazyStraw, "Destroys a tile and grants 3 mana of its color" },
                    { SpellName.CursedRainbow, "Randomly places 3 curse tiles, and 4 wild tiles at the top of the puzzle board" },
                    { SpellName.D20, "Rerolls mana and the puzzle board, grants a coin and a soul heart, and unprimes all enemies" },
                    { SpellName.D4, "Shuffles the puzzle board" },
                    { SpellName.D6, "Rerolls a spell" },
                    { SpellName.D8, "Rerolls mana" },
                    { SpellName.DarkLotus, "Grants 1 mana of 3 random colors" },
                    { SpellName.DeadDove, "Destroys a tile and all tiles above it" },
                    { SpellName.DogTooth, "Attacks for [damage] damage, healing 1/2 heart if it hits an enemy" },
                    { SpellName.Ecoli, "Transforms an enemy into a Poop, Dip, or Squat" },
                    { SpellName.Eraser, "Destroys all tiles of the same type" },
                    { SpellName.Euthanasia, "Deals [damage] damage to the next attacking enemy" },
                    { SpellName.ExorcismKit, "Attacks a random enemy for [damage] damage and heals all other enemies for 2 health" },
                    { SpellName.FishHook, "Attacks for [damage], granting 1 random mana if it hits an enemy" },
                    { SpellName.FlashBulb, "Flashes all enemies, granting a 50% chance of blinding them" },
                    { SpellName.Flip, "Rerolls a tile" },
                    { SpellName.Flush, "Attacks for [damage] damage to all enemies and removes all Poops" },
                    { SpellName.GoldenTick, "Reduces mana costs by 40% and charges all other spells" },
                    { SpellName.HairBall, "Attacks for [damage] damage, splashing enemies behind for 1 damage" },
                    { SpellName.HatPin, "Attacks for [damage] damage to all enemies in the row" },
                    { SpellName.Juiced, "Grants 1 movement" },
                    { SpellName.KrampusCross, "Destroys a row and column of tiles" },
                    { SpellName.Lard, "Heals 1 heart, but reduces movement at the start of the next turn by 1" },
                    { SpellName.LeakyBattery, "Attacks for [damage] damage to all enemies" },
                    { SpellName.Lemon, "Attacks for [damage] damage, blinding the enemy" },
                    { SpellName.Libra, "Averages current mana between all 5 colors" },
                    { SpellName.LilRock, "Attacks for [damage] damage to the furthest enemy" },
                    { SpellName.LithiumBattery, "Grants 2 movement" },
                    { SpellName.LooseChange, "Grants 4 coins when hit for a turn" },
                    { SpellName.LuckyFoot, "Raises luck by 1 for the room" },
                    { SpellName.MagicMarker, "Randomly places 2-3 copies of a tile" },
                    { SpellName.Mallot, "Destroys a tile and places 2 copies beside it" },
                    { SpellName.MamaFoot, "Attacks for [damage] damage to all enemies, but hurts for 1/2 heart" },
                    { SpellName.MamaShoe, "Attacks for [damage] damage to all grounded enemies" },
                    { SpellName.MeatHook, "Attacks for [damage] damage to to the furthest enemy, pulling it to the front row" },
                    { SpellName.MegaBattery, "Grants 3 movement and 2-3 mana of random colors" },
                    { SpellName.MegaBean, "Knocks back all enemies in the front row and poisons all flying enemies" },
                    { SpellName.Melatonin, "Unprimes all enemies" },
                    { SpellName.Metronome, "Grants the effect of a random spell" },
                    { SpellName.MirrorMirror, "Horizontally inverts a row of tiles" },
                    { SpellName.MissingPiece, "Raises puzzle damage by 1 for the room" },
                    { SpellName.MomsLipstick, "Places a heart tile" },
                    { SpellName.MomsPad, "Blinds an enemy" },
                    { SpellName.MsBang, "Destroys a tile and all 8 surrounding tiles" },
                    { SpellName.Mushroom, "Raises spell damage and puzzle damage by 1 for the room and heals 1/2 heart" },
                    { SpellName.NailBoard, "Attacks for [damage] damage to all enemies in the front row" },
                    { SpellName.NavyBean, "Destroys a column of tiles" },
                    { SpellName.Needle, "Attacks for [damage] damage, increasing its damage by 1 for the room if it hits an enemy" },
                    { SpellName.Number1, "Attacks for [damage] damage, granting 1 movement if it hits an enemy" },
                    { SpellName.OldPillow, "Blocks the next attack" },
                    { SpellName.OrangeBelt, "Deals [damage] damage to attacking enemies for a turn, up to [stacking]" },
                    { SpellName.PaperStraw, "Grants mana for each copy of the most common tile" },
                    { SpellName.Pause, "Skips the next enemy phase" },
                    { SpellName.Peace, "Unprimes a random enemy" },
                    { SpellName.Pentagram, "Raises spell damage by 1 for the room" },
                    { SpellName.Pepper, "Boogers and knocks back an enemy" },
                    { SpellName.PintoBean, "Knocks back all enemies in the front row" },
                    { SpellName.Pliers, "Attacks for [damage] damage, granting 1 grey mana and randomly placing a tooth tile if it hits an enemy" },
                    { SpellName.PotatoMasher, "Destroys a tile and randomly places a copy of it" },
                    { SpellName.PrayerCard, "Grants 1/2 soul heart" },
                    { SpellName.PriceTag, "Destroys a spell and grants 10-20 coins" },
                    { SpellName.PuzzleFlick, "Destroys all tiles of the same type, then attacks for damage equal to half the tiles destroyed" },
                    { SpellName.Quake, "Attacks all grounded enemies that are not below a flying enemy for 1 damage. Destroys all obstacles and damages the top enemy in each space by 1 for each obstacle destroyed" },
                    { SpellName.RainbowFinger, "Places a wild tile" },
                    { SpellName.RainbowFlag, "Randomly places 3 wild tiles" },
                    { SpellName.RedD12, "Rerolls a row of tiles" },
                    { SpellName.Refresh, "Randomly adds 1 charge to a spell" },
                    { SpellName.Rock, "Attacks for [damage] damage to the furthest enemy" },
                    { SpellName.RockFriends, "Attacks [count] random enemies for [damage] damage" },
                    { SpellName.RoidRage, "Grants 100% crit chance for the next attack" },
                    { SpellName.RottenMeat, "Heals 1/2 heart, but obscures 4 tiles" },
                    { SpellName.RubberBat, "Attacks for [damage] damage to all enemies in the front row and knocks them back" },
                    { SpellName.SilverChip, "Increases coins gained from the clearing the room by 1-3" },
                    { SpellName.Skewer, "Destroys a row of tiles" },
                    { SpellName.SleightOfHand, "Reduces the mana cost of all other spells by 25% for the room" },
                    { SpellName.SmokeMachine, "Grants 50% dodge chance for a turn" },
                    { SpellName.Snack, "Heals 1/2 heart" },
                    { SpellName.SnotRocket, "Boogers all enemies in a lane" },
                    { SpellName.Stick, "Attacks for [damage] damage, knocking the enemy back" },
                    { SpellName.StopWatch, "Prevents enemies from taking more than 1 action for a turn" },
                    { SpellName.Teleport, "Skips the current room" },
                    { SpellName.TheNegative, "Attacks for [damage] damage to all enemies in a lane" },
                    { SpellName.ThePoop, "Places a poop barrier" },
                    { SpellName.TheRelic, "Grants 1 soul heart" },
                    { SpellName.TheVirus, "Poisons all attacking enemies for a turn" },
                    { SpellName.TimeWalker, "Grants 3 movement" },
                    { SpellName.TinyDice, "Rerolls all tiles of the same type" },
                    { SpellName.Toothpick, "Destroys a tile and grants 1 mana of its color" },
                    { SpellName.TracePaper, "Randomly grants the effect of another owned spell" },
                    { SpellName.TrapDoor, "Skips the current chapter and grants 10 coins" },
                    { SpellName.TrashLid, "Blocks the next 2 attacks" },
                    { SpellName.WatchBattery, "Grants 1 movement" },
                    { SpellName.WhiteBelt, "For a turn, negates enemy curses and mana drain, and limits their damage to 1/2 heart" },
                    { SpellName.WoodenNickel, "Grants 1-2 coins" },
                    { SpellName.WoodenSpoon, "Grants 1 movement immediately and at the start of each turn" },
                    { SpellName.YellowBelt, "Grants 5% dodge chance for the room" },
                    { SpellName.YumHeart, "Heals 1 heart" },
                };
            }
        }
    }
}
