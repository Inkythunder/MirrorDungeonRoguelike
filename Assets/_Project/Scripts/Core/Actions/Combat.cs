using UnityEngine;

namespace Roguelike.Core
{
    /// <summary>
    /// Calculate damage and apply to defender.
    /// </summary>
    public static class Combat
    {
        public static ActionResult Resolve(EntityState attacker, EntityState defender, LevelData level)
        {
            // Having a minimum damage of 1 prevents negative damage which would heal the attacker if the target's
            // total defence value was higher than the attacker's total attack value.
            // It also prevents a situation where both parties could only do 0 damage to eachother creating a stalemate.
            int damage = Mathf.Max(1, attacker.total_attack - defender.total_defence);

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