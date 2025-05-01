using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityStandardAssets.ImageEffects;
using Rewired;
using HarmonyLib;
using System.Reflection.Emit;
using System.Collections;

namespace The_Legend_of_Bum_bo_Windfall.scripts
{
    static class HotkeysMenu
    {
        private static GameObject menuViewReference;
        private static GameObject hotkeysMenu;

        private static Dictionary<string, KeyCode> hotkeys;
        private static List<TextMeshProUGUI> keysText;

        public static void CreateHotkeysMenu(GameObject menuView)
        {
            InputChanges.EditRewiredKeyBindings();

            //Create hotkeys menu
            hotkeysMenu = UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>("Windfall Menu"), menuView.transform);
            hotkeysMenu.SetActive(false);
            RectTransform hotkeysMenuRect;

            if (hotkeysMenu != null)
            {
                hotkeysMenuRect = hotkeysMenu.GetComponent<RectTransform>();

                foreach (TextMeshProUGUI child in hotkeysMenu.transform.GetComponentsInChildren<TextMeshProUGUI>()) {
                    if (child.text == "Key") {
                        keysText.Add(child);
                    }
                }

            }
        }

        private static void UpdateKeys()
        {
            foreach (var keyValuePair in hotkeys) {
                
            }
        }
        private static void UpdateKey(GameObject keyObject, KeyCode keyCode)
        {
            if (keyObject != null)
            {
                Localize keyLocalize = keyObject.GetComponent<Localize>();
                if (keyLocalize != null) Localization.SetKey(keyLocalize, eI2Category.Menu, keyCode.ToString());
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

    class InputChanges()
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(InputChanges));
        }

        private static readonly Dictionary<string, KeyCode> defaultHotkeys = new Dictionary<string, KeyCode>()
        {
            { "SPELL_1", KeyCode.Alpha1 },
            { "SPELL_2", KeyCode.Alpha2 },
            { "SPELL_3", KeyCode.Alpha3 },
            { "SPELL_4", KeyCode.Alpha4 },
            { "SPELL_5", KeyCode.Alpha5 },
            { "SPELL_6", KeyCode.Alpha6 },
            { "TRINKET_1", KeyCode.Alpha1 },
            { "TRINKET_2", KeyCode.Alpha2 },
            { "TRINKET_3", KeyCode.Alpha3 },
            { "TRINKET_4", KeyCode.Alpha4 },
        };

