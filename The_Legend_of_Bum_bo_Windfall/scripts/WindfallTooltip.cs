using HarmonyLib;
using I2.Loc;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    class WindfallTooltip : MonoBehaviour
    {
        public enum Anchor
        {
            Center,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left,
            TopLeft,
        }

        public bool displayAtMouse;
        public Vector3 displayPosition;
        public Anchor displayAnchor;
        public string displayDescription;

        public bool active;

        public bool displayGamepad;

        public static readonly Color entityHoverTintColor = new Color(0.6f, 0.6f, 0.6f);

        public void UpdateDisplayType(bool gamepad)
        {
            displayGamepad = gamepad;
            UpdateDisplayData();
        }

        public void UpdateDisplayData()
        {
            active = true;

            BumboModifier bumboModifier = gameObject.GetComponent<BumboModifier>();
            if (bumboModifier != null)
            {
                if (!bumboModifier.Expanded())
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bumboModifier.TooltipPosition();
                    displayAnchor = Anchor.Right;
                }

                displayDescription = bumboModifier.Description();
                return;
            }

            BumboModifierTemporary bumboModifierTemporary = gameObject.GetComponent<BumboModifierTemporary>();
            if (bumboModifierTemporary != null)
            {
                if (!bumboModifierTemporary.bumboModifier.Expanded())
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bumboModifierTemporary.bumboModifier.TooltipPosition();
                    displayAnchor = Anchor.Right;
                }

                displayDescription = LocalizationModifier.GetLanguageText(bumboModifierTemporary.description, "Indicators");
                return;
            }

            BumboModifierStacking bumboModifierStacking = gameObject.GetComponent<BumboModifierStacking>();
            if (bumboModifierStacking != null)
            {
                if (!bumboModifierStacking.bumboModifier.Expanded())
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bumboModifierStacking.bumboModifier.TooltipPosition();
                    displayAnchor = Anchor.Right;
                }

                displayDescription = bumboModifierStacking.bumboModifier.StackingDescription();
                return;
            }

            SpellView spellView = gameObject.GetComponent<SpellView>();
            if (spellView != null)
            {
                SpellElement spell = spellView.SpellObject;
                if (spell == null || spell.spellName == SpellName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = spellView.transform.position + new Vector3(-0.43f, 0f, 0f);
                    displayAnchor = Anchor.Left;
                }

                displayDescription = string.Empty;
                if (WindfallHelper.app.model.spellModel.spellKA.TryGetValue(spell.spellName, out string spellKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetLanguageText(spellKA, "Spells") + "</u>\n" + WindfallTooltipDescriptions.SpellDescriptionWithValues(spell);
                }
                return;
            }

            SpellViewIndicator spellViewIndicator = gameObject.GetComponent<SpellViewIndicator>();
            if (spellViewIndicator != null)
            {
                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = spellViewIndicator.transform.position + new Vector3(-0.02f, 0.05f, 0f);
                    displayAnchor = Anchor.Top;
                }

                displayDescription = spellViewIndicator.TooltipDescription();
            }

            SpellPickup spellPickup = gameObject.GetComponent<SpellPickup>();
            if (spellPickup != null)
            {
                SpellElement spell = spellPickup.spell;
                if (spell == null || spell.spellName == SpellName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = !displayGamepad;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = spellPickup.transform.Find("Spell Pickup Object").Find("Spell_Holder").GetComponent<MeshRenderer>().bounds.center - spellPickup.transform.right * 0.3f;
                    displayAnchor = Anchor.Right;
                }

                displayDescription = string.Empty;
                if (WindfallHelper.app.model.spellModel.spellKA.TryGetValue(spell.spellName, out string spellKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetLanguageText(spellKA, "Spells") + "</u>\n" + WindfallTooltipDescriptions.SpellDescriptionWithValues(spell);
                }

                return;
            }

            TrinketView trinketView = gameObject.GetComponent<TrinketView>();
            if (trinketView != null)
            {
                if (trinketView.trinketIndex >= WindfallHelper.app.model.characterSheet.trinkets.Count)
                {
                    active = false;
                    return;
                }

                TrinketElement trinket = WindfallHelper.app.controller.GetTrinket(trinketView.trinketIndex);
                if (trinket == null || trinket.trinketName == TrinketName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = trinketView.transform.position + new Vector3(-0.074f, 0.14f, 0f);
                    displayAnchor = Anchor.Top;
                }

                displayDescription = string.Empty;
                if (WindfallHelper.app.model.trinketModel.trinketKA.TryGetValue(trinket.trinketName, out string trinketKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetLanguageText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
                }
                return;
            }

            if (gameObject.transform.parent != null)
            {
                TrinketView trinketViewParent = gameObject.transform.parent.GetComponent<TrinketView>();
                if (gameObject.name == "GlitchVisualObject" && trinketViewParent != null && trinketViewParent.trinketIndex < WindfallHelper.app.model.characterSheet.trinkets.Count)
                {
                    TrinketElement trinket = WindfallHelper.app.model.characterSheet.trinkets[trinketViewParent.trinketIndex];
                    if (trinket == null || trinket.trinketName != TrinketName.Glitch)
                    {
                        active = false;
                        return;
                    }

                    displayAtMouse = false;

                    if (displayAtMouse)
                    {
                        displayPosition = Vector3.zero;
                        displayAnchor = Anchor.TopRight;
                    }
                    else
                    {
                        displayPosition = trinketViewParent.transform.position + new Vector3(-0.074f, 0.14f, 0f);
                        displayAnchor = Anchor.Top;
                    }

                    displayDescription = string.Empty;
                    if (WindfallHelper.app.model.trinketModel.trinketKA.TryGetValue(trinket.trinketName, out string trinketKA))
                    {
                        displayDescription = "<u>" + LocalizationModifier.GetLanguageText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
                    }
                    return;
                }
            }

            TrinketPickupView trinketPickupView = gameObject.GetComponent<TrinketPickupView>();
            if (trinketPickupView != null)
            {
                TrinketElement trinket = trinketPickupView.trinket;
                if (trinket == null || trinket.trinketName == TrinketName.None)
                {
                    active = false;
                    return;
                }

                displayAtMouse = !displayGamepad;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = trinketPickupView.transform.Find("Trinket_Pickup").GetComponent<MeshRenderer>().bounds.center - trinketPickupView.transform.right * 0.3f;
                    displayAnchor = Anchor.Right;
                }

                displayDescription = string.Empty;
                if (WindfallHelper.app.model.trinketModel.trinketKA.TryGetValue(trinket.trinketName, out string trinketKA))
                {
                    displayDescription = "<u>" + LocalizationModifier.GetLanguageText(trinketKA, "Trinkets") + "</u>\n" + WindfallTooltipDescriptions.TrinketDescriptionWithValues(trinket);
                }

                return;
            }

            BumboFacesController bumboFacesController = gameObject.GetComponent<BumboFacesController>();
            if (bumboFacesController != null)
            {
                displayAtMouse = false;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bumboFacesController.transform.position + new Vector3(0.08f, 0f, 0f);
                    displayAnchor = Anchor.Right;
                }

                displayDescription = string.Empty;

                string bumboName = string.Empty;
                string bumboDescription = string.Empty;

                CharacterSheet.BumboType bumboType = WindfallHelper.app.model.characterSheet.bumboType;
                if (WindfallTooltipDescriptions.bumboNames.TryGetValue(bumboType, out string name)) bumboName = LocalizationModifier.GetLanguageText(name, "Characters");
                //Vanilla Bum-bo names have a newline character instead of one of the spaces. This is not desirable for tooltips, so the newline is replaced with a space.
                bumboName = bumboName.Replace("\n", " ");
                if (WindfallTooltipDescriptions.bumboDescriptions.TryGetValue(bumboType, out string description)) bumboDescription = LocalizationModifier.GetLanguageText(description, "Characters");

                displayDescription = "<u>" + bumboName + "</u>\n" + bumboDescription;
                return;
            }

            BloodShieldEffectView bloodShieldEffectView = gameObject.GetComponent<BloodShieldEffectView>();
            if (bloodShieldEffectView == null) bloodShieldEffectView = ObjectDataStorage.GetData<object>(gameObject, EntityChanges.colliderEntityKey) as BloodShieldEffectView;
            if (bloodShieldEffectView != null)
            {
                displayAtMouse = !displayGamepad;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = bloodShieldEffectView.transform.position - bloodShieldEffectView.transform.right * 0.3f;
                    displayAnchor = Anchor.Right;
                }

                //Tint effect
                ObjectTinter objectTinter = bloodShieldEffectView.GetComponent<ObjectTinter>();
                if (objectTinter == null) objectTinter = bloodShieldEffectView.gameObject.AddComponent<ObjectTinter>();
                if (objectTinter != null)
                {
                    if (!(bool)AccessTools.Field(typeof(ObjectTinter), "tinted").GetValue(objectTinter)) objectTinter.Tint(entityHoverTintColor);
                }

                displayDescription = "<u>" + LocalizationModifier.GetLanguageText("BLOOD_SHIELD_EFFECT_NAME", "Enemies") + "</u>\n" + LocalizationModifier.GetLanguageText("BLOOD_SHIELD_EFFECT_ABILITY", "Enemies");
            }

            FogEffectView fogEffectView = gameObject.GetComponent<FogEffectView>();
            if (fogEffectView == null) fogEffectView = ObjectDataStorage.GetData<object>(gameObject, EntityChanges.colliderEntityKey) as FogEffectView;
            if (fogEffectView != null)
            {
                displayAtMouse = !displayGamepad;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = fogEffectView.transform.position - fogEffectView.transform.right * 0.3f;
                    displayAnchor = Anchor.Right;
                }

                //Tint effect
                ObjectTinter objectTinter = fogEffectView.GetComponent<ObjectTinter>();
                if (objectTinter == null) objectTinter = fogEffectView.gameObject.AddComponent<ObjectTinter>();
                if (objectTinter != null)
                {
                    if (!(bool)AccessTools.Field(typeof(ObjectTinter), "tinted").GetValue(objectTinter)) objectTinter.Tint(entityHoverTintColor);
                }

                displayDescription = "<u>" + LocalizationModifier.GetLanguageText("FOG_EFFECT_NAME", "Enemies") + "</u>\n" + LocalizationModifier.GetLanguageText("FOG_EFFECT_ABILITY", "Enemies");
            }

            Enemy enemy = gameObject.GetComponent<Enemy>();
            if (enemy == null) enemy = ObjectDataStorage.GetData<object>(gameObject, EntityChanges.colliderEntityKey) as Enemy;
            if (enemy != null)
            {
                if (enemy is BygoneBoss && !enemy.alive)
                {
                    active = false;
                    return;
                }

                displayAtMouse = !displayGamepad;

                if (displayAtMouse)
                {
                    displayPosition = Vector3.zero;
                    displayAnchor = Anchor.TopRight;
                }
                else
                {
                    displayPosition = enemy.transform.position - enemy.transform.right * 0.3f;
                    displayAnchor = Anchor.Right;
                }

                //Movement
                string movesText = "\n" + LocalizationModifier.GetLanguageText("ACTIONS", "Enemies");
                movesText = movesText.Replace("[value]", enemy.turns.ToString());

                //Damage
                string damageText = "\n" + LocalizationModifier.GetLanguageText("DAMAGE", "Enemies");

                string damageValueText = "1";
                int damage = 1;
                if (enemy.berserk || enemy.brieflyBerserk == 1)
                {
                    damage++;
                }
                if (enemy.championType == Enemy.ChampionType.FullHeart)
                {
                    damage++;
                }
                switch (damage)
                {
                    default:
                        damageValueText = "1/2";
                        break;
                    case 2:
                        damageValueText = "1";
                        break;
                    case 3:
                        damageValueText = "&lt;nobr&gt;1 + 1/2&lt;nobr&gt;";
                        break;
                }

                damageText = damageText.Replace("[value]", damageValueText);

                //Enemy names
                string localizationCategory = enemy is Boss ? "Bosses" : "Enemies";
                string enemyNameText = LocalizationModifier.GetLanguageText(WindfallTooltipDescriptions.EnemyDisplayName(enemy), localizationCategory);

                //Resistances
                string resitanceText = string.Empty;
                switch (enemy.attackImmunity)
                {
                    case Enemy.AttackImmunity.ReducePuzzleDamage:
                        resitanceText = "\n" + LocalizationModifier.GetLanguageText("PUZZLE_DAMAGE_REDUCTION_ABILITY", "Enemies");
                        break;
                    case Enemy.AttackImmunity.ReduceSpellDamage:
                        resitanceText = "\n" + LocalizationModifier.GetLanguageText("SPELL_DAMAGE_REDUCTION_ABILITY", "Enemies");
                        break;
                }
                resitanceText = resitanceText.Replace("[damage]", "1");

                //Invincibility
                string invincibilityText = WindfallTooltipDescriptions.EnemyIsInvincible(enemy) ? "\n" + LocalizationModifier.GetLanguageText("INVULNERABLE_ABILITY", "Enemies") : string.Empty;

                //Damage reduction
                string damageReductionText = WindfallTooltipDescriptions.EnemyDamageReductionWithValues(enemy);

                //Blocking
                string blockText = WindfallTooltipDescriptions.EnemyIsBlocking(enemy) ? "\n" + LocalizationModifier.GetLanguageText("BLOCK_ABILITY", "Enemies") : string.Empty;

                //Enemy descriptions
                string descriptionText = WindfallTooltipDescriptions.EnemyDisplayDescription(enemy);

                //Champion crowns
                string championText = string.Empty;
                if (enemy.championType == Enemy.ChampionType.ManaDrain) championText = "\n" + LocalizationModifier.GetLanguageText("BROWN_CHAMPION_ABILITY", "Enemies");
                else if (enemy.championType == Enemy.ChampionType.Cursed) championText = "\n" + LocalizationModifier.GetLanguageText("PURPLE_CHAMPION_ABILITY", "Enemies");
                else if (enemy.championType == Enemy.ChampionType.Regen) championText = "\n" + LocalizationModifier.GetLanguageText("RED_CHAMPION_ABILITY", "Enemies");
                else if (enemy.championType == Enemy.ChampionType.DeathDamage) championText = "\n" + LocalizationModifier.GetLanguageText("GREEN_CHAMPION_ABILITY", "Enemies");

                //Omit irrelevant tooltip information
                if (!enemy.alive)
                {
                    if (enemy is not StonyEnemy)
                    {
                        movesText = string.Empty;
                        damageText = string.Empty;
                    }
                }
                if (WindfallTooltipDescriptions.nonAttackingEnemies.Contains(enemy.enemyName) || enemy.gameObject.name.Contains("Tainted Shy Gal Mimic 2"))
                {
                    damageText = string.Empty;
                }

                //Tint enemy
                ObjectTinter objectTinter = enemy.objectTinter;
                if (objectTinter != null)
                {
                    if (!(bool)AccessTools.Field(typeof(ObjectTinter), "tinted").GetValue(objectTinter)) objectTinter.Tint(entityHoverTintColor);
                }

                //Output description
                displayDescription = "<u>" + enemyNameText + "</u>" + movesText + damageText + invincibilityText + resitanceText + damageReductionText + blockText + descriptionText + championText;
                return;
            }
        }
    }

    public static class WindfallTooltipDescriptions
    {

        public static readonly Dictionary<CharacterSheet.BumboType, string> bumboNames = new Dictionary<CharacterSheet.BumboType, string>
        {
            { CharacterSheet.BumboType.TheBrave, "BRAVE_NAME" },
            { CharacterSheet.BumboType.TheNimble, "NIMBLE_NAME" },
            { CharacterSheet.BumboType.TheStout, "STOUT_NAME" },
            { CharacterSheet.BumboType.TheWeird, "WEIRD_NAME" },
            { CharacterSheet.BumboType.TheDead, "DEAD_NAME" },
            { CharacterSheet.BumboType.TheLost, "LOST_NAME" },
            { CharacterSheet.BumboType.Eden, "EMPTY_NAME" },
            { (CharacterSheet.BumboType) 10, "WISE_NAME" },
        };

        public static readonly string WISE_DESCRIPTION = "WISE_TOOLTIP_DESCRIPTION";
        public static Dictionary<CharacterSheet.BumboType, string> bumboDescriptions = new Dictionary<CharacterSheet.BumboType, string>
        {
            { CharacterSheet.BumboType.TheBrave, "BRAVE_TOOLTIP_DESCRIPTION" },
            { CharacterSheet.BumboType.TheNimble, "NIMBLE_TOOLTIP_DESCRIPTION" },
            { CharacterSheet.BumboType.TheStout, "STOUT_TOOLTIP_DESCRIPTION" },
            { CharacterSheet.BumboType.TheWeird, "WEIRD_TOOLTIP_DESCRIPTION" },
            { CharacterSheet.BumboType.TheDead, "DEAD_TOOLTIP_DESCRIPTION" },
            { CharacterSheet.BumboType.TheLost, "LOST_TOOLTIP_DESCRIPTION" },
            { CharacterSheet.BumboType.Eden, "EMPTY_TOOLTIP_DESCRIPTION" },
            { (CharacterSheet.BumboType) 10, WISE_DESCRIPTION },
        };

        public static string SpellDescriptionWithValues(SpellElement spell)
        {
            string tooltipDescriptionTerm = spell.Name;
            int lastUnderscoreIndex = tooltipDescriptionTerm.LastIndexOf("_");
            tooltipDescriptionTerm = tooltipDescriptionTerm.Substring(0, lastUnderscoreIndex);
            tooltipDescriptionTerm = tooltipDescriptionTerm + "_TOOLTIP_DESCRIPTION";

            string value = LocalizationModifier.GetLanguageText(tooltipDescriptionTerm, "Spells");

            if (value != string.Empty)
            {
                CharacterSheet characterSheet = WindfallHelper.app?.model?.characterSheet;
                switch (spell.spellName)
                {
                    case (SpellName)1000:
                        if (spell is PlasmaBallSpell plasmaBallSpell)
                        {
                            value = value.Replace("[spread]", plasmaBallSpell.ChainDistance().ToString());
                        }
                        break;
                    case SpellName.BrownBelt:
                        if (characterSheet != null)
                        {
                            value = value.Replace("[damage]", characterSheet.getItemDamage().ToString());
                        }
                        break;
                    case SpellName.D10:
                        string grounded = string.Empty;
                        string flying = string.Empty;
                        if (characterSheet != null)
                        {
                            switch (characterSheet.currentFloor)
                            {
                                case 2:
                                    grounded = LocalizationModifier.GetLanguageText("D10_TOOLTIP_GROUNDED_2", "Spells");
                                    flying = LocalizationModifier.GetLanguageText("D10_TOOLTIP_FLYING_2", "Spells");
                                    break;
                                case 3:
                                    grounded = LocalizationModifier.GetLanguageText("D10_TOOLTIP_GROUNDED_3", "Spells");
                                    flying = LocalizationModifier.GetLanguageText("D10_TOOLTIP_FLYING_3", "Spells");
                                    break;
                                default:
                                    grounded = LocalizationModifier.GetLanguageText("D10_TOOLTIP_GROUNDED_1", "Spells");
                                    flying = LocalizationModifier.GetLanguageText("D10_TOOLTIP_FLYING_1", "Spells");
                                    break;
                            }
                        }
                        value = value.Replace("[grounded]", grounded);
                        value = value.Replace("[flying]", flying);
                        break;
                    case SpellName.Euthanasia:
                        value = value.Replace("[damage]", "5");
                        break;
                    case SpellName.ExorcismKit:
                        string healing = "1";
                        value = value.Replace("[healing]", healing);
                        break;
                    case SpellName.RockFriends:
                        if (characterSheet != null)
                        {
                            int itemDamage = characterSheet.getItemDamage() + 1;
                            value = value.Replace("[count]", itemDamage.ToString());

                            //Handle English pluralization
                            if (value == "1" && LocalizationManager.CurrentLanguage == "English") value = value.Replace("rocks on random enemies, each", "rock on a random enemy,");
                        }
                        break;
                }

                value = value.Replace("[damage]", spell.Damage().ToString());
                value = value.Replace("[stacking]", CollectibleChanges.PercentSpellEffectStackingCap(spell.spellName).ToString());
                return value;
            }
            return string.Empty;
        }

        public static string TrinketDescriptionWithValues(TrinketElement trinket)
        {
            string tooltipDescriptionTerm = trinket.Name;
            int lastUnderscoreIndex = tooltipDescriptionTerm.LastIndexOf("_");
            tooltipDescriptionTerm = tooltipDescriptionTerm.Substring(0, lastUnderscoreIndex);
            tooltipDescriptionTerm = tooltipDescriptionTerm + "_TOOLTIP_DESCRIPTION";

            string value = LocalizationModifier.GetLanguageText(tooltipDescriptionTerm, "Trinkets");

            if (value != null) return value;
            return string.Empty;
        }

        public static string EnemyDisplayName(Enemy enemy)
        {
            if (enemy == null) { return string.Empty; }

            //Get boss
            Boss boss = null;
            if (enemy is Boss) boss = enemy as Boss;

            //Enemy names
            string enemyNameText = string.Empty;
            if (enemyNameText == string.Empty)
            {
                //Enemy names from name
                if (enemyDisplayNamesByEnemyName.TryGetValue(enemy.enemyName, out string enemyNameFromName)) enemyNameText = enemyNameFromName;
            }
            if (enemyNameText == string.Empty)
            {
                //Boss names from name
                if (boss != null && bossDisplayNamesByBossName.TryGetValue((enemy as Boss).bossName, out string bossNameFromName)) enemyNameText = bossNameFromName;
            }
            if (enemyNameText == string.Empty)
            {
                //Enemy and Boss names from type
                if (enemyDisplayNamesByType.TryGetValue(enemy.GetType(), out string enemyNameFromType)) enemyNameText = enemyNameFromType;
            }

            //Get Flipper
            if (enemy is FlipperEnemy) enemyNameText = enemy.attackImmunity == Enemy.AttackImmunity.ReducePuzzleDamage ? "NIB_NAME" : "JIB_NAME";

            //Get Bygone Ghost
            if (enemy.gameObject.name.Contains("Bygone Ghost")) enemyNameText = "BYGONE_GHOST_NAME";

            return enemyNameText;
        }

        private static readonly Dictionary<EnemyName, string> enemyDisplayNamesByEnemyName = new Dictionary<EnemyName, string>
        {
            { EnemyName.Arsemouth, "TALL_BOY_NAME" },
            { EnemyName.BlackBlobby, "BLACK_BLOBBY_NAME" },
            { EnemyName.Blib, "BLIB_NAME" },
            { EnemyName.BlueBoney, "SKULLY_B_NAME" },
            { EnemyName.BoomFly, "BOOM_FLY_NAME" },
            { EnemyName.Burfer, "BURFER_NAME" },
            { EnemyName.Butthead, "SQUAT_NAME" },
            { EnemyName.CornyDip, "CORN_DIP_NAME" },
            { EnemyName.Curser, "CURSER_NAME" },
            { EnemyName.DigDig, "DIG_DIG_NAME" },
            { EnemyName.Dip, "DIP_NAME" },
            { EnemyName.Flipper, "FLIPPER_NAME" },//Missing
            { EnemyName.FloatingCultist, "FLOATER_NAME" },
            { EnemyName.Fly, "FLY_NAME" },
            { EnemyName.Greedling, "GREEDLING_NAME" },
            { EnemyName.GreenBlib, "GREEN_BLIB_NAME" },
            { EnemyName.GreenBlobby, "GREEN_BLOBBY_NAME" },
            { EnemyName.Hanger, "KEEPER_NAME" },
            { EnemyName.Hopper, "LEAPER_NAME" },
            { EnemyName.Host, "HOST_NAME" },
            //{ EnemyName.Imposter, "IMPOSTER_NAME" },
            { EnemyName.Isaacs, "ISAAC_NAME" },
            { EnemyName.Larry, "LARRY_NAME" },
            { EnemyName.Leechling, "SUCK_NAME" },
            { EnemyName.Longit, "LONGITS_NAME" },
            { EnemyName.ManaWisp, "MANA_WISP_NAME" },
            { EnemyName.MaskedImposter, "MASK_NAME" },
            { EnemyName.MeatGolem, "MEAT_GOLUM_NAME" },
            { EnemyName.MegaPoofer, "MEGA_POOFER_NAME" },
            { EnemyName.MirrorHauntLeft, "MIRROR_NAME" },
            { EnemyName.MirrorHauntRight, "MIRROR_NAME" },
            { EnemyName.PeepEye, "PEEPER_EYE_NAME" },
            { EnemyName.Poofer, "POOFER_NAME" },
            { EnemyName.Pooter, "POOTER_NAME" },
            { EnemyName.PurpleBoney, "SKULLY_P_NAME" },
            { EnemyName.RedBlobby, "RED_BLOBBY_NAME" },
            { EnemyName.RedCultist, "RED_FLOATER_NAME" },
            { EnemyName.Screecher, "SCREECHER_NAME" },
            { EnemyName.Shit, "POOP_NAME" },
            { EnemyName.Spookie, "SPOOKIE_NAME" },
            { EnemyName.Stone, "ROCK_NAME" },
            { EnemyName.Stony, "STONY_NAME" },
            { EnemyName.Sucker, "SUCKER_NAME" },
            { EnemyName.Tader, "DADDY_TATO_NAME" },
            { EnemyName.Tado, "TATO_KID_NAME" },
            { EnemyName.TaintedPeepEye, "TAINTED_PEEPER_EYE_NAME" },
            { EnemyName.Tutorial, "KEEPER_NAME" },
            { EnemyName.WalkingCultist, "CULTIST_NAME" },
            { EnemyName.WillOWisp, "WHISP_NAME" },
        };

        private static readonly Dictionary<BossName, string> bossDisplayNamesByBossName = new Dictionary<BossName, string>
        {
            { BossName.Bygone, "BYGONE_BODY_NAME" },
            { BossName.Duke, "DUKE_NAME" },
            { BossName.Dusk, "DUSK_NAME" },
            { BossName.Gibs, "GIBS_NAME" },
            { BossName.Gizzarda, "GIZZARDA_NAME" },
            { BossName.Loaf, "LOAF_NAME" },
            { BossName.Peeper, "PEEPER_NAME" },
            { BossName.Pyre, "PYRE_NAME" },
            { BossName.Sangre, "SANGRE_NAME" },
            { BossName.ShyGal, "SHY_GALS_NAME" },
            { BossName.TaintedDusk, "TAINTED_DUSK_NAME" },
            { BossName.TaintedPeeper, "TAINTED_PEEPER_NAME" },
            { BossName.TaintedShyGal, "TAINTED_SHY_GALS_NAME" },
        };

        private static readonly Dictionary<Type, string> enemyDisplayNamesByType = new Dictionary<Type, string>
        {
            //Enemies
            { typeof(ArsemouthEnemy), "TALL_BOY_NAME" },
            { typeof(BlackBlobbyEnemy), "BLACK_BLOBBY_NAME" },
            { typeof(BlibEnemy), "BLIB_NAME" },
            { typeof(BlueBoneyEnemy), "SKULLY_B_NAME" },
            { typeof(BoomFlyEnemy), "BOOM_FLY_NAME" },
            { typeof(BurferEnemy), "BURFER_NAME" },
            { typeof(ButtheadEnemy), "SQUAT_NAME" },
            //CornyDip
            { typeof(CurserEnemy), "CURSER_NAME" },
            { typeof(DigDigEnemy), "DIG_DIG_NAME" },
            { typeof(DipEnemy), "DIP_NAME" },
            { typeof(FlipperEnemy), "Flipper" },
            { typeof(FloatingCultistEnemy), "FLOATER_NAME" },
            { typeof(FlyEnemy), "FLY_NAME" },
            { typeof(GreedlingEnemy), "GREEDLING_NAME" },
            //GreenBlib
            { typeof(GreenBlobbyEnemy), "GREEN_BLOBBY_NAME" },
            { typeof(HangerEnemy), "KEEPER_NAME" },
            { typeof(HopperEnemy), "LEAPER_NAME" },
            { typeof(HostEnemy), "HOST_NAME" },
            { typeof(ImposterEnemy), "IMPOSTER_NAME" },
            { typeof(IsaacsEnemy), "ISAAC_NAME" },
            { typeof(LarryEnemy), "LARRY_NAME" },
            { typeof(LeecherEnemy), "SUCK_NAME" },
            { typeof(LongitEnemy), "LONGITS_NAME" },
            { typeof(ManaWispEnemy), "MANA_WISP_NAME" },
            { typeof(MaskedImposterEnemy), "MASK_NAME" },
            { typeof(MeatGolemEnemy), "MEAT_GOLUM_NAME" },
            { typeof(MegaPooferEnemy), "MEGA_POOFER_NAME" },
            { typeof(MirrorHauntEnemy), "MIRROR_NAME" },
            { typeof(PeepEyeEnemy), "PEEPER_EYE_NAME" },
            { typeof(PooferEnemy), "POOFER_NAME" },
            { typeof(PooterEnemy), "POOTER_NAME" },
            { typeof(PurpleBoneyEnemy), "SKULLY_P_NAME" },
            { typeof(RedBlobbyEnemy), "RED_BLOBBY_NAME" },
            { typeof(RedCultistEnemy), "RED_FLOATER_NAME" },
            { typeof(ScreecherEnemy), "SCREECHER_NAME" },
            { typeof(ShitEnemy), "POOP_NAME" },
            { typeof(SpookieEnemy), "SPOOKIE_NAME" },
            { typeof(StoneEnemy), "ROCK_NAME" },
            { typeof(StonyEnemy), "STONY_NAME" },
            { typeof(SuckerEnemy), "SUCKER_NAME" },
            { typeof(TaderEnemy), "DADDY_TATO_NAME" },
            { typeof(TadoEnemy), "TATO_KID_NAME" },
            //TaintedPeepEye
            { typeof(TutorialEnemy), "KEEPER_NAME" },
            { typeof(WalkingCultistEnemy), "CULTIST_NAME" },
            { typeof(WilloWispEnemy), "WHISP_NAME" },

            //Bosses
            { typeof(BygoneBoss), "BYGONE_BODY_NAME" },
            { typeof(BygoneGhostBoss), "BYGONE_GHOST_NAME" },
            { typeof(DukeBoss), "DUKE_NAME" },
            { typeof(DuskBoss), "DUSK_NAME" },
            { typeof(GibsBoss), "GIBS_NAME" },
            { typeof(GizzardaBoss), "GIZZARDA_NAME" },
            { typeof(LoafBoss), "LOAF_NAME" },
            { typeof(PeepsBoss), "PEEPER_NAME" },
            { typeof(PyreBoss), "PYRE_NAME" },
            { typeof(CaddyBoss), "SANGRE_NAME" },
            { typeof(ShyGalBoss), "SHY_GALS_NAME" },
            { typeof(TaintedDuskBoss), "TAINTED_DUSK_NAME" },
            //TaintedPeeper
            //TaintedShyGal
        };

        public static string EnemyDisplayDescription(Enemy enemy)
        {
            string localizationCategory = enemy is Boss ? "Bosses" : "Enemies";
            if (enemyDescriptions.TryGetValue(EnemyDisplayName(enemy), out List<string> value))
            {
                string fullDescription = string.Empty;
                foreach (string description in value) fullDescription += "\n" + LocalizationModifier.GetLanguageText(description, localizationCategory);
                return fullDescription;
            }
            return string.Empty;
        }

        private static readonly Dictionary<string, List<string>> enemyDescriptions = new Dictionary<string, List<string>>
        {
            //Enemies
            { "BLACK_BLOBBY_NAME", new List<string> { "BLACK_BLOBBY_ABILITY" } },
            { "BOOM_FLY_NAME", new List<string> { "BOOM_FLY_ABILITY" } },
            { "CULTIST_NAME", new List<string> { "CULTIST_ABILITY" } },
            { "DADDY_TATO_NAME", new List<string> { "DADDY_TATO_ABILITY", "DADDY_TATO_ABILITY_2" } },
            { "DIG_DIG_NAME", new List<string> { "DIG_DIG_ABILITY" } },
            { "GREEDLING_NAME", new List<string> { "GREEDLING_ABILITY" } },
            { "ISAAC_NAME", new List<string> { "ISAAC_ABILITY" } },
            { "JIB_NAME", new List<string> { "JIB_ABILITY" } },
            { "LARRY_NAME", new List<string> { "LARRY_ABILITY" } },
            { "LONGITS_NAME", new List<string> { "LONGITS_ABILITY" } },
            { "MANA_WISP_NAME", new List<string> { "MANA_WISP_ABILITY" } },
            { "MEAT_GOLUM_NAME", new List<string> { "MEAT_GOLUM_ABILITY", "MEAT_GOLUM_ABILITY_2" } },
            { "MEGA_POOFER_NAME", new List<string> { "MEGA_POOFER_ABILITY" } },
            { "NIB_NAME", new List<string> { "NIB_ABILITY" } },
            { "POOFER_NAME", new List<string> { "POOFER_ABILITY" } },
            { "RED_FLOATER_NAME", new List<string> { "RED_FLOATER_ABILITY" } },
            { "SPOOKIE_NAME", new List<string> { "SPOOKIE_ABILITY" } },
            { "SUCKER_NAME", new List<string> { "SUCKER_ABILITY" } },
            { "TATO_KID_NAME", new List<string> { "TATO_KID_ABILITY" } },

            //Bosses
            { "BYGONE_BODY_NAME", new List<string> { "BYGONE_BODY_ABILITY" } },
            { "BYGONE_GHOST_NAME", new List<string> { "BYGONE_GHOST_ABILITY" } },
            { "DUSK_NAME", new List<string> { "DUSK_ABILITY" } },
            { "GIBS_NAME", new List<string> { "GIBS_ABILITY" } },
            { "GIZZARDA_NAME", new List<string> { "GIZZARDA_ABILITY", "GIZZARDA_ABILITY_2" } },
            { "LOAF_NAME", new List<string> { "LOAF_ABILITY" } },
            { "PYRE_NAME", new List<string> { "PYRE_ABILITY" } },
            { "TAINTED_PEEPER_NAME", new List<string> { "TAINTED_PEEPER_ABILITY" } },
            { "TAINTED_DUSK_NAME", new List<string> { "TAINTED_DUSK_ABILITY" } },
        };

        public static string EnemyDamageReductionWithValues(Enemy enemy)
        {
            if (damageReductionEnemies.Contains(enemy.GetType()))
            {
                int damageReduction = 1;

                if (enemy is DukeBoss)
                {
                    DukeBoss dukeBoss = enemy as DukeBoss;
                    DukeBoss.DukeSize dukeSize = (DukeBoss.DukeSize)AccessTools.Field(typeof(DukeBoss), "dukeSize").GetValue(dukeBoss);

                    if (dukeSize == DukeBoss.DukeSize.Large) damageReduction = 1;
                    else if (dukeSize == DukeBoss.DukeSize.Medium) damageReduction = 2;
                    else return string.Empty;
                }

                string damageReductionWithValue = LocalizationModifier.GetLanguageText("DAMAGE_REDUCTION_ABILITY", "Enemies");
                damageReductionWithValue = damageReductionWithValue.Replace("[damage]", damageReduction.ToString());
                return "\n" + damageReductionWithValue;
            }
            return string.Empty;
        }

        private static readonly List<Type> damageReductionEnemies = new List<Type>
        {
            //Enemies
            typeof(SpookieEnemy),

            //Bosses
            typeof(BygoneGhostBoss),
            typeof(DukeBoss),
        };

        public static readonly List<EnemyName> nonAttackingEnemies = new List<EnemyName>
        {
            EnemyName.Arsemouth,
            EnemyName.Curser,
            EnemyName.FloatingCultist,
            EnemyName.TaintedPeepEye,
            EnemyName.Screecher,
            EnemyName.Sucker,
        };

        public static bool EnemyIsBlocking(Enemy enemy)
        {
            bool block = false;

            //Blobby passive ability
            if (enemy is BlackBlobbyEnemy)
            {
                BlackBlobbyEnemy blackBlobbyEnemy = (BlackBlobbyEnemy)enemy;
                if (blackBlobbyEnemy.healthState == BlackBlobbyEnemy.HealthState.full) { block = true; }
            }
            if (enemy is GreenBlobbyEnemy)
            {
                GreenBlobbyEnemy greenBlobbyEnemy = (GreenBlobbyEnemy)enemy;
                if (greenBlobbyEnemy.healthState == GreenBlobbyEnemy.HealthState.full) { block = true; }
            }
            if (enemy is RedBlobbyEnemy)
            {
                RedBlobbyEnemy redBlobbyEnemy = (RedBlobbyEnemy)enemy;
                if (redBlobbyEnemy.healthState == RedBlobbyEnemy.HealthState.full) { block = true; }
            }

            //Shy gal mask
            if (enemy is ShyGalBoss)
            {
                ShyGalBoss shyGalBoss = (ShyGalBoss)enemy;
                if (!(bool)AccessTools.Field(typeof(ShyGalBoss), "is_dumb").GetValue(shyGalBoss) && (shyGalBoss.isMasked || !shyGalBoss.boss)) { block = true; }
            }

            //Red bubble shield
            //if (WindfallHelper.app.model.aiModel.battlefieldEffects[WindfallHelper.app.model.aiModel.battlefieldPositionIndex[enemy.position.x, enemy.position.y]].effect == BattlefieldEffect.Effect.Shield)
            //{
            //    block = true;
            //}

            return block;
        }

        public static bool EnemyIsInvincible(Enemy enemy)
        {
            if (enemy.attackImmunity == Enemy.AttackImmunity.SuperAttack)
            {
                return true;
            }

            bool invincible = false;
            if (potentiallyInvincibleEnemies.Contains(EnemyDisplayName(enemy)))
            {
                invincible = true;

                //Host skull
                if (enemy is HostEnemy)
                {
                    HostEnemy hostEnemy = (HostEnemy)enemy;
                    if (!hostEnemy.IsClosed()) { invincible = false; }
                }

                //Sangre chest
                if (enemy is CaddyBoss)
                {
                    CaddyBoss caddyBoss = (CaddyBoss)enemy;
                    if (caddyBoss.IsChestOpen()) { invincible = false; }
                }
            }

            return invincible;
        }

        private static readonly List<string> potentiallyInvincibleEnemies = new List<string>
        {
            "MANA_WISP_NAME",
            "HOST_NAME",
            "PEEPER_EYE_NAME",
            "SANGRE_NAME",
            "STONY_NAME",
            "TAINTED_PEEPER_EYE_NAME",
        };
    }
}
