// AudioManager.cs
using UnityEngine;

namespace GroceryGame.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Get or create audio source
                if (audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        audioSource = gameObject.AddComponent<AudioSource>();
                    }
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySound(string soundName)
        {
            // For now, just log the sound name
            // Later we can add actual audio clips
            Debug.Log($"Playing sound: {soundName}");

            // You can add actual sound playing here later:
            // audioSource.PlayOneShot(GetAudioClip(soundName));
        }
    }
}