        public static void EditRewiredKeyBindings()
        {
            InputDevice masterDevice = (InputDevice)typeof(InputManager).GetField("m_MasterDevice", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(InputManager.Instance);

            if (masterDevice is not InputDeviceRewired) return;
            InputDeviceRewired masterDeviceRewired = (InputDeviceRewired)masterDevice;

            Player player = (Player)typeof(InputDeviceRewired).GetField("m_Player", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(masterDeviceRewired);
            Controller keyboard = ReInput.controllers.Keyboard;
            ControllerMap keyBoardMap = player.controllers.maps.GetMap(ControllerType.Keyboard, keyboard.id, 0, 0);

            PropertyInfo keyboardMapSetProperty = typeof(Player.ControllerHelper).GetProperty("keyboardMapSet", BindingFlags.Instance | BindingFlags.NonPublic);

            //Add new buttons
            IEnumerable<ControllerMap> keyBoardMapSet = (IEnumerable<ControllerMap>)keyboardMapSetProperty.GetValue(player.controllers);

            List<ControllerMap> keyBoardMapSetList = keyBoardMapSet.ToList();
            keyBoardMap.CreateElementMap(100, Pole.Positive, KeyCode.Alpha1, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(101, Pole.Positive, KeyCode.Alpha2, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(102, Pole.Positive, KeyCode.Alpha3, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(103, Pole.Positive, KeyCode.Alpha4, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(104, Pole.Positive, KeyCode.Alpha5, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(105, Pole.Positive, KeyCode.Alpha6, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(106, Pole.Positive, KeyCode.Alpha7, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(107, Pole.Positive, KeyCode.Alpha8, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(108, Pole.Positive, KeyCode.Alpha9, ModifierKeyFlags.None);
            keyBoardMap.CreateElementMap(109, Pole.Positive, KeyCode.Alpha0, ModifierKeyFlags.None);

            //It seems that adding new actions to Rewired Input Manager is not possible at runtime
            //Consequently, this attempt to integrate new key bindings into the Rewired system will have to be abandoned
            var action = ReInput.mapping.GetAction(100);
            if (action == null) Debug.LogWarning("Action ID 100 not defined in Rewired Input Manager.");

            foreach (var inputAction in ReInput.mapping.Actions)
            {
                Console.WriteLine($"Action: {inputAction.name} (ID: {inputAction.id})");
            }
        }

        //Add spell/trinket hotkey functionality
        [HarmonyPostfix, HarmonyPatch(typeof(GamepadSpellSelector), "Update")]
        static void GamepadSpellSelector_Update(GamepadSpellSelector __instance)
        {
            if (!InputManager.Instance.IsUsingGamepadInput()) return;
            if (!__instance.IsActive || __instance.app.controller.debugController.IsDebugMenuOpen()) return;

            //Selecting spells
            int selectedSpellIndex = -1;
            if (InputManager.Instance.GetButtonDown((eInput)14)) selectedSpellIndex = 0;
            else if (InputManager.Instance.GetButtonDown((eInput)15)) selectedSpellIndex = 1;
            else if (InputManager.Instance.GetButtonDown((eInput)16)) selectedSpellIndex = 2;
            else if (InputManager.Instance.GetButtonDown((eInput)17)) selectedSpellIndex = 3;
            else if (InputManager.Instance.GetButtonDown((eInput)18)) selectedSpellIndex = 4;
            else if (InputManager.Instance.GetButtonDown((eInput)19)) selectedSpellIndex = 5;

            //Selecting trinkets
            int selectedTrinketIndex = -1;
            if (selectedSpellIndex < 0)
            {
                if (InputManager.Instance.GetButtonDown((eInput)20)) selectedTrinketIndex = 0;
                else if (InputManager.Instance.GetButtonDown((eInput)21)) selectedTrinketIndex = 1;
                else if (InputManager.Instance.GetButtonDown((eInput)22)) selectedTrinketIndex = 2;
                else if (InputManager.Instance.GetButtonDown((eInput)23)) selectedTrinketIndex = 3;
            }

            if (selectedSpellIndex < 0 && selectedTrinketIndex < 0) return;

            //Get GamePadSpellSelector Selectable class type and fields/properties
            Type selectableType = typeof(GamepadSpellSelector).GetNestedType("Selectable", BindingFlags.NonPublic);
            FieldInfo m_Spell_Field = selectableType.GetField("m_Spell", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            FieldInfo m_Trinket_Field = selectableType.GetField("m_Trinket", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            FieldInfo m_Index_Field = selectableType.GetField("m_Index", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo IsSelectable_Property = selectableType.GetProperty("IsSelectable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            //Get GamePadSpellSelector m_Selectables list
            FieldInfo m_Selectables_FieldInfo = AccessTools.Field(typeof(GamepadSpellSelector), "m_Selectables");
            object m_Selectables_Raw = m_Selectables_FieldInfo.GetValue(__instance);
            IEnumerable m_Selectables_List = (IEnumerable)m_Selectables_Raw;
            //Get GamePadSpellSelector m_Selection
            FieldInfo m_Selection_FieldInfo = AccessTools.Field(typeof(GamepadSpellSelector), "m_Selection");

            //Functionality
            List<SpellView> spellViews = __instance.app.view.spells;
            GameObject[] trinkets = __instance.app.view.GUICamera.GetComponent<GUISide>().trinkets;
            if (selectedSpellIndex >= 0 && selectedSpellIndex < spellViews.Count) //Spells
            {
                SpellView spellView = spellViews[selectedSpellIndex];
                foreach (var selectable in m_Selectables_List)
                {
                    bool IsSelectable = (bool)IsSelectable_Property?.GetValue(selectable);
                    var m_Spell = m_Spell_Field?.GetValue(selectable);
                    //Find selectable with the same SpellView as the selected SpellView
                    if (IsSelectable && m_Spell != null && m_Spell == spellView)
                    {
                        //Change selection
                        int m_Index = (int)m_Index_Field?.GetValue(selectable);
                        m_Selection_FieldInfo.SetValue(__instance, m_Index);
                        AccessTools.Method(typeof(GamepadSpellSelector), "apply_selection", new Type[] { typeof(int) }).Invoke(__instance, new object[] { -2147483647 });
                        //Trigger the spell
                        InputManager.Instance.ConsumeInput(eInput.Confirm);
                        __instance.Close(false);
                        break;
                    }
                }
            }
            else if (selectedTrinketIndex >= 0 && selectedTrinketIndex < trinkets.Length) //Trinkets
            {
                TrinketView trinketView = trinkets[selectedTrinketIndex].GetComponent<TrinketView>();
                foreach (var selectable in m_Selectables_List)
                {
                    bool IsSelectable = (bool)IsSelectable_Property?.GetValue(selectable);
                    var m_Trinket = m_Trinket_Field?.GetValue(selectable);
                    //Find selectable with the same TrinketView as the selected TrinketView
                    if (IsSelectable && m_Trinket != null && m_Trinket == trinketView)
                    {
                        //Change selection
                        int m_Index = (int)m_Index_Field?.GetValue(selectable);
                        m_Selection_FieldInfo.SetValue(__instance, m_Index);
                        AccessTools.Method(typeof(GamepadSpellSelector), "apply_selection", new Type[] { typeof(int) }).Invoke(__instance, new object[] { -2147483647 });
                        //Trigger the spell
                        InputManager.Instance.ConsumeInput(eInput.Confirm);
                        __instance.Close(false);
                        break;
                    }
                }
            }
        }

        //Add lane select hotkey functionality
        [HarmonyPostfix, HarmonyPatch(typeof(BowlingArrowView), "Update")]
        static void BowlingArrowView_Update(BowlingArrowView __instance)
        {
            if (__instance.app.model.paused) return;
            BumboEvent bumboEvent = __instance.app.model.bumboEvent;
            if (bumboEvent is not SelectColumnEvent && bumboEvent is not SelectSpellColumn) return;
            if (!InputManager.Instance.IsUsingGamepadInput()) return;

            int selectedColumn = -1;
            if (InputManager.Instance.GetButtonDown((eInput)14)) selectedColumn = 1;
            else if (InputManager.Instance.GetButtonDown((eInput)15)) selectedColumn = 2;
            else if (InputManager.Instance.GetButtonDown((eInput)16)) selectedColumn = 3;

            ClickableColumnView[] clickableColumnViews = __instance.app.view.clickableColumnViews;
            if (selectedColumn >= 0 && selectedColumn < clickableColumnViews.Length && clickableColumnViews[selectedColumn].isActiveAndEnabled) __instance.app.view.clickableColumnViews[selectedColumn].ForceClick();
        }

        //****************************************************************
        //***Patches to add room for additional buttons in InputDevices***
        //****************************************************************

        //InputDevice Transpiler
        [HarmonyPatch(typeof(InputDevice), "poll_for_input")]
        static IEnumerable<CodeInstruction> InputDevice_poll_for_input_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count - 1; i++)
            {
                //Checking for the specific IL code
                if (code[i].opcode == OpCodes.Ldc_I4_S && (sbyte)code[i].operand == 14)
                {
                    //Change operand
                    code[i].operand = 28;
                }
            }
            return code;
        }

        //InputDeviceRewired Transpiler
        [HarmonyPatch(typeof(InputDeviceRewired), "poll_for_input")]
        static IEnumerable<CodeInstruction> InputDeviceRewired_poll_for_input_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count - 1; i++)
            {
                //Checking for the specific IL code
                if (code[i].opcode == OpCodes.Ldc_I4_S && (sbyte)code[i].operand == 14)
                {
                    //Change operand
                    code[i].operand = 28;
                }
            }
            return code;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InputManagerRewired), nameof(InputManagerRewired.GetRewiredActionId))]
        static void InputManagerRewired_GetRewiredActionId(InputManagerRewired __instance, eInput Input, ref int __result)
        {
            int inputInteger = (int)Input;
            if (inputInteger >= 14) __result = inputInteger + (100 - 14);
            return;
        }
    }
}
