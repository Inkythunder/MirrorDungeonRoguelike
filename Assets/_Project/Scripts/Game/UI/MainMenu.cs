using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Roguelike.Game
{
    /// <summary>
    /// Title screen. Starts a run with either a player-specified or random seed.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] string game_scene = "Game";
        
        [Header("Widgets")]
        [SerializeField] TMP_InputField seed_input;
        [SerializeField] Button play_button;
        [SerializeField] Button play_seed_button;
        [SerializeField] Button quit_button;

        void Awake()
        {
            play_button.onClick.AddListener(Play);
            play_seed_button.onClick.AddListener(PlaySeed);
            quit_button.onClick.AddListener(Quit);
            
            // Pressing enter in the field = play seed
            seed_input.onSubmit.AddListener(seed => PlaySeed());
        }

        void Update()
        {
            play_seed_button.interactable = !string.IsNullOrWhiteSpace(seed_input.text);
        }

        public void Play()
        {
            RunConfig.ClearSeed();
            SceneManager.LoadScene(game_scene);
        }

        public void PlaySeed()
        {
            if (string.IsNullOrWhiteSpace(seed_input.text)) return;
            
            RunConfig.SetSeed(SeedFromText(seed_input.text));
            SceneManager.LoadScene(game_scene);
        }

        public void Quit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        static int SeedFromText(string text)
        {
            text = text.Trim();
            
            if (!int.TryParse(text, out int seed))
            {
                uint hash = 2166136261u;
                for (int i = 0; i < text.Length; i++)
                {
                    hash ^= text[i];
                    hash *= 16777619u;
                }

                seed = (int)(hash & 0x7FFFFFFF);
            }

            return seed != 0 ? seed : 1;
        }
    }
}
