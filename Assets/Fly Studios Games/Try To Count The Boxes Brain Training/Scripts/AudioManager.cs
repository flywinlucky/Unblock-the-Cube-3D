using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	[Header("Audio Sources")]
	[Tooltip("Background music AudioSource (looping).")]
	public AudioSource musicSource;
	[Tooltip("AudioSource pentru sunetul de click al blocului (atașat direct cu clip).")]
	public AudioSource blockClickSource; // redenumit din sfxSource
	public AudioSource buttonClickSource; // redenumit din sfxSource
	public AudioSource buttonDoneClickSource;
	public AudioSource countDownClickSource;
	public AudioSource countShowResultSource;
	public AudioSource countShowRCubesSource;
	
	// Stări curente (cache)
	private bool _musicEnabled = true;
	private bool _soundEnabled = true;

	// Chei PlayerPrefs (dacă dorești să le folosești din AudioManager direct)
	public const string MusicPrefKey = "MusicEnabled";
	public const string SoundPrefKey = "SoundEnabled";

	private void Start()
	{
		// Opțional: inițializăm din PlayerPrefs dacă nu setăm din exterior
		_musicEnabled = PlayerPrefs.GetInt(MusicPrefKey, 1) == 1;
		_soundEnabled = PlayerPrefs.GetInt(SoundPrefKey, 1) == 1;

		ApplyMusicState();
		ApplySoundState();
	}

	// Play / Stop pentru muzica de background (Play păstrează comportamentul de start)
	public void PlayMusic()
	{
		if (musicSource == null) return;
		if (!_musicEnabled) return;
		if (!musicSource.isPlaying)
		{
			// Dacă a fost pus pe pauză, UnPause; altfel Play
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

	public void StopMusic()
	{
		if (musicSource == null) return;
		if (musicSource.isPlaying)
		{
			musicSource.Stop(); // forțează oprirea și resetează poziția
		}
	}

	// Redă sunetul de block click folosind clip-ul atașat pe blockClickSource
	public void PlayBlockClick()
	{
		if (blockClickSource == null || !_soundEnabled) return;
		AudioClip clip = blockClickSource.clip;
		if (clip == null)
		{
			// fallback: încercăm Play() dacă nu este clip dar e configurat AudioSource
			if (!blockClickSource.isPlaying) blockClickSource.Play();
			return;
		}
		blockClickSource.PlayOneShot(clip);
	}

	// Redă sunetul de button click folosind clip-ul atașat pe buttonClickSource
	public void PlayButtonClick()
	{
		if (buttonClickSource == null || !_soundEnabled) return;
		AudioClip clip = buttonClickSource.clip;
		if (clip == null)
		{
			// fallback: încercăm Play() dacă nu este clip dar e configurat AudioSource
			if (!buttonClickSource.isPlaying) buttonClickSource.Play();
			return;
		}
		buttonClickSource.PlayOneShot(clip);
	}

	// Redă sunetul de button done click folosind clip-ul atașat pe buttonDoneClickSource
	public void PlayButtonDoneClick()
	{
		if (buttonDoneClickSource == null || !_soundEnabled) return;
		AudioClip clip = buttonDoneClickSource.clip;
		if (clip == null)
		{
			// fallback: încercăm Play() dacă nu este clip dar e configurat AudioSource
			if (!buttonDoneClickSource.isPlaying) buttonDoneClickSource.Play();
			return;
		}
		buttonDoneClickSource.PlayOneShot(clip);
	}

	// Redă sunetul de countdown folosind clip-ul atașat pe countDownClickSource
	public void PlayCountdownClick()
	{
		if (countDownClickSource == null || !_soundEnabled) return;
		AudioClip clip = countDownClickSource.clip;
		if (clip == null)
		{
			// fallback: încercăm Play() dacă nu este clip dar e configurat AudioSource
			if (!countDownClickSource.isPlaying) countDownClickSource.Play();
			return;
		}
		countDownClickSource.PlayOneShot(clip);
	}

	// Redă sunetul pentru afișarea rezultatului folosind clip-ul atașat pe countShowResultSource
	public void PlayCountShowResult()
	{
		if (countShowResultSource == null || !_soundEnabled) return;
		AudioClip clip = countShowResultSource.clip;
		if (clip == null)
		{
			// fallback: încercăm Play() dacă nu este clip dar e configurat AudioSource
			if (!countShowResultSource.isPlaying) countShowResultSource.Play();
			return;
		}
		countShowResultSource.PlayOneShot(clip);
	}

	// Redă sunetul pentru afișarea rezultatelor pe "R cubes"
	public void PlayCountShowRCubes()
	{
		if (countShowRCubesSource == null || !_soundEnabled) return;
		AudioClip clip = countShowRCubesSource.clip;
		if (clip == null)
		{
			if (!countShowRCubesSource.isPlaying) countShowRCubesSource.Play();
			return;
		}
		countShowRCubesSource.PlayOneShot(clip);
	}
	
	// Setări enable/disable apelate din SettingsManager
	public void SetMusicEnabled(bool enabled)
	{
		_musicEnabled = enabled;
		PlayerPrefs.SetInt(MusicPrefKey, enabled ? 1 : 0);
		PlayerPrefs.Save();
		ApplyMusicState();
	}

	public void SetSoundEnabled(bool enabled)
	{
		_soundEnabled = enabled;
		PlayerPrefs.SetInt(SoundPrefKey, enabled ? 1 : 0);
		PlayerPrefs.Save();
		ApplySoundState();
	}

	// NOU: Expuse direct pentru control explicit (opțional)
	public void PauseMusic()
	{
		if (musicSource == null) return;
		if (musicSource.isPlaying)
		{
			musicSource.Pause();
		}
	}

	public void ResumeMusic()
	{
		if (musicSource == null) return;
		// Resume doar dacă există clip și a fost pus pe pauză anterior
		if (!musicSource.isPlaying && musicSource.clip != null)
		{
			musicSource.UnPause();
		}
	}

	private void ApplyMusicState()
	{
		if (musicSource == null) return;

		// Dacă muzica este activată -> reluăm (UnPause) sau pornim dacă nu a fost pornită
		if (_musicEnabled)
		{
			musicSource.mute = false;
			// Dacă sursa are clip și este într-o poziție > 0, reluăm; altfel pornim normal
			if (musicSource.clip != null && musicSource.time > 0f)
			{
				musicSource.UnPause();
			}
			else
			{
				// Play va porni de la început dacă nu a fost pornită anterior
				if (!musicSource.isPlaying)
					musicSource.Play();
			}
		}
		// Dacă muzica este dezactivată -> punem pe pauză (nu oprim complet, astfel păstrăm progresul)
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
		// mute/unmute toate sursele de sunet relevante
		if (blockClickSource != null) blockClickSource.mute = !_soundEnabled;
		if (buttonClickSource != null) buttonClickSource.mute = !_soundEnabled;
		if (buttonDoneClickSource != null) buttonDoneClickSource.mute = !_soundEnabled;
		if (countDownClickSource != null) countDownClickSource.mute = !_soundEnabled;
		if (countShowResultSource != null) countShowResultSource.mute = !_soundEnabled;
		if (countShowRCubesSource != null) countShowRCubesSource.mute = !_soundEnabled;
	}
}
