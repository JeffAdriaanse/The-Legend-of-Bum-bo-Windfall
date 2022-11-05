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

        public static void UpdateGamepadMenuButtons(GamepadMenuController gamepadMenuController)
        {
            if (gamepadMenuController == null)
            {
                return;
            }

            List<GameObject> newOptions = new List<GameObject>();

            for (int childCounter = 0; childCounter < gamepadMenuController.transform.childCount; childCounter++)
            {
                Transform childTransform = gamepadMenuController.transform.GetChild(childCounter);

                if (childTransform.gameObject.activeSelf && childTransform.GetComponent<GamepadMenuOptionSelection>() != null)
                {
                    newOptions.Add(childTransform.gameObject);
                }
            }

            if (newOptions.Count > 0)
            {
                gamepadMenuController.m_Buttons = newOptions.ToArray();
            }
        }
    }
}