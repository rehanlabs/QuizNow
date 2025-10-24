using UnityEngine;
using UnityEngine.UI;
using Ami.BroAudio;

namespace QuizGame.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI Slider")]
        public Slider sfxVolumeSlider;
        public SoundID notifySound;

        [Header("Buttons")]
        public Button quitButton;

        private void Start()
        {
            if (sfxVolumeSlider != null)
            {
                // Hook listener
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

                // Load saved value (default 1f = full volume)
                float savedValue = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeSlider.value = savedValue;

                // Apply immediately
                SetSFXVolume(savedValue);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitGame);
            }
        }

        private void SetSFXVolume(float value)
        {
            // BroAudio accepts values 0–10 (not 0–1), so scale accordingly
            float broValue = value * 10f;

            // Set SFX volume (BroAudioType.SFX)
            BroAudio.SetVolume(broValue);

            // Save normalized 0–1 value
            PlayerPrefs.SetFloat("SFXVolume", value);

            notifySound.Play();
        }

        private void OnQuitGame()
        {
            QuitApplication();
        }

        // Called by "Yes" button in confirmation popup
        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // stop play mode in editor
#else
            Application.Quit(); // quit in build
#endif
        }
    }
}
