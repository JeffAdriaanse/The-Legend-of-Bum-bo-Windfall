using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

namespace The_Legend_of_Bum_bo_Windfall
{
    class OtherChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(OtherChanges));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying other changes");
        }

        //Patch: Enables Debug menu
        [HarmonyPostfix, HarmonyPatch(typeof(DebugController), "Start")]
        static void DebugController_Start(DebugController __instance)
        {
            __instance.turnOnDebugKey = true;
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Enabling debug menu");
        }

        //Patch: Fixes a bug in the Mega Boner tile combo logic; it no longer incorrectly breaks out of the for statements that look for enemies
        [HarmonyPatch(typeof(BoneMegaAttackEvent), "AttackEnemy")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);

            Label endLabel = il.DefineLabel();

            bool firstBreak = true;
            int firstBreakIndex = -1;
            for (int codeIndex = 0; codeIndex < code.Count - 1; codeIndex++)
            {
                if (code[codeIndex].opcode == OpCodes.Br && code[codeIndex - 1].opcode == OpCodes.Callvirt && code[codeIndex - 2].opcode == OpCodes.Callvirt)
                {
                    if (firstBreak)
                    {
                        firstBreakIndex = codeIndex;
                        firstBreak = false;
                    }
                    else
                    {
                        code[codeIndex].opcode = OpCodes.Nop;
                        code[codeIndex].labels.Add(endLabel);

                        code[firstBreakIndex] = new CodeInstruction(OpCodes.Br, endLabel);
                        return code;
                    }
                }
            }
            return code;
        }

        //Patch: Fixes Mega Chomper Tile Combo sometimes trying to damage enemies that are already dead, which can cause a softlock
        [HarmonyPrefix, HarmonyPatch(typeof(ToothMegaAttackEvent), "AttackEnemy")]
        static bool ToothMegaAttackEvent_AttackEnemy(ToothMegaAttackEvent __instance)
        {
            List<Enemy> list = new List<Enemy>();
            list.AddRange(__instance.app.model.enemies);
            short num = 0;
            while (num < list.Count)
            {
                if (list[num] != null && (list[num].GetComponent<Enemy>().alive || list[num].GetComponent<Enemy>().isPoop))
                {
                    list[num].GetComponent<Enemy>().Hurt(__instance.app.model.characterSheet.getPuzzleDamage() + 3, Enemy.AttackImmunity.ReducePuzzleDamage, null, -1);
                }
                num += 1;
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Reworking Mega Chomper attack to prevent it from targeting null enemies");
            return false;
        }
    }
}