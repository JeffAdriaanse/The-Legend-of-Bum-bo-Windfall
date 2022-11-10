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
                //Update modifier display position
                UpdateModifierDisplay(existingModifier, index);
                return;
            }

            //Create modifier display
            bumboModifier.displayObject = UnityEngine.Object.Instantiate(Windfall.assetBundle?.LoadAsset<GameObject>("Modifier Display"), app.view.GUICamera.transform.Find("HUD"));

            Transform modifierDisplayTransform = bumboModifier.displayObject.transform;
            modifierDisplayTransform.localPosition = new Vector3(-0.42f, 0.23f, 0.70f);
            modifierDisplayTransform.localEulerAngles = new Vector3(7f, 180f, 0.71f);
            modifierDisplayTransform.localScale = new Vector3(0.05f, 0.05f, 0.07f);

            foreach (MeshRenderer meshRenderer in bumboModifier.displayObject.GetComponentsInChildren<MeshRenderer>())
            {
                if (meshRenderer.gameObject.GetComponent<TextMeshPro>() == null)
                {
                    Shader defaultShader = Shader.Find("Standard");
                    if (meshRenderer?.material?.shader != null && defaultShader != null)
                    {
                        meshRenderer.material.shader = defaultShader;
                    }
                }
            }


            if (bumboModifier.displayObject != null)
            {
                //Update modifier display elements
                //Update modifier display position
                UpdateModifierDisplay(bumboModifier, index);
            }
        }

        static void UpdateModifierDisplay(BumboModifier bumboModifier, int index)
        {
            Transform modifierDisplayTransform = bumboModifier?.displayObject?.transform;

            if (modifierDisplayTransform == null)
            {
                return;
            }

            modifierDisplayTransform.localPosition = new Vector3(modifierDisplayTransform.localPosition.x, modifierDisplayTransform.localPosition.y - (0.2f * index), modifierDisplayTransform.localPosition.z);
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

        CharacterSheet.BumboModifierObject.ModifierType modifierType;
        ModifierCategory modifierCategory;
        SpellName spellSource;
        TrinketName trinketSource;
        float value;

        public GameObject displayObject;

        public BumboModifier(CharacterSheet.BumboModifierObject.ModifierType _modifierType, ModifierCategory _modifierCategory, SpellName _spellSource, TrinketName _trinketSource, float _value)
        {
            modifierType = _modifierType;
            modifierCategory = _modifierCategory;
            spellSource = _spellSource;
            trinketSource = _trinketSource;
            value = _value;
        }
    }
}
