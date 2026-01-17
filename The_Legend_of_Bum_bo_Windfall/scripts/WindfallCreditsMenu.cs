using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace The_Legend_of_Bum_bo_Windfall
{
    class WindfallCreditsMenu : MonoBehaviour
    {
        public void SetUpWindfallCreditsMenu(GameObject menuView)
        {
            gameObject.SetActive(false);
            transform.SetSiblingIndex(Mathf.Max(0, transform.parent.childCount - 2));

            TMP_FontAsset edmundmcmillen_regular = WindfallHelper.GetEdmundMcmillenFont();

            GameObject header = transform.Find("Header").gameObject;
            GameObject createdBy = transform.Find("Created by").gameObject;
            GameObject createdByName = transform.Find("Created by").Find("Name").gameObject;
            GameObject designProgrammingArt = transform.Find("Design, Programming, Art").gameObject;
            GameObject designProgrammingArtName = transform.Find("Design, Programming, Art").Find("Name").gameObject;
            GameObject chineseLocalization = transform.Find("Chinese Localization").gameObject;
            GameObject chineseLocalizationName = transform.Find("Chinese Localization").Find("Name").gameObject;
            GameObject spanishLocalization = transform.Find("Spanish Localization").gameObject;
            GameObject spanishLocalizationName = transform.Find("Spanish Localization").Find("Name").gameObject;
            GameObject cancel = transform.Find("Cancel").gameObject;

            //Localize header
            WindfallHelper.LocalizeObject(header, "Menu/WINDFALL_CREDITS");

            //Initialize buttons
            WindfallHelper.InitializeButton(cancel, CloseWindfallCreditsMenu, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);

            //Localize buttons
            WindfallHelper.LocalizeObject(cancel, "Menu/OPTIONS_CANCEL");

            //Localize other labels
            WindfallHelper.LocalizeObject(createdBy, "Menu/CREATED_BY");
            WindfallHelper.LocalizeObject(createdByName, "Menu/JEFF_ADRIAANSE");
            WindfallHelper.LocalizeObject(designProgrammingArt, "Menu/DESIGN_PROGRAMMING_ART");
            WindfallHelper.LocalizeObject(designProgrammingArtName, "Menu/JEFF_ADRIAANSE");
            WindfallHelper.LocalizeObject(chineseLocalization, "Menu/CHINESE_LOCALIZATION");
            WindfallHelper.LocalizeObject(chineseLocalizationName, "Menu/YAZAWA_AKI_OS");
            WindfallHelper.LocalizeObject(spanishLocalization, "Menu/SPANISH_LOCALIZATION");
            WindfallHelper.LocalizeObject(spanishLocalizationName, "Menu/FROST");

            //Keyboard/gamepad control functionality
            GamepadMenuController gamepadMenuController = gameObject.AddComponent<GamepadMenuController>();
            WindfallHelper.UpdateGamepadMenuButtons(gamepadMenuController, transform.Find("Cancel")?.gameObject, 2);
        }

        public void OpenWindfallCreditsMenu()
        {
            Transform menuViewTransform = transform.parent;

            menuViewTransform.Find("Windfall Menu(Clone)")?.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
        public void CloseWindfallCreditsMenu()
        {
            Transform menuViewTransform = transform.parent;

            gameObject.SetActive(false);
            menuViewTransform.Find("Windfall Menu(Clone)")?.gameObject.SetActive(true);
        }
    }
}
