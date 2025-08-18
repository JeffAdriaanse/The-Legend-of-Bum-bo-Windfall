using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using The_Legend_of_Bum_bo_Windfall.scripts;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    [Serializable]
    public class WindfallPersistentData
    {
        public WindfallPersistentData()
        {
            winCount = 0;

            expandModifiers = true;

            implementBalanceChanges = true;

            antiAliasing = true;
            depthOfField = false;
            motionBlur = true;

            tooltipSize = 0;

            wiseWins = 0;
            wiseMoneyWins = 0;

            unlocks = new bool[4];
            //Unlock 0: Bum-bo the Wise
            //Unlock 1: Plasma Ball
            //Unlock 2: Magnifying Glass
            //Unlock 3: Compost Bag

            hotkeys = new Dictionary<string, KeyCode>();
        }

        /// <summary>
        /// Ensures saved objects are not null
        /// </summary>
        public void Verify()
        {
            if (unlocks == null) unlocks = new bool[4];

            if (hotkeys == null) hotkeys = new Dictionary<string, KeyCode>(HotkeysMenu.defaultHotkeys);
            foreach (KeyValuePair<string, KeyCode> keyValuePair in HotkeysMenu.defaultHotkeys) {
                if (!hotkeys.ContainsKey(keyValuePair.Key)) hotkeys[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        public int winCount;

        public bool expandModifiers;

        //Windfall settings
        public bool implementBalanceChanges;

        public bool antiAliasing;
        public bool depthOfField;
        public bool motionBlur;

        public int tooltipSize;

        //Progression
        public int wiseWins;
        public int wiseMoneyWins;

        public bool[] unlocks;

        public Dictionary<string, KeyCode> hotkeys;
    }

    public static class WindfallPersistentDataController
    {
        private static WindfallPersistentData windfallPersistentData;
        private static readonly string fileName = "windfall.sav";
        private static string DataPath { get
            {
                string path = WindfallHelper.FindFileInCurrentDirectory(fileName);
                if (path == null) path = Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Bepinex/plugins/The Legend of Bum-bo_Windfall").FullName + "/" + fileName;
                return path;
            }
        }

        public static void SaveData(WindfallPersistentData windfallPersistentData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(DataPath, FileMode.Create, FileAccess.Write);
            binaryFormatter.Serialize(fileStream, windfallPersistentData);
            fileStream.Close();

            //Update currently cached data
            WindfallPersistentDataController.windfallPersistentData = windfallPersistentData;
        }

        public static WindfallPersistentData LoadData()
        {
            //Load cached data
            if (windfallPersistentData != null) return windfallPersistentData;

            if (File.Exists(DataPath))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(DataPath, FileMode.Open, FileAccess.Read);
                //Cache newly loaded data
                windfallPersistentData = (WindfallPersistentData)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
                windfallPersistentData.Verify();
                return windfallPersistentData;
            }

            //Cache newly generated data
            windfallPersistentData = new WindfallPersistentData();
            return windfallPersistentData;
        }

        public static void ResetProgression()
        {
            WindfallPersistentData resetData = LoadData();
            resetData.wiseWins = 0;
            resetData.wiseMoneyWins = 0;
            resetData.unlocks = new bool[4];
            SaveData(resetData);
        }
    }
}