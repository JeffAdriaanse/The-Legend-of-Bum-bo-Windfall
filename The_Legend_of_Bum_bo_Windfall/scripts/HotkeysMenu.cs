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
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using Rewired;
using HarmonyLib;
using System.Reflection.Emit;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace The_Legend_of_Bum_bo_Windfall.scripts
{
    class HotkeysMenu : MonoBehaviour
    {
        private Dictionary<string, KeyCode> hotkeys;
        private Dictionary<string, TextMeshProUGUI> keysText = new Dictionary<string, TextMeshProUGUI>();

        private string currentBindingKey;

        private readonly Color ValidHotKeyColor = new Color(0.15f, 0.4f, 0.15f, 1f); //Dark grey
        private readonly Color InvalidHotKeyColor = new Color(0.6f, 0.2f, 0.2f, 1f); //Dark red
        private readonly Color BindingHotKeyColor = new Color(0.2f, 0.2f, 0.6f, 1f); //Dark blue


        public static readonly Dictionary<string, KeyCode> defaultHotkeys = new Dictionary<string, KeyCode>()
        {
            { "SPELL_1", KeyCode.Alpha1 },
            { "SPELL_2", KeyCode.Alpha2 },
            { "SPELL_3", KeyCode.Alpha3 },
            { "SPELL_4", KeyCode.Alpha4 },
            { "SPELL_5", KeyCode.Alpha5 },
            { "SPELL_6", KeyCode.Alpha6 },
            { "TRINKET_1", KeyCode.Alpha7 },
            { "TRINKET_2", KeyCode.Alpha8 },
            { "TRINKET_3", KeyCode.Alpha9 },
            { "TRINKET_4", KeyCode.Alpha0 },
            { "LEFT_LANE", KeyCode.Z },
            { "CENTER_LANE", KeyCode.X },
            { "RIGHT_LANE", KeyCode.C },
            { "SHOW_HIDE_INDICATORS", KeyCode.Tab },
        };

        /*Some KeyCodes correspond to keys that are used for existing actions in the Rewired Input Manager system.
        These KeyCodes are banned from Windfall's input system to ensure that two different actions cannot be assigned to the same key.*/
        private readonly List<KeyCode> bannedKeyCodes = new List<KeyCode>()
        {
            KeyCode.W,
            KeyCode.A,
            KeyCode.S,
            KeyCode.D,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.Return,
            KeyCode.Space,
            KeyCode.Escape,
            KeyCode.E,
            KeyCode.LeftControl,
            KeyCode.LeftShift,
            KeyCode.R,

            KeyCode.Mouse0,
        };

        public void SetUpHotkeysMenu(GameObject menuView)
        {
            gameObject.SetActive(false);
            transform.SetSiblingIndex(Mathf.Max(0, transform.parent.childCount - 2));

            TMP_FontAsset edmundmcmillen_regular = WindfallHelper.GetEdmundMcmillenFont();

            GameObject header = transform.Find("Header").gameObject;
            GameObject save = transform.Find("Save").gameObject;
            GameObject cancel = transform.Find("Cancel").gameObject;

            //Localize header
            WindfallHelper.LocalizeObject(header, "Menu/HOTKEYS");

            //Initialize buttons
            WindfallHelper.InitializeButton(save, SaveHotkeys, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);
            WindfallHelper.InitializeButton(cancel, CloseHotkeysMenu, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Both);

            //Localize buttons
            WindfallHelper.LocalizeObject(save, "Menu/OPTIONS_SAVE");
            WindfallHelper.LocalizeObject(cancel, "Menu/OPTIONS_CANCEL");

            //Action buttons
            foreach (TextMeshProUGUI child in transform.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (child.gameObject.name == "Key")
                {
                    Transform parent = child.transform.parent;
                    if (parent == null) continue;

                    string parentName = parent.name;

                    //Fill keysText Dictionary with the key names and corresponding TextMeshProUGUI components
                    keysText.Add(parentName, child);

                    //Initialize buttons
                    UnityAction unityAction = () => { SetActiveHotkey(parent.name); };
                    WindfallHelper.InitializeButton(parent.gameObject, unityAction, edmundmcmillen_regular, GamepadMenuOptionSelection.eInjectDots.Left);

                    //Localize buttons
                    if (parent.TryGetComponent(out TextMeshProUGUI parentTextMeshProUGUI))
                    {
                        if (parentTextMeshProUGUI.TryGetComponent(out Localize parentLocalize)) Localization.SetKey(parentLocalize, eI2Category.Menu, parentName);
                        else WindfallHelper.LocalizeObject(parent.gameObject, "Menu/" + parentName);
                    }
                }
            }

            //Keyboard/gamepad control functionality
            GamepadMenuController gamepadMenuController = gameObject.AddComponent<GamepadMenuController>();
            WindfallHelper.UpdateGamepadMenuButtons(gamepadMenuController, transform.Find("Cancel")?.gameObject);
        }

        private void SetActiveHotkey(string keyName)
        {
            if (currentBindingKey != null) return;
            currentBindingKey = keyName;
            SetKeyText(keyName, "---", BindingHotKeyColor);
        }

        private void Update()
        {
            if (currentBindingKey != null) {
                foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode) && !bannedKeyCodes.Contains(keyCode))
                    {
                        AssignKey(currentBindingKey, keyCode);
                        currentBindingKey = null;
                        break;
                    }
                }
            }
            return;
        }

        private void AssignKey(string keyName, KeyCode keyCode)
        {
            hotkeys[keyName] = keyCode;
            UpdateKeys();
        }

        //Update text for all keys
        private void UpdateKeys()
        {
            foreach (var keyValuePair in hotkeys) {
                string keyName = keyValuePair.Key;
                UpdateKey(keyName);
            }
        }
        //Update text for a particular key
        private void UpdateKey(string keyName)
        {
            if (hotkeys.TryGetValue(keyName, out var keyCode) && keysText.TryGetValue(keyName, out var textMeshProUGUI)) {
                //Choose color
                Color color;
                if (VerifyHotkey(keyName)) color = ValidHotKeyColor; //Valid key color
                else color = InvalidHotKeyColor; //Invalid key color

                //Set key text
                SetKeyText(keyName, keyCode.ToString(), color);
                textMeshProUGUI.text = keyCode.ToString();
            }
        }

        //Sets the text and text color of a key
        private void SetKeyText(string keyName, string text, Color color)
        {
            if (keysText.TryGetValue(keyName, out var textMeshProUGUI))
            {
                textMeshProUGUI.text = text;
                textMeshProUGUI.color = color;
            }
        }

        //Returns whether all hotkeys are valid
        private bool VerifyHotkeys()
        {
            foreach (var keyValuePair in hotkeys)
            {
                string keyName = keyValuePair.Key;
                if (!VerifyHotkey(keyName)) return false;
            }

            return true;
        }
        //Returns whether they KeyCode associated with the given hotkey is not in bannedKeyCodes and is not assigned to multiple hotkeys
        private bool VerifyHotkey(string keyName)
        {
            return (!bannedKeyCodes.Contains(hotkeys[keyName]) && hotkeys.Values.Count(keyCode => keyCode == hotkeys[keyName]) < 2);
        }

        public void OpenHotkeysMenu()
        {
            Transform menuViewTransform = transform.parent;

            menuViewTransform.Find("Windfall Menu(Clone)")?.gameObject.SetActive(false);
            gameObject.SetActive(true);
            currentBindingKey = null;

            LoadHotkeys();
        }
        public void CloseHotkeysMenu()
        {
            Transform menuViewTransform = transform.parent;

            currentBindingKey = null;
            gameObject.SetActive(false);
            menuViewTransform.Find("Windfall Menu(Clone)")?.gameObject.SetActive(true);
        }

        private void SaveHotkeys()
        {
            if (!VerifyHotkeys()) return;

            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            windfallPersistentData.hotkeys = new Dictionary<string, KeyCode>(hotkeys);
            WindfallPersistentDataController.SaveData(windfallPersistentData);

            CloseHotkeysMenu();
        }

        private void LoadHotkeys()
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            hotkeys = new Dictionary<string, KeyCode>(windfallPersistentData.hotkeys);

            //Ensure all hotkeys are present
            foreach (var keyValuePair in defaultHotkeys)
            {
                if (!hotkeys.ContainsKey(keyValuePair.Key)) hotkeys.Add(keyValuePair.Key, keyValuePair.Value);
            }

            UpdateKeys();
        }
    }

    class InputChanges()
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(InputChanges));
        }

        //Add spell/trinket hotkey functionality
        [HarmonyPostfix, HarmonyPatch(typeof(GamepadSpellSelector), "Update")]
        static void GamepadSpellSelector_Update(GamepadSpellSelector __instance)
        {
            if (__instance.app.controller.debugController.IsDebugMenuOpen()) return;
            if (__instance.app.model.bumboEvent.GetType() != typeof(IdleEvent) && __instance.app.model.bumboEvent.GetType() != typeof(ChanceToCastSpellEvent)) return;

            Dictionary<string, KeyCode> hotkeys = WindfallPersistentDataController.LoadData().hotkeys;

            //Selecting spells
            int selectedSpellIndex = -1;
            if (hotkeys.TryGetValue("SPELL_1", out KeyCode spell1) && Input.GetKeyDown(spell1)) selectedSpellIndex = 0;
            else if (hotkeys.TryGetValue("SPELL_2", out KeyCode spell2) && Input.GetKeyDown(spell2)) selectedSpellIndex = 1;
            else if (hotkeys.TryGetValue("SPELL_3", out KeyCode spell3) && Input.GetKeyDown(spell3)) selectedSpellIndex = 2;
            else if (hotkeys.TryGetValue("SPELL_4", out KeyCode spell4) && Input.GetKeyDown(spell4)) selectedSpellIndex = 3;
            else if (hotkeys.TryGetValue("SPELL_5", out KeyCode spell5) && Input.GetKeyDown(spell5)) selectedSpellIndex = 4;
            else if (hotkeys.TryGetValue("SPELL_6", out KeyCode spell6) && Input.GetKeyDown(spell6)) selectedSpellIndex = 5;

            //Selecting trinkets
            int selectedTrinketIndex = -1;
            if (selectedSpellIndex < 0)
            {
                if (hotkeys.TryGetValue("TRINKET_1", out KeyCode trinket1) && Input.GetKeyDown(trinket1)) selectedTrinketIndex = 0;
                else if (hotkeys.TryGetValue("TRINKET_2", out KeyCode trinket2) && Input.GetKeyDown(trinket2)) selectedTrinketIndex = 1;
                else if (hotkeys.TryGetValue("TRINKET_3", out KeyCode trinket3) && Input.GetKeyDown(trinket3)) selectedTrinketIndex = 2;
                else if (hotkeys.TryGetValue("TRINKET_4", out KeyCode trinket4) && Input.GetKeyDown(trinket4)) selectedTrinketIndex = 3;
            }

            if (selectedSpellIndex < 0 && selectedTrinketIndex < 0) return;

            if (!__instance.IsActive)
            {
                //Initialize the GamepadSpellSelector
                GamepadPuzzleController gamepadPuzzleController = __instance.app.view.puzzle.GetComponent<GamepadPuzzleController>();
                var methodInfo = AccessTools.Method(typeof(GamepadPuzzleController), "spell_menu_closed");
                var closeEventDelegate = AccessTools.MethodDelegate<GamepadSpellSelector.CloseEvent>(methodInfo, gamepadPuzzleController);
                if (__instance.Initialize(GamepadSpellSelector.eMode.UseSpellOrTrinket, true, closeEventDelegate))
                {
                    Type enumType = AccessTools.Inner(typeof(GamepadPuzzleController), "eState");
                    object spellValue = Enum.Parse(enumType, "Spell");
                    AccessTools.Field(typeof(GamepadPuzzleController), "m_State").SetValue(gamepadPuzzleController, spellValue);

                    GameObject m_HoverBlock = (GameObject)AccessTools.Field(typeof(GamepadPuzzleController), "m_HoverBlock").GetValue(gamepadPuzzleController);
                    if (m_HoverBlock != null) m_HoverBlock.SetActive(false);
                }
            }

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

            int selectedColumn = -1;

            Dictionary<string, KeyCode> hotkeys = WindfallPersistentDataController.LoadData().hotkeys;
            if (hotkeys.TryGetValue("LEFT_LANE", out KeyCode leftLane) && Input.GetKeyDown(leftLane)) selectedColumn = 1;
            else if (hotkeys.TryGetValue("CENTER_LANE", out KeyCode centerLane) && Input.GetKeyDown(centerLane)) selectedColumn = 2;
            else if (hotkeys.TryGetValue("RIGHT_LANE", out KeyCode rightLane) && Input.GetKeyDown(rightLane)) selectedColumn = 3;

            ClickableColumnView[] clickableColumnViews = __instance.app.view.clickableColumnViews;
            if (selectedColumn >= 0 && selectedColumn < clickableColumnViews.Length && clickableColumnViews[selectedColumn].isActiveAndEnabled) __instance.app.view.clickableColumnViews[selectedColumn].ForceClick();
        }
    }
}
