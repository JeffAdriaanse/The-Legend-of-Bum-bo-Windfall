using HarmonyLib;
using I2.Loc;
using System;
using System.Collections.Generic;
using TMPro;

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
        static bool triggered = false;
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

        public static void ModifyLanguageSourceData()
        {
            if (triggered)
            {
                return;
            }

            triggered = true;

            //Modify language
            ModifyEnglish();
            //Add language
            AddEnglish();
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

        private static void ModifyEnglish()
        {
            if (LanguageSourceData == null)
            {
                return;
            }

            foreach (string term in EnglishTextModifications.Keys)
            {
                if (term == "Spells/ROCK_FRIENDS_DESCRIPTION" && !WindfallPersistentDataController.LoadData().implementBalanceChanges)
                {
                    continue;
                }

                TermData termData = LanguageSourceData.GetTermData(term);
                if (termData != null)
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

        public static void AddEnglish()
        {
            if (LanguageSourceData == null)
            {
                return;
            }

            foreach (string term in EnglishTextAdditions.Keys)
            {
                TermData termData = LanguageSourceData.AddTerm(term);

                if (EnglishTextAdditions.TryGetValue(term, out string value))
                {
                    termData.SetTranslation(0, value);
                }
            }
        }

        public static string GetEnglishText(string term, string category)
        {
            TermData termData = null;

            if (term == null)
            {
                return string.Empty;
            }

            if (category != null)
            {
                termData = LanguageSourceData.GetTermData(category + "/" + term, false);
            }

            if (termData == null)
            {
                termData = LanguageSourceData.GetTermData(term, true);
            }

            if (termData == null)
            {
                return string.Empty;
            }

            return termData.GetTranslation(0);
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
                    { "Trinkets/WISE_HIDDEN_NAME", "Wise's Ability"},

                    //Trinket descriptions
                    { "Trinkets/BAG_O_SUCKING_DESCRIPTION", "Gain Mana when You Hit!"},
                    { "Trinkets/GLITCH_DESCRIPTION", "What Will It Be?"},
                    { "Trinkets/NINE_VOLT_DESCRIPTION", "Items May Gain Charges"},
                    { "Trinkets/PINKY_DESCRIPTION", "May Gain Wilds on Kills"},
                    { "Trinkets/RAINBOW_TICK_DESCRIPTION", "Reduces Spell Cost"},
                    { "Trinkets/STRAY_BARB_DESCRIPTION", "Attackers May Take Damage"},
                    { "Trinkets/THERMOS_DESCRIPTION", "Charge All Items + Heal"},
                    { "Trinkets/WISE_HIDDEN_DESCRIPTION", "Compost Bag"},

                    //Gizzarda
                    { "Bosses/GIZZARDA_TIP_2", "\"she's very resistant!\nplan ahead!\""},

                    //Characters
                    { "Characters/EMPTY_UNLOCK", "beat the game twice with the first five characters."},

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

                    //GUI Notifications
                    { "GUI Notifications/ENLARGE_TILE", "Pick Tile to Enlarge!"},
                    { "GUI Notifications/NO_VIABLE_SPELLS", "No Viable Spells"},

                    //Unlocks
                    { "Unlocks/EVERYTHING_IS_TERRIBLE_NEW", "EVERYTHING IS TERRIBLE!"},
                    { "Unlocks/THE_GAME_IS_HARDER", "THE GAME IS HARDER!"},

                    //Menu
                    { "Menu/WINDFALL_OPTIONS", "Windfall Options"},
                };
                return additions;
            }

        }
    }
}
