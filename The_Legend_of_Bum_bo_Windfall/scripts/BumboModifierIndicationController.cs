using DG.Tweening;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class BumboModifierIndicationController : MonoBehaviour
    {
        private List<BumboModifier> bumboModifiers;
        private GameObject expansionToggle;

        private void UpdateExpansionToggle()
        {
            if (expansionToggle == null) CreateExpansionToggle();
            expansionToggle?.GetComponent<ExpansionToggle>()?.DisplayAnimation(bumboModifiers.Count > 0);
        }

        public void CreateExpansionToggle()
        {
            if (expansionToggle != null) return;

            string expansionTogglePath = "Toggle";
            if (Windfall.assetBundle.Contains(expansionTogglePath))
            {
                expansionToggle = WindfallHelper.ResetShader(UnityEngine.Object.Instantiate((GameObject)Windfall.assetBundle.LoadAsset<GameObject>(expansionTogglePath), WindfallHelper.app.view.GUICamera.transform.Find("HUD")).transform).gameObject;
                expansionToggle.layer = 5;
                expansionToggle.SetActive(false);
                expansionToggle.AddComponent<ExpansionToggle>();

                BoxCollider boxCollider = expansionToggle.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1f, 1f, 1f);
                boxCollider.isTrigger = true;

                expansionToggle.transform.localPosition = expansionToggle.GetComponent<ExpansionToggle>().HidingPosition();
                expansionToggle.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

                ButtonHoverAnimation buttonHoverAnimation = expansionToggle.AddComponent<ButtonHoverAnimation>();
                buttonHoverAnimation.hoverSoundFx = SoundsView.eSound.NoSound;
                buttonHoverAnimation.clickSoundFx = SoundsView.eSound.NoSound;
            }
        }

        public void ToggleModifierExpansion()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            windfallPersistentData.expandModifiers = !windfallPersistentData.expandModifiers;
            WindfallPersistentDataController.SaveData(windfallPersistentData);

            UpdateModifiers();
        }

        public IEnumerator UpdateModifiersDelayed()
        {
            //wait one frame
            yield return 0;
            UpdateModifiers();
        }

        private void UpdateModifiers()
        {
            if (WindfallHelper.app?.model?.characterSheet == null) return;

            int modifierDispayCounter = 0;

            //Block order:
            //Brown Belt
            //Trash Lid
            //Old Pillow
            //Holy Mantle

            //List order determines modifier display order
            List<SpellName> trackedSpells = new List<SpellName>
            {
                SpellName.BrownBelt, //Modifier object
                SpellName.TrashLid, //Modifier object
                SpellName.OldPillow, //Modifier object

                SpellName.BarbedWire, //Modifier object
                SpellName.Euthanasia, //Modifier object
                SpellName.OrangeBelt, //Modifier object

                SpellName.TheVirus, //Round modifier

                SpellName.LooseChange, //Round modifier

                SpellName.BlindRage, //Room modifier

                SpellName.SmokeMachine, //Modifier object
                SpellName.YellowBelt, //Modifier object

                SpellName.Pause, //Round modifier
                SpellName.StopWatch, //Round modifier

                SpellName.RoidRage, //Round modifier

                SpellName.TwentyTwenty, //Round modifier

                SpellName.WhiteBelt, //Modifier object

                SpellName.WoodenSpoon, //Room modifier
            };

            //Update spell modifiers
            for (int spellCounter = 0; spellCounter < trackedSpells.Count; spellCounter++)
            {
                if (ConvertModifier(null, trackedSpells[spellCounter], TrinketName.None, SpellConversionValue(trackedSpells[spellCounter]), modifierDispayCounter)) modifierDispayCounter++;
            }

            //Update actionPointModifier
            short actionPointModifier = WindfallHelper.app.model.actionPointModifier;
            if (ConvertModifier("actionPointModifier", SpellName.None, TrinketName.None, actionPointModifier != 0 ? actionPointModifier.ToString() : null, modifierDispayCounter)) modifierDispayCounter++;

            UpdateExpansionToggle();
        }

        private string SpellConversionValue(SpellName _spellSource)
        {
            CharacterSheet characterSheet = WindfallHelper.app.model.characterSheet;

            CharacterSheet.BumboRoundModifiers bumboRoundModifiers = characterSheet.bumboRoundModifiers;
            CharacterSheet.BumboRoomModifiers bumboRoomModifiers = characterSheet.bumboRoomModifiers;
            List<CharacterSheet.BumboModifierObject> bumboModifierObjects = characterSheet.bumboModifierObjects;

            switch (_spellSource)
            {
                //Round modifiers
                case SpellName.LooseChange:
                    string coins = WindfallPersistentDataController.LoadData().implementBalanceChanges ? OtherChanges.looseChangeCoinGain.ToString() : "1";
                    return bumboRoundModifiers.coinForHurt ? coins : null;
                case SpellName.Pause:
                    return bumboRoundModifiers.skipEnemyTurns > 0 ? bumboRoundModifiers.skipEnemyTurns.ToString() : null;
                case SpellName.RoidRage:
                    return bumboRoundModifiers.crit ? "100%" : null;
                case SpellName.StopWatch:
                    return bumboRoundModifiers.slow ? "1" : null;
                case SpellName.TheVirus:
                    return bumboRoundModifiers.poisonRounds > 0 ? "1" : null;
                case SpellName.TwentyTwenty:
                    return bumboRoundModifiers.repeatComboCount > 0 ? bumboRoundModifiers.repeatComboCount.ToString() : null;

                //Room modifiers
                case SpellName.BlindRage:
                    return bumboRoomModifiers.damageMultiplier > 1 ? "x" + bumboRoomModifiers.damageMultiplier.ToString() : null;
                case SpellName.WoodenSpoon:
                    return bumboRoomModifiers.actionPoints > 0 ? bumboRoomModifiers.actionPoints.ToString() : null;

                //Modifier objects
                case SpellName.BarbedWire:
                    CharacterSheet.BumboModifierObject barbedWireModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (barbedWireModifierObject != null && barbedWireModifierObject.damageOnHit > 0) ? barbedWireModifierObject.damageOnHit.ToString() : null;
                case SpellName.BrownBelt:
                    CharacterSheet.BumboModifierObject brownBeltModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (brownBeltModifierObject != null) ? WindfallHelper.app.model.characterSheet.getItemDamage().ToString() : null;
                case SpellName.Euthanasia:
                    CharacterSheet.BumboModifierObject euthanasiaModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (euthanasiaModifierObject != null && euthanasiaModifierObject.damageOnHit > 0) ? euthanasiaModifierObject.damageOnHit.ToString() : null;
                case SpellName.OldPillow:
                    CharacterSheet.BumboModifierObject oldPillowModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (oldPillowModifierObject != null && oldPillowModifierObject.blockOnce) ? "1" : null;
                case SpellName.OrangeBelt:
                    CharacterSheet.BumboModifierObject orangeBeltModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (orangeBeltModifierObject != null && orangeBeltModifierObject.counterDamage > 0f) ? Mathf.RoundToInt(orangeBeltModifierObject.counterDamage).ToString() : null;
                case SpellName.SmokeMachine:
                    CharacterSheet.BumboModifierObject smokeMachineModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (smokeMachineModifierObject != null && smokeMachineModifierObject.dodgeChance > 0f) ? Mathf.RoundToInt(smokeMachineModifierObject.dodgeChance * 100).ToString() + "%" : null;
                case SpellName.TrashLid:
                    CharacterSheet.BumboModifierObject trashLidModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (trashLidModifierObject != null && trashLidModifierObject.blockCounter > 0) ? trashLidModifierObject.blockCounter.ToString() : null;
                case SpellName.WhiteBelt:
                    CharacterSheet.BumboModifierObject whiteBeltModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (whiteBeltModifierObject != null) ? "1" : null;
                case SpellName.YellowBelt:
                    CharacterSheet.BumboModifierObject yellowBeltModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (yellowBeltModifierObject != null && yellowBeltModifierObject.dodgeChance > 0f) ? Mathf.RoundToInt(yellowBeltModifierObject.dodgeChance * 100).ToString() + "%" : null;
                default:
                    return null;
            }
        }

        private bool ConvertModifier(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value, int _index)
        {
            BumboModifier bumboModifier = UpdateModifier(_source, _spellSource, _trinketSource, _value, _index);
            if (bumboModifier != null)
            {
                DisplayModifier(bumboModifier);
                return true;
            }
            return false;
        }

        private BumboModifier UpdateModifier(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value, int _index)
        {
            if (bumboModifiers == null) bumboModifiers = new List<BumboModifier>();

            bumboModifiers.RemoveAll(modifier => modifier == null || modifier.gameObject == null);

            BumboModifier existingModifier = null;

            if (_source != null && _source != string.Empty)
            {
                existingModifier = bumboModifiers.Find(bumboModifier => bumboModifier.source == _source);
            }
            if (existingModifier == null && _spellSource != SpellName.None)
            {
                existingModifier = bumboModifiers.Find(bumboModifier => bumboModifier.spellSource == _spellSource);
            }
            if (existingModifier == null && _trinketSource != TrinketName.None)
            {
                existingModifier = bumboModifiers.Find(bumboModifier => bumboModifier.trinketSource == _trinketSource);
            }

            if (existingModifier != null)
            {
                if (_value == null)
                {
                    bumboModifiers.Remove(existingModifier);
                    existingModifier.RemoveModifierAnimation();
                    return null;
                }

                if (existingModifier.value != _value) existingModifier.UpdateModifierAnimation();

                existingModifier.value = _value;
                existingModifier.index = _index;

                return existingModifier;
            }

            if (_value == null) return null;

            BumboModifier bumboModifier = UnityEngine.Object.Instantiate(Windfall.assetBundle?.LoadAsset<GameObject>("Modifier Display V2"), WindfallHelper.app.view.GUICamera.transform.Find("HUD")).AddComponent<BumboModifier>();
            bumboModifier.Init(_source, _spellSource, _trinketSource, _value, _index);

            bumboModifiers.Add(bumboModifier);

            return bumboModifier;
        }

        private void DisplayModifier(BumboModifier bumboModifier)
        {
            if (WindfallHelper.app == null) return;

            bool newModifier = false;
            if (!bumboModifier.displayInitialized)
            {
                InitializeModifierDisplay(bumboModifier);
                newModifier = true;
            }

            //Update modifier display elements and position
            UpdateModifierDisplay(bumboModifier);

            Transform modifierDisplayTransform = bumboModifier.gameObject.transform;
            if (modifierDisplayTransform != null)
            {
                if (newModifier) bumboModifier.AddModifierAnimation();
                else bumboModifier.MoveModifierAnimation();
            }
        }

        private void InitializeModifierDisplay(BumboModifier bumboModifier)
        {
            if (bumboModifier == null) return;

            bumboModifier.displayInitialized = true;

            bumboModifier.objectTinter = bumboModifier.gameObject.AddComponent<ObjectTinter>();

            bumboModifier.gameObject.AddComponent<WindfallTooltip>();

            bumboModifier.boxCollider = bumboModifier.gameObject.GetComponent<BoxCollider>();

            Transform modifierDisplayTransform = bumboModifier.transform;
            modifierDisplayTransform.localPosition = BumboModifier.baseDisplayPosition;
            modifierDisplayTransform.localEulerAngles = new Vector3(0f, 180f, 0f);
            modifierDisplayTransform.localScale = new Vector3(0.1f, 0.1f, 0.09f);

            WindfallHelper.ResetShader(modifierDisplayTransform);

            bumboModifier.modifierDisplayCollectibleTransform = modifierDisplayTransform.Find("Collectible");
            bumboModifier.modifierDisplayBackTransform = modifierDisplayTransform.Find("Back");
            bumboModifier.modifierDisplayIconTransform = modifierDisplayTransform.Find("Icon");
            bumboModifier.modifierDisplayValueTransform = modifierDisplayTransform.Find("Value");
            bumboModifier.effectValueTransform = modifierDisplayTransform.Find("ValueText");

            bumboModifier.stackingTransform = modifierDisplayTransform.Find("Stacking");
            bumboModifier.stackingTransform.gameObject.AddComponent<BumboModifierStacking>().bumboModifier = bumboModifier;
            bumboModifier.stackingTransform.gameObject.AddComponent<WindfallTooltip>();

            bumboModifier.timerTransform = modifierDisplayTransform.Find("Temporary");
            bumboModifier.timerTransform.gameObject.AddComponent<BumboModifierTemporary>().bumboModifier = bumboModifier;
            bumboModifier.timerTransform.gameObject.AddComponent<WindfallTooltip>();

            //Set effect value
            TextMeshPro effectValueTextMeshPro = bumboModifier.effectValueTransform.GetComponent<TextMeshPro>();
            if (effectValueTextMeshPro != null)
            {
                LocalizationModifier.ChangeFont(null, effectValueTextMeshPro, WindfallHelper.GetEdmundMcmillenFont());
            }

            //Set spell texture
            if (bumboModifier.spellSource != SpellName.None)
            {
                SpellElement spellElement = WindfallHelper.app.model.spellModel.spells[bumboModifier.spellSource];

                List<Material> newMaterials = new List<Material>();

                Material iconMaterial = new Material(WindfallHelper.app.model.spellModel.Icon(spellElement.Category, true, spellElement.texturePage));
                if (iconMaterial != null)
                {
                    iconMaterial.SetTextureOffset("_MainTex", spellElement.IconPosition);
                    newMaterials.Add(iconMaterial);
                }

                Material cardboardMaterial = Windfall.assetBundle.LoadAsset<Material>("Cardboard Seam");
                if (cardboardMaterial != null) newMaterials.Add(cardboardMaterial);

                if (newMaterials.Count > 0) bumboModifier.modifierDisplayCollectibleTransform.GetComponent<MeshRenderer>().materials = newMaterials.ToArray();
            }
            else
            {
                //Disable if there is no spell
                bumboModifier.modifierDisplayCollectibleTransform.gameObject.SetActive(false);

                //Center effect icon
                Vector3 localPosition = bumboModifier.modifierDisplayIconTransform.localPosition;
                bumboModifier.modifierDisplayIconTransform.localPosition = new Vector3(1.07f, localPosition.y, localPosition.z);
            }
        }

        private void UpdateModifierDisplay(BumboModifier bumboModifier)
        {
            if (bumboModifier == null) return;

            TextMeshPro effectValueTextMeshPro = bumboModifier.effectValueTransform.GetComponent<TextMeshPro>();
            if (effectValueTextMeshPro != null && bumboModifier.value != null)
            {
                effectValueTextMeshPro.text = bumboModifier.value;
                //effectValueTextMeshPro.color = bumboModifier.valueDisplayType == BumboModifier.ValueDisplayType.Hurt ? Color.white : Color.black;
            }

            bumboModifier.modifierDisplayValueTransform.gameObject.SetActive(bumboModifier.valueDisplayType != BumboModifier.ValueDisplayType.None);
            MeshRenderer valueMeshRenderer = bumboModifier.modifierDisplayValueTransform.GetComponent<MeshRenderer>();
            if (valueMeshRenderer != null)
            {
                float offsetY = bumboModifier.valueDisplayType == BumboModifier.ValueDisplayType.Hurt ? 0.426f : 0f;
                valueMeshRenderer.material.mainTextureOffset = new Vector2(0f, offsetY);
            }

            bumboModifier.effectValueTransform.gameObject.SetActive(bumboModifier.valueDisplayType != BumboModifier.ValueDisplayType.None);

            bumboModifier.timerTransform.gameObject.SetActive(bumboModifier.modifierType == CharacterSheet.BumboModifierObject.ModifierType.Round);
            if (bumboModifier.spellSource != SpellName.None)
            {
                bumboModifier.timerTransform.gameObject.SetActive(BumboModifierTemporary.TemporarySpellsources.Contains(bumboModifier.spellSource));
            }

            bumboModifier.stackingTransform.gameObject.SetActive(bumboModifier.canStack);

            bool overrideIconChange = false;
            if (bumboModifier.spellSource == SpellName.TrashLid && bumboModifier.value == "1")
            {
                ChangeModifierDisplayIcon(bumboModifier, "Shield");
                overrideIconChange = true;
            }

            if (!overrideIconChange) ChangeModifierDisplayIcon(bumboModifier, null);
        }

        private readonly Dictionary<SpellName, string> textureAssignment = new Dictionary<SpellName, string>
        {
            {SpellName.TheVirus, "Thorns Poison" }
        };

        private void ChangeModifierDisplayIcon(BumboModifier bumboModifier, string iconObjectName)
        {
            if (bumboModifier == null) return;

            string newIconObjectName = null;

            if (iconObjectName != null && iconObjectName != string.Empty) newIconObjectName = iconObjectName;
            else if (bumboModifier.iconObjectName != null) newIconObjectName = bumboModifier.iconObjectName;

            if (newIconObjectName == null || (bumboModifier.modifierDisplayIconObjectTransform != null && bumboModifier.modifierDisplayIconObjectTransform.gameObject.name == newIconObjectName)) return;

            if (Windfall.assetBundle != null && Windfall.assetBundle.Contains(newIconObjectName))
            {
                if (bumboModifier.modifierDisplayIconObjectTransform != null)
                {
                    UnityEngine.Object.Destroy(bumboModifier.modifierDisplayIconObjectTransform.gameObject);
                }

                GameObject iconObject = Windfall.assetBundle.LoadAsset<GameObject>(newIconObjectName);

                if (iconObject != null)
                {
                    bumboModifier.modifierDisplayIconObjectTransform = UnityEngine.Object.Instantiate(iconObject, bumboModifier.modifierDisplayIconTransform).transform;

                    //Assign alternate textures
                    if (textureAssignment.TryGetValue(bumboModifier.spellSource, out string textureName))
                    {
                        if (Windfall.assetBundle.Contains(textureName))
                        {
                            bumboModifier.modifierDisplayIconObjectTransform.GetComponent<MeshRenderer>().material = Windfall.assetBundle.LoadAsset<Material>(textureName);
                        }
                    }

                    WindfallHelper.ResetShader(bumboModifier.modifierDisplayIconObjectTransform);

                    bumboModifier.modifierDisplayIconObjectTransform.localEulerAngles = Vector3.zero;
                    bumboModifier.modifierDisplayIconObjectTransform.localPosition = Vector3.zero;
                    bumboModifier.modifierDisplayIconObjectTransform.gameObject.name = newIconObjectName;
                    bumboModifier.modifierDisplayIconObjectTransform.gameObject.layer = 5;
                }
            }
        }
    }

    public class BumboModifierIndicationPatches()
    {
        //Patch: Display bumbo modifiers on RoomStartEvent
        [HarmonyPostfix, HarmonyPatch(typeof(RoomStartEvent), "Execute")]
        static void RoomStartEvent_Execute_ModifierDisplay(RoomStartEvent __instance)
        {
            __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on NewRoundEvent
        [HarmonyPostfix, HarmonyPatch(typeof(NewRoundEvent), "Execute")]
        static void NewRoundEvent_Execute_ModifierDisplay(NewRoundEvent __instance)
        {
            __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on CastSpell
        [HarmonyPostfix, HarmonyPatch(typeof(SpellElement), "CastSpell")]
        static void SpellElement_CastSpell_ModifierDisplay(SpellElement __instance, bool __result)
        {
            if (__result)
            {
                __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
            }
        }
        //Patch: Display bumbo modifiers on Discharge
        [HarmonyPostfix, HarmonyPatch(typeof(SpellElement), "Discharge")]
        static void SpellElement_Discharge_ModifierDisplay(SpellElement __instance, bool __result)
        {
            if (__result)
            {
                __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
            }
        }
        //Patch: Display bumbo modifiers on UseTrinket Use
        [HarmonyPostfix, HarmonyPatch(typeof(UseTrinket), "Use")]
        static void UseTrinket_Use_ModifierDisplay(UseTrinket __instance)
        {
            __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on TakeDamage
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "TakeDamage")]
        static void BumboController_TakeDamage_ModifierDisplay(BumboController __instance)
        {
            __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on Enemy Hurt
        [HarmonyPostfix, HarmonyPatch(typeof(Enemy), "Hurt")]
        static void Enemy_Hurt_ModifierDisplay(Enemy __instance)
        {
            __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on NextComboEvent
        [HarmonyPostfix, HarmonyPatch(typeof(NextComboEvent), "NextEvent")]
        static void NextComboEvent_NextEvent_ModifierDisplay(NextComboEvent __instance)
        {
            __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
        }
        //Patch: Display bumbo modifiers on Enemy Act
        [HarmonyPostfix, HarmonyPatch(typeof(Enemy), nameof(Enemy.Act))]
        static void Enemy_Act_ModifierDisplay(Enemy __instance)
        {
            __instance.app.StartCoroutine(WindfallHelper.BumboModifierIndicationController.UpdateModifiersDelayed());
        }
    }

    class ExpansionToggle : MonoBehaviour
    {
        private Sequence toggleSequence;
        private Sequence displaySequence;
        private readonly float tweenDuration = 0.3f;

        public readonly Vector3 showingRotation = new Vector3(0f, 0f, 180f);
        public readonly Vector3 hidingRotation = new Vector3(0f, 0f, 0f);

        public readonly Vector3 showingPosition = new Vector3(-0.73f, 0.41f, 1.15f);

        private void Start()
        {
            transform.localEulerAngles = TargetRotation();
        }

        //Trigger toggle using Keyboard/gamepad controls
        void Update()
        {
            Dictionary<string, KeyCode> hotkeys = WindfallPersistentDataController.LoadData().hotkeys;
            if (hotkeys.TryGetValue("SHOW_HIDE_INDICATORS", out KeyCode keyCode) && Input.GetKeyDown(keyCode)) OnMouseDown();
        }

        void OnMouseDown()
        {
            if (WindfallHelper.app.model.paused) return;
            if (!gameObject.activeSelf) return;
            if (toggleSequence != null && toggleSequence.IsPlaying()) return;

            WindfallHelper.BumboModifierIndicationController.ToggleModifierExpansion();
            ToggleAnimation();
        }

        void ToggleAnimation()
        {
            if (toggleSequence != null && toggleSequence.IsPlaying()) toggleSequence.Kill(true);
            toggleSequence = DOTween.Sequence();
            toggleSequence.Append(transform.DOLocalRotate(TargetRotation(), tweenDuration).SetEase(Ease.InOutQuad));

            SoundsView.Instance.PlaySound(SoundsView.eSound.PuzzleSlideIn, SoundsView.eAudioSlot.Default, false);
        }

        private Vector3 TargetRotation()
        {
            return WindfallPersistentDataController.LoadData().expandModifiers ? showingRotation : hidingRotation;
        }

        public void DisplayAnimation(bool _active)
        {
            if (_active == gameObject.activeSelf) return;

            Vector3 targetPosition = _active ? showingPosition : HidingPosition();

            if (targetPosition == transform.localPosition) return;

            if (displaySequence != null && displaySequence.IsPlaying()) displaySequence.Kill(false);
            displaySequence = DOTween.Sequence();

            if (_active)
            {
                gameObject.SetActive(true);
                displaySequence.Append(transform.DOLocalMove(targetPosition, tweenDuration).SetEase(Ease.InOutQuad));
            }
            else
            {
                displaySequence.Append(transform.DOLocalMove(targetPosition, tweenDuration).SetEase(Ease.InOutQuad));
                displaySequence.AppendCallback(delegate
                {
                    gameObject.SetActive(false);
                });
            }
        }

        public Vector3 HidingPosition()
        {
            return showingPosition + new Vector3(-0.3f, 0f, 0f);
        }
    }
}
