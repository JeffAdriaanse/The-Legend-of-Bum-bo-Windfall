using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;

namespace The_Legend_of_Bum_bo_Windfall
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInProcess("The Legend of Bum-Bo.exe")]
    public class Windfall : BaseUnityPlugin
    {
        private const string modGUID = "org.bepinex.plugins.thelegendofbumbowindfall";
        private const string modName = "The Legend of Bum-bo: Windfall";
        private const string modVersion = "1.0.4.0";
        private readonly Harmony harmony = new Harmony("org.bepinex.plugins.thelegendofbumbowindfall");

        public static AssetBundle assetBundle;
        void Awake()
        {
            //Load assets
            LoadAssets();

            //Patching with harmony
            harmony.PatchAll();
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying harmony patch");
            EntityFixes.Awake();
            EntityChanges.Awake();
            CollectibleFixes.Awake();
            CollectibleChanges.Awake();
            InterfaceFixes.Awake();
            InterfaceContent.Awake();
            TypoFixes.Awake();
            SaveChanges.Awake();
            OtherChanges.Awake();
        }

        static void LoadAssets()
        {
            assetBundle = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "/Bepinex/plugins/The Legend of Bum-bo_Windfall/windfall");
        }
    }
}