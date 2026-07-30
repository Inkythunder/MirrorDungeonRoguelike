using UnityEngine;

namespace Roguelike.Core
{
    /// <summary>
    /// Drinks one potion from inventory. Costs 1 turn if legal, costs nothing if illegal.
    /// Illegal if player has no potions or has full health already.
    /// </summary>
    public sealed class DrinkPotionAction : IAction
    {
        public const int heal_amount = 10;

        private readonly EntityState actor;

        public DrinkPotionAction(EntityState actor)
        {
            this.actor = actor;
        }

        public ActionResult Perform(LevelData level)
        {
            if (this.actor.potions <= 0)
            {
                level.Log("No potions to drink.");
                return ActionResult.Blocked;
            }

            if (this.actor.hp >= this.actor.max_hp)
            {
                level.Log("Already at full health.");
                return ActionResult.Blocked;
            }

            this.actor.potions--;
            this.actor.hp = Mathf.Min(this.actor.max_hp, this.actor.hp + heal_amount);
            level.Log($"You drank a potion. {this.actor.hp}/{this.actor.max_hp} HP");
            return ActionResult.Drank;
        }
    }
}
