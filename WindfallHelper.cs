namespace The_Legend_of_Bum_bo_Windfall
{
    public static class WindfallHelper
    {
        public static int ChaptersUnlocked(Progression progression)
        {
            int numberOfChapters;
            if (!progression.unlocks[0]) numberOfChapters = 1;
            else if (!progression.unlocks[1]) numberOfChapters = 2;
            else if (!progression.unlocks[2]) numberOfChapters = 3;
            else numberOfChapters = 4;
            return numberOfChapters;
        }
    }
}