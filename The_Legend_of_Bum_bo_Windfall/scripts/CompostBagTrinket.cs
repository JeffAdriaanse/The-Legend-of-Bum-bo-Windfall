using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class CompostBagTrinket : StatTrinket
    {
        public CompostBagTrinket()
        {
            trinketName = (TrinketName)1002;
            Name = "COMPOST_BAG_DESCRIPTION";
            IconPosition = new Vector2(0f, 0f);
            texturePage = 1;
            Category = TrinketCategory.Puzzle;
        }
    }
}
