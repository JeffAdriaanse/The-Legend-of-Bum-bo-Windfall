using HarmonyLib;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SpellElement;
using static TrinketElement;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class CollectibleImport
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(CollectibleImport));
        }

        //Modify SpellModel data
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModel), "spellKA", MethodType.Getter)]
        static void SpellModel_spellKA(SpellModel __instance, ref Dictionary<SpellName, string> __result)
        {
            Dictionary<SpellName, string> returnedDict = new Dictionary<SpellName, string>(__result);
            returnedDict.AddRange(CollectibleImportData.spellKA);
            __result = returnedDict;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModel), "spellNames", MethodType.Getter)]
        static void SpellModel_spellNames(SpellModel __instance, ref Dictionary<string, SpellName> __result)
        {
            Dictionary<string, SpellName> returnedDict = new Dictionary<string, SpellName>(__result);
            returnedDict.AddRange(CollectibleImportData.spellNames);
            __result = returnedDict;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModel), "spells", MethodType.Getter)]
        static void SpellModel_spells(SpellModel __instance, ref Dictionary<SpellName, SpellElement> __result)
        {
            Dictionary<SpellName, SpellElement> returnedDict = new Dictionary<SpellName, SpellElement>(__result);
            returnedDict.AddRange(CollectibleImportData.spells);
            __result = returnedDict;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModel), "validSpells", MethodType.Getter)]
        static void SpellModel_validSpells(SpellModel __instance, ref List<SpellName> __result)
        {
            List<SpellName> returnedList = new List<SpellName>(__result);
            returnedList.AddRange(CollectibleImportData.validSpells);
            __result = returnedList;
        }

        //Modify TrinketModel data
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), "trinketKA", MethodType.Getter)]
        static void TrinketModel_trinketKA(TrinketModel __instance, ref Dictionary<TrinketName, string> __result)
        {
            Dictionary<TrinketName, string> returnedDict = new Dictionary<TrinketName, string>(__result);
            returnedDict.AddRange(CollectibleImportData.trinketKA);
            __result = returnedDict;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), "trinketNames", MethodType.Getter)]
        static void TrinketModel_trinketNames(TrinketModel __instance, ref Dictionary<string, TrinketName> __result)
        {
            Dictionary<string, TrinketName> returnedDict = new Dictionary<string, TrinketName>(__result);
            returnedDict.AddRange(CollectibleImportData.trinketNames);
            __result = returnedDict;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), "trinkets", MethodType.Getter)]
        static void TrinketModel_trinkets(TrinketModel __instance, ref Dictionary<TrinketName, TrinketElement> __result)
        {
            Dictionary<TrinketName, TrinketElement> returnedDict = new Dictionary<TrinketName, TrinketElement>(__result);
            returnedDict.AddRange(CollectibleImportData.trinkets);
            __result = returnedDict;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), "validTrinkets", MethodType.Getter)]
        static void TrinketModel_validTrinkets(TrinketModel __instance, ref List<TrinketName> __result)
        {
            List<TrinketName> returnedList = new List<TrinketName>(__result);
            returnedList.AddRange(CollectibleImportData.validTrinkets);
            __result = returnedList;
        }

        //Patch: Fix spell texture maps
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init_Collectible_Textures(BumboController __instance)
        {
            ReplaceSpellIconMaterials(__instance.app);
            AddSpellMaterialPages(__instance.app);
        }

        //Patch: Fix trinket texture maps
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketModel), nameof(TrinketModel.FillDictionary))]
        static void TrinketModel_FillDictionary_Prefix(TrinketModel __instance, out bool __state)
        {
            __state = __instance.populatedMaterial;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), nameof(TrinketModel.FillDictionary))]
        static void TrinketModel_FillDictionary(TrinketModel __instance, bool __state)
        {
            if (!__state)
            {
                ReplaceTrinketIconMaterials(__instance);
            }
        }

        //Adds spell icon material pages
        private static void AddSpellMaterialPages(BumboApplication app)
        {
            AssetBundle assets = Windfall.assetBundle;

            SpellCategory[] spellCategories = new SpellCategory[]
            {
                SpellCategory.Attack,
            };

            foreach (SpellCategory spellCategory in spellCategories)
            {
                Material activeMaterial = null;
                Material inactiveMaterial = null;

                //Assign hardcoded texture asset paths to each texture of both active and inactive spell materials
                switch (spellCategory)
                {
                    case SpellCategory.Attack:
                        activeMaterial = new Material(WindfallHelper.app.model.spellModel.Icon(spellCategory, true, 0));
                        if (activeMaterial != null)
                        {
                            if (assets.Contains("Attack Spells Active Basecolor"))
                            {
                                activeMaterial.SetTexture("_MainTex", (Texture)assets.LoadAsset("Attack Spells Active Basecolor"));
                            }
                            if (assets.Contains("Attack Spells Active Metallic"))
                            {
                                activeMaterial.SetTexture("_MetallicGlossMap", (Texture)assets.LoadAsset("Attack Spells Active Metallic"));
                            }
                        }

                        inactiveMaterial = new Material(WindfallHelper.app.model.spellModel.Icon(spellCategory, false, 0));
                        if (inactiveMaterial != null)
                        {
                            if (assets.Contains("Attack Spells Inactive Basecolor"))
                            {
                                inactiveMaterial.SetTexture("_MainTex", (Texture)assets.LoadAsset("Attack Spells Inactive Basecolor"));
                            }
                            if (assets.Contains("Attack Spells Inactive Metallic"))
                            {
                                inactiveMaterial.SetTexture("_MetallicGlossMap", (Texture)assets.LoadAsset("Attack Spells Inactive Metallic"));
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (activeMaterial == null) { return; }
                if (inactiveMaterial == null) { return; }

                //Loop through spell model material arrays
                for (int spellMaterialCounter = 0; spellMaterialCounter < app.model.spellModel.spellMaterial.Length; spellMaterialCounter++)
                {
                    //Find array for current spell category
                    if (app.model.spellModel.spellMaterial[spellMaterialCounter] != null && app.model.spellModel.spellMaterial[spellMaterialCounter].category == spellCategory)
                    {
                        //Add material pages
                        app.model.spellModel.spellMaterial[spellMaterialCounter].active.Add(activeMaterial);
                        app.model.spellModel.spellMaterial[spellMaterialCounter].inactive.Add(inactiveMaterial);
                    }
                }
            }
        }

        //Adds a spell icon material page to the SpellMaterial of a spell category
        private static void AddSpellMaterialPage(SpellMaterial spellMaterial, Material activeMaterial, Material inactiveMaterial)
        {
            if (spellMaterial == null) { return; }

            //Add active material
            if (activeMaterial == null || spellMaterial.active == null) { return; }
            spellMaterial.active.Add(activeMaterial);

            //Add inactive material
            if (inactiveMaterial == null || spellMaterial.inactive == null) { return; }
            spellMaterial.inactive.Add(inactiveMaterial);
        }

        private static readonly string spell_attack_ambient_occlusion_path = "Attack Spells Ambient Occlusion";
        private static readonly string spell_attack_normal_path = "Attack Spells Normal";
        private static readonly string collectible_page_ambient_occlusion_path = "Collectible_page_ambient_occlusion";
        private static readonly string collectible_page_height_path = "Collectible_page_height";
        private static readonly string collectible_page_normal_path = "Collectible_page_normal";
        private static readonly string spell_use_metallic_path = "Spell_use_metallic_V3";
        private static readonly string trinket_page_metallic_path = "Trinket_Page_metallic";
        private static readonly string spell_defense_active_basecolor_path = "Spell_Defense_1_active_basecolor";
        private static readonly string spell_defense_inactive_basecolor_path = "Spell_Defense_1_inactive_basecolor";

        private static readonly string[] defaultMaterialTextureMapIDs = new string[]
        {
            "_MainTex",
            "_OcclusionMap",
            "_ParallaxMap",
            "_BumpMap",
            "_MetallicGlossMap",
        };

        private static void ReplaceSpellIconMaterials(BumboApplication app)
        {
            AssetBundle assets = Windfall.assetBundle;

            SpellCategory[] spellCategories = new SpellCategory[]
            {
                SpellCategory.Attack,
                SpellCategory.Defense,
                SpellCategory.Puzzle,
                SpellCategory.Use,
            };

            foreach (SpellCategory spellCategory in spellCategories)
            {
                string[] activeTexturePaths = new string[5];
                string[] inactiveTexturePaths = new string[5];

                //Assign hardcoded texture asset paths to each texture of both active and inactive spell materials
                switch (spellCategory)
                {
                    case SpellCategory.Attack:
                        activeTexturePaths[1] = spell_attack_ambient_occlusion_path;
                        activeTexturePaths[3] = spell_attack_normal_path;
                        inactiveTexturePaths[1] = spell_attack_ambient_occlusion_path;
                        inactiveTexturePaths[3] = spell_attack_normal_path;
                        break;
                    case SpellCategory.Defense:
                        activeTexturePaths[0] = spell_defense_active_basecolor_path;
                        inactiveTexturePaths[0] = spell_defense_inactive_basecolor_path;
                        break;
                    case SpellCategory.Puzzle:
                        inactiveTexturePaths[1] = collectible_page_ambient_occlusion_path;
                        inactiveTexturePaths[2] = collectible_page_height_path;
                        inactiveTexturePaths[3] = collectible_page_normal_path;
                        break;
                    case SpellCategory.Use:
                        inactiveTexturePaths[1] = collectible_page_ambient_occlusion_path;
                        inactiveTexturePaths[2] = collectible_page_height_path;
                        inactiveTexturePaths[3] = collectible_page_normal_path;
                        inactiveTexturePaths[4] = spell_use_metallic_path;
                        break;
                    default:
                        break;
                }

                Dictionary<string, Texture> activeTextureReplacements = new Dictionary<string, Texture>();
                Dictionary<string, Texture> inactiveTextureReplacements = new Dictionary<string, Texture>();

                //Load replacement texture assets
                for (int activeIterator = 0; activeIterator < 2; activeIterator++)
                {
                    bool active = activeIterator == 0;

                    for (int textureMapIterator = 0; textureMapIterator < defaultMaterialTextureMapIDs.Length; textureMapIterator++)
                    {
                        string defaultMaterialTextureMapID = defaultMaterialTextureMapIDs[textureMapIterator];
                        if (defaultMaterialTextureMapID == null || defaultMaterialTextureMapID == string.Empty) { continue; }

                        //Use different texture paths for active vs inactive materials
                        string texturePath;
                        if (active) { texturePath = activeTexturePaths[textureMapIterator]; }
                        else { texturePath = inactiveTexturePaths[textureMapIterator]; }

                        if (texturePath == null || texturePath == string.Empty || !assets.Contains(texturePath)) { continue; }

                        Texture texture = assets.LoadAsset<Texture>(texturePath);
                        if (texture == null) { continue; }

                        //Store texture replacements separately for active vs inactive materials
                        if (active) { activeTextureReplacements.Add(defaultMaterialTextureMapID, texture); }
                        else { inactiveTextureReplacements.Add(defaultMaterialTextureMapID, texture); }
                    }
                }

                //Loop through spell model material arrays
                for (int spellMaterialCounter = 0; spellMaterialCounter < app.model.spellModel.spellMaterial.Length; spellMaterialCounter++)
                {
                    //Find array for current spell category
                    if (app.model.spellModel.spellMaterial[spellMaterialCounter] != null && app.model.spellModel.spellMaterial[spellMaterialCounter].category == spellCategory)
                    {
                        //Access active materials for each page of current spell category
                        for (int materialCounter = 0; materialCounter < app.model.spellModel.spellMaterial[spellMaterialCounter].active.Count; materialCounter++)
                        {
                            Material unmodifiedMaterial = app.model.spellModel.spellMaterial[spellMaterialCounter].active[materialCounter];
                            if (unmodifiedMaterial == null) { continue; }

                            //Replace material textures
                            Material modifiedMaterial = ReplaceMaterialTextures(unmodifiedMaterial, activeTextureReplacements);
                            app.model.spellModel.spellMaterial[spellMaterialCounter].active[materialCounter] = modifiedMaterial;
                        }

                        //Access inactive materials for each page of current spell category
                        for (int materialCounter = 0; materialCounter < app.model.spellModel.spellMaterial[spellMaterialCounter].inactive.Count; materialCounter++)
                        {
                            Material unmodifiedMaterial = app.model.spellModel.spellMaterial[spellMaterialCounter].inactive[materialCounter];
                            if (unmodifiedMaterial == null) { continue; }

                            //Replace material textures
                            Material modifiedMaterial = ReplaceMaterialTextures(unmodifiedMaterial, inactiveTextureReplacements);
                            app.model.spellModel.spellMaterial[spellMaterialCounter].inactive[materialCounter] = modifiedMaterial;
                        }
                    }
                }
            }
        }

        private static void ReplaceTrinketIconMaterials(TrinketModel trinketModel)
        {
            AssetBundle assets = Windfall.assetBundle;

            TrinketCategory[] trinketCategories = new TrinketCategory[]
            {
                TrinketCategory.Special,
            };

            foreach (TrinketCategory trinketCategory in trinketCategories)
            {
                string[] texturePaths = new string[5];

                //Assign hardcoded texture asset paths to each texture of the trinket materials
                switch (trinketCategory)
                {
                    case TrinketCategory.Special:
                        texturePaths[1] = collectible_page_ambient_occlusion_path;
                        texturePaths[2] = collectible_page_height_path;
                        texturePaths[3] = collectible_page_normal_path;
                        texturePaths[4] = trinket_page_metallic_path;
                        break;
                    default:
                        break;
                }

                Dictionary<string, Texture> textureReplacements = new Dictionary<string, Texture>();

                for (int textureMapIterator = 0; textureMapIterator < defaultMaterialTextureMapIDs.Length; textureMapIterator++)
                {
                    //Assign texture IDs
                    string defaultMaterialTextureMapID = defaultMaterialTextureMapIDs[textureMapIterator];
                    if (defaultMaterialTextureMapID == null || defaultMaterialTextureMapID == string.Empty) { continue; }

                    //Assign texture paths
                    string texturePath = texturePaths[textureMapIterator];
                    if (texturePath == null || texturePath == string.Empty || !assets.Contains(texturePath)) { continue; }

                    //Load textures
                    Texture texture = assets.LoadAsset<Texture>(texturePath);
                    if (texture == null) { continue; }

                    //Store texture replacements
                    textureReplacements.Add(defaultMaterialTextureMapID, texture);
                }

                //Find list for current trinket category
                if (trinketModel.trinketMaterial.ContainsKey(trinketCategory) && trinketModel.trinketMaterial[trinketCategory] != null)
                {
                    //Access materials for each page of current trinket category
                    for (int materialCounter = 0; materialCounter < trinketModel.trinketMaterial[trinketCategory].Count; materialCounter++)
                    {
                        Material unmodifiedMaterial = trinketModel.trinketMaterial[trinketCategory][materialCounter];
                        if (unmodifiedMaterial == null) { continue; }

                        //Replace material textures
                        Material modifiedMaterial = ReplaceMaterialTextures(unmodifiedMaterial, textureReplacements);
                        trinketModel.trinketMaterial[trinketCategory][materialCounter] = modifiedMaterial;
                    }
                }
            }
        }

        //Returns a copy of the given material with textures replaced according to the provided texture replacements 
        private static Material ReplaceMaterialTextures(Material material, Dictionary<string, Texture> textureReplacements)
        {
            if (material == null) { return material; }

            if (textureReplacements == null || textureReplacements.Count == 0) { return material; }

            string[] texturePropertyNames = material.GetTexturePropertyNames();
            if (texturePropertyNames == null || texturePropertyNames.Length == 0) { return material; }

            //Replace each texture
            foreach (KeyValuePair<string, Texture> textureReplacement in textureReplacements)
            {
                string textureID = textureReplacement.Key;
                if (textureID == null || !texturePropertyNames.Contains(textureID)) { continue; }

                Texture texture = textureReplacement.Value;
                if (texture == null) { continue; }

                material.SetTexture(textureID, texture);
            }

            return material;
        }
    }

    public static class CollectibleImportData
    {
        //***************SpellModel***************//
        public static Dictionary<SpellName, string> spellKA
        {
            get
            {
                Dictionary<SpellName, string> spellKA = new Dictionary<SpellName, string>()
                {
                    {
                        (SpellName)1000,
                        "PLASMA_BALL_NAME"
                    },
                    {
                        (SpellName)1001,
                        "MAGNIFYING_GLASS_NAME"
                    },
                    {
                        (SpellName)1002,
                        "READING_STONE_NAME"
                    },
                };
                return spellKA;
            }
        }
        public static Dictionary<string, SpellName> spellNames
        {
            get
            {
                Dictionary<string, SpellName> spellNames = new Dictionary<string, SpellName>()
                {
                    {
                        "1000",
                        (SpellName)1000
                    },
                    {
                        "1001",
                        (SpellName)1001
                    },
                    {
                        "1002",
                        (SpellName)1002
                    },
                };
                return spellNames;
            }
        }
        public static Dictionary<SpellName, SpellElement> spells
        {
            get
            {
                Dictionary<SpellName, SpellElement> spells = new Dictionary<SpellName, SpellElement>()
                {
                    {
                        (SpellName)1000,
                        new PlasmaBallSpell()
                    },
                    {
                        (SpellName)1001,
                        new MagnifyingGlassSpell()
                    },
                    {
                        (SpellName)1002,
                        new ReadingStoneSpell()
                    },
                };
                return spells;
            }
        }
        public static List<SpellName> validSpells
        {
            get
            {
                List<SpellName> validSpells = new List<SpellName>()
                {
                    (SpellName)1000,
                    (SpellName)1001,
                };
                return validSpells;
            }
        }

        //***************TrinketModel***************//
        public static Dictionary<TrinketName, string> trinketKA
        {
            get
            {
                Dictionary<TrinketName, string> trinketKA = new Dictionary<TrinketName, string>()
                {
                    {
                        (TrinketName)1000,
                        "OCCULT_HIDDEN_NAME"
                    },
                    {
                        (TrinketName)1001,
                        "WISE_HIDDEN_NAME"
                    },
                };
                return trinketKA;
            }
        }
        public static Dictionary<string, TrinketName> trinketNames
        {
            get
            {
                Dictionary<string, TrinketName> trinketNames = new Dictionary<string, TrinketName>()
                {
                    {
                        "1000",
                        (TrinketName)1000
                    },
                    {
                        "1001",
                        (TrinketName)1001
                    },
                };
                return trinketNames;
            }
        }
        public static Dictionary<TrinketName, TrinketElement> trinkets
        {
            get
            {
                Dictionary<TrinketName, TrinketElement> trinkets = new Dictionary<TrinketName, TrinketElement>()
                {
                    {
                        (TrinketName)1000,
                        new OccultHiddenTrinket()
                    },
                    {
                        (TrinketName)1001,
                        new WiseHiddenTrinket()
                    },
                };
                return trinkets;
            }
        }
        public static List<TrinketName> validTrinkets
        {
            get
            {
                List<TrinketName> validTrinkets = new List<TrinketName>()
                {
                };
                return validTrinkets;
            }
        }
    }
}
