using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
	/// <summary>
	/// Simple audio manager to control background music and UI/game SFX.
	/// Preserves original behavior while improving inspector layout and code readability.
	/// </summary>
	public class AudioManager : MonoBehaviour
	{
		#region Inspector

		[Header("Audio Sources")]
		[Tooltip("Background music AudioSource (looping).")]
		public AudioSource musicSource;

		[Tooltip("AudioSource used for block click sound (clip can be assigned here).")]
		public AudioSource blockClickSource; // renamed in comment only

		#endregion

		#region State & Keys

		// Cached states
		private bool _musicEnabled = true;
		private bool _soundEnabled = true;

		// PlayerPrefs keys (optional usage)
		public const string MusicPrefKey = "MusicEnabled";
		public const string SoundPrefKey = "SoundEnabled";

		#endregion

		#region Unity Events

		private void Start()
		{
			// Optionally initialize values from PlayerPrefs
			_musicEnabled = PlayerPrefs.GetInt(MusicPrefKey, 1) == 1;
			_soundEnabled = PlayerPrefs.GetInt(SoundPrefKey, 1) == 1;

			ApplyMusicState();
			ApplySoundState();
		}

		#endregion

		#region Music Control

		/// <summary>
		/// Play or resume background music if enabled.
		/// </summary>
		public void PlayMusic()
		{
			if (musicSource == null) return;
			if (!_musicEnabled) return;
			if (!musicSource.isPlaying)
			{
				// If paused and has progressed time, unpause; otherwise start playing
				if (musicSource.time > 0f && musicSource.clip != null)
				{
					musicSource.UnPause();
				}
				else
				{
					musicSource.Play();
				}
			}
		}

		/// <summary>
		/// Stop background music and reset position.
		/// </summary>
		public void StopMusic()
		{
			if (musicSource == null) return;
			if (musicSource.isPlaying)
			{
				musicSource.Stop();
			}
		}

		/// <summary>
		/// Pause playback (keeps time position).
		/// </summary>
		public void PauseMusic()
		{
			if (musicSource == null) return;
			if (musicSource.isPlaying)
			{
				musicSource.Pause();
			}
		}

		/// <summary>
		/// Resume music if paused.
		/// </summary>
		public void ResumeMusic()
		{
			if (musicSource == null) return;
			if (!musicSource.isPlaying && musicSource.clip != null)
			{
				musicSource.UnPause();
			}
		}

		#endregion

		#region SFX Control

		/// <summary>
		/// Play the block click sound using the clip assigned to blockClickSource.
		/// </summary>
		public void PlayBlockClick()
		{
			if (blockClickSource == null || !_soundEnabled) return;
			AudioClip clip = blockClickSource.clip;
			if (clip == null)
			{
				// Fallback: if no clip but AudioSource configured, try Play()
				if (!blockClickSource.isPlaying) blockClickSource.Play();
				return;
			}
			blockClickSource.PlayOneShot(clip);
		}

		#endregion

		#region Settings API

		/// <summary>
		/// Enable or disable music and persist choice.
		/// </summary>
		public void SetMusicEnabled(bool enabled)
		{
			_musicEnabled = enabled;
			PlayerPrefs.SetInt(MusicPrefKey, enabled ? 1 : 0);
			PlayerPrefs.Save();
			ApplyMusicState();
		}

		/// <summary>
		/// Enable or disable sound effects and persist choice.
		/// </summary>
		public void SetSoundEnabled(bool enabled)
		{
			_soundEnabled = enabled;
			PlayerPrefs.SetInt(SoundPrefKey, enabled ? 1 : 0);
			PlayerPrefs.Save();
			ApplySoundState();
		}

		#endregion

		#region Internal Helpers

		private void ApplyMusicState()
		{
			if (musicSource == null) return;

			if (_musicEnabled)
			{
				musicSource.mute = false;
				if (musicSource.clip != null && musicSource.time > 0f)
				{
					musicSource.UnPause();
				}
				else
				{
					if (!musicSource.isPlaying)
						musicSource.Play();
				}
			}
			else
			{
				if (musicSource.isPlaying)
				{
					musicSource.Pause();
				}
				musicSource.mute = true;
			}
		}

		private void ApplySoundState()
		{
			if (blockClickSource == null) return;
			blockClickSource.mute = !_soundEnabled;
		}

		#endregion
	}
}