using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using TMPro;
using DG.Tweening;
using System.IO;
using UnityEngine.UI;
using System.Collections;
using I2.Loc;
using System.Linq;
using System.Web;

namespace The_Legend_of_Bum_bo_Windfall
{
    class InterfaceFixes
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(InterfaceFixes));
        }

        //Patch: Fixes 'damage up' notification appearing incorrectly
        //GUISide manaDrainView incorrectly contained the ManaDrainView of the child of the intended GameObject
        [HarmonyPostfix, HarmonyPatch(typeof(GUISide), "Awake")]
        static void GUISide_Awake(GUISide __instance)
        {
            ManaDrainView manaDrainView = __instance.damageUpView.transform.parent.GetComponent<ManaDrainView>();
            if (manaDrainView != null)
            {
                __instance.damageUpView = manaDrainView;
            }
        }

        //Patch: Fixes 'damage up' notification sometimes not appearing when Bum-bo the Brave's passive ability is triggered
        //This patch replaces the 'HealthChange' method of BraveHiddenTriket
        [HarmonyPostfix, HarmonyPatch(typeof(HealthController), nameof(HealthController.UpdateHearts))]
        static void HealthController_UpdateHearts(HealthController __instance)
        {
            CharacterSheet characterSheet = __instance.app.model.characterSheet;

            //Only applies to The Brave
            if (characterSheet.bumboType == CharacterSheet.BumboType.TheBrave)
            {
                //Retrieve previous hit points value
                float previousHitPoints = ObjectDataStorage.GetData(__instance.gameObject, "HitPoints");
                if (previousHitPoints != float.NaN)
                {
                    bool hitPointsReducedBelowThreshold = false;

                    //Check whether damage up occurred from either threshold
                    if (previousHitPoints > 1f && characterSheet.hitPoints <= 1f)
                    {
                        hitPointsReducedBelowThreshold = true;
                    }
                    if (previousHitPoints > 2f && characterSheet.hitPoints <= 2f)
                    {
                        hitPointsReducedBelowThreshold = true;
                    }

                    //Display notification
                    if (hitPointsReducedBelowThreshold)
                    {
                        __instance.app.controller.ShowDamageUp();
                    }
                }
            }
            //Store hit points value for the next health update
            ObjectDataStorage.StoreData(__instance.gameObject, "HitPoints", characterSheet.hitPoints);
        }
        //Disable original method
        [HarmonyPrefix, HarmonyPatch(typeof(BraveHiddenTrinket), nameof(BraveHiddenTrinket.HealthChange))]
        static bool BraveHiddenTrinket_HealthChange()
        {
            return false;
        }

        //Patch: Fixes Bag-O-Trash and Bum-bo the Dead's passive ability sometimes causing spell icon visuals to not update correctly
        //SetSpell now updates the target spell's icon active status
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.SetSpell))]
        static void BumboController_SetSpell(BumboController __instance, int _spell_index, SpellElement _spell)
        {
            __instance.app.view.spells[_spell_index].SetActive(_spell.IsReady());
        }

        //Patch: Fixes map base being visible when transitioning between rooms
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init_MapView(BumboController __instance)
        {
            __instance.app.view.mapView.gameObject.SetActive(false);
        }

        private static readonly string collectible_page_ambient_occlusion_path = "Collectible_page_ambient_occlusion";
        private static readonly string collectible_page_height_path = "Collectible_page_height";
        private static readonly string collectible_page_normal_path = "Collectible_page_normal";
        private static readonly string spell_use_metallic_path = "Spell_use_metallic_V3";
        private static readonly string trinket_page_metallic_path = "Trinket_Page_metallic";
        private static readonly string spell_defense_active_basecolor_path = "Spell_Defense_1_active_basecolor";
        private static readonly string spell_defense_inactive_basecolor_path = "Spell_Defense_1_inactive_basecolor";

        //Patch: Fix spell texture maps
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init_Collectible_Textures(BumboController __instance)
        {
            ReplaceSpellIconMaterials(__instance.app);
        }

        //Patch: Fix trinket texture maps
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketModel), nameof(TrinketModel.FillDictionary))]
        static void TrinketModel_FillDictionary_Prefix(TrinketModel __instance, out bool __state)
        {
            __state = __instance.populatedMaterial;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketModel), nameof(TrinketModel.FillDictionary))]
        static void TrinketModel_FillDictionary(TrinketModel __instance, bool __state)
        {
            if (!__state)
            {
                ReplaceTrinketIconMaterials(__instance);
            }
        }

        private static void ReplaceTrinketIconMaterials(TrinketModel trinketModel)
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            TrinketElement.TrinketCategory[] trinketCategories = new TrinketElement.TrinketCategory[]
            {
                TrinketElement.TrinketCategory.Special,
            };

            foreach (TrinketElement.TrinketCategory trinketCategory in trinketCategories)
            {
                bool replaceAmbientOcclusion = false;
                bool replaceHeight = false;
                bool replaceNormal = false;
                string metallicPath = null;

                switch (trinketCategory)
                {
                    case TrinketElement.TrinketCategory.Special:
                        replaceAmbientOcclusion = true;
                        replaceHeight = true;
                        replaceNormal = true;
                        metallicPath = trinket_page_metallic_path;
                        break;
                    default:
                        break;
                }
                //Find list for current trinket category
                if (trinketModel.trinketMaterial.ContainsKey(trinketCategory) && trinketModel.trinketMaterial[trinketCategory] != null)
                {
                    //Access materials for each page of current trinket category
                    for (int materialCounter = 0; materialCounter < trinketModel.trinketMaterial[trinketCategory].Count; materialCounter++)
                    {
                        Material material = trinketModel.trinketMaterial[trinketCategory][materialCounter];
                        if (material != null)
                        {
                            //Replace each material
                            string[] texturePropertyNames = material.GetTexturePropertyNames();

                            //Ambient Occlusion
                            if (replaceAmbientOcclusion && texturePropertyNames.Contains("_OcclusionMap") && assets.Contains(collectible_page_ambient_occlusion_path))
                            {
                                Texture texture = assets.LoadAsset<Texture>(collectible_page_ambient_occlusion_path);

                                if (texture != null)
                                {
                                    material.SetTexture("_OcclusionMap", texture);
                                }
                            }

                            //Height
                            if (replaceHeight && texturePropertyNames.Contains("_ParallaxMap") && assets.Contains(collectible_page_height_path))
                            {
                                Texture texture = assets.LoadAsset<Texture>(collectible_page_height_path);

                                if (texture != null)
                                {
                                    material.SetTexture("_ParallaxMap", texture);
                                }
                            }

                            //Normal
                            if (replaceNormal && texturePropertyNames.Contains("_BumpMap") && assets.Contains(collectible_page_normal_path))
                            {
                                Texture texture = assets.LoadAsset<Texture>(collectible_page_normal_path);

                                if (texture != null)
                                {
                                    Texture oldBumpMap = material.GetTexture("_BumpMap");
                                    if (oldBumpMap != null && oldBumpMap is Texture2D)
                                    {
                                        material.SetTexture("_BumpMap", texture);
                                    }
                                }
                            }

                            //Metallic
                            if (metallicPath != null && texturePropertyNames.Contains("_MetallicGlossMap") && assets.Contains(metallicPath))
                            {
                                Texture texture = assets.LoadAsset<Texture>(metallicPath);

                                if (texture != null)
                                {
                                    material.SetTexture("_MetallicGlossMap", texture);
                                }
                            }

                            trinketModel.trinketMaterial[trinketCategory][materialCounter] = material;
                        }
                    }
                }
            }
        }

        private static void ReplaceSpellIconMaterials(BumboApplication app)
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            SpellElement.SpellCategory[] spellCategories = new SpellElement.SpellCategory[]
            {
                SpellElement.SpellCategory.Attack,
                SpellElement.SpellCategory.Defense,
                SpellElement.SpellCategory.Puzzle,
                SpellElement.SpellCategory.Use,
            };

            foreach (SpellElement.SpellCategory spellCategory in spellCategories)
            {
                bool replaceAmbientOcclusion = false;
                bool replaceHeight = false;
                bool replaceNormal = false;
                string metallicPath = null;
                string activeBasecolorPath = null;
                string inactiveBasecolorPath = null;

                switch (spellCategory)
                {
                    case SpellElement.SpellCategory.Attack:
                        replaceNormal = true;
                        break;
                    case SpellElement.SpellCategory.Defense:
                        activeBasecolorPath = spell_defense_active_basecolor_path;
                        inactiveBasecolorPath = spell_defense_inactive_basecolor_path;
                        break;
                    case SpellElement.SpellCategory.Puzzle:
                        replaceAmbientOcclusion = true;
                        replaceHeight = true;
                        replaceNormal = true;
                        break;
                    case SpellElement.SpellCategory.Use:
                        replaceAmbientOcclusion = true;
                        replaceHeight = true;
                        replaceNormal = true;
                        metallicPath = spell_use_metallic_path;
                        break;
                    default:
                        break;
                }

                //Loop through spell model material arrays
                for (int spellMaterialCounter = 0; spellMaterialCounter < app.model.spellModel.spellMaterial.Length; spellMaterialCounter++)
                {
                    //Find array for current spell category
                    if (app.model.spellModel.spellMaterial[spellMaterialCounter] != null && app.model.spellModel.spellMaterial[spellMaterialCounter].category == spellCategory)
                    {
                        //Access active materials for each page of current spell category
                        for (int materialCounter = 0; materialCounter < app.model.spellModel.spellMaterial[spellMaterialCounter].active.Count; materialCounter++)
                        {
                            Material material = app.model.spellModel.spellMaterial[spellMaterialCounter].active[materialCounter];
                            if (material != null)
                            {
                                //Replace each material
                                string[] texturePropertyNames = material.GetTexturePropertyNames();

                                //Main Texture
                                if (activeBasecolorPath != null && texturePropertyNames.Contains("_MainTex") && assets.Contains(activeBasecolorPath))
                                {
                                    Texture texture = assets.LoadAsset<Texture>(activeBasecolorPath);

                                    if (texture != null)
                                    {
                                        material.SetTexture("_MainTex", texture);
                                    }
                                }

                                app.model.spellModel.spellMaterial[spellMaterialCounter].active[materialCounter] = material;
                            }
                        }

                        //Access inactive materials for each page of current spell category
                        for (int materialCounter = 0; materialCounter < app.model.spellModel.spellMaterial[spellMaterialCounter].inactive.Count; materialCounter++)
                        {
                            Material material = app.model.spellModel.spellMaterial[spellMaterialCounter].inactive[materialCounter];
                            if (material != null)
                            {
                                //Replace each material
                                string[] texturePropertyNames = material.GetTexturePropertyNames();

                                //Main Texture
                                if (inactiveBasecolorPath != null && texturePropertyNames.Contains("_MainTex") && assets.Contains(inactiveBasecolorPath))
                                {
                                    Texture texture = assets.LoadAsset<Texture>(inactiveBasecolorPath);

                                    if (texture != null)
                                    {
                                        material.SetTexture("_MainTex", texture);
                                    }
                                }

                                //Ambient Occlusion
                                if (replaceAmbientOcclusion && texturePropertyNames.Contains("_OcclusionMap") && assets.Contains(collectible_page_ambient_occlusion_path))
                                {
                                    Texture texture = assets.LoadAsset<Texture>(collectible_page_ambient_occlusion_path);

                                    if (texture != null)
                                    {
                                        material.SetTexture("_OcclusionMap", texture);
                                    }
                                }

                                //Height
                                if (replaceHeight && texturePropertyNames.Contains("_ParallaxMap") && assets.Contains(collectible_page_height_path))
                                {
                                    Texture texture = assets.LoadAsset<Texture>(collectible_page_height_path);

                                    if (texture != null)
                                    {
                                        material.SetTexture("_ParallaxMap", texture);
                                    }
                                }

                                //Normal
                                if (replaceNormal && texturePropertyNames.Contains("_BumpMap") && assets.Contains(collectible_page_normal_path))
                                {
                                    Texture texture = assets.LoadAsset<Texture>(collectible_page_normal_path);

                                    if (texture != null)
                                    {
                                        material.SetTexture("_BumpMap", texture);
                                    }
                                }

                                //Metallic
                                if (metallicPath != null && texturePropertyNames.Contains("_MetallicGlossMap") && assets.Contains(metallicPath))
                                {
                                    Texture texture = assets.LoadAsset<Texture>(metallicPath);

                                    if (texture != null)
                                    {
                                        material.SetTexture("_MetallicGlossMap", texture);
                                    }
                                }

                                app.model.spellModel.spellMaterial[spellMaterialCounter].inactive[materialCounter] = material;
                            }
                        }
                    }
                }
            }
        }

        static Tweener voAudioTweener;
        static Tweener fxAudioTweener;
        static Tweener musicAudioTweener;
        //Patch: Fixes ending cutscene audio not pausing when unlock popups appear after the cutscene has been skipped
        [HarmonyPostfix, HarmonyPatch(typeof(UnlockImageView), "CompleteAnimation")]
        static void UnlockImageView_CompleteAnimation(UnlockImageView __instance)
        {
            float fadeDuration = 2f;

            AudioSource voAudio = __instance.app.view?.introPaperView?.voAudio;
            if (voAudio != null && (voAudioTweener == null || !voAudioTweener.IsPlaying()))
            {
                voAudioTweener = __instance.app.view.introPaperView.voAudio.DOFade(0f, fadeDuration);
            }

            AudioSource fxAudio = __instance.app.view.introPaperView.fxAudio;
            if (fxAudio != null && (fxAudioTweener == null || !fxAudioTweener.IsPlaying()))
            {
                fxAudioTweener = __instance.app.view.introPaperView.fxAudio.DOFade(0f, fadeDuration);
            }

            AudioSource musicAudio = __instance.app.view.introPaperView.musicAudio;
            if (musicAudio != null && (musicAudioTweener == null || !musicAudioTweener.IsPlaying()))
            {
                musicAudioTweener = __instance.app.view.introPaperView.musicAudio.DOFade(0f, fadeDuration);
            }
        }

        //Patch: Fixes quickly skipping ending cutscenes causing the unlock scene to remain dark
        [HarmonyPostfix, HarmonyPatch(typeof(UnlockImageView), "ShowUnlock")]
        static void UnlockImageView_ShowUnlock(UnlockImageView __instance)
        {
            ReintensifyUnlockSceneLights(__instance.app.view);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(UnlockImageView), "RemoveUnlock")]
        static void UnlockImageView_RemoveUnlock(UnlockImageView __instance)
        {
            ReintensifyUnlockSceneLights(__instance.app.view);
        }
        static void ReintensifyUnlockSceneLights(BumboUnlockView bumboUnlockView)
        {
            if (bumboUnlockView != null)
            {
                float targetKeyLightIntensity = bumboUnlockView is BumboCutsceneView ? 1f : 1.28f;
                float targetFillLightIntensity = 1f;

                Light bumboUnlockViewKeyLight = bumboUnlockView.keyLight;
                Light bumboUnlockViewFillLight = bumboUnlockView.fillLight;
                Sequence bumboUnlockViewCameraSequence = bumboUnlockView.cameraSequence;

                if (bumboUnlockViewCameraSequence != null)
                {
                    if (bumboUnlockViewKeyLight != null)
                    {
                        float intensityDifference = targetKeyLightIntensity - bumboUnlockViewKeyLight.intensity;
                        float intensifyDuration = 2f * Mathf.Abs(intensityDifference / targetKeyLightIntensity);

                        if (intensifyDuration >= 0.05f)
                        {
                            bumboUnlockViewCameraSequence.Insert(0f, bumboUnlockViewKeyLight.DOIntensity(targetKeyLightIntensity, intensifyDuration));
                        }
                    }

                    if (bumboUnlockViewFillLight != null)
                    {
                        float intensityDifference = targetFillLightIntensity - bumboUnlockViewFillLight.intensity;
                        float intensifyDuration = 2f * Mathf.Abs(intensityDifference / targetFillLightIntensity);

                        if (intensifyDuration >= 0.05f)
                        {
                            bumboUnlockViewCameraSequence.Insert(0f, bumboUnlockViewFillLight.DOIntensity(targetFillLightIntensity, intensifyDuration));

                        }
                    }
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(UnlockImageViewGambling), "UnlockImage")]
        static void UnlockImageViewGambling_UnlockImage_Prefix(float _unlock_index, out float __state)
        {
            __state = _unlock_index;
        }
        //Patch: Fixes Bum-bo the Lost unlock displaying the wrong text in the Wooden Nickel
        [HarmonyPostfix, HarmonyPatch(typeof(UnlockImageViewGambling), "UnlockImage")]
        static void UnlockImageViewGambling_UnlockImage_Postfix(UnlockImageViewGambling __instance, float __state)
        {
            Localization.SetKey(__instance.unlockText, eI2Category.Unlocks, __instance.unlockKeys[(int)__state]);
        }

        static readonly float UnlockImageFontSize = 1.7f;

        [HarmonyPrefix, HarmonyPatch(typeof(UnlockImageView), "UnlockImage")]
        static void UnlockImageView_UnlockImage_Prefix(float _unlock_index, out float __state)
        {
            __state = _unlock_index;
        }
        //Patch: Fixes unlocks displaying the wrong text
        //Also formats unlock text
        [HarmonyPostfix, HarmonyPatch(typeof(UnlockImageView), "UnlockImage")]
        static void UnlockImageView_UnlockImage_Postfix(UnlockImageView __instance, float __state)
        {
            Localization.SetKey(__instance.unlockText, eI2Category.Unlocks, __instance.unlockKeys[(int)__state]);

            //Unlock text
            TextMeshPro unlockTextMeshPro = __instance.unlockText?.GetComponent<TextMeshPro>();

            //Unlocked text
            Transform unlockedText = __instance.transform.Find("Unlock Image View")?.Find("UnlockedText");

            //Change text for 'Everything is Terrible'
            Localize unlockedTextLocalize = unlockedText?.GetComponent<Localize>();
            if (__state == 7)
            {
                Localization.SetKey(__instance.unlockText, eI2Category.Unlocks, "EVERYTHING_IS_TERRIBLE_NEW");

                if (unlockedTextLocalize != null)
                {
                    LocalizationFontOverrides unlockedTextOverride = unlockedText.GetComponent<LocalizationFontOverrides>();
                    if (unlockedTextOverride != null)
                    {
                        unlockedTextOverride.enabled = false;
                    }
                    Localization.SetKey(unlockedTextLocalize, eI2Category.Unlocks, "THE_GAME_IS_HARDER");
                }
            }
            else
            {
                if (unlockedTextLocalize != null)
                {
                    Localization.SetKey(unlockedTextLocalize, eI2Category.Unlocks, "UNLOCKED");
                }
            }

            bool formattingApplied = false;
            if (unlockTextMeshPro != null)
            {
                if (unlockTextMeshPro.fontSize == UnlockImageFontSize)
                {
                    //Abort if formatting has already been applied
                    formattingApplied = true;
                }
                else
                {
                    unlockTextMeshPro.fontSize = UnlockImageFontSize;
                    unlockTextMeshPro.verticalAlignment = VerticalAlignmentOptions.Middle;

                    RectTransform unlockTextRectTransform = __instance.unlockText?.GetComponent<RectTransform>();
                    if (unlockTextRectTransform != null)
                    {
                        unlockTextRectTransform.anchoredPosition3D = new Vector3(unlockTextRectTransform.anchoredPosition3D.x, unlockTextRectTransform.anchoredPosition3D.y, 0.48f);
                    }
                }
            }

            if (!formattingApplied)
            {
                TextMeshPro unlockedTextMeshPro = unlockedText?.GetComponent<TextMeshPro>();
                if (unlockedTextMeshPro != null)
                {
                    unlockedTextMeshPro.fontSize = UnlockImageFontSize;
                    unlockedTextMeshPro.verticalAlignment = VerticalAlignmentOptions.Middle;
                    unlockedTextMeshPro.characterSpacing = 0;
                }

                RectTransform unlockedTextRectTransform = unlockedText?.GetComponent<RectTransform>();
                if (unlockedTextRectTransform != null)
                {
                    unlockedTextRectTransform.anchoredPosition3D = new Vector3(unlockedTextRectTransform.anchoredPosition3D.x, unlockedTextRectTransform.anchoredPosition3D.y, -0.52f);
                }
            }
        }

        //Patch: Moves main camera on ShowBossSignEvent
        [HarmonyPostfix, HarmonyPatch(typeof(ShowBossSignEvent), "Execute")]
        static void ShowBossSignEvent_Execute(ShowBossSignEvent __instance)
        {
            if (__instance.app.view.mainCameraView != null)
            {
                __instance.app.view.mainCameraView.transform.position += new Vector3(0f, 20f, 0f);
            }
        }

        //Patch: Fixes treasure rooms always having room art of the Sewers of Dross, even when Bum-bo is in a different Chapter
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "Init")]
        static void BumboController_Init(BumboController __instance)
        {
            TreasureRoom treasureRoom = __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>();
            EnemyRoomView enemyRoomView = __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>();

            //Replace treasure room objects with copies from the current enemy room

            //Eastern door
            UnityEngine.Object.Destroy(treasureRoom.doorEast.gameObject);
            treasureRoom.doorEast = UnityEngine.Object.Instantiate(enemyRoomView.doorEast, enemyRoomView.doorEast.transform.position, enemyRoomView.doorEast.transform.rotation, treasureRoom.transform);
            //Northern door
            UnityEngine.Object.Destroy(treasureRoom.doorNorth.gameObject);
            treasureRoom.doorNorth = UnityEngine.Object.Instantiate(enemyRoomView.doorNorth, enemyRoomView.doorNorth.transform.position, enemyRoomView.doorNorth.transform.rotation, treasureRoom.transform);
            //Western door
            UnityEngine.Object.Destroy(treasureRoom.doorWest.gameObject);
            treasureRoom.doorWest = UnityEngine.Object.Instantiate(enemyRoomView.doorWest, enemyRoomView.doorWest.transform.position, enemyRoomView.doorWest.transform.rotation, treasureRoom.transform);

            string eastWall = null;
            string westWall = null;
            string backWall = null;
            string floor = null;

            switch (__instance.app.model.characterSheet.currentFloor)
            {
                case 1:
                    eastWall = "Sewer_Side_Wall (1)";
                    westWall = "Sewer_Side_Wall";
                    backWall = "Sewer_Back_Wall";
                    floor = "Sewer_Floor";
                    break;
                case 2:
                    eastWall = "Cave_Side_Wall (1)";
                    westWall = "Cave_Side_Wall";
                    backWall = "Cave_Back_Wall";
                    floor = "Cave_Floor";
                    break;
                case 3:
                    eastWall = "Temple_East_Side_Wall";
                    westWall = "Temple_West_Side_Wall";
                    backWall = "Temple_Back_Wall";
                    floor = "Temple_Floor";
                    break;
                case 4:
                    eastWall = "Basement_East_Side_Wall";
                    westWall = "Basement_East_Side_Wall (1)";
                    backWall = "Basement_Back_Wall";
                    floor = "Basement_Floor";
                    break;
            }

            if (eastWall != null && enemyRoomView.transform.Find(eastWall) != null)
            {
                Transform transform = enemyRoomView.transform.Find(eastWall);
                UnityEngine.Object.Destroy(treasureRoom.transform.Find("Sewer_Side_Wall (1)").gameObject);
                UnityEngine.Object.Instantiate(transform, transform.position, transform.rotation, treasureRoom.transform);
            }

            if (westWall != null && enemyRoomView.transform.Find(westWall) != null)
            {
                Transform transform = enemyRoomView.transform.Find(westWall);
                UnityEngine.Object.Destroy(treasureRoom.transform.Find("Sewer_Side_Wall").gameObject);
                UnityEngine.Object.Instantiate(transform, transform.position, transform.rotation, treasureRoom.transform);
            }

            if (backWall != null && enemyRoomView.transform.Find(backWall) != null)
            {
                Transform transform = enemyRoomView.transform.Find(backWall);
                UnityEngine.Object.Destroy(treasureRoom.transform.Find("Sewer_Back_Wall").gameObject);
                UnityEngine.Object.Instantiate(transform, transform.position, transform.rotation, treasureRoom.transform);
            }

            if (floor != null && enemyRoomView.transform.Find(floor) != null)
            {
                Transform transform = enemyRoomView.transform.Find(floor);
                UnityEngine.Object.Destroy(treasureRoom.transform.Find("Sewer_Floor").gameObject);
                UnityEngine.Object.Instantiate(transform, transform.position, transform.rotation, treasureRoom.transform);
            }
        }

        //Patch: Fixes boss room doors appearing as normal doors
        [HarmonyPrefix, HarmonyPatch(typeof(BoxController), "SetDoorType")]
        static bool BoxController_SetDoorType(BoxController __instance, DoorView _north, DoorView _east, DoorView _west)
        {
            if (_north.gameObject.activeSelf)
            {
                MapRoom.RoomType roomType = __instance.app.model.mapModel.rooms[__instance.app.model.mapModel.currentRoom.x, __instance.app.model.mapModel.currentRoom.y + 1].roomType;
                DoorView.DoorType doorType = DoorView.DoorType.Normal;
                if (roomType == MapRoom.RoomType.Treasure)
                {
                    doorType = DoorView.DoorType.Treasure;
                }
                if (roomType == MapRoom.RoomType.Boss)
                {
                    doorType = DoorView.DoorType.Boss;
                }
                _north.SetDoorType(doorType);
            }
            if (_east.gameObject.activeSelf)
            {
                MapRoom.RoomType roomType2 = __instance.app.model.mapModel.rooms[__instance.app.model.mapModel.currentRoom.x + 1, __instance.app.model.mapModel.currentRoom.y].roomType;
                DoorView.DoorType doorType2 = DoorView.DoorType.Normal;
                if (roomType2 == MapRoom.RoomType.Treasure)
                {
                    doorType2 = DoorView.DoorType.Treasure;
                }
                if (roomType2 == MapRoom.RoomType.Boss)
                {
                    doorType2 = DoorView.DoorType.Boss;
                }
                _east.SetDoorType(doorType2);
            }
            if (_west.gameObject.activeSelf)
            {
                MapRoom.RoomType roomType3 = __instance.app.model.mapModel.rooms[__instance.app.model.mapModel.currentRoom.x - 1, __instance.app.model.mapModel.currentRoom.y].roomType;
                DoorView.DoorType doorType3 = DoorView.DoorType.Normal;
                if (roomType3 == MapRoom.RoomType.Treasure)
                {
                    doorType3 = DoorView.DoorType.Treasure;
                }
                if (roomType3 == MapRoom.RoomType.Boss)
                {
                    doorType3 = DoorView.DoorType.Boss;
                }
                _west.SetDoorType(doorType3);
            }
            return false;
        }

        //Patch: Changes puzzle block initial scale
        [HarmonyPrefix, HarmonyPatch(typeof(Block), "Start")]
        static bool Block_Start(Block __instance)
        {
            float blockSize = 0.92f;
            __instance.transform.localScale = new Vector3(blockSize, blockSize, blockSize);
            return true;
        }

        //Patch: Fixes ButtonHoverAnimation grabbing the wrong transform value when determining block initial scale
        [HarmonyPostfix, HarmonyPatch(typeof(ButtonHoverAnimation), "Start")]
        static void ButtonHoverAnimation_Start(ButtonHoverAnimation __instance, ref Vector3 ___initialScale, Block ___tileBlock)
        {
            if (___tileBlock)
            {
                ___initialScale = ___tileBlock.transform.localScale;
            }
        }

        //Patch: Mana drain type notifications no longer overlap when multiple are spawned at the same time; they now layer behind each other instead
        [HarmonyPostfix, HarmonyPatch(typeof(ManaDrainView), "HudAppear")]
        static void ManaDrainView_HudAppear(ManaDrainView __instance)
        {
            ManaDrainView[] currentNotifications = UnityEngine.Object.FindObjectsOfType<ManaDrainView>();
            int offsetCounter = 0;
            for (int notificationIndex = 0; notificationIndex < currentNotifications.Length; notificationIndex++)
            {
                //Exclude self (prevents infinite for loop)
                if (__instance != currentNotifications[notificationIndex])
                {
                    Vector3 thisNotificationPosition = __instance.transform.position;
                    Vector3 currentNotificationPosition = currentNotifications[notificationIndex].transform.position;
                    float notificationOffset = 0.02f;
                    float overlapFidelity = notificationOffset / 3;
                    //Check whether notification is too close on each axis
                    if (currentNotificationPosition.x - overlapFidelity <= thisNotificationPosition.x && thisNotificationPosition.x <= currentNotificationPosition.x + overlapFidelity
                        && currentNotificationPosition.y - overlapFidelity <= thisNotificationPosition.y && thisNotificationPosition.y <= currentNotificationPosition.y + overlapFidelity
                        && currentNotificationPosition.z - overlapFidelity <= thisNotificationPosition.z && thisNotificationPosition.z <= currentNotificationPosition.z + overlapFidelity)
                    {
                        //Move notification back and up slightly
                        __instance.transform.Translate(0f, notificationOffset, notificationOffset);
                        //Increase offset counter
                        offsetCounter++;
                        //Restart loop
                        notificationIndex = 0;
                    }
                }
            }
            if (offsetCounter > 0)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Offsetting position of newly spawned ManaDrainView object; object was offset " + offsetCounter + (offsetCounter == 1 ? " time" : " times"));
            }
        }

        //Patch: Fixes a bug that caused invincible enemies to make the boss health bar display their healh amount when healed; the invincible enemy will now not be healed instead
        //Patch also causes the healing 'gulp' sound to not play for the invincible enemy
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "AddHealth")]
        static bool Enemy_AddHealth(Enemy __instance)
        {
            if (__instance.max_health == Enemy.HealthType.INVINCIBLE)
            {
                return false;
            }
            return true;
        }

        //Patch: Fixes enemy healing icons appearing at the back of the lane instead of in front of the enemy being healed
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "Init")]
        static bool Enemy_Init(Enemy __instance)
        {
            if (__instance.indicatorPosition == Vector3.zero)
            {
                __instance.indicatorPosition = new Vector3(0, __instance.enemyType != Enemy.EnemyType.Ground ? 1.25f : 0.5f, -0.5f);
            }
            return true;
        }

        //Patch: Fixes z-fighting between enemies and their spawn dust; dust will now never spawn at the same z coordinate as the enemy's position
        [HarmonyPrefix, HarmonyPatch(typeof(Enemy), "Dust")]
        static bool Enemy_Dust(Enemy __instance, ref float _x_offset, ref float _y_offset, ref float _z_offset)
        {
            if (_z_offset == 0.2f)
            {
                _z_offset += 0.1f;
            }
            return true;
        }

        //Patch: Fixes holding 'r' to restart while in a treasure room causing the game to softlock
        //Also closes GamepadSpellSelector on TreasureChosenEvent
        [HarmonyPrefix, HarmonyPatch(typeof(TreasureChosenEvent), "Execute")]
        static bool TreasureChosenEvent_Execute(TreasureChosenEvent __instance)
        {
            if (UnityEngine.Object.FindObjectOfType<LoadingController>() != null)
            {
                return false;
            }

            __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);
            return true;
        }

        //***************************************************
        //*************Trinket pickup use display************
        //***************************************************
        //These patches fix the number of uses displayed on trinket pickups always appearing as one, even if the trinket has multiple uses

        //Patch: Changes displayed number of uses of trinket pickups in treasure rooms
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketPickupView), "SetTrinket", new Type[] { typeof(TrinketName), typeof(TrinketModel) })]
        static void TrinketPickupView_SetTrinket(TrinketPickupView __instance)
        {
            if (__instance.trinket.Category == TrinketElement.TrinketCategory.Use)
            {
                __instance.trinketUses.text = __instance.trinket.uses.ToString();
            }
        }

        //Patch: Changes displayed number of uses of trinket pickups in the Wooden Nickel
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketPickupView), "SetTrinket", new Type[] { typeof(TrinketName), typeof(int) })]
        static void TrinketPickupView_SetTrinket_Shop(TrinketPickupView __instance)
        {
            if (__instance.trinket.Category == TrinketElement.TrinketCategory.Use)
            {
                __instance.trinketUses.text = __instance.trinket.uses.ToString();
            }
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Disables all pickups when a spell pickup is clicked
        [HarmonyPostfix, HarmonyPatch(typeof(SpellPickup), "OnMouseDown")]
        static void SpellPickup_OnMouseDown(SpellPickup __instance)
        {
            __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>().SetClickable(false);
        }

        //Patch: Disables the end turn sign collider when it spawns, preventing it from being clicked while above the play area
        [HarmonyPostfix, HarmonyPatch(typeof(EndTurnView), "Start")]
        static void EndTurnView_Start(EndTurnView __instance)
        {
            __instance.MakeClickable(false);
        }

        //Patch: Disables the end turn sign collider when it is hidden; hiding the sign immediately after showing it would not update the collider correctly
        [HarmonyPostfix, HarmonyPatch(typeof(EndTurnView), nameof(EndTurnView.HideSign))]
        static void EndTurnView_HideSign(EndTurnView __instance)
        {
            __instance.MakeClickable(false);
        }

        //Patch: Prevents end turn sign from being clicked while the game is paused
        [HarmonyPrefix, HarmonyPatch(typeof(EndTurnView), "OnMouseDown")]
        static bool EndTurnView_OnMouseDown(EndTurnView __instance)
        {
            if (__instance.app.model.paused)
            {
                return false;
            }
            return true;
        }

        //Patch: Fixed toggle spell menu arrow persisting after leaving a treasure room if the treasure room was exited in a certain way
        [HarmonyPostfix, HarmonyPatch(typeof(RoomStartEvent), "Execute")]
        static void RoomStartEvent_Execute(RoomStartEvent __instance)
        {
            __instance.app.view.GUICamera.GetComponent<GUISide>().expandGUIView.Hide();// Hide expandGUIView on room start
        }

        //Patch: Fixed spell HUD clipping into the camera during boss VS screens if the spell HUD was open when exiting the treasure room
        [HarmonyPatch(typeof(GUISide), "HideHud")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count - 1; i++)
            {
                //Checking for IL code
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == -0.5f)
                {
                    //Change operands
                    code[i].operand = -1f;
                    break;
                }
            }
            return code;
        }

        //Patch: Fixed Bum-bo's popsicle stick clipping into the camera view during the Mega Chomper tile combo animation
        [HarmonyPostfix, HarmonyPatch(typeof(ToothMegaAttackEvent), "Execute")]
        static void ToothMegaAttackEvent_Execute(ToothMegaAttackEvent __instance)
        {
            GameObject popsicle = null;
            for (int childIndex = 0; childIndex < __instance.app.view.bumboThrow.transform.childCount; childIndex++)
            {
                if (__instance.app.view.bumboThrow.transform.GetChild(childIndex).name == "Popsicle")
                {
                    popsicle = __instance.app.view.bumboThrow.transform.GetChild(childIndex).gameObject;
                }
            }

            if (popsicle != null)
            {
                popsicle.SetActive(false);
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(2f);
                sequence.AppendCallback(delegate ()
                {
                    popsicle.SetActive(true);
                });
            }
        }

        //Patch: Fixes rooms staying darkened after being cleared during ChanceToCastSpellEvent
        [HarmonyPostfix, HarmonyPatch(typeof(RoomEndEvent), "Execute")]
        static void RoomEndEvent_Execute(RoomEndEvent __instance)
        {
            //Change to default light scheme on room clear
            if (!__instance.app.model.defaultLightOn)
            {
                __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().ChangeLight(EnemyRoomView.RoomLightScheme.Default, 0.2f, true);
            }
        }

        //Patch: Fixes boss room darkening when opening the spell menu while choosing a trinket if the boss was defeated during ChanceToCastSpellEvent
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "ShowEndTurnSign")]
        static bool BumboController_ShowEndTurnSign(BumboController __instance)
        {
            string currentEvent = __instance.app.model.bumboEvent.GetType().ToString();
            if (currentEvent == "BossDyingEvent" || currentEvent == "TrinketReplaceEvent" || currentEvent == "TrinketReplaceCancelledEvent")
            {
                //Abort ShowEndTurnSign
                return false;
            }
            return true;
        }

        //Patch: Fixes spell ready notifications appearing before the SideGUI has expanded
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "SetActiveSpells")]
        static bool BumboController_SetActiveSpells(BumboController __instance)
        {
            int num = 0;
            short num2 = 0;
            while ((int)num2 < __instance.app.model.characterSheet.spells.Count)
            {
                bool flag = __instance.app.model.characterSheet.spells[(int)num2].IsReady();
                if (__instance.app.model.characterSheet.spells[(int)num2].IsChargeable)
                {
                    __instance.app.view.spells[(int)num2].spellMana1.transform.GetChild(0).GetComponent<TextMeshPro>().text = __instance.app.model.characterSheet.spells[(int)num2].charge + " / " + __instance.app.model.characterSheet.spells[(int)num2].requiredCharge;
                }
                if (flag)
                {
                    num++;
                    if (__instance.app.view.spells[(int)num2].spellContainer.GetComponent<MeshRenderer>().material.GetTextureOffset("_MainTex") != new Vector2(0f, -0.5f))
                    {
                        __instance.app.view.spells[(int)num2].spellContainer.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, -0.5f));

                        if (__instance.app.view.sideGUI.activeSelf) //Require SideGUI to be expanded for spell ready notification to appear
                        {
                            __instance.app.view.spellReadyView.spellReady[(int)num2].SetActive(true);
                            DOTween.Kill("spellready" + num2, false);
                            __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition = new Vector3(0.5f, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.y, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.z);
                            Sequence sequence = DOTween.Sequence();
                            TweenSettingsExtensions.SetId<Sequence>(sequence, "spellready" + num2);
                            TweenSettingsExtensions.Append(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOLocalMove(__instance.app.view.spellReadyView.spellReady[(int)num2].transform, new Vector3(0f, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.y, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.z), 1f, false), Ease.OutBounce));
                            TweenSettingsExtensions.OnComplete<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(sequence, 0.5f), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOLocalMove(__instance.app.view.spellReadyView.spellReady[(int)num2].transform, new Vector3(0.5f, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.y, __instance.app.view.spellReadyView.spellReady[(int)num2].transform.localPosition.z), 0.5f, false), Ease.InQuad)), delegate ()
                            {
                                __instance.HideAllSpellReady();
                            });
                        }
                    }
                }
                else
                {
                    __instance.app.model.spellJustNowReady[(int)num2] = false;
                    __instance.app.view.spells[(int)num2].spellContainer.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, -0.25f));
                }
                __instance.app.view.spells[(int)num2].SetActive(flag);
                switch (__instance.app.model.characterSheet.spells[(int)num2].Category)
                {
                    case SpellElement.SpellCategory.Defense:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypeDefense.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].defenseObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            if (!__instance.app.model.spellJustNowReady[(int)num2])
                            {
                                __instance.app.model.spellJustNowReady[(int)num2] = true;
                                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.DefenseSpellReady, SoundsView.eAudioSlot.Default, false);
                            }
                            __instance.app.view.spells[(int)num2].spellTypeDefense.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].defenseObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            }
                        }
                        break;
                    case SpellElement.SpellCategory.Puzzle:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypePuzzle.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].puzzleObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            if (!__instance.app.model.spellJustNowReady[(int)num2])
                            {
                                __instance.app.model.spellJustNowReady[(int)num2] = true;
                                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.PuzzleSpellReady, SoundsView.eAudioSlot.Default, false);
                            }
                            __instance.app.view.spells[(int)num2].spellTypePuzzle.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].puzzleObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        break;
                    case SpellElement.SpellCategory.Use:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypeItem.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].itemObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            __instance.app.view.spells[(int)num2].spellTypeItem.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].itemObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        break;
                    case SpellElement.SpellCategory.Other:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypeSpecial.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].specialObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            __instance.app.view.spells[(int)num2].spellTypeSpecial.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].specialObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        break;
                    default:
                        if (!flag)
                        {
                            __instance.app.view.spells[(int)num2].spellTypeAttack.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].attackObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));
                            }
                        }
                        else
                        {
                            if (!__instance.app.model.spellJustNowReady[(int)num2])
                            {
                                __instance.app.model.spellJustNowReady[(int)num2] = true;
                                __instance.app.view.soundsView.PlaySound(SoundsView.eSound.AttackSpellReady, SoundsView.eAudioSlot.Default, false);
                            }
                            __instance.app.view.spells[(int)num2].spellTypeAttack.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            if (__instance.app.model.iOS)
                            {
                                __instance.app.view.IOSSpellType[(int)num2].attackObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                            }
                        }
                        break;
                }
                num2 += 1;
            }
            return false;
        }

        //Patch: Fixes the opposing navigation arrow remaining triggerable while the character select screen is still rotating
        [HarmonyPrefix, HarmonyPatch(typeof(RotateCharacters), "Rotate")]
        static bool RotateCharacters_Rotate(RotateCharacters __instance)
        {
            bool charactersRotating = false;
            RotateCharacters[] navArrows = __instance.transform.parent.GetComponentsInChildren<RotateCharacters>();
            for (int navArrowCounter = 0; navArrowCounter < navArrows.Length; navArrowCounter++)
            {
                if (navArrows[navArrowCounter].RotationSequence != null && navArrows[navArrowCounter].RotationSequence.IsPlaying())
                {
                    charactersRotating = true;
                }
            }

            if (charactersRotating)
            {
                return false;
            }
            return true;
        }

        //Patch: Fixes back button remaining clickable after selecting a character
        //Also fixes character select screen elements remaining triggerable using gamepad controls after selecting a character using mouse controls
        [HarmonyPostfix, HarmonyPatch(typeof(ChooseBumbo), "ConfirmSelection")]
        static void ChooseBumbo_ConfirmSelection(ChooseBumbo __instance, bool __result)
        {
            if (__result)
            {
                ChooseYourBumbo chooseYourBumbo = __instance.transform.parent?.parent?.GetComponent<ChooseYourBumbo>();

                if (chooseYourBumbo != null)
                {
                    AccessTools.Field(typeof(ChooseYourBumbo), "m_InputBlocked").SetValue(chooseYourBumbo, true);
                }

                //Disable OnClick
                __instance.transform.parent.Find("BackButton").GetComponent<SelectCharacterBackButton>().OnClick = new UnityEngine.Events.UnityEvent();
            }
        }

        //Fixes character select screen elements remaining triggerable using gamepad controls after exiting the character select screen using mouse controls
        [HarmonyPrefix, HarmonyPatch(typeof(SelectCharacterBackButton), "OnMouseDown")]
        static void SelectCharacterBackButton_OnMouseDown(SelectCharacterBackButton __instance)
        {
            if (__instance.gameObject.activeInHierarchy && __instance.active)
            {
                ChooseYourBumbo chooseYourBumbo = __instance.transform.parent?.parent?.GetComponent<ChooseYourBumbo>();

                if (chooseYourBumbo != null)
                {
                    AccessTools.Field(typeof(ChooseYourBumbo), "m_InputBlocked").SetValue(chooseYourBumbo, true);
                }
            }
        }

        //Patch: Fixes the pause menu button remaining clickable while the pause menu is already open
        //Also hides the pause menu when the map is open
        //Also enables the pause menu button when in treasure rooms
        [HarmonyPrefix, HarmonyPatch(typeof(MenuButtonView), "Update")]
        static bool MenuButtonView_Update(MenuButtonView __instance, bool ___showing)
        {
            //Check whether canvas is active
            GameObject canvas = MapMenu.mapMenuCanvas;
            bool canvasNull = canvas == null;
            bool canvasActive = false;
            if (!canvasNull)
            {
                canvasActive = canvas.activeSelf;
            }

            if (!__instance.app.view.menuView.activeSelf && !canvasActive && (__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent"))
            {
                if (!___showing)
                {
                    __instance.Show();
                    return false;
                }
            }
            else if (___showing)
            {
                __instance.Hide();
            }
            return false;
        }

        //Patch: Fixes spell/trinket inventory hover sounds playing while the pause menu is open
        //Also prevents hover animations when in the pause/map menu
        //Also prevents dropdown from changing size while open
        [HarmonyPostfix, HarmonyPatch(typeof(ButtonHoverAnimation), "CheckEnabled")]
        static void ButtonHoverAnimation_CheckEnabled(ButtonHoverAnimation __instance, ref bool __result, RectTransform ___rectTransform)
        {
            if (__instance.app != null && __instance.app.model.paused && ___rectTransform == null)
            {
                __result = false;
            }

            Dropdown dropdown = __instance.GetComponent<Dropdown>();
            if (dropdown != null)
            {
                Transform list = dropdown.transform.Find("Dropdown List");
                if (list != null && list.gameObject.activeSelf)
                {
                    __result = false;
                }
            }
        }

        //Patch: Fixes the pause menu opening sound playing when attempting to open the pause menu using hotkeys while it is already open
        //Prevents opening the pause menu while the map menu is open
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "Update")]
        static bool BumboController_Update(BumboController __instance, ref float ___restartTime)
        {
            if (Input.GetKey(KeyCode.R) && __instance.app.model.characterSheet != null && __instance.app.model.characterSheet.currentFloor > 0)
            {
                ___restartTime += Time.deltaTime;
                if (___restartTime >= 2f)
                {
                    ___restartTime = 0f;
                    __instance.app.Notify("gameover.retry", __instance, new object[0]);
                }
            }
            else
            {
                ___restartTime = 0f;
            }
            if (MapMenu.mapMenuCanvas != null && !MapMenu.mapMenuCanvas.activeSelf && Input.GetKeyDown(KeyCode.Escape) && (__instance.app.model.bumboEvent.GetType().ToString() == "IdleEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "ChanceToCastSpellEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent") && !__instance.app.view.menuView.activeSelf)
            {
                __instance.app.model.paused = true;
                __instance.app.view.menuView.SetActive(true);
                SoundsView.Instance.PlaySound(SoundsView.eSound.Menu_Appear, SoundsView.eAudioSlot.Default, false);
            }
            return false;
        }

        //Patch: Fixes spell UI not appearing when quickly buying a trinket/needle after canceling a purchase
        //Patch also causes the shop clerk to wave when a trinket is purchased
        //Patch also disables treasure room pickups when taking a trinket
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketPickupView), "OnMouseDown")]
        static bool TrinketPickupView_OnMouseDown(TrinketPickupView __instance)
        {
            if (!__instance.app.model.paused)
            {
                if ((__instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceCancelledEvent") && __instance.app.model.characterSheet.trinkets.Count < 4)
                {
                    __instance.Acquire(__instance.CancelAllowed);
                    __instance.app.controller.eventsController.EndEvent();
                    return false;
                }
                //Don't use boss room logic while clicking a trinket during TrinketReplaceCancelledEvent while in a treasure room
                if ((__instance.app.model.bumboEvent.GetType().ToString() == "BossDyingEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceCancelledEvent") && __instance.app.model?.mapModel?.currentRoom?.roomType == MapRoom.RoomType.Boss && (bool)AccessTools.Field(typeof(TrinketPickupView), "clickable").GetValue(__instance) == true)
                {
                    //Disable boss room pickups
                    GameObject[] bossRewardParents = __instance.app.view.bossRewardParents;
                    if (bossRewardParents != null && bossRewardParents.Length > 1)
                    {
                        bossRewardParents[0]?.transform.GetChild(0)?.GetComponent<TrinketPickupView>()?.SetClickable(false);
                        bossRewardParents[1]?.transform.GetChild(0)?.GetComponent<TrinketPickupView>()?.SetClickable(false);
                    }

                    __instance.app.model.trinketReward = __instance.trinket.trinketName;
                    SoundsView.Instance.PlaySound(SoundsView.eSound.Trinket_Sign_Away, __instance.transform.position, SoundsView.eAudioSlot.Default, false);
                    if (__instance.shopIndex == 0) 
                    {
                        __instance.app.view.mainCameraView.transform.DOMove(new Vector3(-0.12f, 0.27f, -3.02f), 0.25f, false).SetEase(Ease.InOutQuad);
                        __instance.app.view.mainCameraView.transform.DORotate(new Vector3(-1.43f, -19.54f, 0f), 0.25f, RotateMode.Fast).SetEase(Ease.InOutQuad);
                    }
                    else if (__instance.shopIndex == 1)
                    {
                        __instance.app.view.mainCameraView.transform.DOMove(new Vector3(0f, 0.27f, -3.02f), 0.25f, false).SetEase(Ease.InOutQuad);
                        __instance.app.view.mainCameraView.transform.DORotate(new Vector3(-1.43f, 24.49f, 0f), 0.25f, RotateMode.Fast).SetEase(Ease.InOutQuad);
                    }
                    __instance.app.controller.eventsController.SetEvent(new TrinketReplaceEvent(__instance.shopIndex, __instance.CancelAllowed));
                    return false;
                }
                if ((bool)AccessTools.Field(typeof(TrinketPickupView), "clickable").GetValue(__instance) == true)
                {
                    //Allow clicking treasure room trinkets while in TrinketReplaceCancelledEvent
                    //Player must be in a treasure room
                    if ((__instance.app.model.bumboEvent.GetType().ToString() == "TreasureStartEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceCancelledEvent") && __instance.app.model?.mapModel?.currentRoom?.roomType == MapRoom.RoomType.Treasure)
                    {
                        //Disable treasure room pickups
                        __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>().SetClickable(false);
                        if (__instance.app.model.characterSheet.trinkets.Count < 4)
                        {
                            __instance.Acquire(__instance.CancelAllowed);
                            return false;
                        }
                        __instance.app.model.trinketReward = __instance.trinket.trinketName;
                        __instance.app.controller.eventsController.SetEvent(new TrinketReplaceEvent(__instance.shopIndex, __instance.CancelAllowed));
                        return false;
                    }
                    //Don't puchase if the sideGUI is moving
                    //Allow trinket purchases during TrinketReplaceCancelledEvent
                    else if (__instance.app.view.gamblingView != null && (__instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceCancelledEvent") && !DOTween.IsTweening(__instance.app.view.sideGUI.transform, false))
                    {
                        if (__instance.app.model.characterSheet.coins >= __instance.AdjustedPrice && (__instance.app.model.characterSheet.trinkets.Count < 4 || __instance.trinket.Category == TrinketElement.TrinketCategory.Prick))
                        {
                            if (__instance.app.view.gamblingView != null && (__instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent" || __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceCancelledEvent"))
                            {
                                if (__instance.app.model.gamblingModel.cameraAt != 2)
                                {
                                    return false;
                                }
                                //Make prick pusher wave
                                __instance.app.view.gamblingView.shopClerkView.Wave();
                                __instance.app.view.gamblingView.shopRegisterView.Pay(__instance.AdjustedPrice);
                                __instance.app.controller.ModifyCoins(-__instance.AdjustedPrice);
                                __instance.Acquire(__instance.CancelAllowed);
                            }
                            return false;
                        }
                        if (__instance.app.model.bumboEvent.GetType().ToString() != "SpellModifyEvent" && __instance.app.model.characterSheet.coins >= __instance.AdjustedPrice && __instance.app.model.characterSheet.trinkets.Count >= 4)
                        {
                            if (__instance.app.view.gamblingView != null && __instance.app.model.bumboEvent.GetType().ToString() != "TreasureStartEvent")
                            {
                                if (__instance.app.model.gamblingModel.cameraAt != 2)
                                {
                                    return false;
                                }
                                //Make prick pusher wave
                                __instance.app.view.gamblingView.shopClerkView.Wave();
                                __instance.app.view.gamblingView.shopRegisterView.Pay(__instance.AdjustedPrice);
                                __instance.app.model.gamblingModel.trinketReward = __instance.trinket.trinketName;
                                __instance.app.controller.gamblingController.selectedShopIdx = __instance.shopIndex;
                            }
                            __instance.app.controller.ModifyCoins(-__instance.AdjustedPrice);
                            if (__instance.app.view.gamblingView == null)
                            {
                                __instance.app.model.trinketReward = __instance.trinket.trinketName;
                            }
                            __instance.app.controller.eventsController.SetEvent(new TrinketReplaceEvent(__instance.shopIndex, __instance.CancelAllowed));
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        //Patch: Moves the camera during TrinketReplaceEvent while in treasure rooms
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketReplaceEvent), "Execute")]
        static void TrinketReplaceEvent_Execute(TrinketReplaceEvent __instance)
        {
            if (__instance.app.view.gamblingView == null && __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure)
            {
                if (__instance.index == 0)
                {
                    ShortcutExtensions.DOMove(__instance.app.view.mainCameraView.transform, new Vector3(0.04f, 0.55f, -3.89f), 0.25f, false).SetEase(Ease.InOutQuad);
                    ShortcutExtensions.DORotate(__instance.app.view.mainCameraView.transform, new Vector3(0.94f, -45.2f, 0f), 0.25f, RotateMode.Fast).SetEase(Ease.InOutQuad);
                }
                else if (__instance.index == 1)
                {
                    ShortcutExtensions.DOMove(__instance.app.view.mainCameraView.transform, new Vector3(0.04f, 0.55f, -3.89f), 0.25f, false).SetEase(Ease.InOutQuad);
                    ShortcutExtensions.DORotate(__instance.app.view.mainCameraView.transform, new Vector3(0.94f, 37.58f, 0f), 0.25f, RotateMode.Fast).SetEase(Ease.InOutQuad);
                }
            }
        }
        //Patch: Sets treasure trinket index
        [HarmonyPostfix, HarmonyPatch(typeof(TreasureRoom), "SetTrinkets")]
        static void TreasureRoom_SetTrinkets(TreasureRoom __instance, int _item_number)
        {
            List<GameObject> pickups = (List<GameObject>)AccessTools.Field(typeof(TreasureRoom), "pickups").GetValue(__instance);
            if (pickups != null && pickups.Count > 0)
            {
                TrinketPickupView pickup = pickups[pickups.Count - 1]?.GetComponent<TrinketPickupView>();
                if (pickup != null)
                {
                    pickup.shopIndex = _item_number - 1;
                    AccessTools.Field(typeof(TreasureRoom), "pickups").SetValue(__instance, pickups);
                }
            }
        }

        //Patch: Keeps the game paused for one frame after exiting the pause menu
        [HarmonyPostfix, HarmonyPatch(typeof(PauseButtonView), "Continue")]
        static void PauseButtonView_Continue(PauseButtonView __instance)
        {
            __instance.app.model.paused = true;
            __instance.app.StartCoroutine(UnPause(__instance.app));
        }
        static IEnumerator UnPause(BumboApplication app)
        {
            //wait one frame
            yield return 0;
            app.model.paused = false;
        }

        //Patch: Hide trinket prices
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketPickupView), "HopAndDisappear")]
        static void TrinketPickupView_HopAndDisappear(TrinketPickupView __instance)
        {
            //Hide trinket price
            if (__instance.priceView != null)
            {
                __instance.priceView.Hide();
            }
        }

        //Patch: Hide price discounts when losing steam sale
        [HarmonyPostfix, HarmonyPatch(typeof(ItemPriceView), "ReducePrice")]
        static void ItemPriceView_ReducePrice(ItemPriceView __instance, int discount)
        {
            if (discount == 0)
            {
                __instance.salesPrice.gameObject.SetActive(false);
                __instance.salesPriceTag.SetActive(false);
            }
        }

        //Patch: Prevents cancel view from disappearing immediately when hiding
        //Also adjusts placement of boss cancel view
        //Also forces treasure cancel to raise higher when hiding
        [HarmonyPrefix, HarmonyPatch(typeof(CancelView), "Hide")]
        static bool CancelView_Hide_Prefix(CancelView __instance, out bool __state)
        {
            if (__instance.animationSequence?.id as string == "hiding" || !__instance.gameObject.activeSelf)
            {
                __state = false;
                return false;
            }

            if (__instance.name == "Boss Cancel View")
            {
                __instance.SetColliderActive(false);
                __instance.animationSequence.Complete(true);
                __instance.animationSequence = DOTween.Sequence();
                __instance.animationSequence.Append(ShortcutExtensions.DOLocalMoveY(__instance.transform, 1.75f, 0.25f, false).SetEase(Ease.InOutQuad)).OnComplete(delegate
                {
                    __instance.gameObject.SetActive(false);
                });

                __state = true;
                return false;
            }
            else if (__instance.transform.parent.name == "Treasure Room")
            {
                __instance.SetColliderActive(false);
                __instance.animationSequence.Complete(true);
                __instance.animationSequence = DOTween.Sequence();
                __instance.animationSequence.Append(ShortcutExtensions.DOLocalMoveY(__instance.transform, 1.75f, 0.25f, false).SetEase(Ease.InOutQuad)).OnComplete(delegate
                {
                    __instance.gameObject.SetActive(false);
                });
                __instance.animationSequence.timeScale = 0.5f;

                __state = true;
                return false;
            }

            __state = true;
            return true;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(CancelView), "Hide")]
        static void CancelView_Hide_Postfix(CancelView __instance, bool __state)
        {
            if (__state)
            {
                __instance.animationSequence?.SetId("hiding");
            }
        }
        //Patch: Forces easing for cancel view
        [HarmonyPrefix, HarmonyPatch(typeof(CancelView), "Show", new Type[] { typeof(Vector3), typeof(bool) })]
        static void CancelView_Show(CancelView __instance, ref bool ease)
        {
            ease = true;
        }
        //Patch: Slows treasure room show sequence
        [HarmonyPostfix, HarmonyPatch(typeof(CancelView), "Show", new Type[] { typeof(Vector3), typeof(bool) })]
        static void CancelView_Show_Postfix(CancelView __instance, ref bool ease)
        {
            if (__instance.transform.parent.name == "Treasure Room")
            {
                __instance.animationSequence.timeScale = 0.5f;
            }
        }
        //Patch: Initialize boss cancel position
        [HarmonyPostfix, HarmonyPatch(typeof(CancelView), "Awake")]
        static void CancelView_Awake(CancelView __instance)
        {
            if (__instance.name == "Boss Cancel View")
            {
                __instance.transform.localPosition += new Vector3(0, 0.8f, 0);
            }
        }

        //Patch: Allow gamepad controls in the Wooden Nickel after canceling replacing a trinket (ends TrinketReplaceCancelledEvent)
        //Also removes trinket reward display on cancel
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketReplaceCancelledEvent), "Execute")]
        static void TrinketReplaceCancelledEvent_Execute(TrinketReplaceCancelledEvent __instance)
        {
            if (__instance.app.view.gamblingView != null)
            {
                InterfaceContent.RemoveTrinketRewardDisplay();
                __instance.app.controller.eventsController.SetEvent(new GamblingEvent());
            }
        }

        //Patch: Disables Wooden Nickel exit button while the game is paused
        //Also disables Wooden Nickel exit button while the camera is rotating
        [HarmonyPrefix, HarmonyPatch(typeof(ExitGamblingView), "OnMouseDown")]
        static bool ExitGamblingView_OnMouseDown(ExitGamblingView __instance)
        {
            if (__instance.app.model.paused || DOTween.IsTweening(__instance.view.gamblingCameraView.transform, true))
            {
                return false;
            }
            return true;
        }

        //Patch: Disables ExpandGUIView while the game is paused
        //Also disables ExpandGUIView while the spell menu is moving
        [HarmonyPrefix, HarmonyPatch(typeof(ExpandGUIView), "OnMouseDown")]
        static bool ExpandGUIView_OnMouseDown(ExpandGUIView __instance)
        {
            if (__instance.app.model.paused || SpellMenuClickDisbled(__instance.app))
            {
                return false;
            }
            return true;
        }

        //Patch: Prevents clicking spell slots while the spell menu is moving
        [HarmonyPrefix, HarmonyPatch(typeof(SpellView), "OnMouseDown")]
        static bool SpellView_OnMouseDown(SpellView __instance)
        {
            if (__instance.app.model.paused || SpellMenuClickDisbled(__instance.app) || MouseClickedWhileUsingGamepadControls(__instance.app))
            {
                return false;
            }
            return true;
        }

        //Patch: Prevents clicking trinket slots while the spell menu is moving
        //Patch: Remove purchased trinket
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketView), "OnMouseDown")]
        static bool TrinketView_OnMouseDown(TrinketView __instance)
        {
            if (SpellMenuClickDisbled(__instance.app) || MouseClickedWhileUsingGamepadControls(__instance.app))
            {
                return false;
            }

            if (!__instance.app.model.paused && __instance.app.model.bumboEvent.GetType().ToString() == "TrinketReplaceEvent" && __instance.app.model.gamblingModel?.cameraAt != 0)
            {
                int index = (__instance.app.model.bumboEvent as TrinketReplaceEvent).index;

                if (__instance.app.view.gamblingView != null && index >= 0)
                {
                    TrinketPickupView pickup = __instance.app.view.gamblingView.shop.GetPickup(index).GetComponent<TrinketPickupView>();
                    if (pickup != null)
                    {
                        pickup.SetClickable(false);
                        pickup.HopAndDisappear();
                        SoundsView.Instance.PlaySound(SoundsView.eSound.Trinket_Sign_Away, __instance.transform.position, SoundsView.eAudioSlot.Default, false);
                    }
                }
            }
            return true;
        }

        //Patch: Prevents interacting with the spell menu using gamepad controls while the spell menu is moving
        [HarmonyPrefix, HarmonyPatch(typeof(GamepadSpellSelector), "Update")]
        static bool GamepadSpellSelector_Update(GamepadSpellSelector __instance)
        {
            if (SpellMenuClickDisbled(__instance.app))
            {
                return false;
            }
            return true;
        }

        static bool SpellMenuClickDisbled(BumboApplication app)
        {
            return SpellMenuTransitioning() || TreasureCameraTransitioning(app);
        }

        static bool SpellMenuTransitioning()
        {
            return DOTween.IsTweening("ShowingGUI", false) || DOTween.IsTweening("HidingGUI", false);
        }

        static bool TreasureCameraTransitioning(BumboApplication app)
        {
            return DOTween.IsTweening(app.view.mainCameraView.transform, false) && app.model?.mapModel?.currentRoom?.roomType == MapRoom.RoomType.Treasure;
        }

        static bool MouseClickedWhileUsingGamepadControls(BumboApplication app)
        {
            //UNUSED METHOD
            return false;

            GamepadSpellSelector gamepadSpellSelector = app.view.GUICamera?.GetComponent<GamepadSpellSelector>();

            bool gamepadSpellSelectorActive = false;
            if (gamepadSpellSelector != null && gamepadSpellSelector.IsActive)
            {
                gamepadSpellSelectorActive = true;
            }

            return (InputManager.Instance.IsUsingGamepadInput() && gamepadSpellSelectorActive);
        }

        //***************************************************
        //************Shop Nav Arrow Clickability************
        //***************************************************
        //These patches disable shop navigation arrows when they are disappearing

        static Sequence gamblingNavigationSequenceLeft;
        static Sequence gamblingNavigationSequenceRight;

        [HarmonyPrefix, HarmonyPatch(typeof(GamblingNavigation), "Hide")]
        static bool GamblingNavigation_Hide(GamblingNavigation __instance)
        {
            if (__instance.gameObject.activeSelf)
            {
                AccessTools.Field(typeof(GamblingNavigation), "isHidden").SetValue(__instance, true);

                if (__instance.direction > 0)
                {
                    HideNavigation(__instance, ref gamblingNavigationSequenceLeft);
                }
                else
                {
                    HideNavigation(__instance, ref gamblingNavigationSequenceRight);
                }
            }
            return false;
        }

        static void HideNavigation(GamblingNavigation __instance, ref Sequence GamblingNavigationSequence)
        {
            if (GamblingNavigationSequence != null)
            {
                GamblingNavigationSequence.Kill(true);
            }
            GamblingNavigationSequence = DOTween.Sequence().Append(ShortcutExtensions.DOScale(__instance.transform, new Vector3(0.8f, 0.8f, 0.8f), 0.2f).SetEase(Ease.InOutQuad)).Append(ShortcutExtensions.DOScale(__instance.transform, new Vector3(0f, 0f, 0.6f), 0.1f).SetEase(Ease.InOutQuad)).AppendCallback(delegate
            {
                __instance.gameObject.SetActive(false);
            });
        }

        [HarmonyPrefix, HarmonyPatch(typeof(GamblingNavigation), "Show")]
        static bool GamblingNavigation_Show(GamblingNavigation __instance)
        {
            AccessTools.Field(typeof(GamblingNavigation), "isHidden").SetValue(__instance, false);

            if (__instance.direction > 0)
            {
                ShowNavigation(__instance, ref gamblingNavigationSequenceLeft);
            }
            else
            {
                ShowNavigation(__instance, ref gamblingNavigationSequenceRight);
            }
            return false;
        }

        static void ShowNavigation(GamblingNavigation __instance, ref Sequence GamblingNavigationSequence)
        {
            if (GamblingNavigationSequence != null)
            {
                GamblingNavigationSequence.Kill(true);
            }
            __instance.gameObject.SetActive(true);
            GamblingNavigationSequence = DOTween.Sequence().Append(ShortcutExtensions.DOScale(__instance.transform, new Vector3(0.8f, 0.8f, 0.6f), 0.2f).SetEase(Ease.InOutQuad)).Append(ShortcutExtensions.DOScale(__instance.transform, new Vector3(0.6f, 0.6f, 0.6f), 0.1f).SetEase(Ease.InOutQuad));
        }

        //Patch: Prevents arrows from being clickable while the game is paused
        //Also plays navigation arrow clicked sound when triggered using gamepad controls
        //Also prevents arrows from being clickable while they are tweening
        [HarmonyPrefix, HarmonyPatch(typeof(GamblingNavigation), "OnMouseDown")]
        static bool GamblingNavigation_OnMouseDown(GamblingNavigation __instance)
        {
            bool gamblingNavigationSequencePlaying = false;
            if ((gamblingNavigationSequenceLeft != null && gamblingNavigationSequenceLeft.IsPlaying()) || (gamblingNavigationSequenceRight != null && gamblingNavigationSequenceRight.IsPlaying()))
            {
                gamblingNavigationSequencePlaying = true;
            }

            if (__instance.app.model.paused || gamblingNavigationSequencePlaying)
            {
                return false;
            }

            ButtonHoverAnimation buttonHoverAnimation = __instance.GetComponent<ButtonHoverAnimation>();
            if (buttonHoverAnimation && InputManager.Instance.IsUsingGamepadInput())
            {
                buttonHoverAnimation.Clicked();
            }
            return true;
        }
        //***************************************************
        //***************************************************
        //***************************************************

        //Patch: Fixes room lights not illuminating properly when quickly transitioning to a choose lane event or enemy turn event when out of moves
        [HarmonyPrefix, HarmonyPatch(typeof(EnemyRoomView), "ChangeLight")]
        static bool EnemyRoomView_ChangeLight(EnemyRoomView __instance, EnemyRoomView.RoomLightScheme _scheme)
        {
            if ((__instance.app.model.bumboEvent.GetType().ToString() == "SelectSpellColumn" || __instance.app.model.bumboEvent.GetType().ToString() == "StartMonsterTurnEvent") && _scheme == EnemyRoomView.RoomLightScheme.EndTurn)
            {
                //Prevent room light scheme from being changed to EndTurn during SelectSpellColumn or StartMonsterTurnEvent
                return false;
            }
            return true;
        }

        //Patch: Fixes shop needles/trinkets being unclickable after winning a trinket at the cup game while having no empty trinket slots
        [HarmonyPrefix, HarmonyPatch(typeof(CupGamble), "ReadyGame")]
        static bool CupGamble_ReadyGame(CupGamble __instance)
        {
            if (__instance.app.model.bumboEvent.GetType().ToString() != "CupGameResultEvent")
            {
                __instance.readyToPlay = true;
                __instance.gameObject.GetComponent<BoxCollider>().enabled = true;
                __instance.MakeSkullsUnclickable();
                //Prevent Cup Game from erroneously ending current event
                return false;
            }
            return true;
        }

        //Patch: Fixes Prick Pusher not tipping hat after a trinket is purchased
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketChosenToReplaceEvent), "Execute")]
        static void TrinketChosenToReplaceEvent_Execute(TrinketChosenToReplaceEvent __instance)
        {
            if (__instance.app.view.gamblingView != null && __instance.app.model.gamblingModel.cameraAt == 2)
            {
                __instance.app.view.gamblingView.shopClerkView.TipHat();
            }
        }

        //Patch: Fixes Prick Pusher not idling after canceling a purchase
        [HarmonyPostfix, HarmonyPatch(typeof(SpellModifyDelayEvent), "NextEvent")]
        static void SpellModifyDelayEvent_NextEvent(SpellModifyDelayEvent __instance, bool ___delay)
        {
            if (!___delay)
            {
                __instance.app.view.gamblingView.shopClerkView.Idle();
            }
        }

        //Patch: Fixes characters sometimes desyncing on character select screen; Bum-bo carousel no longer resets when returning from the main menu
        //Also resets carousel index and angle when carousel is reset
        [HarmonyPrefix, HarmonyPatch(typeof(SelectCharacterView), "CreateObjects")]
        static bool SelectCharacterView_CreateObjects(SelectCharacterView __instance, ref int ___currentAngle, ref int ___index)
        {
            if (__instance.bumboObjects != null && __instance.bumboObjects.Count > 0)
            {
                //Abort SelectCharacterView CreateObjects; character objects have already been created
                return false;
            }
            ___index = 1;
            ___currentAngle = 0;
            return true;
        }

        //Patch: Resets Bum-bo carousel when progress is deleted
        [HarmonyPostfix, HarmonyPatch(typeof(TitleController), "DeleteProgress")]
        static void TitleController_DeleteProgress(TitleController __instance)
        {
            for (int i = __instance.selectCharacterView.bumboObjects.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(__instance.selectCharacterView.bumboObjects[i]);
            }
            __instance.selectCharacterView.bumboObjects.Clear();

            CharDescView charDescView = GameObject.Find("CharDescView").GetComponent<CharDescView>();
            if (charDescView)
            {
                charDescView.ChangeText(CharacterSheet.BumboType.TheBrave);
            }
        }

        //Patch: Changes the color of the heart symbol on the cup game sign from red to blue; the cup game provides soul health, not red health
        [HarmonyPostfix, HarmonyPatch(typeof(CupGamble), "Start")]
        static void CupGamble_Start(CupGamble __instance)
        {
            GameObject cupGameSign = __instance.view.cupGameSignView.gameObject;

            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            var texture = assets.LoadAsset<Texture2D>("Cash Register");

            cupGameSign.GetComponent<MeshRenderer>().material.mainTexture = texture;
        }

        //Patch: Changes the color of the heart symbol on the stat wheel when playing as Bum-bo the Dead; the wheel provides soul health, not red health
        [HarmonyPostfix, HarmonyPatch(typeof(WheelView), "Start")]
        static void WheelView_Start(WheelView __instance)
        {
            if (__instance.app.model.characterSheet.bumboType != CharacterSheet.BumboType.TheDead)
            {
                return;
            }

            GameObject wheelSliceHeart = __instance.statSlices[5];

            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            var texture = assets.LoadAsset<Texture2D>("Wheel");

            wheelSliceHeart.GetComponent<MeshRenderer>().material.mainTexture = texture;
        }

        //Patch: Changes wheel back mesh to remove red spot
        [HarmonyPostfix, HarmonyPatch(typeof(WheelView), "Start")]
        static void WheelView_Start_Wheel_Back(WheelView __instance)
        {
            GameObject wheelBack = __instance.transform.Find("Wheel_Back").gameObject;

            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            var mesh = assets.LoadAsset<Mesh>("Wheel_Back_Updated");

            wheelBack.GetComponent<MeshFilter>().mesh = mesh;
        }

        //Patch: Changes mesh of use spell icon to fix texture mapping issue
        [HarmonyPostfix, HarmonyPatch(typeof(SpellView), "Start")]
        static void SpellView_Start(SpellView __instance)
        {
            GameObject useIcon = __instance.spellTypeItem;

            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            var iconMesh = assets.LoadAsset<Mesh>("Use_Spell_Icon");

            useIcon.GetComponent<MeshFilter>().mesh = iconMesh;
        }

        //Patch: Corrects character stats on the character select screen that do not match in game values
        //Stout: added 1 soul heart
        //Dead: added 1 puzzle damage
        [HarmonyPostfix, HarmonyPatch(typeof(SelectCharacterView), "Start")]
        static void SelectCharacterView_Start(SelectCharacterView __instance)
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            GameObject stoutStatsObject = __instance.stout.transform.Find("Bumbo The Stout").Find("stout_select").Find("bumbo_select_stats").gameObject;
            var textureStout = assets.LoadAsset<Texture2D>("Stout Stats");
            if (stoutStatsObject != null && textureStout != null)
            {
                //Change stout stats texture
                stoutStatsObject.GetComponent<MeshRenderer>().material.mainTexture = textureStout;
            }

            GameObject deadStatsObject = __instance.dead.transform.Find("Bumbo The Dead").Find("bumbo_select").Find("bumbo_select_stats").gameObject;
            var textureDead = assets.LoadAsset<Texture2D>("Dead Stats");
            if (deadStatsObject != null && textureDead != null)
            {
                //Change dead stats texture
                deadStatsObject.GetComponent<MeshRenderer>().material.mainTexture = textureDead;
            }
        }

        //Patch: Changes the color of Bum-bo the Empty's throwing hand from beige to gray
        [HarmonyPostfix, HarmonyPatch(typeof(BumboThrowView), "ChangeArm", new Type[] { typeof(Block.BlockType), typeof(CharacterSheet.BumboType) })]
        static void BumboThrowView_ChangeArm(BumboThrowView __instance, CharacterSheet.BumboType _bumbo_type)
        {
            //Change hand color if character is Bum-bo the Empty
            if (_bumbo_type == CharacterSheet.BumboType.Eden)
            {
                AssetBundle assets = Windfall.assetBundle;
                if (assets == null)
                {
                    Debug.Log("Failed to load AssetBundle!");
                    return;
                }
                var emptyHandTexture = assets.LoadAsset<Texture2D>("Empty Throw Hand");

                foreach (GameObject gameObject in new GameObject[]
                {
                __instance.AtkBone,
                __instance.AtkBooger,
                __instance.AtkTooth,
                __instance.AtkHeart,
                __instance.AtkPoop,
                __instance.AtkPee,
                __instance.AtkCurse,
                __instance.AtkWild,
                __instance.AtkFist
                })
                {
                    MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
                    if (emptyHandTexture != null)
                    {
                        component.material.mainTexture = emptyHandTexture;
                    }
                }
            }
        }

        //Patch: Fixes attempting to use mouse and gamepad/keyboard controls at the same time causing the input manager to switch between them every frame
        static readonly float inputTypeChangeDelay = 0.2f;
        static bool allowInputTypeChange = true;
        [HarmonyPrefix, HarmonyPatch(typeof(InputManager), "set_input_type")]
        static bool InputManager_set_input_type(InputManager __instance, InputManager.eInputType Type)
        {
            if ((InputManager.eInputType)AccessTools.Field(typeof(InputManager), "m_InputType")?.GetValue(__instance) != Type && allowInputTypeChange)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] InputTypeChange");
                allowInputTypeChange = false;
                __instance.StartCoroutine(InputTypeChangeDelay());
                return true;
            }
            return false;
        }
        static IEnumerator InputTypeChangeDelay()
        {
            yield return new WaitForSeconds(inputTypeChangeDelay);
            allowInputTypeChange = true;
        }

        //public static LoadingController loadingController;
        //[HarmonyPostfix, HarmonyPatch(typeof(LoadingController), "Start")]
        //static void LoadingController_Start(LoadingController __instance)
        //{
        //    loadingController = __instance;
        //}
        ////Patch: Prevents gamepad input while the game is loading
        //[HarmonyPrefix, HarmonyPatch(typeof(InputManager), "IsUsingGamepadInput")]
        //static bool InputManager_IsUsingGamepadInput(InputManager __instance, ref bool __result)
        //{
        //    if (loadingController != null && loadingController.gameObject.activeSelf)
        //    {
        //        __result = false;
        //        return false;
        //    }
        //    return true;
        //}

        //Patch: Makes sure GamepadSpellSelector is closed when the player clicks a spell that is viable for the current action
        [HarmonyPrefix, HarmonyPatch(typeof(SpellView), "OnMouseDown")]
        static void SpellView_OnMouseDown_Gamepad_Fix(SpellView __instance)
        {
            if (!__instance.app.model.paused && (SpellElement)AccessTools.Field(typeof(SpellView), "spell").GetValue(__instance) != null && __instance.gamepadActiveObject != null && __instance.gamepadActiveObject.activeSelf)
            {
                __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);
            }
        }

        //Patch: Makes sure GamepadSpellSelector is closed when the player clicks a trinket that is viable for the current action
        [HarmonyPrefix, HarmonyPatch(typeof(TrinketView), "OnMouseDown")]
        static void TrinketView_OnMouseDown_Gamepad_Fix(TrinketView __instance)
        {
            if (!__instance.app.model.paused && __instance.app.model.characterSheet.trinkets.Count > __instance.trinketIndex && __instance.app.controller.GetTrinket(__instance.trinketIndex) != null && __instance.gamepadActiveObject != null && __instance.gamepadActiveObject.activeSelf)
            {
                __instance.app.view.GUICamera.GetComponent<GamepadSpellSelector>().Close(true);
            }
        }
    }

    static class SoundsModification
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(SoundsModification));
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
}