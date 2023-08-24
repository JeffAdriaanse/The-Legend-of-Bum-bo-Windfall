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
        public static void SaveData(WindfallPersistentData windfallPersistentData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(dataPath, FileMode.Create, FileAccess.Write);
            binaryFormatter.Serialize(fileStream, windfallPersistentData);
            fileStream.Close();
        }

        public static WindfallPersistentData LoadData()
        {
            if (File.Exists(dataPath))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(dataPath, FileMode.Open, FileAccess.Read);
                WindfallPersistentData windfallPersistentData = (WindfallPersistentData)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
                return windfallPersistentData;
            }
            return new WindfallPersistentData();
        }

        private static readonly string dataPath = Directory.GetCurrentDirectory() + "/Bepinex/plugins/The Legend of Bum-bo_Windfall/windfall.sav";
    }
}