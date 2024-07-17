using System;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
        }

        public int winCount;

        public bool expandModifiers;

        public bool implementBalanceChanges;

        public bool antiAliasing;
        public bool depthOfField;
        public bool motionBlur;

        public int tooltipSize;
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

            //Erase currently cached data
            WindfallPersistentDataController.windfallPersistentData = null;
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
                return windfallPersistentData;
            }

            //Cache newly generated data
            windfallPersistentData = new WindfallPersistentData();
            return windfallPersistentData;
        }
    }
}