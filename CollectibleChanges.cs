using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using DG.Tweening;


namespace The_Legend_of_Bum_bo_Windfall
{
    class CollectibleChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(CollectibleChanges));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying collectible changes");
        }

        //Changing all use trinkets such that they can be used on turn end
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketElement), "CanBeUsedOnTurnEnd", MethodType.Getter)]
        public static void TrinketElement_CanBeUsedOnTurnEnd_Getter(TrinketElement __instance, ref bool __result)
        {
            if (__instance.Category == TrinketElement.TrinketCategory.Use)
            {
                __result = true;
            }
        }

        //Patch: Causes the player's turn to end automatically after using an activated trinket while out of moves if they can't do anything
        [HarmonyPostfix, HarmonyPatch(typeof(UseTrinket), "Use")]
        static void UseTrinket_Use(UseTrinket __instance)
        {
            //Disabled for Boom and Death, since they manually set a new event when used
            if (__instance.trinketName != TrinketName.Boom && __instance.trinketName != TrinketName.Death)
            {
                __instance.app.controller.eventsController.SetEvent(new IdleEvent());
            }
        }

        //Patch: Causes the player's turn to end automatically after using Modeling Clay while out of moves if they can't do anything (unlike other use trinkets, Modeling Clay doesn't call the base Use method)
        [HarmonyPostfix, HarmonyPatch(typeof(ModelingClayTrinket), "Use")]
        static void ModelingClayTrinket_Use(ModelingClayTrinket __instance)
        {
            __instance.app.controller.eventsController.SetEvent(new IdleEvent());
        }
    }
}
