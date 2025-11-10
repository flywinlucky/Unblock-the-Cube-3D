using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Tap_Away_Block_Puzzle_3D
{
	public class SettingsManager : MonoBehaviour
	{
		#region Inspector

		[Header("UI References")]
		[Tooltip("Button used to toggle sound (may be a Button with an Image).")]
		public Button soundButton;

		[Tooltip("Button used to toggle music (may be a Button with an Image).")]
		public Button musicButton;

		[Tooltip("Button used to restart the current level/scene.")]
		public Button restartButton;

		[Tooltip("Button used to open/close the settings panel.")]
		public Button openSoundPanelButton;

		[Tooltip("Settings panel GameObject that will be shown/hidden.")]
		public GameObject settingsPanel;

		[Header("Sound Sprites")]
		[Tooltip("Sprite used when sound is enabled.")]
		public Sprite soundOnSprite;

		[Tooltip("Sprite used when sound is disabled.")]
		public Sprite soundOffSprite;

		[Header("Music Sprites")]
		[Tooltip("Sprite used when music is enabled.")]
		public Sprite musicOnSprite;

		[Tooltip("Sprite used when music is disabled.")]
		public Sprite musicOffSprite;

		[Header("Audio Integration")]
		[Tooltip("Optional reference to an AudioManager in the scene.")]
		public AudioManager audioManager;

		[Header("Settings Button Icons")]
		[Tooltip("Icon sprite used for the settings button when panel is closed (open icon).")]
		public Sprite settingsIconSprite;

		[Tooltip("Icon sprite used for the settings button when panel is open (close icon).")]
		public Sprite closeIconSprite;

		[Tooltip("Optional Image component (child) used for the settings button icon.")]
		public Image openSoundPanelIcon;

		#endregion

		#region Private

		private bool _soundEnabled = true;
		private bool _musicEnabled = true;
		private string soundKey = "SoundEnabled";
		private string musicKey = "MusicEnabled";

		#endregion

		private void Start()
		{
			// Load persisted states (default ON)
			_soundEnabled = PlayerPrefs.GetInt(soundKey, 1) == 1;
			_musicEnabled = PlayerPrefs.GetInt(musicKey, 1) == 1;

			// Ensure Image references for buttons
			EnsureButtonImages();

			// Apply initial sprites
			ApplySoundSprite();
			ApplyMusicSprite();

			// Bind listeners safely
			if (soundButton != null)
			{
				soundButton.onClick.RemoveListener(ToggleSound);
				soundButton.onClick.AddListener(ToggleSound);
			}
			if (musicButton != null)
			{
				musicButton.onClick.RemoveListener(ToggleMusic);
				musicButton.onClick.AddListener(ToggleMusic);
			}

			// Apply states to AudioManager if available
			if (audioManager != null)
			{
				audioManager.SetSoundEnabled(_soundEnabled);
				audioManager.SetMusicEnabled(_musicEnabled);
			}

			// Start with settings panel closed
			if (settingsPanel != null) settingsPanel.SetActive(false);

			// Bind settings open/close button
			if (openSoundPanelButton != null)
			{
				openSoundPanelButton.onClick.RemoveListener(ToggleSettingsPanel);
				openSoundPanelButton.onClick.AddListener(ToggleSettingsPanel);
			}

			// Bind restart button
			if (restartButton != null)
			{
				restartButton.onClick.RemoveListener(RestartCurrentScene);
				restartButton.onClick.AddListener(RestartCurrentScene);
			}

			// Fallback: use button image as icon if a child icon wasn't assigned
			if (openSoundPanelIcon == null && openSoundPanelButton != null)
			{
				if (openSoundPanelButton.image != null)
					openSoundPanelIcon = openSoundPanelButton.image;
			}

			if (openSoundPanelIcon != null && settingsIconSprite != null)
			{
				openSoundPanelIcon.sprite = settingsIconSprite;
			}
		}

		private void EnsureButtonImages()
		{
			// Ensure buttons have Image components to swap sprites (maintains compatibility)
			if (soundButton != null)
			{
				if (soundButton.image == null)
				{
					Image img = soundButton.GetComponent<Image>();
					if (img == null) img = soundButton.gameObject.AddComponent<Image>();
					soundButton.image = img;
				}
			}

			if (musicButton != null)
			{
				if (musicButton.image == null)
				{
					Image img = musicButton.GetComponent<Image>();
					if (img == null) img = musicButton.gameObject.AddComponent<Image>();
					musicButton.image = img;
				}
			}

			if (openSoundPanelButton != null)
			{
				if (openSoundPanelButton.image == null)
				{
					Image img = openSoundPanelButton.GetComponent<Image>();
					if (img == null) img = openSoundPanelButton.gameObject.AddComponent<Image>();
					openSoundPanelButton.image = img;
				}

				// If no child icon assigned, try to find a child Image (excluding the button's own Image)
				if (openSoundPanelIcon == null)
				{
					Image[] images = openSoundPanelButton.GetComponentsInChildren<Image>(true);
					foreach (var img in images)
					{
						if (img.gameObject != openSoundPanelButton.gameObject)
						{
							openSoundPanelIcon = img;
							break;
						}
					}
				}
			}
		}

		private void OnDestroy()
		{
			if (soundButton != null) soundButton.onClick.RemoveListener(ToggleSound);
			if (musicButton != null) musicButton.onClick.RemoveListener(ToggleMusic);
			if (openSoundPanelButton != null) openSoundPanelButton.onClick.RemoveListener(ToggleSettingsPanel);
			if (restartButton != null) restartButton.onClick.RemoveListener(RestartCurrentScene);
		}

		/// <summary>
		/// Toggle sound enabled state and persist it.
		/// </summary>
		public void ToggleSound()
		{
			_soundEnabled = !_soundEnabled;
			PlayerPrefs.SetInt(soundKey, _soundEnabled ? 1 : 0);
			PlayerPrefs.Save();
			ApplySoundSprite();

			if (audioManager != null) audioManager.SetSoundEnabled(_soundEnabled);
			// TODO: Add global sound on/off behavior if needed
		}

		/// <summary>
		/// Toggle music enabled state and persist it.
		/// </summary>
		public void ToggleMusic()
		{
			_musicEnabled = !_musicEnabled;
			PlayerPrefs.SetInt(musicKey, _musicEnabled ? 1 : 0);
			PlayerPrefs.Save();
			ApplyMusicSprite();

			if (audioManager != null) audioManager.SetMusicEnabled(_musicEnabled);
			// TODO: Add music start/stop behavior if needed
		}

		private void ApplySoundSprite()
		{
			if (soundButton != null && soundButton.image != null)
			{
				soundButton.image.sprite = _soundEnabled ? soundOnSprite : soundOffSprite;
			}
			else
			{
				Image img = TryFindImageByName("SoundImage");
				if (img != null) img.sprite = _soundEnabled ? soundOnSprite : soundOffSprite;
			}
		}

		private void ApplyMusicSprite()
		{
			if (musicButton != null && musicButton.image != null)
			{
				musicButton.image.sprite = _musicEnabled ? musicOnSprite : musicOffSprite;
			}
			else
			{
				Image img = TryFindImageByName("MusicImage");
				if (img != null) img.sprite = _musicEnabled ? musicOnSprite : musicOffSprite;
			}
		}

		private Image TryFindImageByName(string name)
		{
			GameObject go = GameObject.Find(name);
			if (go != null) return go.GetComponent<Image>();
			return null;
		}

		/// <summary>
		/// Toggle the settings panel open/close and update the settings button icon.
		/// </summary>
		public void ToggleSettingsPanel()
		{
			if (settingsPanel == null) return;
			bool willBeActive = !settingsPanel.activeSelf;
			settingsPanel.SetActive(willBeActive);

			Image imgToUse = openSoundPanelIcon != null ? openSoundPanelIcon : (openSoundPanelButton != null ? openSoundPanelButton.image : null);
			if (imgToUse != null)
			{
				if (willBeActive)
				{
					if (closeIconSprite != null) imgToUse.sprite = closeIconSprite;
				}
				else
				{
					if (settingsIconSprite != null) imgToUse.sprite = settingsIconSprite;
				}
			}
		}

		/// <summary>
		/// Reloads the currently active scene.
		/// </summary>
		public void RestartCurrentScene()
		{
			Scene currentScene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(currentScene.name);
		}
	}
}