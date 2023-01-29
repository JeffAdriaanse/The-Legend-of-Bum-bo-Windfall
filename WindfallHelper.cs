using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class WindfallHelper
    {
        public static BumboApplication app;
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

        public static int ChaptersUnlocked(Progression progression)
        {
            int numberOfChapters;
            if (!progression.unlocks[0]) numberOfChapters = 1;
            else if (!progression.unlocks[1]) numberOfChapters = 2;
            else if (!progression.unlocks[2]) numberOfChapters = 3;
            else numberOfChapters = 4;
            return numberOfChapters;
        }

        //Method goes two children deep when searching for buttons
        public static void UpdateGamepadMenuButtons(GamepadMenuController gamepadMenuController, GameObject cancelButton)
        {
            if (gamepadMenuController == null)
            {
                return;
            }

            List<GameObject> newOptions = new List<GameObject>();

            //Search for children with GamepadMenuOptionSelection
            for (int childCounter = 0; childCounter < gamepadMenuController.transform.childCount; childCounter++)
            {
                Transform childTransform = gamepadMenuController.transform.GetChild(childCounter);

                if (childTransform.gameObject.activeSelf && childTransform.GetComponent<GamepadMenuOptionSelection>() != null)
                {
                    newOptions.Add(childTransform.gameObject);
                }

                //Search for sub-children with GamepadMenuOptionSelection
                for (int subChildCounter = 0; subChildCounter < childTransform.childCount; subChildCounter++)
                {
                    Transform subChildTransform = childTransform.GetChild(subChildCounter);

                    if (subChildTransform.gameObject.activeSelf && subChildTransform.GetComponent<GamepadMenuOptionSelection>() != null)
                    {
                        newOptions.Add(subChildTransform.gameObject);
                    }
                }
            }

            if (newOptions.Count > 0)
            {
                gamepadMenuController.m_Buttons = newOptions.ToArray();
            }

            //Add cancel button
            if (cancelButton != null)
            {
                gamepadMenuController.m_CancelButton = cancelButton;
            }
        }

        static Shader defaultShader;
        public static Transform ResetShader(Transform transform)
        {
            if (transform == null)
            {
                return null;
            }

            foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>())
            {
                if (meshRenderer != null && !meshRenderer.GetComponent<TextMeshPro>())
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
            }

            return transform;
        }
    }

    //Stores floats as components on GameObjects; if the GameObject is destroyed, the data will be lost
    public class ObjectDataStorage : MonoBehaviour
    {
        public Dictionary<string, float> data = new Dictionary<string, float>();

        public static void StoreData(GameObject targetObject, string key, float value)
        {
            if (targetObject == null)
            {
                return;
            }

            ObjectDataStorage objectDataStorage = targetObject.GetComponent<ObjectDataStorage>();

            if (objectDataStorage == null)
            {
                objectDataStorage = targetObject.AddComponent<ObjectDataStorage>();
            }

            objectDataStorage.data[key] = value;
        }

        public static float GetData(GameObject targetObject, string key)
        {
            if (targetObject == null)
            {
                return float.NaN;
            }

            ObjectDataStorage objectDataStorage = targetObject.GetComponent<ObjectDataStorage>();

            if (objectDataStorage != null && objectDataStorage.data != null)
            {
                if (objectDataStorage.data.TryGetValue(key, out float value))
                {
                    return value;
                }
            }

            return float.NaN;
        }
    }
}