using System;
using System.Collections;
using System.Collections.Generic;
using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Game
{
    public enum TurnPhase
    {
        waiting_for_input,
        resolving,
        game_over
    }
    
    /// <summary>
    /// Turn cycle controlled here.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Tooltip("How long a turn takes on screen.")]
        [SerializeField] float step_duration = 0.12f;
        
        LevelData level;
        FlowField field;
        Rng rng;

        private readonly List<EntityState> acting_buffer = new List<EntityState>();

        public TurnPhase phase { get; private set; } = TurnPhase.waiting_for_input;
        public int turn_count { get; private set; }

        public event Action player_died;

        public bool AcceptsInput()
        {
            return phase == TurnPhase.waiting_for_input && level != null;
        }

        public void Begin(LevelData level, Rng rng)
        {
            StopAllCoroutines();
            
            this.level = level;
            this.rng = rng;
            this.field = new FlowField(level.map);
            
            phase = TurnPhase.waiting_for_input;
            turn_count = 0;
        }

        /// <summary>
        /// Executes the player's chosed action if it was a legal action and runs eveyone else's turn.
        /// If it was not legal, returns false so the turn count does not progress.
        /// </summary>
        public bool SubmitPlayerAction(IAction action)
        {
            if (!AcceptsInput()) return false;

            ActionResult result = action.Perform(level);
            if (result == ActionResult.Blocked) return false;

            StartCoroutine(ResolveTurn());
            return true;
        }

        IEnumerator ResolveTurn()
        {
            phase = TurnPhase.resolving;

            RunEnemyTurns();

            // During 'step_duration' is when enemies position is interpolating to their new position.
            // All enemies appear to move at once during this time.
            yield return new WaitForSeconds(step_duration);

            turn_count++;

            if (!level.player.IsAlive)
            {
                phase = TurnPhase.game_over;
                player_died?.Invoke();
                yield break;
            }
            
            phase = TurnPhase.waiting_for_input;
        }

        void RunEnemyTurns()
        {
            // One flood fill for the whole level before any movement.
            // Every enemy reads the same map for efficiency
            field.Rebuild(level.player.position);
            
            acting_buffer.Clear();
            foreach (EntityState entity in level.AllEntities)
            {
                if (entity.faction == Faction.Hostile)
                {
                    acting_buffer.Add(entity);
                }
            }

            foreach (EntityState enemy in acting_buffer)
            {
                // In case killed earlier on this turn
                if (!enemy.IsAlive) continue;
                // Stop if player is dead
                if (!level.player.IsAlive) continue;

                EnemyAI.Decide(enemy, level, field, rng).Perform(level);
            }
        }
    }
}

