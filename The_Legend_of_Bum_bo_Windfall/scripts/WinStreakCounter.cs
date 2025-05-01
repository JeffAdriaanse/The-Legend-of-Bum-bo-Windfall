using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class WinStreakCounter
    {
        static GameObject winStreakCounter;
        public static void CreateWinStreakCounter(TitleController titleController)
        {
            winStreakCounter = UnityEngine.Object.Instantiate(Windfall.assetBundle.LoadAsset<GameObject>("Win Streak Counter"), titleController.mainMenu.transform);
            winStreakCounter.transform.SetSiblingIndex(1);
            RectTransform winStreakCounterRect = winStreakCounter.GetComponent<RectTransform>();
            winStreakCounterRect.anchoredPosition = new Vector2(260, -50);
            winStreakCounterRect.localRotation = Quaternion.Euler(winStreakCounterRect.localRotation.eulerAngles.x, winStreakCounterRect.localRotation.eulerAngles.y, 350);

            GameObject streak = winStreakCounter.transform.Find("Streak").gameObject;
            streak.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 62);
            WindfallHelper.LocalizeObject(streak, null);

            GameObject header = winStreakCounter.transform.Find("Header").gameObject;
            WindfallHelper.LocalizeObject(header, "Menu/WIN_STREAK");

            UpdateWinStreakCounter();
        }

        public static void UpdateWinStreakCounter()
        {
            if (winStreakCounter != null)
            {
                Text streakText = winStreakCounter.transform.Find("Streak").GetComponent<Text>();

                if (WindfallHelper.ChaptersUnlocked(ProgressionController.LoadProgression()) != 4)
                {
                    streakText.text = "-";
                }
                else
                {
                    int streak = WindfallPersistentDataController.LoadData().winCount;
                    streakText.text = streak.ToString();
                }
            }
        }

        public static void UpdateWinCount(int winCount)
        {
            WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
            windfallPersistentData.winCount = winCount;
            WindfallPersistentDataController.SaveData(windfallPersistentData);
            UpdateWinStreakCounter();
        }

        public static void GameWon()
        {
            int streak = WindfallPersistentDataController.LoadData().winCount;
            streak = (streak <= 0) ? 1 : streak + 1;
            UpdateWinCount(streak);
        }

        public static void GameLost()
        {
            int streak = WindfallPersistentDataController.LoadData().winCount;
            streak = (streak >= 0) ? -1 : streak - 1;
            UpdateWinCount(streak);
        }

        public static void ResetStreak(bool maintainNegativeStreak)
        {
            if (maintainNegativeStreak)
            {
                if (WindfallPersistentDataController.LoadData().winCount > 0) UpdateWinCount(0);
            }
            else
            {
                UpdateWinCount(0);
            }
        }
    }
}