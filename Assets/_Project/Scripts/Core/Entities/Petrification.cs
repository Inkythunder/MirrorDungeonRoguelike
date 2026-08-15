using System.Collections.Generic;

namespace Roguelike.Core
{
    public class Petrification
    {
        // Turn hostile enemies in the primary world petrified upon entering the mirror world.
        public static void Petrify(LevelData level)
        {
            level.petrified_enemies.Clear();
            level.guardians_awakened = 0;

            var candidates = new List<EntityState>(level.primary.AllEntities);
            foreach (EntityState entity in candidates)
            {
                if (entity.faction == Faction.Player || !entity.IsAlive) continue;
                
                level.primary.RemoveEntity(entity);
                entity.faction = Faction.Petrified;
                level.mirror.AddEntity(entity);

                if (!entity.is_guardian)
                {
                    level.petrified_enemies.Add(entity);
                }
            }
        }

        // Restore petrified enemies in the mirror world to hostile ones in the primary world
        public static void Restore(LevelData level)
        {
            foreach (EntityState entity in level.petrified_enemies)
            {
                level.mirror.RemoveEntity(entity);
                entity.faction = Faction.Hostile;
                level.primary.AddEntity(entity);
            }

            level.petrified_enemies.Clear();

            var mirror_entities = new List<EntityState>(level.mirror.AllEntities);
            foreach (EntityState entity in mirror_entities)
            {
                if (!entity.is_guardian || !entity.IsAlive) continue;
                level.mirror.RemoveEntity(entity);
                entity.faction = Faction.Petrified;
                level.primary.AddEntity(entity);
            }

            level.guardians_awakened = 0;
        }
    }
}