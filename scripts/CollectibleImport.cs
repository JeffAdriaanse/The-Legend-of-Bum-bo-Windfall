using HarmonyLib;
using MonoMod.Utils;
using System;
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

        //Patch: Fix spell texture maps and add new pages
        [HarmonyPrefix, HarmonyPatch(typeof(SpellModel), nameof(SpellModel.Icon))]
        static void SpellModel_Icon(SpellModel __instance)
        {
            if (!ObjectDataStorage.GetData<bool>(__instance, "hasImportedMaterials"))
            {
                ObjectDataStorage.StoreData<bool>(__instance, "hasImportedMaterials", true);

                ReplaceSpellIconMaterials(__instance);
                AddSpellMaterialPages(__instance);
            }
        }

        //Patch: Fix trinket texture maps
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketModel), nameof(TrinketModel.Icon))]
        static void TrinketModel_Icon(TrinketModel __instance)
        {
            if (!ObjectDataStorage.GetData<bool>(__instance, "hasImportedMaterials"))
            {
                ObjectDataStorage.StoreData<bool>(__instance, "hasImportedMaterials", true);

                ReplaceTrinketIconMaterials(__instance);
                AddTrinketMaterialPages(__instance);
            }
        }

        private static readonly string CollectiblePageAmbientOcclusionPath = "Collectible Page Ambient Occlusion";
        private static readonly string CollectiblePageHeightPath = "Collectible Page Height";
        private static readonly string CollectiblePageNormalPath = "Collectible Page Normal";

        private static readonly string CollectiblePageMetallicBlotchesPath = "Collectible Page Metallic Blotches";

        private static readonly string WindfallSpellAttack1ActiveBasecolorPath = "Windfall Spell Attack 1 Active Basecolor";
        private static readonly string WindfallSpellAttack1InactiveBasecolorPath = "Windfall Spell Attack 1 Inactive Basecolor";
        private static readonly string WindfallSpellAttack1InactiveMetallicPath = "Windfall Spell Attack 1 Inactive Metallic";

        private static readonly string VanillaSpellDefense1ActiveBasecolorPath = "Vanilla Spell Defense 1 Active Basecolor";
        private static readonly string VanillaSpellDefense1InactiveBasecolorPath = "Vanilla Spell Defense 1 Inactive Basecolor";

        private static readonly string WindfallSpellPuzzle1ActiveBasecolorPath = "Windfall Spell Puzzle 1 Active Basecolor";
        private static readonly string WindfallSpellPuzzle1InactiveBasecolorPath = "Windfall Spell Puzzle 1 Inactive Basecolor";
        private static readonly string WindfallSpellPuzzle1InactiveMetallicPath = "Windfall Spell Puzzle 1 Inactive Metallic";

        private static readonly string VanillaSpellUse1InactiveMetallicPath = "Vanilla Spell Use 1 Inactive Metallic";

        private static readonly string WindfallTrinketPuzzleMod1BasecolorPath = "Windfall Trinket Puzzle Mod 1 Basecolor";

        private static readonly string[] defaultMaterialTextureMapIDs = new string[]
        {
            "_MainTex",
            "_OcclusionMap",
            "_ParallaxMap",
            "_BumpMap",
            "_MetallicGlossMap",
        };

        //Adds spell icon material pages
        private static void AddSpellMaterialPages(SpellModel spellModel)
        {
            AssetBundle assets = Windfall.assetBundle;

            SpellCategory[] spellCategories = new SpellCategory[]
            {
                SpellCategory.Attack,
                SpellCategory.Puzzle,
            };

            foreach (SpellCategory spellCategory in spellCategories)
            {
                Material activeMaterial = null;
                Material inactiveMaterial = null;

                //Assign hardcoded texture asset paths to each texture of both active and inactive spell materials
                switch (spellCategory)
                {
                    case SpellCategory.Attack:
                        activeMaterial = new Material(spellModel.Icon(spellCategory, true, 0));
                        if (activeMaterial != null)
                        {
                            activeMaterial.SetTexture("_MainTex", (Texture)assets.LoadAsset(WindfallSpellAttack1ActiveBasecolorPath));
                            activeMaterial.SetTexture("_MetallicGlossMap", (Texture)assets.LoadAsset(CollectiblePageMetallicBlotchesPath));
                        }

                        inactiveMaterial = new Material(spellModel.Icon(spellCategory, false, 0));
                        if (inactiveMaterial != null)
                        {
                            inactiveMaterial.SetTexture("_MainTex", (Texture)assets.LoadAsset(WindfallSpellAttack1InactiveBasecolorPath));
                            inactiveMaterial.SetTexture("_MetallicGlossMap", (Texture)assets.LoadAsset(WindfallSpellAttack1InactiveMetallicPath));
                        }
                        break;
                    case SpellCategory.Puzzle:
                        activeMaterial = new Material(spellModel.Icon(spellCategory, true, 0));
                        if (activeMaterial != null)
                        {
                            activeMaterial.SetTexture("_MainTex", (Texture)assets.LoadAsset(WindfallSpellPuzzle1ActiveBasecolorPath));
                            activeMaterial.SetTexture("_MetallicGlossMap", (Texture)assets.LoadAsset(CollectiblePageMetallicBlotchesPath));
                        }

                        inactiveMaterial = new Material(spellModel.Icon(spellCategory, false, 0));
                        if (inactiveMaterial != null)
                        {
                            inactiveMaterial.SetTexture("_MainTex", (Texture)assets.LoadAsset(WindfallSpellPuzzle1InactiveBasecolorPath));
                            inactiveMaterial.SetTexture("_MetallicGlossMap", (Texture)assets.LoadAsset(WindfallSpellPuzzle1InactiveMetallicPath));
                        }
                        break;
                    default:
                        break;
                }

                if (activeMaterial == null) { return; }
                if (inactiveMaterial == null) { return; }

                //Loop through spell model material arrays
                for (int spellMaterialCounter = 0; spellMaterialCounter < spellModel.spellMaterial.Length; spellMaterialCounter++)
                {
                    //Find array for current spell category
                    if (spellModel.spellMaterial[spellMaterialCounter] != null && spellModel.spellMaterial[spellMaterialCounter].category == spellCategory)
                    {
                        //Add material pages
                        spellModel.spellMaterial[spellMaterialCounter].active.Add(activeMaterial);
                        spellModel.spellMaterial[spellMaterialCounter].inactive.Add(inactiveMaterial);
                    }
                }
            }
        }

        private static void ReplaceSpellIconMaterials(SpellModel spellModel)
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
                        activeTexturePaths[1] = CollectiblePageAmbientOcclusionPath;
                        activeTexturePaths[3] = CollectiblePageNormalPath;
                        inactiveTexturePaths[1] = CollectiblePageAmbientOcclusionPath;
                        inactiveTexturePaths[3] = CollectiblePageNormalPath;
                        break;
                    case SpellCategory.Defense:
                        activeTexturePaths[0] = VanillaSpellDefense1ActiveBasecolorPath;
                        inactiveTexturePaths[0] = VanillaSpellDefense1InactiveBasecolorPath;
                        break;
                    case SpellCategory.Puzzle:
                        inactiveTexturePaths[1] = CollectiblePageAmbientOcclusionPath;
                        inactiveTexturePaths[2] = CollectiblePageHeightPath;
                        inactiveTexturePaths[3] = CollectiblePageNormalPath;
                        break;
                    case SpellCategory.Use:
                        inactiveTexturePaths[1] = CollectiblePageAmbientOcclusionPath;
                        inactiveTexturePaths[2] = CollectiblePageHeightPath;
                        inactiveTexturePaths[3] = CollectiblePageNormalPath;
                        inactiveTexturePaths[4] = VanillaSpellUse1InactiveMetallicPath;
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
                for (int spellMaterialCounter = 0; spellMaterialCounter < spellModel.spellMaterial.Length; spellMaterialCounter++)
                {
                    //Find array for current spell category
                    if (spellModel.spellMaterial[spellMaterialCounter] != null && spellModel.spellMaterial[spellMaterialCounter].category == spellCategory)
                    {
                        //Access active materials for each page of current spell category
                        for (int materialCounter = 0; materialCounter < spellModel.spellMaterial[spellMaterialCounter].active.Count; materialCounter++)
                        {
                            Material unmodifiedMaterial = spellModel.spellMaterial[spellMaterialCounter].active[materialCounter];
                            if (unmodifiedMaterial == null) { continue; }

                            //Replace material textures
                            Material modifiedMaterial = ReplaceMaterialTextures(unmodifiedMaterial, activeTextureReplacements);
                            spellModel.spellMaterial[spellMaterialCounter].active[materialCounter] = modifiedMaterial;
                        }

                        //Access inactive materials for each page of current spell category
                        for (int materialCounter = 0; materialCounter < spellModel.spellMaterial[spellMaterialCounter].inactive.Count; materialCounter++)
                        {
                            Material unmodifiedMaterial = spellModel.spellMaterial[spellMaterialCounter].inactive[materialCounter];
                            if (unmodifiedMaterial == null) { continue; }

                            //Replace material textures
                            Material modifiedMaterial = ReplaceMaterialTextures(unmodifiedMaterial, inactiveTextureReplacements);
                            spellModel.spellMaterial[spellMaterialCounter].inactive[materialCounter] = modifiedMaterial;
                        }
                    }
                }
            }
        }

        //Adds trinket icon material pages
        private static void AddTrinketMaterialPages(TrinketModel trinketModel)
        {
            AssetBundle assets = Windfall.assetBundle;

            TrinketCategory[] trinketCategories = new TrinketCategory[]
            {
                TrinketCategory.Puzzle,
            };

            foreach (TrinketCategory trinketCategory in trinketCategories)
            {
                Material material = null;

                //Assign hardcoded texture asset paths to each texture of the trinket material
                switch (trinketCategory)
                {
                    case TrinketCategory.Puzzle:
                        material = new Material(trinketModel.Icon(trinketCategory, 0));
                        if (material != null)
                        {
                            material.SetTexture("_MainTex", (Texture)assets.LoadAsset(WindfallTrinketPuzzleMod1BasecolorPath));
                        }
                        break;
                    default:
                        break;
                }

                if (material == null) { return; }

                //Loop through trinket model material arrays
                for (int trinketMaterialCounter = 0; trinketMaterialCounter < trinketModel.materials.Length; trinketMaterialCounter++)
                {
                    //Find array for current trinket category
                    if (trinketModel.materials[trinketMaterialCounter] != null && trinketModel.materials[trinketMaterialCounter].category == trinketCategory)
                    {
                        //Add material pages
                        trinketModel.materials[trinketMaterialCounter].material.Add(material);
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
                        texturePaths[1] = CollectiblePageAmbientOcclusionPath;
                        texturePaths[2] = CollectiblePageHeightPath;
                        texturePaths[3] = CollectiblePageNormalPath;
                        texturePaths[4] = CollectiblePageMetallicBlotchesPath;
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

                //Loop through trinket model material arrays
                for (int trinketMaterialCounter = 0; trinketMaterialCounter < trinketModel.materials.Length; trinketMaterialCounter++)
                {
                    //Find array for current trinket category
                    if (trinketModel.materials[trinketMaterialCounter] != null && trinketModel.materials[trinketMaterialCounter].category == trinketCategory)
                    {
                        //Access materials for each page of current trinket category
                        for (int materialCounter = 0; materialCounter < trinketModel.materials[trinketMaterialCounter].material.Count; materialCounter++)
                        {
                            Material unmodifiedMaterial = trinketModel.materials[trinketMaterialCounter].material[materialCounter];
                            if (unmodifiedMaterial == null) { continue; }

                            //Replace material textures
                            Material modifiedMaterial = ReplaceMaterialTextures(unmodifiedMaterial, textureReplacements);
                            trinketModel.materials[trinketMaterialCounter].material[materialCounter] = modifiedMaterial;
                        }
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

                };

                WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
                if (windfallPersistentData.unlocks[1]) validSpells.Add((SpellName)1000); //Plasma Ball
                if (windfallPersistentData.unlocks[2]) validSpells.Add((SpellName)1001); //Magnifying Glass

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
                    {
                        (TrinketName)1002,
                        "COMPOST_BAG_NAME"
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
                    {
                        "1002",
                        (TrinketName)1002
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
                    {
                        (TrinketName)1002,
                        new CompostBagTrinket()
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

                WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
                if (windfallPersistentData.unlocks[3]) validTrinkets.Add((TrinketName)1002); //Compost Bag

                return validTrinkets;
            }
        }
    }
}
