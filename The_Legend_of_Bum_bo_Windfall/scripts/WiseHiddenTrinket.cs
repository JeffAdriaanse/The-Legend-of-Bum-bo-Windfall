using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class WiseHiddenTrinket : TrinketElement
    {
        public WiseHiddenTrinket()
        {
            trinketName = (TrinketName)1001;
            Name = "WISE_HIDDEN_DESCRIPTION";
            IconPosition = new Vector2(0f, 0f);
            texturePage = 1;
            Category = TrinketCategory.Puzzle;
        }
    }
}
