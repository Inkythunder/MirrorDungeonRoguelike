using UnityEngine;

namespace Roguelike.Core
{
    public sealed class EntityState
    {
        public string name = "Entity";
        public Faction faction;

        public Vector2Int position;

        public int max_hp = 10;
        public int hp = 10;
        public int attack = 3;
        public int defence = 0;

        public bool IsAlive => hp > 0;

        public EnemyBehaviour behaviour = EnemyBehaviour.Wanderer;
        public int aggro_range = 8;
    }
}

