using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class CharacterImport
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(CharacterImport));
        }

        private static readonly int WISE_INSERT_POSITION = 6;
        [HarmonyPostfix, HarmonyPatch(typeof(SelectCharacterView), "Start")]
        static void SelectCharacterView_Start(SelectCharacterView __instance)
        {
            List<CharacterSheet.BumboType> bumboTypes = (List<CharacterSheet.BumboType>)AccessTools.Field(typeof(SelectCharacterView), "bumboTypes").GetValue(__instance);
            if (bumboTypes != null && bumboTypes.Count >= WISE_INSERT_POSITION && !bumboTypes.Contains((CharacterSheet.BumboType)10)) bumboTypes.Insert(WISE_INSERT_POSITION, (CharacterSheet.BumboType)10);
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
            luck = 2,
            puzzleDamage = 1,
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
                    toothCost = 3,
                }
            },
            startingTrinkets = new TrinketName[] { TrinketName.Hoof },
            hiddenTrinket = (TrinketName)1001,
        };

        private static readonly string WISE_DESCRIPTION = "moved tile becomes wild!";

        private static readonly float WISE_SCALE = 1f;
        private static readonly Vector3 WISE_LOCALSCALE = new Vector3(WISE_SCALE, WISE_SCALE, WISE_SCALE);
        private static readonly Vector3 WISE_SELECT_LOCALPOSITION = new Vector3(-0.0001f, 0f, 0.0044f);
        private static readonly Vector3 WISE_SELECT_LOCALROTATION = new Vector3(90f, 0f, 0f);
        private static readonly Vector3 WISE_DUCK_LOCALPOSITION = new Vector3(-0.0001f, -0.0004f, 0.0044f);
        private static readonly Vector3 WISE_DUCK_LOCALROTATION = new Vector3(270f, 90f, 0f);
        private static readonly Vector3 WISE_UPPERCUT_LOCALPOSITION = new Vector3(0.0001f, 0f, 0.0044f);
        private static readonly Vector3 WISE_UPPERCUT_LOCALROTATION = new Vector3(90f, 14f, 0f);
        private static readonly Vector3 WISE_ANTICIPATE_LOCALPOSITION = new Vector3(0.0002f, -0.0002f, 0.0043f);
        private static readonly Vector3 WISE_ANTICIPATE_LOCALROTATION = new Vector3(270f, 58f, 0f);

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
            Texture2D wiseTexture = Windfall.assetBundle.LoadAsset<Texture2D>("Bumbo the Wise");

            //Bumbo select
            GameObject wiseSelect = wiseSelectParent.transform.Find("Bumbo Select").gameObject;
            WindfallHelper.Reskin(wiseSelect, Windfall.assetBundle.LoadAsset<Mesh>("Wise Select"), null, wiseTexture, WISE_SELECT_LOCALPOSITION, WISE_SELECT_LOCALROTATION, WISE_LOCALSCALE);

            //Bumbo duck
            GameObject wiseDuck = wiseSelectParent.transform.Find("Bumbo Duck").gameObject;
            WindfallHelper.Reskin(wiseDuck, Windfall.assetBundle.LoadAsset<Mesh>("Wise Duck"), null, wiseTexture, WISE_DUCK_LOCALPOSITION, WISE_DUCK_LOCALROTATION, WISE_LOCALSCALE);

            //Bumbo uppercut
            GameObject wiseUppercut = wiseSelectParent.transform.Find("Bumbo Uppercut").gameObject;
            WindfallHelper.Reskin(wiseUppercut, Windfall.assetBundle.LoadAsset<Mesh>("Wise Uppercut"), null, wiseTexture, WISE_UPPERCUT_LOCALPOSITION, WISE_UPPERCUT_LOCALROTATION, WISE_LOCALSCALE);

            //Bumbo anticipate
            GameObject wiseAnticipate = wiseSelectParent.transform.Find("Lost Anticipate").gameObject;
            WindfallHelper.Reskin(wiseAnticipate, Windfall.assetBundle.LoadAsset<Mesh>("Wise Anticipate"), null, wiseTexture, WISE_ANTICIPATE_LOCALPOSITION, WISE_ANTICIPATE_LOCALROTATION, WISE_LOCALSCALE);

            //Collectibles
            Transform wiseSpells = wiseSelectView.spells.transform.Find("bumbo_select_brave_spells")?.Find("Brave Spells");

            //Spell 1
            BumboSelectSpellView spell1 = wiseSpells?.Find("Spell 1")?.GetComponent<BumboSelectSpellView>();
            spell1.bumboType = (CharacterSheet.BumboType)10;

            //Spell 2
            BumboSelectSpellView spell2 = wiseSpells?.Find("Spell 2 ")?.GetComponent<BumboSelectSpellView>();
            spell2.bumboType = (CharacterSheet.BumboType)10;

            //Trinket 1
            BumboSelectTrinketView trinket1 = wiseSelectView.spells.transform.Find("bumbo_select_brave_spells")?.Find("Brave Trinkets")?.Find("Trinket 1")?.GetComponent<BumboSelectTrinketView>();
            trinket1.bumboType = (CharacterSheet.BumboType)10;

            //Deactivate object
            wiseSelectView.gameObject.SetActive(false);

            //Add character description
            CharDescView charDescView = GameObject.FindObjectOfType<CharDescView>();
            GameObject[] newText = new GameObject[11];
            for (int i = 0; i < newText.Length; i++)
            {
                if (i == (int)(CharacterSheet.BumboType)10)
                {
                    newText[i] = GameObject.Instantiate(newText[0], newText[0].transform.parent);
                    newText[i].GetComponent<TextMeshPro>().text = WISE_DESCRIPTION;
                    newText[i].SetActive(false);
                    continue;
                }

                GameObject textObject;
                if (i < charDescView.text.Length && charDescView.text[i] != null) textObject = charDescView.text[i];
                else textObject = new GameObject();
                newText[i] = textObject;
            }
            charDescView.text = newText;
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
            Texture2D wiseTexture = Windfall.assetBundle.LoadAsset<Texture2D>("Bumbo the Wise");

            //Bumbo select
            GameObject wiseSelect = wiseBumboAnimation.bumboSelect;
            WindfallHelper.Reskin(wiseSelect, Windfall.assetBundle.LoadAsset<Mesh>("Wise Select"), null, wiseTexture, WISE_SELECT_LOCALPOSITION, WISE_SELECT_LOCALROTATION, WISE_LOCALSCALE);

            //Bumbo duck
            GameObject wiseDuck = wiseBumboAnimation.bumboDuck;
            WindfallHelper.Reskin(wiseDuck, Windfall.assetBundle.LoadAsset<Mesh>("Wise Duck"), null, wiseTexture, WISE_DUCK_LOCALPOSITION, WISE_DUCK_LOCALROTATION, WISE_LOCALSCALE);

            //Bumbo uppercut
            GameObject wiseUppercut = wiseBumboAnimation.bumboUppercut;
            WindfallHelper.Reskin(wiseUppercut, Windfall.assetBundle.LoadAsset<Mesh>("Wise Uppercut"), null, wiseTexture, WISE_UPPERCUT_LOCALPOSITION, WISE_UPPERCUT_LOCALROTATION, WISE_LOCALSCALE);

            //Bumbo anticipate
            GameObject wiseAnticipate = wiseBumboAnimation.bumboBackside;
            WindfallHelper.Reskin(wiseAnticipate, Windfall.assetBundle.LoadAsset<Mesh>("Wise Anticipate"), null, wiseTexture, WISE_ANTICIPATE_LOCALPOSITION, WISE_ANTICIPATE_LOCALROTATION, WISE_LOCALSCALE);
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

        [HarmonyPrefix, HarmonyPatch(typeof(SelectCharacterView), nameof(SelectCharacterView.BumboIsUnlocked))]
        static bool SelectCharacterView_BumboIsUnlocked(SelectCharacterView __instance, CharacterSheet.BumboType _type, ref bool __result)
        {
            if (_type == (CharacterSheet.BumboType)10)
            {
                __result = __instance.progression.unlocks[5];
                return false;
            }
            return true;
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
    }
}
