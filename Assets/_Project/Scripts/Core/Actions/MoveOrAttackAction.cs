using UnityEngine;

namespace Roguelike.Core
{
    /// <summary>
    /// Move to a target cell or if the cell has an occupant (who isn't an ally) attack them.
    /// </summary>
    public sealed class MoveOrAttackAction : IAction
    {
        private readonly EntityState _actor;
        private readonly Vector2Int _direction;

        public MoveOrAttackAction(EntityState actor, Vector2Int direction)
        {
            _actor = actor;
            _direction = direction;
        }

        public ActionResult Perform(LevelData level)
        {
            Vector2Int target = _actor.position + _direction;

            if (!level.map.IsWalkable(target))
            {
                return ActionResult.Blocked;
            }

            EntityState occupant = level.EntityAt(target);
            if (occupant != null)
            {
                if (occupant.faction == _actor.faction)
                {
                    return ActionResult.Blocked;
                }

                return Combat.Resolve(_actor, occupant, level);
            }
            
            level.MoveEntity(_actor, target);
            return ActionResult.Moved;
        }
    }
}