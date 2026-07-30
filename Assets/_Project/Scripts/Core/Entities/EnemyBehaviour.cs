namespace Roguelike.Core
{
    public enum EnemyBehaviour
    {
        /// <summary>
        /// Aimlessly wanders around. Only attacks if the player happens to be adjacent.
        /// </summary>
        Wanderer,
        /// <summary>
        /// Follows the flow field to hunt the player once they're inside aggro range.
        /// </summary>
        Chaser
    }
}