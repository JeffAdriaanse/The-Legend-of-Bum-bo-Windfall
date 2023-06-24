using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class PlasmaBallSpell : AttackSpell
    {
        public PlasmaBallSpell()
        {
            base.Name = "PLASMA_BALL_DESCRIPTION";
            base.Category = SpellElement.SpellCategory.Attack;
            base.Target = SpellElement.TargetType.FirstInSelectedColumn;
            base.IconPosition = new Vector2(0f, 0f);
            base.spellName = (SpellName)1000;
            base.manaSize = SpellElement.ManaSize.L;
            this.statusEffects = new StatusEffect();
            base.AnimationType = SpellElement.AttackAnimationType.Poke;
            base.baseDamage = 0;
            this.hasAnimation = true;
        }

        public override Sequence AttackAnimation()
        {
            //Play Zap sound

            //Locate closest enemy
            Transform transform = base.app.controller.ClosestEnemy(base.app.model.attackColumn);

            //If null, miss attack
            if (transform == null)
            {
                base.app.Notify("miss.attack", base.app.controller.eventsController, Array.Empty<object>());
                return base.AttackAnimation();
            }

            //Grab enemy component
            Enemy enemy = transform.GetComponent<Enemy>();

            //Initialize animation sequence
            Sequence sequence = DOTween.Sequence();

            //Do animation

            //Hurt enemies

            //Return
            return sequence;
        }
    }
}
