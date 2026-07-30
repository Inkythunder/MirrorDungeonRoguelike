using UnityEngine;

namespace Roguelike.Core
{
    /// <summary>
    /// Move to a target cell or if the cell has an occupant (who isn't an ally) attack them.
    /// If it's 
    /// </summary>
    public sealed class MoveOrAttackAction : IAction
    {
        private readonly EntityState actor;
        private readonly Vector2Int _direction;

        public MoveOrAttackAction(EntityState actor, Vector2Int direction)
        {
            this.actor = actor;
            _direction = direction;
        }

        public ActionResult Perform(LevelData level)
        {
            Vector2Int target = actor.position + _direction;

            if (!level.map.IsWalkable(target))
            {
                return ActionResult.Blocked;
            }

            EntityState occupant = level.EntityAt(target);
            if (occupant != null)
            {
                if (occupant.faction == actor.faction)
                {
                    return ActionResult.Blocked;
                }

                return Combat.Resolve(actor, occupant, level);
            }
            
            level.MoveEntity(actor, target);
            
            // Walking over an item picks it up. (Would be funny to make enemies pick up weapons too)
            if (this.actor.faction == Faction.Player)
            {
                Inventory.PickUpAt(actor, level);
            }
            return ActionResult.Moved;
        }
    }
}