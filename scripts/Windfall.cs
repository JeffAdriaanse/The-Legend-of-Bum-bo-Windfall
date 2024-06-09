using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

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
            PuzzleChanges.Awake();
            SoundsModification.Awake();
            InterfaceContent.Awake();
            TextFixes.Awake();
            SaveChanges.Awake();
            OccultSpirits.Awake();
            OtherChanges.Awake();

            Logger.LogInfo($"Loaded {modGUID}");
        }

        public static AssetBundle assetBundle;
        private static string AssetBundleName = "windfall";
        public static bool LoadAssets()
        {
            string assetBundlePath = WindfallHelper.FindFileInCurrentDirectory(AssetBundleName);

            if (assetBundlePath != null) assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            else { Debug.LogError("Could not find Windfall assetBundle. Please ensure asset file is placed in the mod directory."); }

            return assetBundle != null;
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
