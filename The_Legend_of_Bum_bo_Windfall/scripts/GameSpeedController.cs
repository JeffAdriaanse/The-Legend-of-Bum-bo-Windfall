using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class GameSpeedController : MonoBehaviour
    {
        bool setInitialAnimationSpeeds = true;
        private float initialPuzzleAnimationSpeed = 0.66f;
        private float initialEnemyAnimationSpeed = 0.75f;

        public int GameSpeed
        {
            get
            {
                WindfallPersistentData windfallPersistentData = WindfallPersistentDataController.LoadData();
                return windfallPersistentData != null ? WindfallPersistentDataController.LoadData().gameSpeed : 0;
            }
        }
        public float GameSpeedMultiplier
        {
            get { return 1 + (GameSpeed * 0.5f); }
        }

        public float TweenDurationMultiplier
        {
            get { return 1f / GameSpeedMultiplier; }
        }

        public void UpdateGameSpeed()
        {
            BumboModel bumboModel = WindfallHelper.app?.model;

            //Enemy and puzzle match animations
            if (bumboModel != null)
            {
                if (setInitialAnimationSpeeds)
                {
                    setInitialAnimationSpeeds = false;
                    initialPuzzleAnimationSpeed = bumboModel.puzzleAnimationSpeed;
                    initialEnemyAnimationSpeed = bumboModel.enemyAnimationSpeed;
                }

                bumboModel.puzzleAnimationSpeed = initialPuzzleAnimationSpeed * TweenDurationMultiplier;
                bumboModel.enemyAnimationSpeed = initialEnemyAnimationSpeed * TweenDurationMultiplier;
            }

            //Puzzle board animation
            Puzzle puzzle = WindfallHelper.app?.view?.puzzle;
            if (puzzle != null && puzzle.TryGetComponent<Animator>(out Animator animator))
            {
                animator.speed = GameSpeedMultiplier;
            }
        }
    }

    [HarmonyPatch]
    public class SequenceSpeedMultiplierPatch()
    {
        //Changes the timescale of a sequence according to the GameSpeedMultiplier
        private static void MultiplySequenceAnimationSpeed(Sequence sequence)
        {
            if (WindfallHelper.GameSpeedController == null) return;
            //Change sequence timescale
            sequence.timeScale *= WindfallHelper.GameSpeedController.GameSpeedMultiplier;
        }

        private static readonly MethodInfo MethodInfo_DOTweeen_Sequence = AccessTools.Method(typeof(DOTween), nameof(DOTween.Sequence));
        public static readonly MethodInfo MethodInfo_MultiplySequenceAnimationSpeed = AccessTools.Method(typeof(SequenceSpeedMultiplierPatch), nameof(MultiplySequenceAnimationSpeed));

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            //Skull game
            yield return AccessTools.Method(typeof(CupGameEvent), nameof(CupGameEvent.Execute));
            yield return AccessTools.Method(typeof(CupGameResultEvent), nameof(CupGameResultEvent.Execute));
            yield return AccessTools.Method(typeof(CupClerkView), nameof(CupClerkView.AnimateShuffle));
            yield return AccessTools.Method(typeof(GamblingBangView), nameof(GamblingBangView.Disappear));

            //Stat wheel
            yield return AccessTools.Method(typeof(WheelView), "Spin");
            yield return AccessTools.Method(typeof(RegisterView), nameof(RegisterView.Pay));
            yield return AccessTools.Method(typeof(WheelSpin), "Reward");

            //Wooden Nickel exit
            yield return AccessTools.Method(typeof(ExitGamblingView), nameof(ExitGamblingView.OnMouseDown));
            yield return AccessTools.Method(typeof(GamblingController), nameof(GamblingController.StartChapterIntro));

            //Intros view
            yield return AccessTools.Method(typeof(IntrosView), nameof(IntrosView.Animate));

            //Reset camera at start
            yield return AccessTools.Method(typeof(ResetCameraAtStartEvent), nameof(ResetCameraAtStartEvent.Execute));

            //NewRoundDelay
            yield return AccessTools.Method(typeof(NewRoundDelay), nameof(NewRoundDelay.Execute));

            //Show boss sign
            yield return AccessTools.Method(typeof(ShowBossSignEvent), nameof(ShowBossSignEvent.Execute));
            //Boss dying
            yield return AccessTools.Method(typeof(BossDyingEvent), nameof(BossDyingEvent.Execute));

            //Reward
            yield return AccessTools.Method(typeof(RewardEvent), nameof(RewardEvent.Execute));
            yield return AccessTools.Method(typeof(GrabBossRewardEvent), nameof(GrabBossRewardEvent.Execute));

            //Move to room
            yield return AccessTools.Method(typeof(MoveToRoomEvent), nameof(MoveToRoomEvent.Execute));
            //Move into room
            yield return AccessTools.Method(typeof(MoveIntoRoomEvent), nameof(MoveIntoRoomEvent.HopInBack));
            yield return AccessTools.Method(typeof(MoveIntoRoomEvent), nameof(MoveIntoRoomEvent.HopInFront));
            yield return AccessTools.Method(typeof(MoveIntoRoomEvent), nameof(MoveIntoRoomEvent.ResetRoom));
            yield return AccessTools.Method(typeof(MoveIntoRoomEvent), nameof(MoveIntoRoomEvent.WrapAround));

            //Bumbo counter nextEvent (camera portion)
            yield return AccessTools.Method(typeof(BumboCounterEvent), nameof(BumboCounterEvent.NextEvent));
            //Enemies attack nextEvent (camera portion)
            yield return AccessTools.Method(typeof(EnemiesAttackEvent), nameof(EnemiesAttackEvent.NextEvent));

            //Treasure spell replace
            yield return AccessTools.Method(typeof(TreasureSpellReplaceEvent), nameof(TreasureSpellReplaceEvent.NextEvent));

            //Treasure chosen
            yield return AccessTools.Method(typeof(TreasureChosenEvent), nameof(TreasureChosenEvent.Execute));

            //Trinket pickup HopAndDisappear
            yield return AccessTools.Method(typeof(TrinketPickupView), nameof(TrinketPickupView.HopAndDisappear));

            //Spell pickup HopAndDisappear
            yield return AccessTools.Method(typeof(SpellPickup), nameof(SpellPickup.HopAndDisappear));

            //Spell modify delay (Wooden Nickel)
            yield return AccessTools.Method(typeof(SpellModifyDelayEvent), nameof(SpellModifyDelayEvent.Execute));
            yield return AccessTools.Method(typeof(SpellModifyDelayEvent), nameof(SpellModifyDelayEvent.NextEvent));

            //Spell ready notifications
            yield return AccessTools.Method(typeof(BumboController), nameof(BumboController.SetActiveSpells));
            yield return AccessTools.Method(typeof(BumboController), nameof(BumboController.ShowSpellReady));

            //Cancel view
            yield return AccessTools.Method(typeof(CancelView), nameof(CancelView.Hide));
            yield return AccessTools.Method(typeof(CancelView), nameof(CancelView.Show), new Type[] { typeof(CancelView.Where) });
            yield return AccessTools.Method(typeof(CancelView), nameof(CancelView.Show), new Type[] { typeof(Vector3), typeof(bool) });

            //Menu button view
            yield return AccessTools.Method(typeof(MenuButtonView), nameof(MenuButtonView.Hide));
            yield return AccessTools.Method(typeof(MenuButtonView), nameof(MenuButtonView.Show));

            //BumboModifierIndication handles animation speed itself, so no transpiler is needed for it

            //Cancel trinketReplace or spellReplace 
            yield return AccessTools.Method(typeof(EventsController), nameof(EventsController.OnNotification));

            //Monster counter
            yield return AccessTools.Method(typeof(MonsterCounterEvent), nameof(MonsterCounterEvent.Execute));
            //Monster end counter
            yield return AccessTools.Method(typeof(MonsterEndCounterEvent), nameof(MonsterEndCounterEvent.Execute));

            //End enemies turn (with booger)
            yield return AccessTools.Method(typeof(EndEnemiesTurnEvent), nameof(EndEnemiesTurnEvent.Execute));
            //Enemies unbooger
            yield return AccessTools.Method(typeof(EnemiesUnboogerEvent), nameof(EnemiesUnboogerEvent.Execute));

            //GUI notifications
            yield return AccessTools.Method(typeof(GUINotificationView), nameof(GUINotificationView.ShowNotification), new Type[] { typeof(string), typeof(SpellElement), typeof(bool) });
            yield return AccessTools.Method(typeof(GUINotificationView), nameof(GUINotificationView.ShowNotification), new Type[] { typeof(string), typeof(TrinketElement), typeof(bool) });
            yield return AccessTools.Method(typeof(GUINotificationView), nameof(GUINotificationView.HideNotification));

            //Attack spells (that don't have custom animations)
            yield return AccessTools.Method(typeof(AttackSpellEvent), nameof(AttackSpellEvent.Execute));

            //Plasma Ball and Magnifying glass handle animation speed themselves, so no transpiler is needed for them

            //Attack fly EndOfMonsterRound implementation is replaced, so its animation speed is modified in its replacement prefix

            //Rock friends
            yield return AccessTools.Method(typeof(RockFriendsSpell), nameof(RockFriendsSpell.AttackAnimation));
            yield return AccessTools.Method(typeof(RockFriendsSpell), nameof(RockFriendsSpell.DropRock));

            //Leaky battery
            yield return AccessTools.Method(typeof(LeakyBatterySpell), nameof(LeakyBatterySpell.CastSpell));

            //Quake
            yield return AccessTools.Method(typeof(QuakeSpell), nameof(QuakeSpell.CastSpell));
            yield return AccessTools.Method(typeof(QuakeSpell), nameof(QuakeSpell.ExplodeStoneAndPoop));
            yield return AccessTools.Method(typeof(QuakeSpell), nameof(QuakeSpell.DropRock));

            //Throw attacks
            yield return AccessTools.Method(typeof(ThrowAttackEvent), nameof(ThrowAttackEvent.Execute));

            //Booger attacks
            yield return AccessTools.Method(typeof(BoogerAttackEvent), nameof(BoogerAttackEvent.Execute));
            //Booger at start
            yield return AccessTools.Method(typeof(BoogerAtStartEvent), nameof(BoogerAtStartEvent.Execute));
            //Mega booger tile combo
            yield return AccessTools.Method(typeof(ComboBoogerMegaView), nameof(ComboBoogerMegaView.Animate));

            //Bone at start
            yield return AccessTools.Method(typeof(BoneAtStartEvent), nameof(BoneAtStartEvent.Execute));
            //Mega bone tile combo
            yield return AccessTools.Method(typeof(BoneMegaAttackEvent), nameof(BoneMegaAttackEvent.AttackEnemy));
            yield return AccessTools.Method(typeof(ComboBoneMegaView), nameof(ComboBoneMegaView.Animate));

            //Mega tooth tile combo
            yield return AccessTools.Method(typeof(ComboToothMegaView), nameof(ComboToothMegaView.Animate));

            //Poop placement
            yield return AccessTools.Method(typeof(PoopEvent), nameof(PoopEvent.Execute));
            //Poop at start
            yield return AccessTools.Method(typeof(PoopAtStartEvent), nameof(PoopAtStartEvent.Execute));

            //Clear matches (ClearPuzzleEvent)
            yield return AccessTools.Method(typeof(Puzzle), nameof(Puzzle.ClearMatches));

            //Move puzzle
            yield return AccessTools.Method(typeof(MovePuzzleEvent), nameof(MovePuzzleEvent.Execute));
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Multiply_AnimationSpeed(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Call, MethodInfo_DOTweeen_Sequence))
                .Repeat(matchAction: cm =>
                {
                    cm.ThrowIfInvalid("Could not find call to DOTween.Sequence")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Call, MethodInfo_MultiplySequenceAnimationSpeed)
                    );
                });
            return codeMatcher.Instructions();
        }
    }

    [HarmonyPatch]
    public class DOMoveDORotateSpeedMultiplierPatch()
    {
        //Changes the timescale of a tweener according to the GameSpeedMultiplier
        private static void MultiplyTweenerAnimationSpeed(Tweener tweener)
        {
            if (WindfallHelper.GameSpeedController == null) return;
            //Change tweener timescale
            tweener.timeScale *= WindfallHelper.GameSpeedController.GameSpeedMultiplier;
        }

        public static readonly MethodInfo MethodInfo_MultiplyTweenerAnimationSpeed = AccessTools.Method(typeof(DOMoveDORotateSpeedMultiplierPatch), nameof(MultiplyTweenerAnimationSpeed));

        private static readonly MethodInfo MethodInfo_DOMove = AccessTools.Method(typeof(ShortcutExtensions), nameof(ShortcutExtensions.DOMove), new Type[] { typeof(UnityEngine.Transform), typeof(UnityEngine.Vector3), typeof(float), typeof(bool) });
        private static readonly MethodInfo MethodInfo_DORotate = AccessTools.Method(typeof(ShortcutExtensions), nameof(ShortcutExtensions.DORotate), new Type[] { typeof(UnityEngine.Transform), typeof(UnityEngine.Vector3), typeof(float), typeof(DG.Tweening.RotateMode) });


        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            //SelectColumnEvent
            yield return AccessTools.Method(typeof(MainCameraController), nameof(MainCameraController.ColumnSelectAngle));

            //Wooden Nickel arrow navigation
            yield return AccessTools.Method(typeof(GamblingNavigation), nameof(GamblingNavigation.OnMouseDown));
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Multiply_AnimationSpeed(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchForward(true, new CodeMatch(ci => ci.opcode == OpCodes.Call && ci.operand is MethodInfo && ((MethodInfo)ci.operand == MethodInfo_DOMove || (MethodInfo)ci.operand == MethodInfo_DORotate)))
                .Repeat(matchAction: cm =>
                {
                    cm.ThrowIfInvalid("Could not find call to ShortcutExtensions.DOMove or ShortcutExtensions.DORotate")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Call, MethodInfo_MultiplyTweenerAnimationSpeed)
                    );
                });
            return codeMatcher.Instructions();
        }
    }

    public class GameSpeedControllerPatches()
    {
        //Patch: Initialize game speed
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.Init))]
        static void BumboController_Init(BumboController __instance)
        {
            WindfallHelper.GameSpeedController.UpdateGameSpeed();
        }

        //Patch: Ensure SpellView shake resets transform properly at high game speeds
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.ShowGUI))]
        static void BumboController_ShowGUI_SpellView_Transform(BumboController __instance)
        {
            foreach (SpellView spellView in WindfallHelper.app.view.spells)
            {
                Vector3 localPosition = (Vector3)AccessTools.Field(typeof(SpellView), "localPosition").GetValue(spellView);
                Vector3 localRotation = (Vector3)AccessTools.Field(typeof(SpellView), "localRotation").GetValue(spellView);

                if (localPosition == Vector3.zero || localRotation == Vector3.zero) return;

                spellView.transform.localPosition = localPosition;
                spellView.transform.localRotation = Quaternion.Euler(localRotation);
                spellView.transform.localScale = Vector3.one;
            }
        }

        //Patch: Ensures doors don't erroneously stay open at high game speeds
        [HarmonyPrefix, HarmonyPatch(typeof(DoorView), nameof(DoorView.ResetDoor))]
        static void DoorView_ResetDoor(DoorView __instance)
        {
            List<Tween> tweens = DOTween.TweensByTarget(__instance.door.GetComponent<MeshRenderer>().material);
            if (tweens == null || tweens.Count == 0) return;
            foreach (Tween t in tweens) t.Kill(false);
        }

        private static void MultiplyTweensAnimationSpeed(List<Tween> tweens)
        {
            if (tweens == null) return;
            foreach (Tween tween in tweens)
            {
                if (tween != null) tween.timeScale *= WindfallHelper.GameSpeedController.GameSpeedMultiplier;
            }
        }

        private static void MultiplyTweensAnimationSpeedByID(string id)
        {
            List<Tween> tweens = DOTween.TweensById(id);
            MultiplyTweensAnimationSpeed(tweens);
        }

        private static void MultiplyTweensAnimationSpeedByTarget(object target)
        {
            List<Tween> tweens = DOTween.TweensByTarget(target);
            MultiplyTweensAnimationSpeed(tweens);
        }

        //Patch: Apply game speed setting to CameraView.transitionToPerspective
        [HarmonyPostfix, HarmonyPatch(typeof(CameraView), nameof(CameraView.transitionToPerspective))]
        static void CameraView_transitionToPerspective(CameraView __instance)
        {
            MultiplyTweensAnimationSpeedByID("perspective");
        }

        //Patch: Apply game speed setting to BumboController.HideGUI
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.HideGUI))]
        static void BumboController_HideGUI(BumboController __instance)
        {
            MultiplyTweensAnimationSpeedByID("HidingGUI");
        }

        //Patch: Apply game speed setting to BumboController.ShowGUI
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.ShowGUI))]
        static void BumboController_ShowGUI(BumboController __instance)
        {
            MultiplyTweensAnimationSpeedByID("ShowingGUI");
        }

        //Patch: Apply game speed setting to BumboCounterEvent.NextEvent (bumbo hurt portion)
        [HarmonyPostfix, HarmonyPatch(typeof(BumboCounterEvent), nameof(BumboCounterEvent.NextEvent))]
        static void BumboCounterEvent_NextEvent(BumboCounterEvent __instance)
        {
            MultiplyTweensAnimationSpeedByTarget(WindfallHelper.app.view.bumboHurt.transform);
        }

        //Patch: Apply game speed setting to EnemiesAttackEvent.NextEvent (bumbo hurt portion)
        [HarmonyPostfix, HarmonyPatch(typeof(EnemiesAttackEvent), nameof(EnemiesAttackEvent.NextEvent))]
        static void EnemiesAttackEvent_NextEvent(EnemiesAttackEvent __instance)
        {
            MultiplyTweensAnimationSpeedByTarget(WindfallHelper.app.view.bumboHurt.transform);
        }

        private static readonly MethodInfo MethodInfo_SpellElement_AttackAnimation = AccessTools.Method(typeof(SpellElement), nameof(SpellElement.AttackAnimation));

        //Patch: Apply game speed setting to custom attack spell animations
        [HarmonyTranspiler, HarmonyPatch(typeof(AttackSpellEvent), nameof(AttackSpellEvent.Execute))]
        static IEnumerable<CodeInstruction> AttackSpellEvent_Execute(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Callvirt, MethodInfo_SpellElement_AttackAnimation))
                    .ThrowIfInvalid("Could not find call to SpellElement.AttackAnimation")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Call, SequenceSpeedMultiplierPatch.MethodInfo_MultiplySequenceAnimationSpeed)
                    );
            return codeMatcher.Instructions();
        }

        //Patch: Ensure castWhereSignView does not fail to appear at high game speeds
        [HarmonyTranspiler, HarmonyPatch(typeof(SelectColumnEvent), nameof(SelectColumnEvent.Execute))]
        static IEnumerable<CodeInstruction> SelectColumnEvent_Execute(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchForward(true, new CodeMatch[]
            {
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(UnityEngine.GameObject), "get_activeSelf")),
                new CodeMatch(ci => ci.opcode == OpCodes.Brtrue || ci.opcode == OpCodes.Brtrue_S), })
            .ThrowIfInvalid("Could not find transpiler pattern")
            .Set(OpCodes.Pop, null);

            return codeMatcher.Instructions();
        }

        //Patch: Ensure BlibEnemy AnimateAttack moves correctly at high game speeds
        [HarmonyTranspiler, HarmonyPatch(typeof(BlibEnemy), nameof(BlibEnemy.AnimateAttack))]
        static IEnumerable<CodeInstruction> BlibEnemy_AnimateAttack(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchForward(true, new CodeMatch[]
            {
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(DG.Tweening.TweenExtensions),  nameof(DG.Tweening.TweenExtensions.Duration), new Type[] { typeof(DG.Tweening.Tween), typeof(bool) } )),
                new CodeMatch(OpCodes.Ldc_R4, 0.1f) })
            .ThrowIfInvalid("Could not find transpiler pattern")
            .RemoveInstructions(1)
            //.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .Insert(new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldc_R4, 0.1f),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Enemy), nameof(Enemy.AttackAnimTime), new Type[] { typeof(float) }))
            );

            return codeMatcher.Instructions();
        }

        //Patch: Applies game speed setting to ResetCameraEvent
        [HarmonyPrefix, HarmonyPatch(typeof(ResetCameraEvent), nameof(ResetCameraEvent.Execute))]
        static void ResetCameraEvent_Execute(ResetCameraEvent __instance)
        {
            __instance.transition_time = 0.5f * __instance.app.model.enemyAnimationSpeed;
        }
    }
}
