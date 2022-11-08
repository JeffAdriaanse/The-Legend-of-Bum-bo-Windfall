using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class WindfallHelper
    {
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
    }
}