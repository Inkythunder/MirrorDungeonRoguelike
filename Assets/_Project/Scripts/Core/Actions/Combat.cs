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
            level.ReportDamage(defender.position, damage);

            if (defender.is_key && !defender.IsAlive)
            {
                defender.hp = defender.max_hp;
                level.Log($"{defender.name} refuses to die...");
                return ActionResult.Attacked;
            }

            if (!defender.IsAlive)
            {
                level.Log($"{defender.name} dies.");
                level.RemoveEntity(defender);
                if(attacker.faction == Faction.Player) level.CountKill(defender);
            }

            return ActionResult.Attacked;
        }
    }
}