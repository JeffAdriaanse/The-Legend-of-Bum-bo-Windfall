using DG.Tweening;
using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class BumboModifier : MonoBehaviour
    {
        public bool displayInitialized = false;

        static Color hiddenTintColor = new Color(0.2f, 0.2f, 0.2f);

        private static readonly float removalHorizontalOffset = 0.6f;
        private static readonly float hidingHorizontalOffset = 0.3f;
        private static readonly float punchScale = 0.012f;
        private static readonly float tweenDuration = 0.3f;

        public static readonly Vector3 baseDisplayPosition = new Vector3(-0.54f, 0.26f, 1.17f);
        static readonly float displayIndexOffset = 0.2f;

        public enum ModifierCategory
        {
            None,
            Block,
            Retaliate,
            Dodge,
        }

        public enum ValueDisplayType
        {
            None,
            Standard,
            Hurt,
        }

        public CharacterSheet.BumboModifierObject.ModifierType modifierType;
        public ModifierCategory modifierCategory;

        public string source;
        public SpellName spellSource;
        public TrinketName trinketSource;

        public string value;
        public ValueDisplayType valueDisplayType;

        public bool canStack;

        public string iconObjectName;

        public int index;

        public ObjectTinter objectTinter;

        public BoxCollider boxCollider;

        public Transform modifierDisplayCollectibleTransform;
        public Transform modifierDisplayBackTransform;
        public Transform modifierDisplayIconTransform;
        public Transform modifierDisplayValueTransform;
        public Transform effectValueTransform;
        public Transform timerTransform;
        public Transform stackingTransform;

        public Transform modifierDisplayIconObjectTransform;

        public Sequence movementSequence;
        public Sequence updateSequence;

        public bool Expanded()
        {
            return WindfallPersistentDataController.LoadData().expandModifiers;
        }
        Vector3 TargetPosition()
        {
            return Expanded() ? ShowingPosition() : HidingPosition();
        }
        Vector3 ShowingPosition()
        {
            return baseDisplayPosition + new Vector3(0, -displayIndexOffset * index, 0);
        }
        Vector3 HidingPosition()
        {
            return ShowingPosition() + new Vector3(-hidingHorizontalOffset, 0, 0);
        }
        Vector3 RemovalPosition()
        {
            return ShowingPosition() + new Vector3(-removalHorizontalOffset, 0, 0);
        }

        public Vector3 TooltipPosition()
        {
            return transform.parent.TransformPoint(transform.localPosition + new Vector3(0.07f, -0.01f, 0f));
            //return transform.parent.TransformPoint(TargetPosition() + new Vector3(0.04f, -0.01f, 0f));
        }

        public void UpdateTint()
        {
            if (objectTinter == null)
            {
                return;
            }

            if (Expanded())
            {
                objectTinter.NoTint();
                return;
            }
            objectTinter.Tint(hiddenTintColor);
        }

        public void AddModifierAnimation()
        {
            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(false);
            }

            transform.localPosition = RemovalPosition();

            movementSequence = DOTween.Sequence();
            movementSequence.Append(transform.DOLocalMove(TargetPosition(), tweenDuration).SetEase(Ease.InOutQuad));

            UpdateTint();
        }

        public void RemoveModifierAnimation()
        {
            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(false);
            }

            movementSequence.Append(transform.DOLocalMove(RemovalPosition(), tweenDuration).SetEase(Ease.InOutQuad));
            movementSequence.AppendCallback(delegate
            {
                UnityEngine.Object.Destroy(gameObject);
            });
        }

        public void MoveModifierAnimation()
        {
            Vector3 targetPosition = TargetPosition();
            if (targetPosition == Vector3.zero || targetPosition == transform.localPosition)
            {
                return;
            }

            if (movementSequence != null && movementSequence.IsPlaying())
            {
                movementSequence.Kill(false);
            }
            movementSequence = DOTween.Sequence();

            movementSequence.Append(transform.DOLocalMove(targetPosition, tweenDuration).SetEase(Ease.InOutQuad));
            movementSequence.AppendCallback(delegate { UpdateTint(); });
        }

        public void UpdateModifierAnimation()
        {
            if (updateSequence != null && updateSequence.IsPlaying())
            {
                updateSequence.Kill(true);
            }

            updateSequence = DOTween.Sequence();
            updateSequence.Append(transform.DOPunchScale(new Vector3(punchScale, punchScale, punchScale), tweenDuration, 1, 0));
        }

        public string Description()
        {
            if (value == null)
            {
                return "";
            }

            string description = string.Empty;

            switch (modifierCategory)
            {
                case ModifierCategory.Block:
                    description = LocalizationModifier.GetLanguageText("BLOCK_STATUS", "Indicators");
                    description = description.Replace("[value]", value);
                    //Handle English pluralization
                    if (value == "1" && LocalizationManager.CurrentLanguage == "English")
                    {
                        description = description.Replace("attacks", "attack");
                        description = description.Replace("1", "an");
                    }
                    break;
                case ModifierCategory.Dodge:
                    description = LocalizationModifier.GetLanguageText("DODGE_STATUS", "Indicators");
                    description = description.Replace("[value]", value);
                    break;
                case ModifierCategory.Retaliate:
                    description = LocalizationModifier.GetLanguageText("RETALIATE_STATUS", "Indicators");
                    description = description.Replace("[value]", value);
                    break;
            }

            switch (source)
            {
                case "actionPointModifier":
                    if (value.Contains("-"))
                    {
                        string displayValue = value;
                        int minusIndex = displayValue.IndexOf("-");
                        if (minusIndex >= 0) displayValue = displayValue.Remove(minusIndex, 1);

                        description = LocalizationModifier.GetLanguageText("ACTION_POINT_LOSS_STATUS", "Indicators");
                        description = description.Replace("[value]", value);

                        if (value == "1" && LocalizationManager.CurrentLanguage == "English") description = description.Replace("moves", "move");
                    }
                    else
                    {
                        description = LocalizationModifier.GetLanguageText("ACTION_POINT_GAIN_STATUS", "Indicators");
                        description = description.Replace("[value]", value);

                        if (value == "1" && LocalizationManager.CurrentLanguage == "English") description = description.Replace("moves", "move");
                    }

                    break;
            }

            if (description == string.Empty)
            {
                string displayValue = value;
                switch (spellSource)
                {
                    case SpellName.BrownBelt:
                        description = "BROWN_BELT_STATUS";
                        break;
                    case SpellName.BlindRage:
                        description = "BLIND_RAGE_STATUS";
                        displayValue = displayValue.Remove(0, 1);
                        break;
                    case SpellName.Euthanasia:
                        description = "EUTHANASIA_STATUS";
                        break;
                    case SpellName.LooseChange:
                        description = "LOOSE_CHANGE_STATUS";
                        break;
                    case SpellName.Pause:
                        description = "PAUSE_STATUS";
                        break;
                    case SpellName.RoidRage:
                        description = "ROID_RAGE_STATUS";
                        break;
                    case SpellName.StopWatch:
                        description = "STOP_WATCH_STATUS";
                        break;
                    case SpellName.TheVirus:
                        description = "THE_VIRUS_STATUS";
                        break;
                    case SpellName.TwentyTwenty:
                        description = "TWENTY_TWENTY_STATUS";
                        break;
                    case SpellName.WhiteBelt:
                        description = "WHITE_BELT_STATUS";
                        break;
                    case SpellName.WoodenSpoon:
                        description = "WOODEN_SPOON_STATUS";
                        break;
                }

                description = LocalizationModifier.GetLanguageText(description, "Indicators");
                description = description.Replace("[value]", displayValue);
            }

            return description;
        }

        public string StackingDescription()
        {
            if (value == null)
            {
                return string.Empty;
            }

            string stacking = "STACKING_STATUS";

            switch (spellSource)
            {
                case SpellName.BarbedWire:
                    stacking = "STACKING_DAMAGE_STATUS";
                    break;
                case SpellName.OrangeBelt:
                    stacking = "STACKING_DAMAGE_STATUS";
                    break;
                case SpellName.YellowBelt:
                    stacking = "STACKING_DODGE_STATUS";
                    break;
            }

            stacking = LocalizationModifier.GetLanguageText(stacking, "Indicators");
            string stackingCap = CollectibleChanges.PercentSpellEffectStackingCap(spellSource).ToString();
            stacking = stacking.Replace("[value]", stackingCap);
            return stacking;
        }

        public void Init(string _source, SpellName _spellSource, TrinketName _trinketSource, string _value, int _index)
        {
            if (_source != null && _source != string.Empty)
            {
                switch (_source)
                {
                    case "actionPointModifier":
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = true;
                        iconObjectName = "MoveChange";
                        break;
                }

                source = _source;
            }
            else if (_spellSource != SpellName.None)
            {
                switch (_spellSource)
                {
                    case SpellName.BarbedWire:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.Hurt;
                        canStack = true;
                        iconObjectName = "Thorns";
                        break;
                    case SpellName.BlindRage:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = true;
                        iconObjectName = "Vulnerable";
                        break;
                    case SpellName.BrownBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.Hurt;
                        canStack = false;
                        iconObjectName = "ShieldThorns";
                        break;
                    case SpellName.Euthanasia:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.Hurt;
                        canStack = false;
                        iconObjectName = "Thorns";
                        break;
                    case SpellName.LooseChange:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = false;
                        iconObjectName = "HurtCoin";
                        break;
                    case SpellName.OldPillow:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "Shield";
                        break;
                    case SpellName.OrangeBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.Hurt;
                        canStack = true;
                        iconObjectName = "Thorns";
                        break;
                    case SpellName.Pause:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "Slow";
                        break;
                    case SpellName.RoidRage:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = false;
                        iconObjectName = "Critical";
                        break;
                    case SpellName.SmokeMachine:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Dodge;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = false;
                        iconObjectName = "Dodge";
                        break;
                    case SpellName.StopWatch:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "Slow";
                        break;
                    case SpellName.TheVirus:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.Retaliate;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "Thorns";
                        break;
                    case SpellName.TrashLid:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Block;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "DoubleShield";
                        break;
                    case SpellName.TwentyTwenty:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "ComboMultiplier";
                        break;
                    case SpellName.WhiteBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Round;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.None;
                        canStack = false;
                        iconObjectName = "StatusProtection";
                        break;
                    case SpellName.WoodenSpoon:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.None;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = true;
                        iconObjectName = "MoveGain";
                        break;
                    case SpellName.YellowBelt:
                        modifierType = CharacterSheet.BumboModifierObject.ModifierType.Room;
                        modifierCategory = ModifierCategory.Dodge;
                        valueDisplayType = ValueDisplayType.Standard;
                        canStack = true;
                        iconObjectName = "Dodge";
                        break;
                }
                spellSource = _spellSource;
            }
            else if (_trinketSource != TrinketName.None)
            {
                trinketSource = _trinketSource;
            }

            value = _value;
            index = _index;
        }
    }

    class BumboModifierTemporary : MonoBehaviour
    {
        public BumboModifier bumboModifier;
        public readonly string description = "TEMPORARY_STATUS";

        public Vector3 TooltipPosition()
        {
            return bumboModifier.TooltipPosition();
        }

        public static List<SpellName> TemporarySpellsources
        {
            get
            {
                List<SpellName> effects = new List<SpellName>
                {
                    SpellName.LooseChange,
                    SpellName.OrangeBelt,
                    SpellName.Pause,
                    SpellName.SmokeMachine,
                    SpellName.StopWatch,
                    SpellName.TheVirus,
                    SpellName.TwentyTwenty,
                    SpellName.WhiteBelt,
                };

                if (!WindfallPersistentDataController.LoadData().implementBalanceChanges)
                {
                    effects.Add(SpellName.BarbedWire);
                    effects.Add(SpellName.RoidRage);
                    effects.Remove(SpellName.TheVirus);
                }

                return effects;
            }
        }
    }

    class BumboModifierStacking : MonoBehaviour
    {
        public BumboModifier bumboModifier;

        public Vector3 TooltipPosition()
        {
            return bumboModifier.TooltipPosition();
        }
    }

}
