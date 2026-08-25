using System.Collections.Generic;

namespace Roguelike.Game
{
    public class RunStats
    {
        public int kills { get; private set; }

        private readonly Dictionary<string, int> kills_by_name = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, int> by_name => kills_by_name;

        public void CountKill(string enemy_name)
        {
            kills++;
            kills_by_name.TryGetValue(enemy_name, out int count);
            kills_by_name[enemy_name] = count + 1;
        }
    }
}