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
        [SerializeField] private float hold_seconds = 5f;
        [SerializeField] private float fade_seconds = 1.5f;

        private struct Line
        {
            public string message;
            public float created_at;
        }

        private readonly List<Line> lines = new List<Line>();
        private string drawn = string.Empty;
        
        // Oldest line is dimmest.
        private static readonly float[] age_alpha = { 1f, 0.8f, 0.6f, 0.4f };

        public void Add(string message)
        {
            lines.Add(new Line { message = message, created_at = Time.unscaledTime });
        
            // Drop from the front so newest lines are always the last one drawn.
            if (lines.Count > max_lines) lines.RemoveRange(0, lines.Count - max_lines);
            Redraw();
        }

        public void Clear()
        {
            lines.Clear();
            Redraw();
        }

        void Update()
        {
            if (lines.Count == 0) return;
            
            // Lines are in chronological order.
            float lifetime = hold_seconds + fade_seconds;
            int expired = 0;
            while (expired < lines.Count && Time.unscaledTime - lines[expired].created_at >= lifetime) expired++;
            if (expired > 0) lines.RemoveRange(0, expired);
            
            Redraw();
        }

        void Redraw()
        {
            var builder = new System.Text.StringBuilder();

            for (int i = 0; i < lines.Count; i++)
            {
                int age = lines.Count - i - 1;
                float alpha = age_alpha[Mathf.Min(age, age_alpha.Length - 1)] * TimeFade(lines[i]);

                builder.Append($"<alpha=#{Mathf.RoundToInt(alpha * 255) :X2}>").AppendLine(lines[i].message);
            }

            string next = builder.ToString();
            if (next == drawn) return;

            drawn = next;
            text.text = next;
        }

        float TimeFade(Line line)
        {
            if (fade_seconds <= 0f) return 1f;

            float elapsed = Time.unscaledTime - line.created_at;
            return Mathf.Clamp01(1f - (elapsed - hold_seconds) / fade_seconds);
        }
    }
}