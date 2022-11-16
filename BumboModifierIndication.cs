using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class BumboModifierIndication
    {
        static BumboApplication app;

        static List<BumboModifier> bumboModifiers;

        static readonly Vector3 baseDisplayPosition = new Vector3(-0.42f, 0.25f, 0.70f);
        static readonly float displayIndexOffset = 0.12f;

        static readonly Color outlineColor = Color.white;
        static readonly float outlineWidth = 0.3f;

        //Make UpdateModifiers method
        //Call it after the following methods:
        //TakeDamage
        //StartRoom
        //StartRound
        //Etc...

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

            List<SpellName> trackedSpells = new List<SpellName>
            {
                //Round modifiers
                SpellName.Pause,
                SpellName.RoidRage,
                SpellName.StopWatch,
                SpellName.TwentyTwenty,

                //Room modifiers
                SpellName.BlindRage,
                SpellName.TheVirus,

                //Modifier objects
                SpellName.BarbedWire,
                SpellName.BrownBelt,
            };

            foreach (SpellName trackedSpell in trackedSpells)
            {
                if (ConvertModifier(null, trackedSpell, TrinketName.None, SpellConversionValue(trackedSpell, bumboRoundModifiers, bumboRoomModifiers, bumboModifierObjects), modifierDispayCounter))
                {
                    modifierDispayCounter++;
                }
            }
        }

        static string SpellConversionValue(SpellName _spellSource, CharacterSheet.BumboRoundModifiers bumboRoundModifiers, CharacterSheet.BumboRoomModifiers bumboRoomModifiers, List<CharacterSheet.BumboModifierObject> bumboModifierObjects)
        {
            switch (_spellSource)
            {
                //Round modifiers
                case SpellName.Pause:
                    return bumboRoundModifiers.skipEnemyTurns > 0 ? "0" : null;
                case SpellName.RoidRage:
                    return bumboRoundModifiers.crit ? "100%" : null;
                case SpellName.StopWatch:
                    return bumboRoundModifiers.slow ? "1" : null;
                case SpellName.TwentyTwenty:
                    return bumboRoundModifiers.repeatComboCount > 0 ? "x2" : null;

                //Room modifiers
                case SpellName.BlindRage:
                    return bumboRoomModifiers.damageMultiplier > 1 ? "x" + bumboRoomModifiers.damageMultiplier.ToString() : null;
                case SpellName.TheVirus:
                    return bumboRoomModifiers.poisonOnHit ? "1" : null;

                //Modifier objects
                case SpellName.BarbedWire:
                    CharacterSheet.BumboModifierObject barbedWireModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == SpellName.BarbedWire);
                    return (barbedWireModifierObject != null && barbedWireModifierObject.damageOnHit > 0) ? barbedWireModifierObject.damageOnHit.ToString() : null;
                case SpellName.BrownBelt:
                    CharacterSheet.BumboModifierObject brownBeltModifierObject = bumboModifierObjects.Find(modifierObject => modifierObject.spellName == SpellName.BrownBelt);
                    return (brownBeltModifierObject != null && brownBeltModifierObject.blockAndCounter) ? app.model.characterSheet.getItemDamage().ToString() : null;

                default:
                    return null;
            }
        }

        static bool ConvertModifier(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value, int _index)
        {
            BumboModifier bumboModifier = UpdateModifier(_source, _spellSource, _trinketSource, _value);
            if (bumboModifier != null)
            {
                DisplayModifier(bumboModifier, _index);
                return true;
            }
            return false;
        }

        static BumboModifier UpdateModifier(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value)
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
                    existingModifier.HideModifier();
                    return null;
                }

                existingModifier.value = _value;
                
                return existingModifier;
            }

            if (_value == null)
            {
                return null;
            }

            BumboModifier bumboModifier = new BumboModifier(_source, _spellSource, _trinketSource, _value);

            bumboModifiers.Add(bumboModifier);

            return bumboModifier;
        }

        static void DisplayModifier(BumboModifier bumboModifier, int index)
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
            UpdateModifierDisplay(bumboModifier, index);

            Transform modifierDisplayTransform = bumboModifier.displayObject?.transform;
            if (modifierDisplayTransform != null)
            {
                Vector3 targetPostition = new Vector3(modifierDisplayTransform.localPosition.x, baseDisplayPosition.y - (displayIndexOffset * index), modifierDisplayTransform.localPosition.z);
                if (newModifier)
                {
                    modifierDisplayTransform.localPosition = targetPostition;
                    bumboModifier.ShowModifier();
                }
                else
                {
                    bumboModifier.MoveModifier(targetPostition);
                }
            }
        }

        static void CreateModifierDisplay(BumboModifier bumboModifier)
        {
            //Create modifier display
            bumboModifier.displayObject = UnityEngine.Object.Instantiate(Windfall.assetBundle?.LoadAsset<GameObject>("Modifier Display"), app.view.GUICamera.transform.Find("HUD"));

            Transform modifierDisplayTransform = bumboModifier.displayObject.transform;
            modifierDisplayTransform.localPosition = baseDisplayPosition;
            modifierDisplayTransform.localEulerAngles = new Vector3(0f, 180f, 0f);
            modifierDisplayTransform.localScale = new Vector3(0.05f, 0.05f, 0.07f);

            bumboModifier.modifierDisplayCollectibleTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayCollectible"));
            bumboModifier.modifierDisplayBackTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayBack"));
            bumboModifier.modifierDisplayIconTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayIcon"));
            bumboModifier.modifierDisplayValueTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("Value"));
            bumboModifier.effectValueTransform = bumboModifier?.displayObject?.transform.Find("EffectValue");

            //Set effect value
            TextMeshPro effectValueTextMeshPro = bumboModifier.effectValueTransform.GetComponent<TextMeshPro>();
            if (effectValueTextMeshPro != null)
            {
                LocalizationModifier.ChangeFont(null, effectValueTextMeshPro, LocalizationModifier.edFont);
                effectValueTextMeshPro.outlineColor = outlineColor;
                effectValueTextMeshPro.outlineWidth = outlineWidth;
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
            }

            return transform;
        }

        static void UpdateModifierDisplay(BumboModifier bumboModifier, int index)
        {
            if (bumboModifier == null)
            {
                return;
            }

            TextMeshPro effectValueTextMeshPro = bumboModifier.effectValueTransform.GetComponent<TextMeshPro>();
            if (effectValueTextMeshPro != null)
            {
                effectValueTextMeshPro.text = bumboModifier.value;
                effectValueTextMeshPro.gameObject.SetActive(bumboModifier.modifierCategory != BumboModifier.ModifierCategory.Block);
            }

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

    class BumboModifier
    {
        static readonly float horizontalOffset = 0.4f;
        static readonly float tweenDuration = 0.3f;

        public enum ModifierCategory
        {
            None,
            Block,
            Retaliate,
            Dodge,
        }

        public CharacterSheet.BumboModifierObject.ModifierType modifierType;
        public ModifierCategory modifierCategory;
        public string source;
        public SpellName spellSource;
        public TrinketName trinketSource;
        public string value;
        public string iconObjectName;

        public GameObject displayObject;

        public Transform modifierDisplayCollectibleTransform;
        public Transform modifierDisplayBackTransform;
        public Transform modifierDisplayIconTransform;
        public Transform modifierDisplayValueTransform;
        public Transform effectValueTransform;

        public Transform modifierDisplayIconObjectTransform;

        public Sequence movementSequence;

        public void ShowModifier()
        {
            if (displayObject == null)
            {
                return;
            }

            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(true);
            }
            Vector3 oldPosition = displayObject.transform.localPosition;
            displayObject.transform.localPosition -= new Vector3(horizontalOffset, 0f, 0f);

            movementSequence = DOTween.Sequence();
            movementSequence.Append(displayObject.transform.DOLocalMove(oldPosition, tweenDuration).SetEase(Ease.InOutQuad));
        }

        public void HideModifier()
        {
            if (displayObject == null)
            {
                return;
            }
            
            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(true);
            }

            Vector3 position = displayObject.transform.localPosition + new Vector3(-horizontalOffset, 0f, 0f);

            movementSequence.Append(displayObject.transform.DOLocalMove(position, tweenDuration).SetEase(Ease.InOutQuad));
            movementSequence.AppendCallback(delegate
            {
                UnityEngine.Object.Destroy(displayObject);
            });
        }

        public void MoveModifier(Vector3 newLocalPosition)
        {
            if (displayObject == null)
            {
                return;
            }

            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(true);
            }
            movementSequence = DOTween.Sequence();

            Vector3 position = newLocalPosition;
            if (position == Vector3.zero)
            {
                position = displayObject.transform.localPosition;
            }

            movementSequence.Append(displayObject.transform.DOLocalMove(position, tweenDuration).SetEase(Ease.InOutQuad));
        }

        public BumboModifier(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value)
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
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        iconObjectName = "Retaliate";
                        break;
                    case SpellName.BlindRage:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.None;
                        iconObjectName = "Vulnerable";
                        break;
                    case SpellName.BrownBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        iconObjectName = "ShieldSword";
                        break;
                    case SpellName.Euthanasia:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Retaliate;
                        iconObjectName = "Retaliate";
                        break;
                    case SpellName.OldPillow:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        iconObjectName = "Shield";
                        break;
                    case SpellName.OrangeBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        iconObjectName = "HurtRetaliate";
                        break;
                    case SpellName.Pause:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        iconObjectName = "Slow";
                        break;
                    case SpellName.RoidRage:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        iconObjectName = "Critical";
                        break;
                    case SpellName.SmokeMachine:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Dodge;
                        iconObjectName = "Dodge";
                        break;
                    case SpellName.StopWatch:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        iconObjectName = "Slow";
                        break;
                    case SpellName.TheVirus:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        iconObjectName = "HurtRetaliatePoison";
                        break;
                    case SpellName.TrashLid:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        iconObjectName = "DoubleShield";
                        break;
                    case SpellName.TwentyTwenty:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        iconObjectName = "ComboMultiplier";
                        break;
                    case SpellName.YellowBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Dodge;
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
        }
    }
}
