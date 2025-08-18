using DG.Tweening;
using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class BumboModifierIndication
    {
        static List<BumboModifier> bumboModifiers;

        static GameObject expansionToggle;

        private static void UpdateExpansionToggle()
        {
            if (expansionToggle == null)
            {
                CreateExpansionToggle();
            }

            ExpansionToggle expansionToggleComponent = expansionToggle.GetComponent<ExpansionToggle>();
            if (expansionToggleComponent != null)
            {
                expansionToggleComponent.DisplayAnimation(bumboModifiers.Count > 0);
            }
        }

        public static void CreateExpansionToggle()
        {
            if (expansionToggle != null)
            {
                return;
            }

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

        public static void ToggleModifierExpansion()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            windfallPersistentData.expandModifiers = !windfallPersistentData.expandModifiers;
            WindfallPersistentDataController.SaveData(windfallPersistentData);

            UpdateModifiers();
        }

        public static IEnumerator UpdateModifiersDelayed()
        {
            //wait one frame
            yield return 0;
            UpdateModifiers();
        }

        public static void UpdateModifiers()
        {
            if (WindfallHelper.app?.model?.characterSheet == null)
            {
                return;
            }

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
                if (ConvertModifier(null, trackedSpells[spellCounter], TrinketName.None, SpellConversionValue(trackedSpells[spellCounter]), modifierDispayCounter))
                {
                    modifierDispayCounter++;
                }
            }

            //Update actionPointModifier
            short actionPointModifier = WindfallHelper.app.model.actionPointModifier;
            if (ConvertModifier("actionPointModifier", SpellName.None, TrinketName.None, actionPointModifier != 0 ? actionPointModifier.ToString() : null, modifierDispayCounter))
            {
                modifierDispayCounter++;
            }

            UpdateExpansionToggle();
        }

        static string SpellConversionValue(SpellName _spellSource)
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

        static bool ConvertModifier(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value, int _index)
        {
            BumboModifier bumboModifier = UpdateModifier(_source, _spellSource, _trinketSource, _value, _index);
            if (bumboModifier != null)
            {
                DisplayModifier(bumboModifier);
                return true;
            }
            return false;
        }

        static BumboModifier UpdateModifier(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value, int _index)
        {
            if (bumboModifiers == null)
            {
                bumboModifiers = new List<BumboModifier>();
            }

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

                if (existingModifier.value != _value)
                {
                    existingModifier.UpdateModifierAnimation();
                }

                existingModifier.value = _value;
                existingModifier.index = _index;

                return existingModifier;
            }

            if (_value == null)
            {
                return null;
            }

            BumboModifier bumboModifier = UnityEngine.Object.Instantiate(Windfall.assetBundle?.LoadAsset<GameObject>("Modifier Display V2"), WindfallHelper.app.view.GUICamera.transform.Find("HUD")).AddComponent<BumboModifier>();
            bumboModifier.Init(_source, _spellSource, _trinketSource, _value, _index);

            bumboModifiers.Add(bumboModifier);

            return bumboModifier;
        }

        static void DisplayModifier(BumboModifier bumboModifier)
        {
            if (WindfallHelper.app == null)
            {
                return;
            }

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
                if (newModifier)
                {
                    bumboModifier.AddModifierAnimation();
                }
                else
                {
                    bumboModifier.MoveModifierAnimation();
                }
            }
        }

        static void InitializeModifierDisplay(BumboModifier bumboModifier)
        {
            if (bumboModifier == null)
            {
                return;
            }

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
                if (cardboardMaterial != null)
                {
                    newMaterials.Add(cardboardMaterial);
                }

                if (newMaterials.Count > 0)
                {
                    bumboModifier.modifierDisplayCollectibleTransform.GetComponent<MeshRenderer>().materials = newMaterials.ToArray();
                }
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

        static void UpdateModifierDisplay(BumboModifier bumboModifier)
        {
            if (bumboModifier == null)
            {
                return;
            }

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

            if (!overrideIconChange)
            {
                ChangeModifierDisplayIcon(bumboModifier, null);
            }
        }

        static readonly Dictionary<SpellName, string> textureAssignment = new Dictionary<SpellName, string>
        {
            {SpellName.TheVirus, "Thorns Poison" }
        };

        static void ChangeModifierDisplayIcon(BumboModifier bumboModifier, string iconObjectName)
        {
            if (bumboModifier == null)
            {
                return;
            }

            string newIconObjectName = null;

            if (iconObjectName != null && iconObjectName != string.Empty)
            {
                newIconObjectName = iconObjectName;
            }
            else if (bumboModifier.iconObjectName != null)
            {
                newIconObjectName = bumboModifier.iconObjectName;
            }

            if (newIconObjectName == null || (bumboModifier.modifierDisplayIconObjectTransform != null && bumboModifier.modifierDisplayIconObjectTransform.gameObject.name == newIconObjectName))
            {
                return;
            }

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
            if (WindfallHelper.app.model.paused)
            {
                return;
            }

            if (!gameObject.activeSelf)
            {
                return;
            }

            if (toggleSequence != null && toggleSequence.IsPlaying())
            {
                return;
            }

            BumboModifierIndication.ToggleModifierExpansion();

            ToggleAnimation();
        }

        void ToggleAnimation()
        {
            if (toggleSequence != null && toggleSequence.IsPlaying())
            {
                toggleSequence.Kill(true);
            }
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
            if (_active == gameObject.activeSelf)
            {
                return;
            }

            Vector3 targetPosition = _active ? showingPosition : HidingPosition();

            if (targetPosition == transform.localPosition)
            {
                return;
            }

            if (displaySequence != null && displaySequence.IsPlaying())
            {
                displaySequence.Kill(false);
            }
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

    class BumboModifier : MonoBehaviour
    {
        public bool displayInitialized = false;

        static Color hiddenTintColor = new Color(0.2f, 0.2f, 0.2f);

        static readonly float removalHorizontalOffset = 0.6f;
        static readonly float hidingHorizontalOffset = 0.3f;
        static readonly float punchScale = 0.012f;
        static readonly float tweenDuration = 0.3f;

        public static readonly Vector3 baseDisplayPosition = new Vector3(-0.54f, 0.26f, 1.17f);
        static readonly float displayIndexOffset = 0.2f;

        public enum ModifierCategory
        {
            None,
            Block,
            Retaliate,
            Dodge,
        }

        public enum ValueDisplayType
        {
            None,
            Standard,
            Hurt,
        }

        public CharacterSheet.BumboModifierObject.ModifierType modifierType;
        public ModifierCategory modifierCategory;

        public string source;
        public SpellName spellSource;
        public TrinketName trinketSource;

        public string value;
        public ValueDisplayType valueDisplayType;

        public bool canStack;

        public string iconObjectName;

        public int index;

        public ObjectTinter objectTinter;

        public BoxCollider boxCollider;

        public Transform modifierDisplayCollectibleTransform;
        public Transform modifierDisplayBackTransform;
        public Transform modifierDisplayIconTransform;
        public Transform modifierDisplayValueTransform;
        public Transform effectValueTransform;
        public Transform timerTransform;
        public Transform stackingTransform;

        public Transform modifierDisplayIconObjectTransform;

        public Sequence movementSequence;
        public Sequence updateSequence;

        public bool Expanded()
        {
            return WindfallPersistentDataController.LoadData().expandModifiers;
        }
        Vector3 TargetPosition()
        {
            return Expanded() ? ShowingPosition() : HidingPosition();
        }
        Vector3 ShowingPosition()
        {
            return baseDisplayPosition + new Vector3(0, -displayIndexOffset * index, 0);
        }
        Vector3 HidingPosition()
        {
            return ShowingPosition() + new Vector3(-hidingHorizontalOffset, 0, 0);
        }
        Vector3 RemovalPosition()
        {
            return ShowingPosition() + new Vector3(-removalHorizontalOffset, 0, 0);
        }

        public Vector3 TooltipPosition()
        {
            return transform.parent.TransformPoint(transform.localPosition + new Vector3(0.07f, -0.01f, 0f));
            //return transform.parent.TransformPoint(TargetPosition() + new Vector3(0.04f, -0.01f, 0f));
        }

        public void UpdateTint()
        {
            if (objectTinter == null)
            {
                return;
            }

            if (Expanded())
            {
                objectTinter.NoTint();
                return;
            }
            objectTinter.Tint(hiddenTintColor);
        }

        public void AddModifierAnimation()
        {
            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(false);
            }

            transform.localPosition = RemovalPosition();

            movementSequence = DOTween.Sequence();
            movementSequence.Append(transform.DOLocalMove(TargetPosition(), tweenDuration).SetEase(Ease.InOutQuad));

            UpdateTint();
        }

        public void RemoveModifierAnimation()
        {
            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(false);
            }

            movementSequence.Append(transform.DOLocalMove(RemovalPosition(), tweenDuration).SetEase(Ease.InOutQuad));
            movementSequence.AppendCallback(delegate
            {
                UnityEngine.Object.Destroy(gameObject);
            });
        }

        public void MoveModifierAnimation()
        {
            Vector3 targetPosition = TargetPosition();
            if (targetPosition == Vector3.zero || targetPosition == transform.localPosition)
            {
                return;
            }

            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(false);
            }
            movementSequence = DOTween.Sequence();

            movementSequence.Append(transform.DOLocalMove(targetPosition, tweenDuration).SetEase(Ease.InOutQuad));
            movementSequence.AppendCallback(delegate{ UpdateTint(); });
        }

        public void UpdateModifierAnimation()
        {
            if (updateSequence != null && updateSequence.IsPlaying())
            {
                updateSequence.Kill(true);
            }

            updateSequence = DOTween.Sequence();
            updateSequence.Append(transform.DOPunchScale(new Vector3(punchScale, punchScale, punchScale), tweenDuration, 1, 0));
        }

        public string Description()
        {
            if (value == null)
            {
                return "";
            }

            string description = string.Empty;

            switch (modifierCategory)
            {
                case ModifierCategory.Block:
                    description = LocalizationModifier.GetLanguageText("BLOCK_STATUS", "Indicators");
                    description = description.Replace("[value]", value);
                    //Handle English pluralization
                    if (value == "1" && LocalizationManager.CurrentLanguage == "English")
                    {
                        description = description.Replace("attacks", "attack");
                        description = description.Replace("1", "an");
                    }
                    break;
                case ModifierCategory.Dodge:
                    description = LocalizationModifier.GetLanguageText("DODGE_STATUS", "Indicators");
                    description = description.Replace("[value]", value);
                    break;
                case ModifierCategory.Retaliate:
                    description = LocalizationModifier.GetLanguageText("RETALIATE_STATUS", "Indicators");
                    description = description.Replace("[value]", value);
                    break;
            }

            switch (source)
            {
                case "actionPointModifier":
                    if (value.Contains("-"))
                    {
                        string displayValue = value;
                        int minusIndex = displayValue.IndexOf("-");
                        if (minusIndex >= 0) displayValue = displayValue.Remove(minusIndex, 1);

                        description = LocalizationModifier.GetLanguageText("ACTION_POINT_LOSS_STATUS", "Indicators");
                        description = description.Replace("[value]", value);

                        if (value == "1" && LocalizationManager.CurrentLanguage == "English") description = description.Replace("moves", "move");
                    }
                    else
                    {
                        description = LocalizationModifier.GetLanguageText("ACTION_POINT_GAIN_STATUS", "Indicators");
                        description = description.Replace("[value]", value);

                        if (value == "1" && LocalizationManager.CurrentLanguage == "English") description = description.Replace("moves", "move");
                    }

                    break;
            }

            if (description == string.Empty)
            {
                string displayValue = value;
                switch (spellSource)
                {
                    case SpellName.BrownBelt:
                        description = "BROWN_BELT_STATUS";
                        break;
                    case SpellName.BlindRage:
                        description = "BLIND_RAGE_STATUS";
                        displayValue = displayValue.Remove(0, 1);
                        break;
                    case SpellName.Euthanasia:
                        description = "EUTHANASIA_STATUS";
                        break;
                    case SpellName.LooseChange:
                        description = "LOOSE_CHANGE_STATUS";
                        break;
                    case SpellName.Pause:
                        description = "PAUSE_STATUS";
                        break;
                    case SpellName.RoidRage:
                        description = "ROID_RAGE_STATUS";
                        break;
                    case SpellName.StopWatch:
                        description = "STOP_WATCH_STATUS";
                        break;
                    case SpellName.TheVirus:
                        description = "THE_VIRUS_STATUS";
                        break;
                    case SpellName.TwentyTwenty:
                        description = "TWENTY_TWENTY_STATUS";
                        break;
                    case SpellName.WhiteBelt:
                        description = "WHITE_BELT_STATUS";
                        break;
                    case SpellName.WoodenSpoon:
                        description = "WOODEN_SPOON_STATUS";
                        break;
                }

                description = LocalizationModifier.GetLanguageText(description, "Indicators");
                description = description.Replace("[value]", displayValue);
            }

            return description;
        }

        public string StackingDescription()
        {
            if (value == null)
            {
                return string.Empty;
            }

            string stacking = "STACKING_STATUS";

            switch (spellSource)
            {
                case SpellName.BarbedWire:
                    stacking = "STACKING_DAMAGE_STATUS";
                    break;
                case SpellName.OrangeBelt:
                    stacking = "STACKING_DAMAGE_STATUS";
                    break;
                case SpellName.YellowBelt:
                    stacking = "STACKING_DODGE_STATUS";
                    break;
            }

            stacking = LocalizationModifier.GetLanguageText(stacking, "Indicators");
            string stackingCap = CollectibleChanges.PercentSpellEffectStackingCap(spellSource).ToString();
            stacking = stacking.Replace("[value]", stackingCap);
            return stacking;
        }

        public void Init(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value, int _index)
        {
            if (_source != null && _source != string.Empty)
            {
                switch (_source)
                {
                    case "actionPointModifier":
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = true;
                        iconObjectName = "MoveChange";
                        break;
                }

                source = _source;
            }
            else if (_spellSource != SpellName.None)
            {
                switch (_spellSource)
                {
                    case SpellName.BarbedWire:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.Hurt;
                        canStack = true;
                        iconObjectName = "Thorns";
                        break;
                    case SpellName.BlindRage:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = true;
                        iconObjectName = "Vulnerable";
                        break;
                    case SpellName.BrownBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.Hurt;
                        canStack = false;
                        iconObjectName = "ShieldThorns";
                        break;
                    case SpellName.Euthanasia:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.Hurt;
                        canStack = false;
                        iconObjectName = "Thorns";
                        break;
                    case SpellName.LooseChange:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = false;
                        iconObjectName = "HurtCoin";
                        break;
                    case SpellName.OldPillow:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "Shield";
                        break;
                    case SpellName.OrangeBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.Hurt;
                        canStack = true;
                        iconObjectName = "Thorns";
                        break;
                    case SpellName.Pause:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "Slow";
                        break;
                    case SpellName.RoidRage:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = false;
                        iconObjectName = "Critical";
                        break;
                    case SpellName.SmokeMachine:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Dodge;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = false;
                        iconObjectName = "Dodge";
                        break;
                    case SpellName.StopWatch:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "Slow";
                        break;
                    case SpellName.TheVirus:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "Thorns";
                        break;
                    case SpellName.TrashLid:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "DoubleShield";
                        break;
                    case SpellName.TwentyTwenty:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "ComboMultiplier";
                        break;
                    case SpellName.WhiteBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "StatusProtection";
                        break;
                    case SpellName.WoodenSpoon:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = true;
                        iconObjectName = "MoveGain";
                        break;
                    case SpellName.YellowBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Dodge;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = true;
                        iconObjectName = "Dodge";
                        break;
                }
                spellSource = _spellSource;
            }
            else if (_trinketSource != TrinketName.None)
            {
                trinketSource = _trinketSource;
            }

            value = _value;
            index = _index;
        }
    }

    class BumboModifierTemporary : MonoBehaviour
    {
        public BumboModifier bumboModifier;
        public readonly string description = "TEMPORARY_STATUS";

        public Vector3 TooltipPosition()
        {
            return bumboModifier.TooltipPosition();
        }

        public static List<SpellName> TemporarySpellsources
        {
            get
            {
                List<SpellName> effects = new List<SpellName>
                {
                    SpellName.LooseChange,
                    SpellName.OrangeBelt,
                    SpellName.Pause,
                    SpellName.SmokeMachine,
                    SpellName.StopWatch,
                    SpellName.TheVirus,
                    SpellName.TwentyTwenty,
                    SpellName.WhiteBelt,
                };

                if (!WindfallPersistentDataController.LoadData().implementBalanceChanges)
                {
                    effects.Add(SpellName.BarbedWire);
                    effects.Add(SpellName.RoidRage);
                    effects.Remove(SpellName.TheVirus);
                }

                return effects;
            }
        }
    }

    class BumboModifierStacking : MonoBehaviour
    {
        public BumboModifier bumboModifier;

        public Vector3 TooltipPosition()
        {
            return bumboModifier.TooltipPosition();
        }
    }
}
