using HarmonyLib;
using I2.Loc;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class CharacterImport
    {
        //Character select: two collectibles
        private static readonly Vector3 Spell1ALocalPosition = new Vector3(-0.3408f, -0.3794f, -0.0503f); //Brave: -0.3626f, -0.3976f, -0.0706f
        private static readonly Vector3 Spell1ALocalRotation = new Vector3(1.0155f, 4.081f, 4.838f); //Brave: 1.0155f, 7.7767f, 13.0342f
        private static readonly Vector3 Spell1ALocalScale = new Vector3(1f, 1f, 1f);
        private static readonly Vector3 Spell2ALocalPosition = new Vector3(0.2504f, -0.3493f, -0.1007f); //Brave: 0.183f, -0.342f, -0.148f
        private static readonly Vector3 Spell2ALocalRotation = new Vector3(359.9419f, 358.0826f, 357.9784f); //Brave: 0f, 0f, 0f
        private static readonly Vector3 Spell2ALocalScale = new Vector3(1f, 1f, 1f);

        //Character select: collectible popsicle
        private static readonly Vector3 CollectiblePopsicleLocalPosition = new Vector3(0f, 0.008f, -0.007f);

        private static readonly int WiseInsertPosition = 6;
        [HarmonyPostfix, HarmonyPatch(typeof(SelectCharacterView), "Start")]
        static void SelectCharacterView_Start(SelectCharacterView __instance)
        {
            List<CharacterSheet.BumboType> bumboTypes = (List<CharacterSheet.BumboType>)AccessTools.Field(typeof(SelectCharacterView), "bumboTypes").GetValue(__instance);
            if (bumboTypes != null && bumboTypes.Count >= WiseInsertPosition && !bumboTypes.Contains((CharacterSheet.BumboType)10)) bumboTypes.Insert(WiseInsertPosition, (CharacterSheet.BumboType)10);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SelectCharacterView), nameof(SelectCharacterView.BumboWins))]
        static void SelectCharacterView_BumboWins(CharacterSheet.BumboType _type, ref int __result)
        {
            if (_type == (CharacterSheet.BumboType)10) __result = WindfallPersistentDataController.LoadData().wiseWins;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SelectCharacterView), nameof(SelectCharacterView.BumboMoneyWins))]
        static void SelectCharacterView_BumboMoneyWins(CharacterSheet.BumboType _type, ref int __result)
        {
            if (_type == (CharacterSheet.BumboType)10) __result = WindfallPersistentDataController.LoadData().wiseMoneyWins;
        }

        private static readonly BumboObject WiseObject = new BumboObject
        {
            bumboType = (CharacterSheet.BumboType)10,
            soulHearts = 0f,
            hitPoints = 2f,
            itemDamage = 1,
            luck = 1,
            puzzleDamage = 2,
            dexterity = 1,
            startingSpells = new StartingSpell[]
            {
                new StartingSpell()
                {
                    spell = (SpellName)1000,
                    poopCost = 4,
                },

                new StartingSpell()
                {
                    spell = (SpellName)1001,
                    peeCost = 3,
                }
            },
            startingTrinkets = new TrinketName[] { (TrinketName)1003 },
            hiddenTrinket = (TrinketName)1001,
        };

        private static readonly Vector3 WiseNameLocalPosition = new Vector3(-0.006f, 0.0002f, 0.004f);
        private static readonly Vector3 WiseStatsLocalPosition = new Vector3(0.0032f, 0.0003f, 0.0029f);
        private static readonly float WiseScale = 1f;
        private static readonly Vector3 WiseLocalscale = new Vector3(WiseScale, WiseScale, WiseScale);
        private static readonly Vector3 WiseSelectLocalposition = new Vector3(-0.0001f, 0.0002f, 0.0044f);
        private static readonly Vector3 WiseSelectLocalrotation = new Vector3(90f, 0f, 0f);
        private static readonly Vector3 WiseDuckLocalposition = new Vector3(-0.0001f, -0.0002f, 0.0044f);
        private static readonly Vector3 WiseDuckLocalrotation = new Vector3(270f, 90f, 0f);
        private static readonly Vector3 WiseUppercutLocalposition = new Vector3(0.0001f, 0.0002f, 0.0044f);
        private static readonly Vector3 WiseUppercutLocalrotation = new Vector3(90f, 14f, 0f);
        private static readonly Vector3 WiseAnticipateLocalposition = new Vector3(0.0002f, -0.0002f, 0.0043f);
        private static readonly Vector3 WiseAnticipateLocalrotation = new Vector3(270f, 58f, 0f);

        [HarmonyPostfix, HarmonyPatch(typeof(TitleController), "Awake")]
        static void TitleController_Awake(TitleController __instance)
        {
            AddWiseSelect(__instance);
        }

        private static void AddWiseSelect(TitleController titleController)
        {
            //Add Bum-bo the Wise select object
            SelectCharacterView selectCharacterView = titleController.ChooseBumbo.m_SelectBumbo.selectCharacterView;
            BumboSelectView wiseSelectView = GameObject.Instantiate(selectCharacterView.lost, selectCharacterView.transform).GetComponent<BumboSelectView>();
            wiseSelectView.gameObject.name = "The Wise";
            wiseSelectView.bumboType = (CharacterSheet.BumboType)10;

            GameObject wiseSelectParent = wiseSelectView.bumboSelect;

            //Materials and meshes
            Material wiseMaterial = Windfall.assetBundle.LoadAsset<Material>("Bumbo the Wise");

            //Bumbo name
            GameObject wiseName = wiseSelectParent.transform.Find("bumbo_select_name").gameObject;
            WindfallHelper.Reskin(wiseName, null, null, Windfall.assetBundle.LoadAsset<Texture2D>("Wise Text"), false);
            WindfallHelper.ReTransform(wiseName, WiseNameLocalPosition, Vector3.zero, Vector3.zero, "rotation scale");

            //Bumbo stats
            GameObject wiseStats = wiseSelectParent.transform.Find("bumbo_select_stats").gameObject;
            WindfallHelper.Reskin(wiseStats, null, null, Windfall.assetBundle.LoadAsset<Texture2D>("Wise Text"), false);
            WindfallHelper.ReTransform(wiseStats, WiseStatsLocalPosition, Vector3.zero, Vector3.zero, "rotation scale");

            //Bumbo select
            GameObject wiseSelect = wiseSelectParent.transform.Find("Bumbo Select").gameObject;
            WindfallHelper.Reskin(wiseSelect, Windfall.assetBundle.LoadAsset<Mesh>("Wise Select"), wiseMaterial, null);
            WindfallHelper.ReTransform(wiseSelect, WiseSelectLocalposition, WiseSelectLocalrotation, WiseLocalscale, string.Empty);

            //Bumbo duck
            GameObject wiseDuck = wiseSelectParent.transform.Find("Bumbo Duck").gameObject;
            WindfallHelper.Reskin(wiseDuck, Windfall.assetBundle.LoadAsset<Mesh>("Wise Duck"), wiseMaterial, null);
            WindfallHelper.ReTransform(wiseDuck, WiseDuckLocalposition, WiseDuckLocalrotation, WiseLocalscale, string.Empty);

            //Bumbo uppercut
            GameObject wiseUppercut = wiseSelectParent.transform.Find("Bumbo Uppercut").gameObject;
            WindfallHelper.Reskin(wiseUppercut, Windfall.assetBundle.LoadAsset<Mesh>("Wise Uppercut"), wiseMaterial, null);
            WindfallHelper.ReTransform(wiseUppercut, WiseUppercutLocalposition, WiseUppercutLocalrotation, WiseLocalscale, string.Empty);

            //Bumbo anticipate
            GameObject wiseAnticipate = wiseSelectParent.transform.Find("Lost Anticipate").gameObject;
            WindfallHelper.Reskin(wiseAnticipate, Windfall.assetBundle.LoadAsset<Mesh>("Wise Anticipate"), wiseMaterial, null);
            WindfallHelper.ReTransform(wiseAnticipate, WiseAnticipateLocalposition, WiseAnticipateLocalrotation, WiseLocalscale, string.Empty);

            //Collectibles
            Transform wiseSpells = wiseSelectView.spells.transform.Find("bumbo_select_brave_spells")?.Find("Brave Spells");

            //Spell 1
            BumboSelectSpellView spell1 = wiseSpells?.Find("Spell 1")?.GetComponent<BumboSelectSpellView>();
            spell1.bumboType = (CharacterSheet.BumboType)10;
            //WindfallHelper.ReTransform(spell1.gameObject, Spell1ALocalPosition, Spell1ALocalRotation, Spell1ALocalScale, string.Empty);

            //Spell 2
            BumboSelectSpellView spell2 = wiseSpells?.Find("Spell 2 ")?.GetComponent<BumboSelectSpellView>(); //Note that there is a whitespace in the GameObject name "Spell 2 "
            spell2.bumboType = (CharacterSheet.BumboType)10;
            //WindfallHelper.ReTransform(spell2.gameObject, Spell2ALocalPosition, Spell2ALocalRotation, Spell2ALocalScale, string.Empty);

            //Trinket 1
            BumboSelectTrinketView trinket1 = wiseSelectView.spells.transform.Find("bumbo_select_brave_spells")?.Find("Brave Trinkets")?.Find("Trinket 1")?.GetComponent<BumboSelectTrinketView>();
            trinket1.bumboType = (CharacterSheet.BumboType)10;

            //Deactivate object
            wiseSelectView.gameObject.SetActive(false);

            //Unlock requirement text
            Localize unlockLocalize = wiseSelectView.lockedObject.transform.Find("Unlock_Condition").Find("Unlock Text").GetComponent<Localize>();
            Localization.SetKey(unlockLocalize, eI2Category.Characters, "WISE_UNLOCK");

            //Character description text
            CharDescView charDescView = GameObject.FindObjectOfType<CharDescView>();
            GameObject[] newText = new GameObject[11];
            for (int i = 0; i < newText.Length; i++)
            {
                if (i == (int)(CharacterSheet.BumboType)10)
                {
                    newText[i] = GameObject.Instantiate(newText[0], newText[0].transform.parent);
                    Localization.SetKey(newText[i].GetComponent<Localize>(), eI2Category.Characters, "WISE_DESCRIPTION");
                    //newText[i].GetComponent<TextMeshPro>().text = WiseSelectDescription;
                    newText[i].SetActive(false);
                    continue;
                }

                GameObject textObject;
                if (i < charDescView.text.Length && charDescView.text[i] != null) textObject = charDescView.text[i];
                else textObject = new GameObject();
                newText[i] = textObject;
            }
            charDescView.text = newText;

            //Localized name text
            GameObject wiseSelectName = wiseSelectParent.transform.Find("Font Name").Find("Text (TMP)").gameObject;
            Localization.SetKey(wiseSelectName.GetComponent<Localize>(), eI2Category.Characters, "WISE_NAME");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GamblingController), "Start")]
        static void GamblingController_Start(GamblingController __instance)
        {
            AddWiseGamblingChapterIntro(__instance);
        }

        private static void AddWiseGamblingChapterIntro(GamblingController gamblingController)
        {
            GameObject lost = gamblingController.view.bumbos[(int)CharacterSheet.BumboType.TheLost].gameObject;
            GameObject wise = GameObject.Instantiate(lost, lost.transform.parent);
            wise.SetActive(false);
            wise.name = "wise_select";
            SelectBumboAnimation wiseBumboAnimation = wise.GetComponent<SelectBumboAnimation>();

            //Add The Wise to GamblingView bumbos
            SelectBumboAnimation[] newBumbos = new SelectBumboAnimation[11];
            for (int i = 0; i < newBumbos.Length; i++)
            {
                if (i == (int)(CharacterSheet.BumboType)10)
                {
                    newBumbos[i] = wiseBumboAnimation;
                    continue;
                }

                SelectBumboAnimation selectBumboAnimation;
                if (i < gamblingController.view.bumbos.Length && gamblingController.view.bumbos[i] != null) selectBumboAnimation = gamblingController.view.bumbos[i];
                else selectBumboAnimation = null;
                newBumbos[i] = selectBumboAnimation;
            }
            gamblingController.view.bumbos = newBumbos;

            //Materials and meshes
            Material wiseMaterial = Windfall.assetBundle.LoadAsset<Material>("Bumbo the Wise");

            //Bumbo select
            GameObject wiseSelect = wiseBumboAnimation.bumboSelect;
            WindfallHelper.Reskin(wiseSelect, Windfall.assetBundle.LoadAsset<Mesh>("Wise Select"), wiseMaterial, null);
            WindfallHelper.ReTransform(wiseSelect, WiseSelectLocalposition, WiseSelectLocalrotation, WiseLocalscale, string.Empty);

            //Bumbo duck
            GameObject wiseDuck = wiseBumboAnimation.bumboDuck;
            WindfallHelper.Reskin(wiseDuck, Windfall.assetBundle.LoadAsset<Mesh>("Wise Duck"), wiseMaterial, null);
            WindfallHelper.ReTransform(wiseDuck, WiseDuckLocalposition, WiseDuckLocalrotation, WiseLocalscale, string.Empty);

            //Bumbo uppercut
            GameObject wiseUppercut = wiseBumboAnimation.bumboUppercut;
            WindfallHelper.Reskin(wiseUppercut, Windfall.assetBundle.LoadAsset<Mesh>("Wise Uppercut"), wiseMaterial, null);
            WindfallHelper.ReTransform(wiseUppercut, WiseUppercutLocalposition, WiseUppercutLocalrotation, WiseLocalscale, string.Empty);

            //Bumbo anticipate
            GameObject wiseAnticipate = wiseBumboAnimation.bumboBackside;
            WindfallHelper.Reskin(wiseAnticipate, Windfall.assetBundle.LoadAsset<Mesh>("Wise Anticipate"), wiseMaterial, null);
            WindfallHelper.ReTransform(wiseAnticipate, WiseAnticipateLocalposition, WiseAnticipateLocalrotation, WiseLocalscale, string.Empty);
        }

        //Make The Wise appear in the carousel
        [HarmonyPrefix, HarmonyPatch(typeof(SelectCharacterView), "AddBumbo")]
        static bool SelectCharacterView_AddBumbo(SelectCharacterView __instance, CharacterSheet.BumboType _type, float _rotation, ref GameObject __result)
        {
            if (_type == (CharacterSheet.BumboType)10)
            {
                GameObject bumboTheWise = UnityEngine.Object.Instantiate<GameObject>(__instance.transform.Find("The Wise").gameObject, __instance.transform.position, Quaternion.identity, __instance.transform);
                bumboTheWise.transform.localEulerAngles = new Vector3(0f, _rotation, 0f);
                bumboTheWise.SetActive(true);
                bumboTheWise.GetComponent<BumboSelectView>().isUnlocked = __instance.BumboIsUnlocked(_type);
                __result = bumboTheWise;
                return false;
            }
            return true;
        }

        //Bum-bo the Wise unlock requirement: Beat The Basement
        [HarmonyPrefix, HarmonyPatch(typeof(SelectCharacterView), nameof(SelectCharacterView.BumboIsUnlocked))]
        static bool SelectCharacterView_BumboIsUnlocked(SelectCharacterView __instance, CharacterSheet.BumboType _type, ref bool __result)
        {
            if (_type == (CharacterSheet.BumboType)10)
            {
                __result = /*__instance.progression.unlocks[6];*/true;
                return false;
            }
            return true;
        }

        //Bum-bo the Wise hurt
        [HarmonyPostfix, HarmonyPatch(typeof(BumboHurtView), nameof(BumboHurtView.TurnOnHurt))]
        static void BumboHurtView_TurnOnHurt(BumboHurtView __instance)
        {
            if (__instance.app.model.characterSheet.bumboType == (CharacterSheet.BumboType)10) ToggleWiseHurtAndAnticipate(__instance, true, false);
        }

        //Bum-bo the Wise anticipate
        [HarmonyPostfix, HarmonyPatch(typeof(BumboHurtView), nameof(BumboHurtView.TurnOnAnticipate))]
        static void BumboHurtView_TurnOnAnticipate(BumboHurtView __instance)
        {
            if (__instance.app.model.characterSheet.bumboType == (CharacterSheet.BumboType)10) ToggleWiseHurtAndAnticipate(__instance, false, true);
        }

        private static void ToggleWiseHurtAndAnticipate(BumboHurtView bumboHurtView, bool hurt, bool anticipate)
        {
            GameObject wise = bumboHurtView.transform.Find("Hurt Object").Find("Wise")?.gameObject;
            if (wise == null) wise = CreateWiseHurt(bumboHurtView);

            GameObject wiseHurt = wise.transform.Find("Wise Hurt").gameObject;
            GameObject wiseAnticipate = wise.transform.Find("Wise Anticipate").gameObject;

            wiseHurt?.SetActive(hurt);
            wiseAnticipate?.SetActive(anticipate);
        }

        private static GameObject CreateWiseHurt(BumboHurtView bumboHurtView)
        {
            GameObject wiseHurtParent = GameObject.Instantiate(bumboHurtView.transform.Find("Hurt Object").Find("Brave").gameObject, bumboHurtView.transform.Find("Hurt Object"));
            wiseHurtParent.name = "Wise";
            wiseHurtParent.transform.localPosition = Vector3.zero;

            GameObject wiseAnticipate = wiseHurtParent.transform.Find("Brave Anticipate").gameObject;
            wiseAnticipate.name = "Wise Anticipate";
            WindfallHelper.Reskin(wiseAnticipate, Windfall.assetBundle.LoadAsset<Mesh>("Wise Anticipate"), Windfall.assetBundle.LoadAsset<Material>("Bumbo the Wise"), null); ;
            WindfallHelper.ReTransform(wiseAnticipate, new Vector3(0.04f, 0.53f, -0.0153f), new Vector3(0f, 0f, 239f), new Vector3(165f, 165f, 165f), string.Empty);

            GameObject wiseHurt = wiseHurtParent.transform.Find("Brave Hurt").gameObject;
            wiseHurt.name = "Wise Hurt";
            WindfallHelper.Reskin(wiseHurt, Windfall.assetBundle.LoadAsset<Mesh>("Wise Hurt"), Windfall.assetBundle.LoadAsset<Material>("Bumbo the Wise"), null);
            WindfallHelper.ReTransform(wiseHurt, new Vector3(0.04f, 0.42f, -0.0153f), new Vector3(0f, 0f, 355f), new Vector3(165f, 165f, 165f), string.Empty);

            return wiseHurtParent;
        }

        //Bum-bo the Wise throw
        [HarmonyPostfix, HarmonyPatch(typeof(BumboThrowView), nameof(BumboThrowView.ChangeArm), new Type[] { typeof(Block.BlockType), typeof(CharacterSheet.BumboType) })]
        static void BumboThrowView_ChangeArm(BumboThrowView __instance, CharacterSheet.BumboType _bumbo_type)
        {
            GameObject wiseThrowParent = __instance.transform.Find("wise throw")?.gameObject;

            if (_bumbo_type != (CharacterSheet.BumboType)10)
            {
                wiseThrowParent?.SetActive(false);
                return;
            }

            if (wiseThrowParent == null) wiseThrowParent = CreateWiseThrow(__instance);

            wiseThrowParent.SetActive(true);
        }

        private static GameObject CreateWiseThrow(BumboThrowView bumboThrowView)
        {
            GameObject wiseThrowParent = GameObject.Instantiate(bumboThrowView.transform.Find("brave throw").gameObject, bumboThrowView.transform);
            wiseThrowParent.name = "wise throw";

            GameObject wiseThrow = wiseThrowParent.transform.Find("Chuck").gameObject;
            WindfallHelper.Reskin(wiseThrow, Windfall.assetBundle.LoadAsset<Mesh>("Wise Throw"), Windfall.assetBundle.LoadAsset<Material>("Bumbo the Wise"), null);
            WindfallHelper.ReTransform(wiseThrow, new Vector3(0.0778f, 0.345f, -0.0604f), new Vector3(0f, 0f, 0f), new Vector3(100f, 100f, 100f), string.Empty);

            GameObject wiseAim = wiseThrowParent.transform.Find("Windup").gameObject;
            WindfallHelper.Reskin(wiseAim, Windfall.assetBundle.LoadAsset<Mesh>("Wise Aim"), Windfall.assetBundle.LoadAsset<Material>("Bumbo the Wise"), null);
            WindfallHelper.ReTransform(wiseAim, new Vector3(-0.04f, 0.3273f, 0.0024f), new Vector3(0f, 180f, 50f), new Vector3(100f, 100f, 100f), string.Empty);

            return wiseThrowParent;
        }

        //Bum-bo the Wise VS
        [HarmonyPostfix, HarmonyPatch(typeof(BossSignView), nameof(BossSignView.SetBumbo))]
        static void BossSignView_SetBumbo(BossSignView __instance, CharacterSheet.BumboType _bumbo)
        {
            Transform bumbo = __instance.bumbos[0].transform.parent;
            GameObject wise = bumbo.Find("BumboWise")?.gameObject;

            if (_bumbo != (CharacterSheet.BumboType)10)
            {
                wise?.SetActive(false);
                return;
            }

            if (wise == null) wise = CreateWiseVS(bumbo);

            wise.SetActive(true);
        }

        private static GameObject CreateWiseVS(Transform bumbo)
        {
            GameObject wise = GameObject.Instantiate(bumbo.Find("BumboStout").gameObject, bumbo);
            wise.name = "BumboWise";

            WindfallHelper.Reskin(wise, null, null, Windfall.assetBundle.LoadAsset<Texture2D>("Bumbo the Wise VS"), false);

            return wise;
        }

        //Make Bum-bo the Wise use the same voice lines as Bum-bo the Dead
        [HarmonyPrefix, HarmonyPatch(typeof(SoundsView), nameof(SoundsView.PlayBumboSound))]
        static void SoundsView_PlayBumboSound(ref CharacterSheet.BumboType BumboType)
        {
            if (BumboType == (CharacterSheet.BumboType)10) BumboType = CharacterSheet.BumboType.TheDead;
        }

        //Add Bum-bo the Wise to CharacterSheets
        [HarmonyPrefix, HarmonyPatch(typeof(CharacterSheet), "Awake")]
        static void CharacterSheet_Awake(CharacterSheet __instance)
        {
            BumboObject[] newBumboList = new BumboObject[11];
            for (int i = 0; i < __instance.bumboList.Length; i++) newBumboList[i] = __instance.bumboList[i];
            newBumboList[(int)(CharacterSheet.BumboType)10] = new BumboObject(WiseObject);
            __instance.bumboList = newBumboList;
        }

        //Add Bum-bo the Wise face and vanilla tooltip
        [HarmonyPostfix, HarmonyPatch(typeof(BumboFacesController), "Start")]
        static void BumboFacesController_Start(BumboFacesController __instance)
        {
            GameObject wiseFace = GameObject.Instantiate(__instance.bumboFace, __instance.bumboFace.transform.parent);
            WindfallHelper.Reskin(wiseFace, Windfall.assetBundle.LoadAsset<Mesh>("Wise Head"), Windfall.assetBundle.LoadAsset<Material>("Bumbo the Wise"), null);
            WindfallHelper.ReTransform(wiseFace, new Vector3(-0.0141f, 0.0222f, -0.002f), new Vector3(90f, 0f, 0f), new Vector3(30f, 30f, 30f), string.Empty);
            wiseFace.SetActive(__instance.app.model.characterSheet.bumboType == (CharacterSheet.BumboType)10);

            string[] tooltips = (string[])AccessTools.Field(typeof(BumboFacesController), "toolTips").GetValue(__instance);

            string[] newTooltips = new string[11];
            for (int i = 0; i < newTooltips.Length; i++)
            {
                if (i == (int)(CharacterSheet.BumboType)10)
                {
                    newTooltips[i] = WindfallTooltipDescriptions.WISE_DESCRIPTION;
                    continue;
                }

                string tooltip;

                if (i < tooltips.Length && tooltips[i] != null) tooltip = tooltips[i];
                else tooltip = null;
                newTooltips[i] = tooltip;
            }
            AccessTools.Field(typeof(BumboFacesController), "toolTips").SetValue(__instance, newTooltips);
        }

        //Add Bum-bo the Wise death image
        [HarmonyPostfix, HarmonyPatch(typeof(DeadBumboPicView), nameof(DeadBumboPicView.SetPic))]
        static void DeadBumboPicView_SetPic(DeadBumboPicView __instance, CharacterSheet.BumboType _bumbo_type)
        {
            GameObject wise = __instance.transform.Find("Wise")?.gameObject;
            if (wise == null) wise = GameObject.Instantiate(__instance.brave, __instance.transform);
            wise.name = "Wise";
            WindfallHelper.Reskin(wise, null, null, Windfall.assetBundle.LoadAsset<Texture2D>("Bumbo the Wise Dead"), false);
            wise.SetActive(_bumbo_type == (CharacterSheet.BumboType)10);
        }
    }
}
