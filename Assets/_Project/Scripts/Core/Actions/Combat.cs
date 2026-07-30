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
            // Prevents negative damage which would heal the attacker if the target's
            // total defence value was higher than the attacker's total attack value.
            int damage = Mathf.Max(0, attacker.total_attack - defender.total_defence);

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