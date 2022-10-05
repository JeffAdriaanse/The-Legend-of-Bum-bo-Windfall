using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using TMPro;
using I2.Loc;

namespace The_Legend_of_Bum_bo_Windfall
{
    class TypoFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(TypoFixes));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying corrections to typos");
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
    }

    static class LocalizationModifier
    {
        static bool triggered = false;
        static LanguageSourceData languageSourceData;
        public static void ModifyLanguageSourceData()
        {
            if (!triggered)
            {
                if (LocalizationManager.Sources.Count > 0)
                {
                    languageSourceData = LocalizationManager.Sources[0];
                }
                if (languageSourceData == null)
                {
                    return;
                }
                triggered = true;

                //Modify language
                ModifyEnglish();
            }
        }

        public static void ModifyEnglish()
        {
            foreach (string term in EnglishTextModifications.Keys)
            {
                TermData termData = languageSourceData.GetTermData(term);
                if (termData == null)
                {
                    Console.WriteLine("[The Legend of Bum-bo: Windfall] Term " + term + " is null");
                    break;
                }
                else
                {
                    string[] languages = termData.Languages;
                    int englishIndex = 0;
                    for (int languageCounter = 0; languageCounter < languages.Length; languageCounter++)
                    {
                        if (languages[languageCounter].Equals("english", StringComparison.OrdinalIgnoreCase))
                        {
                            englishIndex = languageCounter;
                        }
                    }

                    if (EnglishTextModifications.TryGetValue(term, out string value))
                    {
                        termData.SetTranslation(englishIndex, value);
                    }
                }
            }
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
                    { "Spells/ROCK_FRIENDS_DESCRIPTION", "Hits Enemies = to Spell Damage"},
                    { "Spells/STICK_DESCRIPTION", "Whack Away!"},

                    //Trinkets
                    { "Trinkets/CURVED_HORN_NAME", "Curved Horn"},
                    { "Trinkets/DRAKULA_TEETH_NAME", "Dracula Teeth"},

                    //Trinket descriptions
                    { "Trinkets/BAG_O_SUCKING_DESCRIPTION", "Gain Mana when You Hit!"},
                    { "Trinkets/GLITCH_DESCRIPTION", "What Will It Be?"},
                    { "Trinkets/NINE_VOLT_DESCRIPTION", "Items May Gain Charges"},
                    { "Trinkets/PINKY_DESCRIPTION", "May Gain Wilds on Kills"},
                    { "Trinkets/RAINBOW_TICK_DESCRIPTION", "Reduces Spell Cost"},

                    //Gizzarda
                    { "Bosses/GIZZARDA_TIP_2", "\"she's very resistant!\nplan ahead!\""},

                    //Characters
                    { "Characters/EMPTY_UNLOCK", "beat the game twice with the first five characters."},
                };
                return modifications;
            }
        }
    }
}
