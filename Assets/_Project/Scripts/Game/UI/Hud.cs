using System.Collections.Generic;
using Roguelike.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Roguelike.Game
{
    /// <summary>
    /// Pulls what to display from the model every frame.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        [SerializeField] GameManager game_manager;
        [SerializeField] TurnManager turn_manager;
        
        [Header("Widgets")]
        [SerializeField] Image hp_fill;
        [SerializeField] TMP_Text hp_text;
        [SerializeField] TMP_Text depth_text;
        [SerializeField] TMP_Text inventory_text;
        [SerializeField] TMP_Text debug_text;
        [SerializeField] GameObject death_panel;
        [SerializeField] TMP_Text death_text;
        [SerializeField] EdgeMarker glyph_arrow;

        private void LateUpdate()
        {
            debug_text.gameObject.SetActive(DebugMode.enabled);
            
            LevelData level = game_manager.level;
            if (level == null || level.player == null) return;

            EntityState player = level.player;
            
            // How much to fill the health bar by
            hp_fill.fillAmount = Mathf.Clamp01((float)player.hp / player.max_hp);

            // Clamp value to never go below 0.
            // HP will often go below zero but showing -5 / 20 hp looks like a bug
            int hp = Mathf.Max(0, player.hp);
            hp_text.text = $"{hp}/{player.max_hp}";

            // If player is standing over the stairs, show that they can press E to descent.
            // Otherwise, just show the depth level.
            if (level.in_mirror_world && level.turns_per_awakening > 0)
            {
                int until_next = level.turns_per_awakening - (level.mirror_turns % level.turns_per_awakening);

                float urgency = 1f - (float)(until_next - 1) / Mathf.Max(1, level.turns_per_awakening - 1);
                string hex = ColorUtility.ToHtmlStringRGB(Color.Lerp(Color.white, Color.red, urgency));
                
                depth_text.text = $"Depth{level.depth} | Something wakes in <color=#{hex}>{until_next}</color> turns";
            }
            else if (player.position == level.stairs_position)
            {
                depth_text.text = level.stairs_locked
                    ? $"Depth {level.depth} | Sealed - find the glyph"
                    : $"Depth {level.depth} | [E] to descend";
            }
            else
            {
                depth_text.text = $"Depth {level.depth}";    
            }

            if (level.in_mirror_world && level.stairs_locked)
            {
                glyph_arrow.Show(level.glyph_position);
            }
            else
            {
                glyph_arrow.Hide();
            }

            // Show the contents of the inventory: Weapon name and tier, Armour tier, potion quantity.
            inventory_text.text =
                $"WPN: {player.weapon_name} | ATK: {player.total_attack} | DEF: {player.total_defence} | POTIONS: {player.potions}";

            // Show the seed and turn count only in debug mode.
            if (DebugMode.enabled)
            {
                EntityState hostile_count_source = null;
                debug_text.text = 
                    $"Seed: {level.seed} | Turn count {turn_manager.turn_count}\n"
                    + $"Pos: {player.position.x}, {player.position.y} | World: {(level.in_mirror_world ? "MIRROR" : "PRIMARY")}\n"
                    + $"Mirror turns: {level.mirror_turns} | Guardians awake: {level.guardians_awakened}\n";
            }
            
            // Show the death panel based on the game_over state.
            death_panel.SetActive(game_manager.game_over);

            if (game_manager.game_over)
            {
                RunStats stats = game_manager.run_stats;

                string breakdown = "";
                foreach (KeyValuePair<string, int> entry in stats.by_name)
                {
                    breakdown += $"\n {entry.Key} x{entry.Value}";
                }
                
                death_text.text = "GAME OVER\n" 
                  + $"Depth: {level.depth} | Seed: {level.seed}\n" 
                  + $"Kills: {stats.kills}{breakdown}\n"
                  + "Press [R] to restart";
            }
        }
    }
}