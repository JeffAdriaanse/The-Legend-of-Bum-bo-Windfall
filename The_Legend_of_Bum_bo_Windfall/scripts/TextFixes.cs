using HarmonyLib;
using I2.Loc;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    class TextFixes
    {
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

            //Modify Spanish
            string spanishName = "Spanish";
            ModifyLanguage(LanguageSourceData.GetLanguageIndex(spanishName), GetLanguageTextModification(spanishName));
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

                    if (key != null && value != null) dictionary[key] = value;
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
                if (textMeshProUGUI != null) textMeshProUGUI.font = font;
                else if (textMeshPro != null) textMeshPro.font = font;
            }
        }

        private static void ModifyLanguage(int lanugageIndex, Dictionary<string, string> textModifications)
        {
            if (LanguageSourceData == null) return;

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
            if (LanguageSourceData == null) return;

            foreach (string term in textAdditions.Keys)
            {
                TermData termData = LanguageSourceData.AddTerm(term);
                if (textAdditions.TryGetValue(term, out string value)) termData.SetTranslation(lanugageIndex, value);
            }
        }

        public static string GetLanguageText(string term, string category)
        {
            return GetLanguageText(LanguageSourceData.GetLanguageIndex(LocalizationManager.CurrentLanguage), term, category);
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
    }
}
