using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace The_Legend_of_Bum_bo_Windfall.scripts
{
    static class HotkeysMenu
    {
        private static GameObject menuViewReference;
        private static GameObject hotkeysMenu;

        private static Dictionary<string, KeyCode> hotkeys;

        public static void CreateHotkeysMenu(GameObject menuView)
        {

        }

        private static void UpdateKeys()
        {

        }
        private static void UpdateKey(GameObject keyObject, KeyCode keyCode)
        {
            if (keyObject != null)
            {
                TextMeshProUGUI keyText = keyObject.GetComponent<TextMeshProUGUI>();
                if (keyText != null) keyText.text = keyCode.ToString();
            }
        }

        public static void OpenHotkeysMenu()
        {
            if (menuViewReference == null)
            {
                return;
            }

            menuViewReference.transform.Find("Windfall Menu")?.gameObject.SetActive(false);
            hotkeysMenu.SetActive(true);

            LoadHotkeys();
        }
        public static void CloseHotkeysMenu()
        {
            if (menuViewReference == null)
            {
                return;
            }

            hotkeysMenu.SetActive(false);
            menuViewReference.transform.Find("Windfall Menu")?.gameObject.SetActive(true);
        }

        static void SaveHotkeys()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            windfallPersistentData.hotkeys = hotkeys;
            WindfallPersistentDataController.SaveData(windfallPersistentData);

            CloseHotkeysMenu();
        }

        private static void LoadHotkeys()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            hotkeys = windfallPersistentData.hotkeys;

            UpdateKeys();
        }
    }
}
