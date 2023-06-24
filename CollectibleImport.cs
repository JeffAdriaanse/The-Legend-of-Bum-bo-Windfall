using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class CollectibleImport
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(CollectibleImport));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), nameof(BumboController.Init))]
        static void BumboController_Init(BumboController __instance)
        {
            //Add spell materials
            //AddSpellMaterial()
        }

        //Adds spell icon materials
        static void AddSpellMaterial(Material material, SpellModel spellModel, SpellElement.SpellCategory spellCategory, bool active)
        {
            foreach (SpellMaterial spellMaterial in spellModel.spellMaterial)
            {
                if (spellMaterial.category == spellCategory)
                {
                    if (active)
                    {
                        spellMaterial.active.Add(material);
                    }
                    else
                    {
                        spellMaterial.inactive.Add(material);
                    }
                }
            }
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
                        "PlasmaBall",
                        (SpellName)1000
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
                        new SpellElement()
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
                };
                return validSpells;
            }
        }
    }
}
