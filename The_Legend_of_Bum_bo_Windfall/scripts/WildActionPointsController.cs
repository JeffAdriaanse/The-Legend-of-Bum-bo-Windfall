using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall.scripts
{
    public class WildActionPointsController : MonoBehaviour
    {
        private static readonly string wildsUsedKey = "wildsUsed";

        private static Shader scrollingRainbow;
        private static Shader ScrollingRainbow
        {
            get
            {
                if (scrollingRainbow == null) scrollingRainbow = Windfall.assetBundle.LoadAsset<Shader>("ScrollingRainbow");
                return scrollingRainbow;
            }
        }

        private int WildsLeft
        {
            get { return NumberOfAllowedWilds() - WildsPlaced(); }
        }

        public void UpdateActionPointsAppearance()
        {
            ActionPointObject actionPointObject = WindfallHelper.app.view.actionPoints;
            int action_points = actionPointObject.action_points;
            int startIndex = (action_points - WildsLeft) + 1;
            if (startIndex < 0 || ForceAllowWilds()) startIndex = 0;
            int endIndex = action_points;

            for (int i = 0; i < actionPointObject.action_point_objects.Length; i++)
            {
                bool rainbow = i + 1 >= startIndex && i + 1 <= endIndex;
                UpdateActionPointAppearance(actionPointObject.action_point_objects[i].transform, rainbow);
            }
        }

        private void UpdateActionPointAppearance(Transform actionPoint, bool rainbow)
        {
            if (rainbow)
            {
                Material material = actionPoint.GetComponent<MeshRenderer>().material;
                material.shader = ScrollingRainbow;
                //material.SetFloat("_RainbowStrength", 0.5f);
                //material.SetFloat("_RainbowSpeed", 0.75f);
                //material.SetFloat("_RainbowScale", 1.5f);
            }
            else WindfallHelper.ResetShader(actionPoint);
        }

        public bool AttemptPlaceWild()
        {
            if (ForceAllowWilds()) return true;
            else if (WildsLeft > 0)
            {
                RegisterWildPlaced();
                return true;
            }
            return false;
        }

        private int NumberOfAllowedWilds()
        {
            int allowedWilds = 0;
            for (int trinketIterator = -1; trinketIterator < WindfallHelper.app.model.characterSheet.trinkets.Count; trinketIterator++)
            {
                TrinketElement trinketElement = null;
                if (trinketIterator == -1) trinketElement = WindfallHelper.app.model.characterSheet.hiddenTrinket;
                else trinketElement = WindfallHelper.app.controller.GetTrinket(trinketIterator);
                if (turnWildOnDragTrinkets.TryGetValue(trinketElement.trinketName, out int value) && value > 0) allowedWilds += value * WindfallHelper.app.controller.trinketController.EffectMultiplier();
            }
            return allowedWilds;
        }

        private bool ForceAllowWilds()
        {
            for (int trinketIterator = -1; trinketIterator < WindfallHelper.app.model.characterSheet.trinkets.Count; trinketIterator++)
            {
                TrinketElement trinketElement = null;
                if (trinketIterator == -1) trinketElement = WindfallHelper.app.model.characterSheet.hiddenTrinket;
                else trinketElement = WindfallHelper.app.controller.GetTrinket(trinketIterator);
                if (turnWildOnDragTrinkets.TryGetValue(trinketElement.trinketName, out int value) && value == -1) return true;
            }
            return false;
        }

        private void RegisterWildPlaced()
        {
            int wildsPlaced = 0;
            if (ObjectDataStorage.HasData<int>(WindfallHelper.app.model.characterSheet, wildsUsedKey)) wildsPlaced = ObjectDataStorage.GetData<int>(WindfallHelper.app.model.characterSheet, wildsUsedKey);
            wildsPlaced += 1;
            ObjectDataStorage.StoreData(WindfallHelper.app.model.characterSheet, wildsUsedKey, wildsPlaced);
        }

        public void ResetWildsPlaced()
        {
            ObjectDataStorage.StoreData(WindfallHelper.app.model.characterSheet, wildsUsedKey, 0);
        }

        private int WildsPlaced()
        {
            if (ObjectDataStorage.HasData<int>(WindfallHelper.app.model.characterSheet, wildsUsedKey)) return ObjectDataStorage.GetData<int>(WindfallHelper.app.model.characterSheet, wildsUsedKey);
            return 0;
        }

        private static readonly Dictionary<TrinketName, int> turnWildOnDragTrinkets = new Dictionary<TrinketName, int>
        {
            { (TrinketName)1001, -1 },
            { (TrinketName)1002, 1 },
        };
    }

    public class WildActionPointsPatches
    {
        //Patch: Adds rainbow effect to wild movement points
        [HarmonyPostfix, HarmonyPatch(typeof(ActionPointObject), nameof(ActionPointObject.SetActionPoints))]
        static void ActionPointObject_SetActionPoints(ActionPointObject __instance)
        {
            WindfallHelper.WildActionPointsController.UpdateActionPointsAppearance();
        }

        //Patch: Resets wilds placed on turn start
        [HarmonyPostfix, HarmonyPatch(typeof(NewRoundEvent), nameof(NewRoundEvent.Execute))]
        static void NewRoundEvent_Execute(NewRoundEvent __instance)
        {
            WindfallHelper.WildActionPointsController.ResetWildsPlaced();
            WindfallHelper.WildActionPointsController.UpdateActionPointsAppearance();
        }

        //Patch: Resets wilds placed on room start
        [HarmonyPostfix, HarmonyPatch(typeof(RoomStartEvent), nameof(RoomStartEvent.Execute))]
        static void RoomStartEvent_Execute(RoomStartEvent __instance)
        {
            WindfallHelper.WildActionPointsController.ResetWildsPlaced();
            WindfallHelper.WildActionPointsController.UpdateActionPointsAppearance();
        }
    }
}
