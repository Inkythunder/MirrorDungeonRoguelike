using UnityEngine;

namespace Roguelike.Core
{
    public static class EnemyAI
    {
        public static IAction Decide(EntityState enemy, LevelData level, FlowField field, Rng rng)
        {
            EntityState player = level.player;
            if (player == null || !player.IsAlive) return new WaitAction();

            int distance = Mathf.Abs(enemy.position.x - player.position.x) + 
                           Mathf.Abs(enemy.position.y - player.position.y);

            if (distance == 1)
            {
                return new MoveOrAttackAction(enemy, player.position - enemy.position);
            }

            if (enemy.behaviour == EnemyBehaviour.Chaser && distance <= enemy.aggro_range)
            {
                if (field.TryGetStep(enemy.position, level, player.position, 
                        out Vector2Int step))
                {
                    return new MoveOrAttackAction(enemy, step);
                }
            }

            return Wander(enemy, level, rng);
        }

        static IAction Wander(EntityState enemy, LevelData level, Rng rng)
        {
            int random_direction = rng.Range(0, 4);
            for (int i = 0; i < 4; i++)
            {
                Vector2Int direction = Direction.Cardinals4[(random_direction + i) % 4];
                if (level.CanEnter(enemy.position + direction))
                {
                    return new MoveOrAttackAction(enemy, direction);
                }
            }

            return new WaitAction();
        }
    }
}