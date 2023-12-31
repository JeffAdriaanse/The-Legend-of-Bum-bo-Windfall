using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace The_Legend_of_Bum_bo_Windfall
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInProcess("The Legend of Bum-bo.exe")]
    public class Windfall : BaseUnityPlugin
    {
        private const string modGUID = "org.bepinex.plugins.thelegendofbumbowindfall";
        private const string modName = "The Legend of Bum-bo: Windfall";
        private const string modVersion = "1.2.0.0";
        public static readonly Harmony harmony = new Harmony("org.bepinex.plugins.thelegendofbumbowindfall");

        public static AssetBundle assetBundle;
        private void Awake()
        {
            Logger.LogInfo($"Loading {modGUID}");

            //Load assets
            if (!LoadAssets())
            {
                Logger.LogError("Could not find assets file for The Legend of Bum-bo: Windfall. Ensure 'windfall' file is placed in the mod folder.");
                return;
            }

            //Determine which achievements unlock method exists to be patched
            GetAchievementsUnlockMethodExistence();

            //Patching with harmony
            harmony.PatchAll();
            EntityFixes.Awake();
            EntityChanges.Awake();
            CollectibleFixes.Awake();
            CollectibleChanges.Awake();
            CollectibleImport.Awake();
            InterfaceFixes.Awake();
            SoundsModification.Awake();
            InterfaceContent.Awake();
            TypoFixes.Awake();
            SaveChanges.Awake();
            OtherChanges.Awake();

            Logger.LogInfo($"Loaded {modGUID}");
        }

        public static bool LoadAssets()
        {
            foreach (string path in AssetBundlePaths)
            {
                if (File.Exists(path))
                {
                    assetBundle = AssetBundle.LoadFromFile(path);
                }
                if (assetBundle != null)
                {
                    break;
                }
            }

            return assetBundle != null;
        }

        private static List<string> AssetBundlePaths
        {
            get
            {
                List<string> paths = new List<string>()
                {
                    Directory.GetCurrentDirectory() + "/Bepinex/plugins/The Legend of Bum-bo_Windfall/windfall",
                    Directory.GetCurrentDirectory() + "/Bepinex/plugins/windfall",
                };

                return paths;
            }
        }

        public static bool achievementsSteam = false;
        public static bool achievementsGOG = false;
        public static bool achievementsEGS = false;
        private static void GetAchievementsUnlockMethodExistence()
        {
            //Steam
            List<string> steamAchievementsMethods = AccessTools.GetMethodNames(typeof(AchievementsSteam));
            achievementsSteam = steamAchievementsMethods.Contains("Unlock");

            //GOG
            List<string> GOGAchievementsMethods = AccessTools.GetMethodNames(typeof(AchievementsGOG));
            achievementsGOG = GOGAchievementsMethods.Contains("Unlock");

            //Epic
            List<string> epicAchievementsMethods = AccessTools.GetMethodNames(typeof(AchievementsEGS));
            achievementsEGS = epicAchievementsMethods.Contains("Unlock");
        }
    }
}
