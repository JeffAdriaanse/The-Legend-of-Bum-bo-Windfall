using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class BumboModifierIndication
    {
        static BumboApplication app;

        static List<BumboModifier> bumboModifiers;

        public static void GetApp(BumboApplication _app)
        {
            if (app != null)
            {
                return;
            }

            if (_app != null)
            {
                app = _app;
            }
            else
            {
                app = GameObject.FindObjectOfType<BumboApplication>();
            }
        }

        static GameObject expansionToggle;
        static Vector3 togglePosition = new Vector3(-0.73f, 0.42f, 1.15f);

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

            string expansionTogglePath = "Shield";
            if (Windfall.assetBundle.Contains(expansionTogglePath))
            {
                expansionToggle = ResetShader(UnityEngine.Object.Instantiate((GameObject)Windfall.assetBundle.LoadAsset(expansionTogglePath), app.view.GUICamera.transform.Find("HUD")).transform).gameObject;
                expansionToggle.layer = 5;
                expansionToggle.SetActive(false);
                expansionToggle.AddComponent<ExpansionToggle>();

                BoxCollider boxCollider = expansionToggle.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(0.02f, 0.02f, 0.02f);
                boxCollider.isTrigger = true;

                expansionToggle.transform.localPosition = expansionToggle.GetComponent<ExpansionToggle>().HidingPosition();
                expansionToggle.transform.localScale = new Vector3(5f, 5f, 5f);

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
            if (app?.model?.characterSheet == null)
            {
                return;
            }

            int modifierDispayCounter = 0;

            //Get modifiers
            CharacterSheet.BumboRoundModifiers bumboRoundModifiers = app.model.characterSheet.bumboRoundModifiers;
            CharacterSheet.BumboRoomModifiers bumboRoomModifiers = app.model.characterSheet.bumboRoomModifiers;
            List<CharacterSheet.BumboModifierObject> bumboModifierObjects = app.model.characterSheet.bumboModifierObjects;

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

                SpellName.TheVirus, //Room modifier

                SpellName.BlindRage, //Room modifier

                SpellName.SmokeMachine, //Modifier object
                SpellName.YellowBelt, //Modifier object

                SpellName.Pause, //Round modifier
                SpellName.StopWatch, //Round modifier

                SpellName.RoidRage, //Round modifier

                SpellName.TwentyTwenty, //Round modifier

                SpellName.WhiteBelt, //Modifier object
            };

            for (int spellCounter = 0; spellCounter < trackedSpells.Count; spellCounter++)
            {
                if (ConvertModifier(null, trackedSpells[spellCounter], TrinketName.None, SpellConversionValue(trackedSpells[spellCounter], bumboRoundModifiers, bumboRoomModifiers, bumboModifierObjects), modifierDispayCounter))
                {
                    modifierDispayCounter++;
                }
            }

            UpdateExpansionToggle();
        }

        static string SpellConversionValue(SpellName _spellSource, CharacterSheet.BumboRoundModifiers bumboRoundModifiers, CharacterSheet.BumboRoomModifiers bumboRoomModifiers, List<CharacterSheet.BumboModifierObject> bumboModifierObjects)
        {
            switch (_spellSource)
            {
                //Round modifiers
                case SpellName.Pause:
                    return bumboRoundModifiers.skipEnemyTurns > 0 ? bumboRoundModifiers.skipEnemyTurns.ToString() : null;
                case SpellName.RoidRage:
                    return bumboRoundModifiers.crit ? "100%" : null;
                case SpellName.StopWatch:
                    return bumboRoundModifiers.slow ? "1" : null;
                case SpellName.TwentyTwenty:
                    return bumboRoundModifiers.repeatComboCount > 0 ? bumboRoundModifiers.repeatComboCount.ToString() : null;

                //Room modifiers
                case SpellName.BlindRage:
                    return bumboRoomModifiers.damageMultiplier > 1 ? "x" + bumboRoomModifiers.damageMultiplier.ToString() : null;
                case SpellName.TheVirus:
                    return bumboRoundModifiers.poisonRounds > 0 ? "1" : null;

                //Modifier objects
                case SpellName.BarbedWire:
                    CharacterSheet.BumboModifierObject barbedWireModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (barbedWireModifierObject != null && barbedWireModifierObject.damageOnHit > 0) ? barbedWireModifierObject.damageOnHit.ToString() : null;
                case SpellName.BrownBelt:
                    CharacterSheet.BumboModifierObject brownBeltModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == _spellSource);
                    return (brownBeltModifierObject != null) ? app.model.characterSheet.getItemDamage().ToString() : null;
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

            BumboModifier bumboModifier = new BumboModifier(_source, _spellSource, _trinketSource, _value, _index);

            bumboModifiers.Add(bumboModifier);

            return bumboModifier;
        }

        static void DisplayModifier(BumboModifier bumboModifier)
        {
            if (app == null)
            {
                return;
            }

            bool newModifier = false;
            if (bumboModifier.displayObject == null)
            {
                CreateModifierDisplay(bumboModifier);
                newModifier = true;
            }

            //Update modifier display elements and position
            UpdateModifierDisplay(bumboModifier);

            Transform modifierDisplayTransform = bumboModifier.displayObject?.transform;
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

        static void CreateModifierDisplay(BumboModifier bumboModifier)
        {
            //Create modifier display
            bumboModifier.displayObject = UnityEngine.Object.Instantiate(Windfall.assetBundle?.LoadAsset<GameObject>("Modifier Display"), app.view.GUICamera.transform.Find("HUD"));

            bumboModifier.objectTinter = bumboModifier.displayObject.AddComponent<ObjectTinter>();

            bumboModifier.boxCollider = bumboModifier.displayObject.GetComponent<BoxCollider>();

            Transform modifierDisplayTransform = bumboModifier.displayObject.transform;
            modifierDisplayTransform.localPosition = BumboModifier.baseDisplayPosition;
            modifierDisplayTransform.localEulerAngles = new Vector3(0f, 180f, 0f);
            modifierDisplayTransform.localScale = new Vector3(0.09f, 0.09f, 0.11f);

            bumboModifier.modifierDisplayCollectibleTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayCollectible"));
            bumboModifier.modifierDisplayBackTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayBack"));
            bumboModifier.modifierDisplayIconTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayIcon"));
            bumboModifier.modifierDisplayValueTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("Value"));
            bumboModifier.effectValueTransform = bumboModifier?.displayObject?.transform.Find("EffectValue");
            bumboModifier.timerTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("Temporary"));

            //Set effect value
            TextMeshPro effectValueTextMeshPro = bumboModifier.effectValueTransform.GetComponent<TextMeshPro>();
            if (effectValueTextMeshPro != null)
            {
                LocalizationModifier.ChangeFont(null, effectValueTextMeshPro, LocalizationModifier.edFont);
            }

            //Set spell texture
            if (bumboModifier.spellSource != SpellName.None)
            {
                SpellElement spellElement = app.model.spellModel.spells[bumboModifier.spellSource];

                List<Material> newMaterials = new List<Material>();

                Material iconMaterial = new Material(app.model.spellModel.Icon(spellElement.Category, true, spellElement.texturePage));
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
                Debug.Log("Materials Set: " + newMaterials.Count);
            }
        }

        static Shader defaultShader;
        static Transform ResetShader(Transform transform)
        {
            MeshRenderer meshRenderer = transform?.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                if (defaultShader == null)
                {
                    defaultShader = Shader.Find("Standard");
                }

                if (meshRenderer?.material?.shader != null && defaultShader != null)
                {
                    meshRenderer.material.shader = defaultShader;
                }

                meshRenderer.material.shaderKeywords = new string[] { "_GLOSSYREFLECTIONS_OFF", "_SPECULARHIGHLIGHTS_OFF" };
            }

            return transform;
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
                float offsetY = bumboModifier.valueDisplayType == BumboModifier.ValueDisplayType.Hurt ? 0.145f : 0f;
                valueMeshRenderer.material.mainTextureOffset = new Vector2(0f, offsetY);
            }

            bumboModifier.effectValueTransform.gameObject.SetActive(bumboModifier.valueDisplayType != BumboModifier.ValueDisplayType.None);

            bumboModifier.timerTransform.gameObject.SetActive(bumboModifier.modifierType == CharacterSheet.BumboModifierObject.ModifierType.Round);

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

                    ResetShader(bumboModifier.modifierDisplayIconObjectTransform);

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

        public readonly Vector3 showingRotation = new Vector3(0f, 180f, 90f);
        public readonly Vector3 hidingRotation = new Vector3(0f, 180f, 270f);

        public readonly Vector3 showingPosition = new Vector3(-0.73f, 0.42f, 1.15f);

        ExpansionToggle()
        {
            transform.localEulerAngles = TargetRotation();
        }

        void OnMouseDown()
        {
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
                displaySequence.Kill(true);
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

    class BumboModifier
    {
        static Color hiddenTintColor = new Color(0.2f, 0.2f, 0.2f);

        static readonly float removalHorizontalOffset = 0.6f;
        static readonly float hidingHorizontalOffset = 0.3f;
        static readonly float punchScale = 0.012f;
        static readonly float tweenDuration = 0.3f;

        public static readonly Vector3 baseDisplayPosition = new Vector3(-0.64f, 0.3f, 1.15f);
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

        public string iconObjectName;

        public int index;

        public GameObject displayObject;

        public ObjectTinter objectTinter;

        public BoxCollider boxCollider;

        public Transform modifierDisplayCollectibleTransform;
        public Transform modifierDisplayBackTransform;
        public Transform modifierDisplayIconTransform;
        public Transform modifierDisplayValueTransform;
        public Transform effectValueTransform;
        public Transform timerTransform;

        public Transform modifierDisplayIconObjectTransform;

        public Sequence movementSequence;
        public Sequence updateSequence;

        bool Expanded()
        {
            return WindfallPersistentDataController.LoadData().expandModifiers;
        }
        public Vector3 TargetPosition()
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
            if (displayObject == null)
            {
                return;
            }

            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(true);
            }

            displayObject.transform.localPosition = RemovalPosition();

            movementSequence = DOTween.Sequence();
            movementSequence.Append(displayObject.transform.DOLocalMove(TargetPosition(), tweenDuration).SetEase(Ease.InOutQuad));

            UpdateTint();
        }

        public void RemoveModifierAnimation()
        {
            if (displayObject == null)
            {
                return;
            }

            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(true);
            }

            movementSequence.Append(displayObject.transform.DOLocalMove(RemovalPosition(), tweenDuration).SetEase(Ease.InOutQuad));
            movementSequence.AppendCallback(delegate
            {
                UnityEngine.Object.Destroy(displayObject);
            });
        }

        public void MoveModifierAnimation()
        {
            if (displayObject == null)
            {
                return;
            }

            Vector3 targetPosition = TargetPosition();
            if (targetPosition == Vector3.zero || targetPosition == displayObject.transform.localPosition)
            {
                return;
            }

            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(true);
            }
            movementSequence = DOTween.Sequence();

            movementSequence.Append(displayObject.transform.DOLocalMove(targetPosition, tweenDuration).SetEase(Ease.InOutQuad));
            movementSequence.AppendCallback(delegate{ UpdateTint(); });
        }

        public void UpdateModifierAnimation()
        {
            if (displayObject == null)
            {
                return;
            }

            if (updateSequence != null && updateSequence.IsPlaying())
            {
                updateSequence.Kill(true);
            }

            updateSequence = DOTween.Sequence();
            updateSequence.Append(displayObject.transform.DOPunchScale(new Vector3(punchScale, punchScale, punchScale), tweenDuration, 1, 0));
        }

        public BumboModifier(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value, int _index)
        {
            if (_source != null && _source != string.Empty)
            {
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
                        iconObjectName = "Retaliate";
                        break;
                    case SpellName.BlindRage:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        iconObjectName = "Vulnerable";
                        break;
                    case SpellName.BrownBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.Hurt;
                        iconObjectName = "ShieldSword";
                        break;
                    case SpellName.Euthanasia:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.Hurt;
                        iconObjectName = "Retaliate";
                        break;
                    case SpellName.OldPillow:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.None;
                        iconObjectName = "Shield";
                        break;
                    case SpellName.OrangeBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.Hurt;
                        iconObjectName = "HurtRetaliate";
                        break;
                    case SpellName.Pause:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        iconObjectName = "Slow";
                        break;
                    case SpellName.RoidRage:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        iconObjectName = "Critical";
                        break;
                    case SpellName.SmokeMachine:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Dodge;
                        valueDisplayType = ValueDisplayType.Standard;
                        iconObjectName = "Dodge";
                        break;
                    case SpellName.StopWatch:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        iconObjectName = "Slow";
                        break;
                    case SpellName.TheVirus:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.None;
                        iconObjectName = "HurtRetaliatePoison";
                        break;
                    case SpellName.TrashLid:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.None;
                        iconObjectName = "DoubleShield";
                        break;
                    case SpellName.TwentyTwenty:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        iconObjectName = "ComboMultiplier";
                        break;
                    case SpellName.YellowBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Dodge;
                        valueDisplayType = ValueDisplayType.Standard;
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
}
