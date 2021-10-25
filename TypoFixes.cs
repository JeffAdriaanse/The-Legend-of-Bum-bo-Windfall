using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using TMPro;

namespace The_Legend_of_Bum_bo_Windfall
{
    class TypoFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(TypoFixes));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying corrections to typos");
        }

        //Patch: Clarifies Bum-bo the Empty's unlock condition text
        [HarmonyPostfix, HarmonyPatch(typeof(BumboSelectView), "Start")]
        static void BumboSelectView_Start(BumboSelectView __instance)
        {
            if (__instance.bumboType == CharacterSheet.BumboType.Eden)
            {
                Transform unlockCondition = __instance.bumboSelect.transform.Find("Locked").Find("Unlock_Condition");
                unlockCondition.localScale = Vector3.Scale(unlockCondition.localScale, new Vector3(1.22f, 1, 1));

                Transform unlockText = unlockCondition.Find("Unlock Text");
                unlockText.localScale = Vector3.Scale(unlockText.localScale, new Vector3(1 / 1.22f, 1, 1));
                unlockText.GetComponent<TextMeshPro>().text = "beat the game twice with the first five characters.";
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Updating Bum-bo the Empty's unlock condition text");
        }

        //Patch: Corrects a typo in one of Gizzarda's boss sign tips
        [HarmonyPostfix, HarmonyPatch(typeof(BossSignView), "SetBosses")]
        static void BossSignView_SetBosses(BossSignView __instance)
        {
            foreach (GameObject tip in __instance.tips)
            {
                TextMeshPro tipText = tip.GetComponent<TextMeshPro>();
                if (tipText && tipText.text.Contains("shes is very resistant!"))
                {
                    tipText.text = "\"she's very resistant!\nplan ahead!\"";
                    Console.WriteLine("[The Legend of Bum-bo: Windfall] Correcting typo in Gizzarda boss sign tip");
                }
            }
        }

        //Patch: Fixes various spell name typos
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModel), "spellKA", MethodType.Getter)]
        static void SpellModel_spellKA(ref Dictionary<SpellName, string> __result)
        {
            Dictionary<SpellName, string> returnedDict = new Dictionary<SpellName, string>(__result);

            returnedDict[SpellName.Mallot] = "Mallet";
            returnedDict[SpellName.TinyDice] = "Tiny Dice";
            returnedDict[SpellName.SleightOfHand] = "Sleight of Hand";
            returnedDict[SpellName.ExorcismKit] = "Exorcism Kit";

            __result = returnedDict;
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Fixing spell name typos");
        }

        //Patch: Fixes Curved Horn trinket name typo
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), "trinketKA", MethodType.Getter)]
        static void TrinketModel_trinketKA(ref Dictionary<TrinketName, string> __result)
        {
            Dictionary<TrinketName, string> returnedDict = new Dictionary<TrinketName, string>(__result);

            returnedDict[TrinketName.CurvedHorn] = "Curved Horn";

            __result = returnedDict;
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Fixing Curved Horn name typo");
        }
    }
}
