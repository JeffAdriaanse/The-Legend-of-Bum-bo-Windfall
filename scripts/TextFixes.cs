﻿using HarmonyLib;
using I2.Loc;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using static PlayerPrefsSaveData;

namespace The_Legend_of_Bum_bo_Windfall
{
    class TextFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(TextFixes));
        }

        //Patch: Modify source data
        [HarmonyPostfix, HarmonyPatch(typeof(BumboIntroController), "Start")]
        static void BumboIntroController_Start()
        {
            LocalizationModifier.ModifyLanguageSourceData();
        }

        //Patch: Modify source data
        [HarmonyPostfix, HarmonyPatch(typeof(BumboAltIntroController), "Start")]
        static void BumboAltIntroController_Start()
        {
            LocalizationModifier.ModifyLanguageSourceData();
        }

        //Patch: Fix Bag-O-Trash trinketKA
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), "trinketKA", MethodType.Getter)]
        static void TrinketModel_get_trinketKA(ref Dictionary<TrinketName, string> __result)
        {
            __result[TrinketName.BagOTrash] = "BAG_O_TRASH_NAME";
        }
    }

    static class LocalizationModifier
    {
        public static LanguageSourceData LanguageSourceData
        {
            get
            {
                if (LocalizationManager.Sources.Count > 0)
                {
                    return LocalizationManager.Sources[0];
                }
                return null;
            }
        }
        static bool triggered = false;
        public static void ModifyLanguageSourceData()
        {
            if (triggered) return;
            triggered = true;

            ReplaceEdFont();
            AddFallbackChineseFont();

            //Modify English
            string englishName = "English";
            ModifyLanguage(LanguageSourceData.GetLanguageIndex(englishName), GetLanguageTextModification(englishName));

            //Modify Chinese
            string chineseName = "Chinese";
            ModifyLanguage(LanguageSourceData.GetLanguageIndex(chineseName), GetLanguageTextModification(chineseName));
        }

        private static Dictionary<string, string> GetLanguageTextModification(string languageName)
        {
            TextAsset languageText = Windfall.assetBundle.LoadAsset<TextAsset>("localization" + languageName);
            if (languageText == null) return null;

            // Read and parse the XML file into a dictionary
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            try
            {
                XDocument doc = XDocument.Parse(languageText.text);

                foreach (var setting in doc.Descendants("Term"))
                {
                    var key = setting.Attribute("Key")?.Value;
                    var value = setting.Attribute("Value")?.Value;

                    if (key != null && value != null)
                    {
                        dictionary[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing XML: {ex.Message}");
            }

            if (dictionary == null) return null;
            return dictionary;
        }

        public static TMP_FontAsset edFont;
        private static void AcquireFonts()
        {
            //Unused method
            return;

            if (LanguageSourceData == null)
            {
                return;
            }

            if (LanguageSourceData.Assets != null && LanguageSourceData.mAssetDictionary != null)
            {
                if (LanguageSourceData.mAssetDictionary.TryGetValue("EdmundMcMillen SDF", out UnityEngine.Object edFontValue))
                {
                    if (edFontValue is TMP_FontAsset)
                    {
                        edFont = edFontValue as TMP_FontAsset;
                    }
                }
            }
        }

        private static void ReplaceEdFont()
        {
            TMP_FontAsset edFont = WindfallHelper.GetEdmundMcmillenFont();
            Material edFontOutline = Windfall.assetBundle.LoadAsset<Material>("Edmundmcmillen-Regular SDF - Outline");
            //Add font to assets
            LanguageSourceData.Assets.Add(edFont);
            LanguageSourceData.Assets.Add(edFontOutline);
            LanguageSourceData.UpdateAssetDictionary();
            //Assign as English font
            if (LanguageSourceData.mDictionary.TryGetValue("FONT_FACE_MAIN", out TermData termDataMain)) termDataMain.SetTranslation(LanguageSourceData.GetLanguageIndex("English"), edFont.name);
            if (LanguageSourceData.mDictionary.TryGetValue("FONT_FACE_OUTLINE", out TermData termDataOutline)) termDataOutline.SetTranslation(LanguageSourceData.GetLanguageIndex("English"), edFontOutline.name);
        }

        private static void AddFallbackChineseFont()
        {
            if (LanguageSourceData.mAssetDictionary.TryGetValue("SmartFinger(zh) SDF", out UnityEngine.Object smartFinger))
            {
                if (smartFinger is TMP_FontAsset)
                {
                    TMP_FontAsset smartFingerFont = smartFinger as TMP_FontAsset;
                    smartFingerFont.fallbackFontAssetTable.Add(Windfall.assetBundle.LoadAsset<TMP_FontAsset>("SmartFinger-Regular SDF"));
                }
            }
        }

        public static void ChangeFont(TextMeshProUGUI textMeshProUGUI, TextMeshPro textMeshPro, TMP_FontAsset font)
        {
            if (font != null)
            {
                if (textMeshProUGUI != null)
                {
                    textMeshProUGUI.font = font;
                }
                else if (textMeshPro != null)
                {
                    textMeshPro.font = font;
                }
            }
        }

        private static void ModifyLanguage(int lanugageIndex, Dictionary<string, string> textModifications)
        {
            if (LanguageSourceData == null)
            {
                return;
            }

            foreach (string term in textModifications.Keys)
            {
                TermData termData = null;
                if (!LanguageSourceData.ContainsTerm(term)) termData = LanguageSourceData.AddTerm(term);
                else termData = LanguageSourceData.GetTermData(term);

                if (termData != null)
                {
                    if (textModifications.TryGetValue(term, out string value)) termData.SetTranslation(lanugageIndex, value);
                }
            }
        }

        public static void AddToLanguage(int lanugageIndex, Dictionary<string, string> textAdditions)
        {
            if (LanguageSourceData == null)
            {
                return;
            }

            foreach (string term in textAdditions.Keys)
            {
                TermData termData = LanguageSourceData.AddTerm(term);

                if (textAdditions.TryGetValue(term, out string value))
                {
                    termData.SetTranslation(lanugageIndex, value);
                }
            }
        }

        public static string GetLanguageText(string term, string category)
        {
            TermData termData = null;

            if (term == null) return string.Empty;

            if (category != null) termData = LanguageSourceData.GetTermData(category + "/" + term, false);

            if (termData == null) termData = LanguageSourceData.GetTermData(term, true);

            if (termData == null) return string.Empty;

            string translation = termData.GetTranslation(LanguageSourceData.GetLanguageIndex(LocalizationManager.CurrentLanguage));
            return translation != null ? translation : string.Empty;
        }

        public static string GetLanguageText(int lanugageIndex, string term, string category)
        {
            TermData termData = null;

            if (term == null) return string.Empty;

            if (category != null) termData = LanguageSourceData.GetTermData(category + "/" + term, false);

            if (termData == null) termData = LanguageSourceData.GetTermData(term, true);

            if (termData == null) return string.Empty;

            string translation = termData.GetTranslation(lanugageIndex);
            return translation != null ? translation : string.Empty;
        }

        public static Dictionary<string, string> EnglishTextModifications
        {
            get
            {
                Dictionary<string, string> modifications = new Dictionary<string, string>
                {
                    //Spells
                    { "Spells/EXORCISM_KIT_NAME", "Exorcism Kit"},
                    { "Spells/MALLET_NAME", "Mallet"},
                    { "Spells/SLEIGHT_OF_HAND_NAME", "Sleight of Hand"},
                    { "Spells/TINY_DICE_NAME", "Tiny Dice"},

                    //Spell descriptions
                    { "Spells/BARBED_WIRE_DESCRIPTION", "Damage Attackers In Room"},
                    { "Spells/BUMBO_SHAKE_DESCRIPTION", "Shuffles the Puzzle Board"},
                    { "Spells/D4_DESCRIPTION", "Shuffles the Puzzle Board"},
                    { "Spells/DOG_TOOTH_DESCRIPTION", "Attack that Heals You"},
                    { "Spells/EUTHANASIA_DESCRIPTION", "Hurts an Attacking Enemy"},
                    { "Spells/PENTAGRAM_DESCRIPTION", "Gain +1 Spell Damage"},
                    { "Spells/ROCK_DESCRIPTION", "Hits the Furthest Enemy"},
                    { "Spells/ROCK_FRIENDS_DESCRIPTION", "Hits Enemies = to Spell Damage"},
                    { "Spells/STICK_DESCRIPTION", "Whack Away!"},
                    { "Spells/THE_VIRUS_DESCRIPTION", "Poisons Attacking Enemies"},
                    { "Spells/YELLOW_BELT_DESCRIPTION", "+5% to Dodge Attacks"},

                    //Spell GUI Notifications
                    //{ "GUI Notifications/ATTACKERS_GET_HURT", "Attackers\nIn Room\nGet Hurt!" }, //Barbed Wire (unchanged)
                    { "GUI Notifications/HURT_NEXT_ENEMY", "Hurt Next\nAttacking Enemy!" }, //Euthanasia
                    { "GUI Notifications/GAINED_ITEM_DAMAGE", "Gained Spell Damage!" }, //Experimental, spell damage up
                    { "GUI Notifications/SUPER_LIKELY_DODGE", "Super Likely\nTo Dodge\nNext Wave!" }, //Smoke Machine
                    { "GUI Notifications/TRY_DODGE_ATTACK", "Bum-bo\nTry To Dodge\nAttacks!" }, //Yellow Belt

                    //Trinkets
                    { "Trinkets/CURVED_HORN_NAME", "Curved Horn"},
                    { "Trinkets/DRAKULA_TEETH_NAME", "Dracula Teeth"},

                    //Trinket descriptions
                    { "Trinkets/BAG_O_SUCKING_DESCRIPTION", "Gain Mana when You Hit!"},
                    { "Trinkets/GLITCH_DESCRIPTION", "What Will It Be?"},
                    { "Trinkets/NINE_VOLT_DESCRIPTION", "Items May Gain Charges"},
                    { "Trinkets/PINKY_DESCRIPTION", "May Gain Wilds on Kills"},
                    { "Trinkets/RAINBOW_TICK_DESCRIPTION", "Reduces Spell Cost"},
                    { "Trinkets/STRAY_BARB_DESCRIPTION", "Attackers May Take Damage"},
                    { "Trinkets/THERMOS_DESCRIPTION", "Charge All Items + Heal"},

                    //Gizzarda
                    { "Bosses/GIZZARDA_TIP_2", "\"she's very resistant!\nplan ahead!\""},

                    //Characters
                    { "Characters/EMPTY_UNLOCK", "beat the game twice with the Brave, Nimble, Stout, Weird, and Dead."},

                    //Unlocks
                    { "Unlocks/BUMBO_SMASH", "BUM-BO SMASH"},
                    { "Unlocks/BUMBO_THE_DEAD", "BUM-BO THE DEAD"},
                    { "Unlocks/BUMBO_THE_EMPTY", "BUM-BO THE EMPTY"},
                    { "Unlocks/BUMBO_THE_LOST", "BUM-BO THE LOST"},
                    { "Unlocks/BUMBO_THE_NIMBLE", "BUM-BO THE NIMBLE"},
                    { "Unlocks/BUMBO_THE_STOUT", "BUM-BO THE STOUT"},
                    { "Unlocks/BUMBO_THE_WEIRD", "BUM-BO THE WEIRD"},
                    { "Unlocks/NEEDLE", "NEEDLE"},
                    { "Unlocks/STICK", "STICK"},
                    { "Unlocks/TOOTHPICK", "TOOTHPICK"},
                };
                return modifications;
            }
        }

        public static Dictionary<string, string> EnglishTextAdditions
        {
            get
            {
                Dictionary<string, string> additions = new Dictionary<string, string>
                {
                    //Spells
                    { "Spells/MAGNIFYING_GLASS_NAME", "Magnifying Glass"},
                    { "Spells/MAGNIFYING_GLASS_DESCRIPTION", "Enlarges a Random Tile"},
                    { "Spells/PLASMA_BALL_NAME", "Plasma Ball"},
                    { "Spells/PLASMA_BALL_DESCRIPTION", "Chain Attack"},
                    { "Spells/READING_STONE_NAME", "Reading Stone"},
                    { "Spells/READING_STONE_DESCRIPTION", "Enlarges a Tile"},

                    //Trinkets
                    { "Trinkets/COMPOST_BAG_NAME", "Compost Bag"},
                    { "Trinkets/COMPOST_BAG_DESCRIPTION", "Moved Tile Becomes Wild!"},
                    { "Trinkets/WISE_HIDDEN_NAME", "Wise's Ability"},
                    { "Trinkets/WISE_HIDDEN_DESCRIPTION", "Compost Bag"},
                    { "Trinkets/MILK_NAME", "Milk!"},
                    { "Trinkets/MILK_DESCRIPTION", "Gain +1 Movement when Hurt!"},

                    //GUI Notifications
                    { "GUI Notifications/ENLARGE_TILE", "Pick Tile to Enlarge!"},
                    { "GUI Notifications/NO_VIABLE_SPELLS", "No Viable Spells"},

                    //Characters
                    { "Characters/WISE_UNLOCK", "beat chapter 4."},

                    //Unlocks
                    { "Unlocks/EVERYTHING_IS_TERRIBLE_NEW", "EVERYTHING IS TERRIBLE!"},
                    { "Unlocks/THE_GAME_IS_HARDER", "THE GAME IS HARDER!"},
                    { "Unlocks/BUMBO_THE_WISE", "BUM-BO THE WISE"},
                    { "Unlocks/PLASMA_BALL", "PLASMA BALL"},
                    { "Unlocks/MAGNIFYING_GLASS", "MAGNIFYING GLASS"},
                    { "Unlocks/COMPOST_BAG", "COMPOST BAG"},

                    //Menu
                    { "Menu/WINDFALL_OPTIONS", "Windfall Options"},
                    { "Menu/MINI_CREDITS_ROLL", "Mini Credits Roll"},
                };
                return additions;
            }

        }
    }
}
