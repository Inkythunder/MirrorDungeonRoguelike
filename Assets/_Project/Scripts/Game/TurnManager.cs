using System.Collections;
using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Game
{
    public enum TurnPhase
    {
        WaitingForInput,
        Resolving
    }
    
    /// <summary>
    /// Turn cycle controlled here.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Tooltip("How long a turn takes on screen.")]
        [SerializeField] float step_duration = 0.12f;
        
        LevelData level;

        public TurnPhase phase { get; private set; } = TurnPhase.WaitingForInput;
        public int turn_count { get; private set; }

        public void Begin(LevelData level)
        {
            this.level = level;
            phase = TurnPhase.WaitingForInput;
            turn_count = 0;
            StopAllCoroutines();
        }

        public bool AcceptsInput => phase == TurnPhase.WaitingForInput && level != null;

        public bool SubmitPlayerAction(IAction action)
        {
            if (!AcceptsInput) return false;

            ActionResult result = action.Perform(level);
            if (result == ActionResult.Blocked) return false;

            StartCoroutine(ResolveTurn());
            return true;
        }

        IEnumerator ResolveTurn()
        {
            phase = TurnPhase.Resolving;

            yield return new WaitForSeconds(step_duration);

            turn_count++;
            phase = TurnPhase.WaitingForInput;
        }
    }
}

