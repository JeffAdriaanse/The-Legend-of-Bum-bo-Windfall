using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class SoundsModification
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(SoundsModification));
        }

        //Patch: Modify SoundsView soundMap data
        [HarmonyPostfix, HarmonyPatch(typeof(SoundsView), "Awake")]
        static void SoundsView_Awake(SoundsView __instance)
        {
            //Add new sounds
            __instance.soundGroups.Add(SoundsImportData.windfallSounds);

            ////Create a copy of the private soundMap field
            //var soundMapVar = AccessTools.Field(typeof(SoundsView), "soundMap").GetValue(__instance);
            //if (soundMapVar == null || soundMapVar is not Dictionary<SoundsView.eSound, AudioClip[]>) { return; }
            //Dictionary<SoundsView.eSound, AudioClip[]>  soundMap = (Dictionary<SoundsView.eSound, AudioClip[]>)soundMapVar;

            ////Modify the copy
            //soundMap.AddRange(SoundsImportData.soundMap);

            ////Assign the copy to the private soundMap field
            //AccessTools.Field(typeof(SoundsView), "soundMap").SetValue(__instance, soundMap);
        }

        //Stores sounds and their age (in frames)
        static Dictionary<SoundsView.eSound, int> soundAges = new Dictionary<SoundsView.eSound, int>();

        static readonly int muteDuration = 3;

        //Patch: Increments sound ages
        [HarmonyPostfix, HarmonyPatch(typeof(SoundsView), "Update")]
        static void SoundsView_Update()
        {
            List<SoundsView.eSound> currentSounds = new List<SoundsView.eSound>();

            //Copy current sounds
            foreach (SoundsView.eSound sound in soundAges.Keys)
            {
                currentSounds.Add(sound);
            }

            foreach (SoundsView.eSound sound in currentSounds)
            {
                if (soundAges.TryGetValue(sound, out int currentAge))
                {
                    if (soundAges[sound] >= muteDuration)
                    {
                        //Remove sound when it is too old
                        soundAges.Remove(sound);
                    }
                    else
                    {
                        //Increase age
                        soundAges[sound] = currentAge + 1;
                    }
                }
            }
        }

        //Patch: Aborts 2D sounds
        [HarmonyPrefix, HarmonyPatch(typeof(SoundsView), "PlaySound")]
        [HarmonyPatch(new Type[] { typeof(SoundsView.eSound), typeof(SoundsView.eAudioSlot), typeof(bool) })]
        static bool SoundsView_PlaySound_Prefix(SoundsView.eSound Sound, out bool __state)
        {
            if (soundAges.ContainsKey(Sound))
            {
                Debug.Log("Cancelled " + Sound.ToString() + " sound. Age: " + soundAges[Sound].ToString());
                __state = false;
                return false;
            }
            Debug.Log("Didn't cancel " + Sound.ToString() + " sound");
            __state = true;
            return true;
        }

        //Patch: Aborts 3D sounds
        [HarmonyPrefix, HarmonyPatch(typeof(SoundsView), "PlaySound")]
        [HarmonyPatch(new Type[] { typeof(SoundsView.eSound), typeof(Vector3), typeof(SoundsView.eAudioSlot), typeof(bool) })]
        static bool SoundsView_PlaySound_3D_Prefix(SoundsView.eSound Sound, out bool __state)
        {
            if (soundAges.ContainsKey(Sound))
            {
                Debug.Log("Cancelled " + Sound.ToString() + " 3D sound. Age: " + soundAges[Sound].ToString());
                __state = false;
                return false;
            }
            Debug.Log("Didn't cancel " + Sound.ToString() + " 3D sound");
            __state = true;
            return true;
        }

        //Patch: Tracks 2D sounds
        [HarmonyPostfix, HarmonyPatch(typeof(SoundsView), "PlaySound")]
        [HarmonyPatch(new Type[] { typeof(SoundsView.eSound), typeof(SoundsView.eAudioSlot), typeof(bool) })]
        static void SoundsView_PlaySound(SoundsView.eSound Sound, bool __state)
        {
            if (__state)
            {
                soundAges[Sound] = 0;
            }
        }

        //Patch: Tracks 3D sounds
        [HarmonyPostfix, HarmonyPatch(typeof(SoundsView), "PlaySound")]
        [HarmonyPatch(new Type[] { typeof(SoundsView.eSound), typeof(Vector3), typeof(SoundsView.eAudioSlot), typeof(bool) })]
        static void SoundsView_PlaySound_3D(SoundsView.eSound Sound, bool __state)
        {
            if (__state)
            {
                soundAges[Sound] = 0;
            }
        }
    }

    public static class SoundsImportData
    {
        //***************SoundsView***************//
        public static SoundsView.SoundGroup windfallSounds = new SoundsView.SoundGroup
        {
            soundItems = new SoundsView.SoundItem[]
            {
                new SoundsView.SoundItem
                {
                    soundId = (SoundsView.eSound)1000,
                    sounds = new AudioClip[]
                    {
                        Windfall.assetBundle.LoadAsset<AudioClip>("Electric_Shock_V4")
                    }
                }
            }
        };
    }
}
