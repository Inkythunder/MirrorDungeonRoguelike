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
        private readonly Vector2Int direction;

        public MoveOrAttackAction(EntityState actor, Vector2Int direction)
        {
            this.actor = actor;
            this.direction = direction;
        }

        public ActionResult Perform(LevelData level)
        {
            Vector2Int target = actor.position + direction;

            if (!level.map.IsWalkable(target))
            {
                return ActionResult.Blocked;
            }

            EntityState occupant = level.EntityAt(target);
            if (occupant != null)
            {
                // If the target is a statue, we want to push it, not attack.
                if (occupant.faction == Faction.Petrified)
                {
                    // Only the player can shove a statue.
                    if (actor.faction != Faction.Player) return ActionResult.Blocked;
                    
                    // The space on the far side of the statue relative to the player.
                    Vector2Int beyond = target + direction;
                
                    // Push only if the far side is clear.
                    if (!level.map.IsWalkable(beyond) || level.EntityAt(beyond) != null) return ActionResult.Blocked;
                
                    // Push the statue one square away from the player. Player does not follow it.
                    level.MoveEntity(occupant, beyond);
                    level.Log("You push the statue.");
                    level.CheckGlyph();
                    return ActionResult.Pushed;
                }
                
                // If the target is the same faction (hostile on hostile violence) that's not allowed.
                if (occupant.faction == actor.faction)
                {
                    return ActionResult.Blocked;
                }

                // If the target is not the same faction, and not a statue, we're fighting it.
                return Combat.Resolve(actor, occupant, level);
            }
            
            // If there's nothing standing in the target location, we're just walking.
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