using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Roguelike.Game
{
    /// <summary>
    /// Displays the last few messages about what happened in the game.
    /// Lines fade rather than just disappear.
    /// </summary>
    public class MessageLog : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField] private int max_lines = 5;

        private readonly List<string> lines = new List<string>();

        public void Add(string message)
        {
            lines.Add(message);
        
            // Drop from the front so newest lines are always the last one drawn.
            if (lines.Count > max_lines) lines.RemoveRange(0, lines.Count - max_lines);
            Redraw();
        }

        public void Clear()
        {
            lines.Clear();
            Redraw();
        }

        void Redraw()
        {
            var builder = new System.Text.StringBuilder();

            for (int i = 0; i < lines.Count; i++)
            {
                // Oldest line is dimmest.
                int age = lines.Count - i - 1;
                string alpha = age switch
                {
                    0 => "FF",
                    1 => "CC",
                    2 => "99",
                    _ => "66"
                };

                builder.Append($"<alpha=#{alpha}>").AppendLine(lines[i]);
            }

            text.text = builder.ToString();
        }
    }
}