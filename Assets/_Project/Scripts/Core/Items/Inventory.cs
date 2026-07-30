namespace Roguelike.Core
{
    /// <summary>
    /// Controls what happens when player steps on to a tile with an item on.
    /// </summary>
    public static class Inventory
    {
        public static void PickUpAt(EntityState entity, LevelData level)
        {
            ItemState item = level.ItemAt(entity.position);
            if (item == null) return;

            switch (item.type)
            {
                case ItemType.Potion:
                    entity.potions++;
                    level.RemoveItemAt(entity.position);
                    level.Log($"Got a {item.name}");
                    break;
                
                case ItemType.Weapon:
                    if (item.tier > entity.weapon_power)
                    {
                        entity.weapon_name = item.name;
                        entity.weapon_power = item.tier;
                        level.RemoveItemAt(entity.position);
                        level.Log($"Picked up the {item.name}");
                    }
                    else
                    {
                        level.Log($"{item.name} was stepped on but ignored.");
                    }
                    break;
                
                case ItemType.Armour:
                    if (item.tier > entity.armour_power)
                    {
                        entity.armour_power = item.tier;
                        level.RemoveItemAt(entity.position);
                        level.Log($"Got {item.name}");
                    }
                    else
                    {
                        level.Log($"{item.name} was stepped on but ignored.");
                    }
                    break;
            }
        }
    }
}
