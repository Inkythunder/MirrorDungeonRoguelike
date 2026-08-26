namespace Roguelike.Core
{
    public class KnockDown
    {
        public static void Strike(EntityState entity, LevelData level)
        {
            entity.hp = entity.max_hp;
            entity.downed_turns = entity.downed_duration;
            level.Log($"{entity.name} collapses in a heap.");
        }

        public static void Tick(LevelData level)
        {
            TickWorld(level.primary, level);
            TickWorld(level.mirror, level);
        }

        static void TickWorld(WorldContents world, LevelData level)
        {
            foreach (EntityState entity in world.AllEntities)
            {
                if (entity.downed_turns <= 0) continue;

                entity.downed_turns--;
                if (entity.downed_turns == 0) level.Log($"{entity.name} staggers back to its... feet?");
            }
        }
    }
}
