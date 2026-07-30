using UnityEngine;

namespace Roguelike.Core
{
    public sealed class EntityState
    {
        public string name = "Entity";
        public Faction faction;

        public Vector2Int position;

        // ----- ----- Combat stats ----- -----
        public int max_hp = 10;
        public int hp = 10;
        public int attack = 3;
        public int defence = 0;
        
        // ----- Equipment and consumables -----
        public string weapon_name = "Fists";
        public int weapon_power;
        public int armour_power;
        public int potions;

        public int total_attack => attack + weapon_power;
        public int total_defence => defence + armour_power;
        public bool IsAlive => hp > 0;

        public EnemyBehaviour behaviour = EnemyBehaviour.Wanderer;
        public int aggro_range = 8;
    }
}

