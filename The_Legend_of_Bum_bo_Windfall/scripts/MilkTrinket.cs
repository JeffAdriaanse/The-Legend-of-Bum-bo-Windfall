using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class MilkTrinket : StatTrinket
    {
        public MilkTrinket()
        {
            trinketName = (TrinketName)1003;
            Name = "MILK_DESCRIPTION";
            IconPosition = new Vector2(0f, 0f);
            texturePage = 1;
            Category = TrinketCategory.Stat;
        }

        /// <summary>
        /// Trinket OnHurt effect. Functionality is reliant on <see cref="OtherChanges"/> bumboTurn ObjectDataStorage.
        /// </summary>
        public override void OnHurt()
        {
            int moveGain = WindfallHelper.app.controller.trinketController.EffectMultiplier();
            if (ObjectDataStorage.GetData<bool>(WindfallHelper.app.model.gameObject, "bumboTurn")) WindfallHelper.app.controller.ModifyActionPoint(moveGain);
            else WindfallHelper.app.model.actionPointModifier += (short)moveGain;
        }
    }
}
