using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	[Header("Audio Sources")]
	[Tooltip("Background music AudioSource (looping).")]
	public AudioSource musicSource;
	[Tooltip("SFX AudioSource for clicks / UI / blocks.")]
	public AudioSource sfxSource;

	[Header("Optional Clips")]
	[Tooltip("Clip pentru sunet click bloc (folosit cu PlaySFX).")]
	public AudioClip blockClickClip;

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

	// Play / Stop pentru muzica de background
	public void PlayMusic()
	{
		if (musicSource == null) return;
		if (!_musicEnabled) return;
		if (!musicSource.isPlaying)
		{
			musicSource.Play();
		}
	}

	public void StopMusic()
	{
		if (musicSource == null) return;
		if (musicSource.isPlaying)
		{
			musicSource.Stop();
		}
	}

	// Redă un SFX (ex: click de bloc). Dacă clip e null folosește implicita blockClickClip.
	public void PlaySFX(AudioClip clip = null)
	{
		if (sfxSource == null || !_soundEnabled) return;
		AudioClip toPlay = clip != null ? clip : blockClickClip;
		if (toPlay == null) return;
		sfxSource.PlayOneShot(toPlay);
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

	private void ApplyMusicState()
	{
		if (musicSource == null) return;
		musicSource.mute = !_musicEnabled;
		if (_musicEnabled) PlayMusic();
		else StopMusic();
	}

	private void ApplySoundState()
	{
		if (sfxSource == null) return;
		sfxSource.mute = !_soundEnabled;
	}
}
