using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class BumboModifierIndication
    {
        static BumboApplication app;

        static List<BumboModifier> bumboModifiers;

        static readonly Vector3 baseDisplayPosition = new Vector3(-0.42f, 0.23f, 0.70f);
        static readonly float displayIndexOffset = 0.2f;

        //Make UpdateModifiers method
        //Call it after the following methods:
        //TakeDamage
        //StartRoom
        //StartRound
        //Etc...

        static void GetApp(BumboApplication _app)
        {
            if (app != null)
            {
                app = _app;
            }
            else
            {
                app = GameObject.FindObjectOfType<BumboApplication>();
            }
        }

        static void AddModifier(CharacterSheet.BumboModifierObject.ModifierType _modifierType, BumboModifier.ModifierCategory _modifierCategory, SpellName _spellSource, TrinketName _trinketSource, float _value)
        {
            if (bumboModifiers == null)
            {
                bumboModifiers = new List<BumboModifier>();
            }

            BumboModifier bumboModifier = new BumboModifier(_modifierType, _modifierCategory, _spellSource, _trinketSource, _value);
            bumboModifiers.Add(bumboModifier);
        }

        static void DisplayModifier(BumboModifier bumboModifier, int index)
        {
            if (app == null)
            {
                return;
            }

            BumboModifier existingModifier = bumboModifiers.Find(modifier => modifier == bumboModifier);
            if (existingModifier != null)
            {
                //Update modifier display elements and position
                UpdateModifierDisplay(existingModifier, index);
                return;
            }

            CreateModifierDisplay(bumboModifier);

            if (bumboModifier.displayObject != null)
            {
                //Update modifier display elements and position
                UpdateModifierDisplay(bumboModifier, index);
            }
        }

        static void CreateModifierDisplay(BumboModifier bumboModifier)
        {
            //Create modifier display
            bumboModifier.displayObject = UnityEngine.Object.Instantiate(Windfall.assetBundle?.LoadAsset<GameObject>("Modifier Display"), app.view.GUICamera.transform.Find("HUD"));

            Transform modifierDisplayTransform = bumboModifier.displayObject.transform;
            modifierDisplayTransform.localPosition = baseDisplayPosition;
            modifierDisplayTransform.localEulerAngles = new Vector3(7f, 180f, 0.71f);
            modifierDisplayTransform.localScale = new Vector3(0.05f, 0.05f, 0.07f);

            bumboModifier.modifierDisplayCollectibleTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayCollectible"));
            bumboModifier.modifierDisplayBackTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayBack"));
            bumboModifier.modifierDisplayIconTransform = ResetShader(bumboModifier?.displayObject?.transform.Find("ModifierDisplayIcon"));
            bumboModifier.effectValueTransform = bumboModifier?.displayObject?.transform.Find("EffectValue");
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

            Transform modifierDisplayTransform = bumboModifier.displayObject?.transform;
            if (modifierDisplayTransform != null)
            {
                modifierDisplayTransform.localPosition = new Vector3(modifierDisplayTransform.localPosition.x, baseDisplayPosition.y - (displayIndexOffset * index), modifierDisplayTransform.localPosition.z);
            }

            bumboModifier.effectValueTransform.GetComponent<TextMeshPro>().text = bumboModifier.value.ToString();

            if (bumboModifier.spellSource == SpellName.TrashLid && bumboModifier.value == 2)
            {
                ChangeModifierDisplayIcon(bumboModifier, "DoubleShield");
                return;
            }

            ChangeModifierDisplayIcon(bumboModifier, null);
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
            else if (BumboModifier.SpellDisplayIconObjects.TryGetValue(bumboModifier.spellSource, out string spellIconObjectName))
            {
                newIconObjectName = spellIconObjectName;
            }

            if (newIconObjectName == null || bumboModifier?.modifierDisplayIconObjectTransform?.gameObject?.name == newIconObjectName)
            {
                return;
            }

            if (Windfall.assetBundle != null && Windfall.assetBundle.Contains(newIconObjectName))
            {
                GameObject iconObject = Windfall.assetBundle.LoadAsset<GameObject>(newIconObjectName);

                if (iconObject != null)
                {
                    bumboModifier.modifierDisplayIconObjectTransform = UnityEngine.Object.Instantiate(iconObject, bumboModifier.modifierDisplayIconTransform).transform;
                    bumboModifier.modifierDisplayIconObjectTransform.gameObject.name = newIconObjectName;
                }
            }
        }
    }

    class BumboModifier
    {
        public enum ModifierCategory
        {
            None,
            Block,
            Retaliate,
            Dodge,
        }

        public CharacterSheet.BumboModifierObject.ModifierType modifierType;
        public ModifierCategory modifierCategory;
        public SpellName spellSource;
        public TrinketName trinketSource;
        public float value;

        public GameObject displayObject;

        public Transform modifierDisplayCollectibleTransform;
        public Transform modifierDisplayBackTransform;
        public Transform modifierDisplayIconTransform;
        public Transform effectValueTransform;

        public Transform modifierDisplayIconObjectTransform;


        public BumboModifier(CharacterSheet.BumboModifierObject.ModifierType _modifierType, ModifierCategory _modifierCategory, SpellName _spellSource, TrinketName _trinketSource, float _value)
        {
            modifierType = _modifierType;
            modifierCategory = _modifierCategory;
            spellSource = _spellSource;
            trinketSource = _trinketSource;
            value = _value;
        }

        public static Dictionary<SpellName, string> SpellDisplayIconObjects
        {
            get
            {
                Dictionary<SpellName, string> iconObjects = new Dictionary<SpellName, string>
                {
                    { SpellName.BarbedWire, "Retaliate" },
                    { SpellName.BlindRage, "Vulnerable" },
                    { SpellName.BrownBelt, "ShieldSword" },
                    { SpellName.Euthanasia, "Retaliate" },
                    { SpellName.OldPillow, "Shield" },
                    { SpellName.OrangeBelt, "HurtRetaliate" },
                    { SpellName.Pause, "Slow" },
                    { SpellName.RoidRage, "Critical" },
                    { SpellName.SmokeMachine, "Dodge" },
                    { SpellName.StopWatch, "Slow" },
                    { SpellName.TheVirus, "HurtRetaliatePoison" },
                    { SpellName.TrashLid, "DoubleShield" },
                    { SpellName.TwentyTwenty, "ComboMultiplier" },
                    { SpellName.YellowBelt, "Dodge" },
                };
                return iconObjects;
            }
        }
    }
}
