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
    }
}
