namespace Roguelike.Core
{
    /// <summary>
    /// Calculate damage and apply to defender.
    /// </summary>
    public static class Combat
    {
        public static ActionResult Resolve(EntityState attacker, EntityState defender, LevelData level)
        {
            int damage = attacker.attack - defender.defence;

            defender.hp -= damage;
            level.Log($"{attacker.name} hits {defender.name} for {damage}");

            if (!defender.IsAlive)
            {
                level.Log($"{defender.name} dies.");
                level.RemoveEntity(defender);
            }

            return ActionResult.Attacked;
        }
    }
}