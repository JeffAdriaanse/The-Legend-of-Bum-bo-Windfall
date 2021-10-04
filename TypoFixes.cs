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
    }
}
