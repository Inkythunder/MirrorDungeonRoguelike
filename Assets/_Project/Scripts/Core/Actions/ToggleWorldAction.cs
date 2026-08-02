namespace Roguelike.Core
{
    /// <summary>
    /// Steps between the primary world with hostile entities and the mirror world with puzzles.
    /// </summary>
    public sealed class ToggleWorldAction : IAction
    {
        readonly EntityState player;

        public ToggleWorldAction(EntityState player)
        {
            this.player = player;
        }

        public ActionResult Perform(LevelData level)
        {
            if (player != level.player) return ActionResult.Blocked;
            
            // Player is an entity so it must be removed from one occupancy index and join the other.
            level.RemoveEntity(player);
            level.ToggleWorld();
            level.AddEntity(player);
            
            level.Log(level.in_mirror_world ? "You enter the mirror world." : "You re-enter the real world.");

            return ActionResult.Moved;
        }
    }
}