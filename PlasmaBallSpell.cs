using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class PlasmaBallSpell : AttackSpell
    {
        public PlasmaBallSpell()
        {
            Name = "PLASMA_BALL_DESCRIPTION";
            Category = SpellCategory.Attack;
            Target = TargetType.FirstInSelectedColumn;
            texturePage = 1;
            IconPosition = new Vector2(0f, 0f);
            spellName = (SpellName)1000;
            manaSize = ManaSize.L;
            statusEffects = new StatusEffect();
            AnimationType = AttackAnimationType.Poke;
            baseDamage = 0;
            hasAnimation = true;
        }

        //Deals 1 damage. Damage can be buffed by Damage Needles
        public override int Damage()
        {
            return baseDamage + 1 + SpellDamageModifier();
        }

        //Electricity iteration count scales with spell damage
        private static int ChainDistance()
        {
            return WindfallHelper.app.model.characterSheet.getItemDamage();
        }

        private readonly float chainAnimationDelay = 0.6f;
        public override Sequence AttackAnimation()
        {
            //Spell ends attack manually
            app.model.waitTilAttackIsFinished = true;

            //Play Zap sound

            //Locate closest enemy
            Transform transform = app.controller.ClosestEnemy(app.model.attackColumn);

            //If null, miss attack
            if (transform == null)
            {
                app.Notify("miss.attack", app.controller.eventsController, Array.Empty<object>());

                //Abort attack and end event
                Sequence missSequence = DOTween.Sequence();
                missSequence.AppendInterval(0.5f).AppendCallback(delegate
                {
                    app.controller.eventsController.EndEvent();
                });

                return missSequence;
            }

            //Grab enemy component
            Enemy enemy = transform.GetComponent<Enemy>();

            //Attack sequence
            enemy.Hurt((float)Damage(), Enemy.AttackImmunity.ReduceSpellDamage, statusEffects, enemy.position.x);

            List<Enemy> overallChainedEnemies = new List<Enemy> { enemy };
            List<Enemy> currentChainedEnemies = new List<Enemy> { enemy };

            DOTween.Sequence().AppendInterval(chainAnimationDelay).AppendCallback(delegate
            {
                ChainElectricity(overallChainedEnemies, currentChainedEnemies, 0);
            });

            //Return
            return DOTween.Sequence();
        }

        //Propagates electricity spread
        private void ChainElectricity(List<Enemy> overallChainedEnemies, List<Enemy> currentChainedEnemies, int chainIterator)
        {
            bool endAttack = false;

            //End attack if overall chained enemies is null
            if (overallChainedEnemies == null)
            {
                endAttack = true;
            }

            //End attack if current chained enemies is null or if there are none
            if (currentChainedEnemies == null || currentChainedEnemies.Count < 1)
            {
                endAttack = true;
            }

            //End attack when electricity is finished
            if (chainIterator >= ChainDistance())
            {
                endAttack = true;
            }

            //End event and abort attack
            if (endAttack)
            {
                app.controller.eventsController.EndEvent();
                return;
            }

            AIModel aiModel = app.model.aiModel;

            //List next chained enemies
            List<Enemy> nextChainedEnemies = new List<Enemy>();

            //Spread electricity from all enemies in current chain iteration
            foreach (Enemy chainedEnemy in currentChainedEnemies)
            {
                bool ground = chainedEnemy.enemyType == Enemy.EnemyType.Ground;
                BattlefieldPosition chainedEnemyBattlefieldPosition = aiModel.battlefieldPositions[aiModel.battlefieldPositionIndex[chainedEnemy.position.x, chainedEnemy.position.y]];

                //List all positions occupied by the current enemy
                List<BattlefieldPosition> currentChainedEnemyPositions = new List<BattlefieldPosition>();
                currentChainedEnemyPositions.Add(chainedEnemyBattlefieldPosition);

                //If the enemy is wide, add side positions to the list
                if (chainedEnemy.enemyWidth == 3)
                {
                    List<BattlefieldPosition> battlefieldPositions = WindfallHelper.AdjacentBattlefieldPositions(aiModel, chainedEnemyBattlefieldPosition, false, true, false);
                    foreach (BattlefieldPosition battlefieldPosition in battlefieldPositions)
                    {
                        //Failsafe: Ensure adjacent positions contain the current enemy
                        Enemy chainedEnemyAdjacent = WindfallHelper.GetEnemyByBattlefieldPosition(battlefieldPosition, ground, true);
                        if (chainedEnemyAdjacent != null && chainedEnemyAdjacent == chainedEnemy)
                        {
                            currentChainedEnemyPositions.Add(battlefieldPosition);
                        }
                    }
                    currentChainedEnemyPositions.AddRange(WindfallHelper.AdjacentBattlefieldPositions(aiModel, chainedEnemyBattlefieldPosition, false, true, false));
                }

                //List all enemies adjacent to the current enemy
                List<Enemy> adjacentUnchainedEnemies = new List<Enemy>();
                foreach (BattlefieldPosition battlefieldPosition in currentChainedEnemyPositions)
                {
                    //Vertically adjacent enemies (same battlefield position, different enemy type)
                    Enemy localAdjacentEnemy = WindfallHelper.GetEnemyByBattlefieldPosition(battlefieldPosition, !ground, true);
                    if (localAdjacentEnemy != null)
                    {
                        adjacentUnchainedEnemies.Add(localAdjacentEnemy);
                    }

                    //Horizontally adjacent enemies
                    foreach (BattlefieldPosition adjacentBattlefieldPosition in WindfallHelper.AdjacentBattlefieldPositions(aiModel, battlefieldPosition, false))
                    {
                        //Do not include the current enemy positions
                        if (currentChainedEnemyPositions.Contains(adjacentBattlefieldPosition))
                        {
                            continue;
                        }

                        //Add adjacent enemies if they are of the same ground/flying type
                        Enemy adjacentEnemy = WindfallHelper.GetEnemyByBattlefieldPosition(adjacentBattlefieldPosition, ground, true);
                        if (adjacentEnemy != null)
                        {
                            adjacentUnchainedEnemies.Add(adjacentEnemy);
                        }
                    }
                }

                //Ignore already chained enemies
                adjacentUnchainedEnemies.RemoveAll((Enemy enemy) => overallChainedEnemies.Contains(enemy));

                //Track enemies in current chain iteration
                nextChainedEnemies.AddRange(adjacentUnchainedEnemies);

                //Track enemies that have been chained overall
                overallChainedEnemies.AddRange(adjacentUnchainedEnemies);
            }

            DOTween.Sequence().AppendCallback(delegate
            {
                foreach (Enemy enemy in nextChainedEnemies)
                {
                    enemy.Hurt((float)Damage(), Enemy.AttackImmunity.ReduceSpellDamage, statusEffects, enemy.position.x);
                }
            }).AppendInterval(chainAnimationDelay).AppendCallback(delegate
            {
                //Recursive method
                ChainElectricity(overallChainedEnemies, nextChainedEnemies, chainIterator + 1);
            });
        }
    }
}
