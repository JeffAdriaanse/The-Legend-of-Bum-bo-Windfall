using System;
using BepInEx;
using HarmonyLib;

namespace The_Legend_of_Bum_bo_Windfall
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInProcess("The Legend of Bum-Bo.exe")]
    public class Windfall : BaseUnityPlugin
    {
        private const string modGUID = "org.bepinex.plugins.thelegendofbumbowindfall";
        private const string modName = "The Legend of Bum-bo: Windfall";
        private const string modVersion = "0.0.2.0";
        private readonly Harmony harmony = new Harmony("org.bepinex.plugins.thelegendofbumbowindfall");
        void Awake()
        {
            //Patching with harmony
            harmony.PatchAll();
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying harmony patch");
            EntityFixes.Awake();
            CollectibleFixes.Awake();
            CollectibleChanges.Awake();
            InterfaceFixes.Awake();
            InterfaceContent.Awake();
            TypoFixes.Awake();
            SaveChanges.Awake();
            OtherChanges.Awake();
        }
    }
}