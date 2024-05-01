using System;
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
        private static readonly string fileName = "windfall.sav";
        private static string DataPath { get { return WindfallHelper.FindFileInCurrentDirectory(fileName); } }

        public static void SaveData(WindfallPersistentData windfallPersistentData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(DataPath, FileMode.Create, FileAccess.Write);
            binaryFormatter.Serialize(fileStream, windfallPersistentData);
            fileStream.Close();
        }

        public static WindfallPersistentData LoadData()
        {
            if (File.Exists(DataPath))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(DataPath, FileMode.Open, FileAccess.Read);
                WindfallPersistentData windfallPersistentData = (WindfallPersistentData)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
                return windfallPersistentData;
            }
            return new WindfallPersistentData();
        }
    }
}