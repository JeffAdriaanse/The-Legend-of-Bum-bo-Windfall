using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
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

        private static readonly GameObject PLASMA_PARTICLES = Windfall.assetBundle.LoadAsset<GameObject>("Plasma_Trail_Particles");

        //Deals 1 damage. Damage can be buffed by Damage Needles
        public override int Damage()
        {
            return baseDamage + 1 + SpellDamageModifier();
        }

        //Electricity iteration count scales with spell damage
        public int ChainDistance()
        {
            return WindfallHelper.app.model.characterSheet.getItemDamage() + 1;
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
                missSequence.AppendInterval(0.6f).AppendCallback(delegate
                {
                    app.controller.eventsController.EndEvent();
                });

                return missSequence;
            }

            //Grab enemy component
            Enemy enemy = transform.GetComponent<Enemy>();

            List<Enemy> overallChainedEnemies = new List<Enemy> { enemy };
            //Create an initial enemy chain with only the starting enemy
            List<List<Enemy>> currentEnemyChains = new List<List<Enemy>> { new List<Enemy> { enemy } };

            //Attack sequence
            ShockEnemy(currentEnemyChains[currentEnemyChains.Count - 1]);

            DOTween.Sequence().AppendInterval(chainAnimationDelay).AppendCallback(delegate
            {
                ChainElectricity(overallChainedEnemies, currentEnemyChains, 0);
            });

            //Return
            return DOTween.Sequence();
        }

        //Propagates electricity spread
        private void ChainElectricity(List<Enemy> overallChainedEnemies, List<List<Enemy>> currentEnemyChains, int chainIterator)
        {
            bool endAttack = false;

            //End attack if overall chained enemies is null
            if (overallChainedEnemies == null)
            {
                endAttack = true;
            }

            //End attack if current enemy chains is null or empty
            if (currentEnemyChains == null || currentEnemyChains.Count < 1)
            {
                endAttack = true;
            }

            //End attack when electricity is finished
            if (chainIterator >= ChainDistance())
            {
                endAttack = true;
            }

            AIModel aiModel = app.model.aiModel;

            //List next chained enemies
            List<List<Enemy>> nextEnemyChains = new List<List<Enemy>>();

            //Spread electricity from all enemies in current chain iteration
            foreach (List<Enemy> enemyChain in currentEnemyChains)
            {
                //End attack if current enemy chain is null or empty
                if (enemyChain == null || enemyChain.Count < 1)
                {
                    endAttack = true;
                }

                Enemy chainedEnemy = enemyChain[enemyChain.Count - 1];
                //End attack if current enemy is null
                if (chainedEnemy == null)
                {
                    endAttack = true;
                }

                //End attack
                if (endAttack)
                {
                    break;
                }

                bool ground = chainedEnemy.enemyType == Enemy.EnemyType.Ground;
                BattlefieldPosition chainedEnemyBattlefieldPosition = aiModel.battlefieldPositions[aiModel.battlefieldPositionIndex[chainedEnemy.position.x, chainedEnemy.position.y]];

                //List all positions occupied by the current enemy
                List<BattlefieldPosition> currentChainedEnemyPositions = new List<BattlefieldPosition>();
                currentChainedEnemyPositions.Add(chainedEnemyBattlefieldPosition);


                //If the enemy is wide, add side positions to the list
                if (chainedEnemy.enemyWidth == 3)
                {
                    currentChainedEnemyPositions.AddRange(WindfallHelper.AdjacentBattlefieldPositions(aiModel, chainedEnemyBattlefieldPosition, false, true, false));
                }

                //List all enemies adjacent to the current enemy
                List<Enemy> adjacentUnchainedEnemies = new List<Enemy>();
                foreach (BattlefieldPosition battlefieldPosition in currentChainedEnemyPositions)
                {
                    //Vertically adjacent enemies (same battlefield position, different enemy type)
                    Enemy localAdjacentEnemy = WindfallHelper.GetEnemyByBattlefieldPosition(battlefieldPosition, !ground, true);
                    //Ignore already chained enemies
                    if (localAdjacentEnemy != null && !overallChainedEnemies.Contains(localAdjacentEnemy))
                    {
                        adjacentUnchainedEnemies.Add(localAdjacentEnemy);
                        //Prioritize chaining to vertically adjacent enemies
                        break;
                    }

                    //Add all ground and flying enemies that are nearby
                    for (int groundIterator = 0; groundIterator < 2; groundIterator++)
                    {
                        bool groundIteration = groundIterator == 0;
                        //Horizontally adjacent enemies, including diagonals
                        foreach (BattlefieldPosition adjacentBattlefieldPosition in WindfallHelper.AdjacentBattlefieldPositions(aiModel, battlefieldPosition, true))
                        {
                            //Do not include the current enemy positions
                            if (currentChainedEnemyPositions.Contains(adjacentBattlefieldPosition))
                            {
                                continue;
                            }

                            //Add adjacent enemies
                            Enemy adjacentEnemy = WindfallHelper.GetEnemyByBattlefieldPosition(adjacentBattlefieldPosition, groundIteration, true);
                            //Ignore already chained enemies
                            if (adjacentEnemy != null && !overallChainedEnemies.Contains(adjacentEnemy))
                            {
                                adjacentUnchainedEnemies.Add(adjacentEnemy);
                            }
                        }
                    }
                }

                //Choose a random chain target
                if (adjacentUnchainedEnemies.Count > 0)
                {
                    int randomEnemyIndex = UnityEngine.Random.Range(0, adjacentUnchainedEnemies.Count);
                    Enemy randomEnemy = adjacentUnchainedEnemies[randomEnemyIndex];
                    adjacentUnchainedEnemies = new List<Enemy> { randomEnemy };
                }

                //Track enemies in current chain iteration
                foreach (Enemy enemy in adjacentUnchainedEnemies)
                {
                    List<Enemy> newEnemyChain = new List<Enemy>(enemyChain);
                    newEnemyChain.Add(enemy);

                    nextEnemyChains.Add(newEnemyChain);
                }

                //Track enemies that have been chained overall
                overallChainedEnemies.AddRange(adjacentUnchainedEnemies);
            }

            //End event and finish attack
            if (nextEnemyChains.Count < 1)
            {
                app.controller.eventsController.EndEvent();
                return;
            }

            DOTween.Sequence().AppendCallback(delegate
            {
                foreach (List<Enemy> enemyChain in nextEnemyChains)
                {
                    ShockEnemy(enemyChain);
                }
            }).AppendInterval(chainAnimationDelay).AppendCallback(delegate
            {
                //Recursive method
                ChainElectricity(overallChainedEnemies, nextEnemyChains, chainIterator + 1);
            });
        }

        private void ShockEnemy(List<Enemy> enemyChain)
        {
            if (enemyChain == null) { return; }

            Enemy enemy = enemyChain[enemyChain.Count - 1];
            if (enemy == null)
            {
                return;
            }

            //Estimate enemy position
            float enemyTypeHeightModifier = enemy.enemyType == Enemy.EnemyType.Flying ? 1f : 0f;
            Vector3 plasmaPosition = enemy.transform.position + new Vector3(0f, 0.33f + enemyTypeHeightModifier, 0f);

            //Spawn plasma particles
            if (PLASMA_PARTICLES != null)
            {
                GameObject plasmaTrail = GameObject.Instantiate(PLASMA_PARTICLES);

                //Move particle to enemy position
                plasmaTrail.transform.position = plasmaPosition;
            }

            //Play sound
            SoundsView.Instance.PlaySound((SoundsView.eSound)1000, plasmaPosition);

            //Hurt enemy
            enemy.Hurt((float)Damage(), Enemy.AttackImmunity.ReduceSpellDamage, statusEffects, enemy.position.x);
        }
    }
}
