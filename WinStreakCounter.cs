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
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            winStreakCounter = UnityEngine.Object.Instantiate(assets.LoadAsset<GameObject>("Win Streak Counter"), titleController.mainMenu.transform);
            winStreakCounter.transform.SetSiblingIndex(1);
            RectTransform winStreakCounterRect = winStreakCounter.GetComponent<RectTransform>();
            winStreakCounterRect.anchoredPosition = new Vector2(260, -50);
            winStreakCounterRect.localRotation = Quaternion.Euler(winStreakCounterRect.localRotation.eulerAngles.x, winStreakCounterRect.localRotation.eulerAngles.y, 350);
            winStreakCounter.transform.Find("Streak").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 62);

            TextMeshProUGUI headerText = winStreakCounter.transform.Find("Header").GetComponent<TextMeshProUGUI>();
            if (headerText != null && WindfallHelper.GetEdmundMcmillenFont() != null)
            {
                headerText.font = WindfallHelper.GetEdmundMcmillenFont();
            }

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