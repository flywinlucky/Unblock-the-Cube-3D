using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
	[Header("UI References")]
	[Tooltip("Butonul pentru sound (poate fi Button sau doar Image).")]
	public Button soundButton;
	[Tooltip("Butonul pentru music (poate fi Button sau doar Image).")]
	public Button musicButton;

	[Header("Sound Sprites")]
	public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    
	[Header("Music Sprites")]
	public Sprite musicOnSprite;
	public Sprite musicOffSprite;

	[Header("Audio Integration")]
	[Tooltip("Referință la AudioManager din scenă (opțional).")]
	public AudioManager audioManager;

	// stările curente
	private bool _soundEnabled = true;
	private bool _musicEnabled = true;
	private string soundKey = "SoundEnabled";
	private string musicKey = "MusicEnabled";

	private void Start()
	{
		// Încarcă stările din PlayerPrefs (implicit ON)
		_soundEnabled = PlayerPrefs.GetInt(soundKey, 1) == 1;
		_musicEnabled = PlayerPrefs.GetInt(musicKey, 1) == 1;

		// Asigură referințele la Image dacă butoane sunt folosite
		EnsureButtonImages();

		// Aplică sprite-urile inițiale
		ApplySoundSprite();
		ApplyMusicSprite();

		// --- Adăugăm listeneri la butoane (dacă sunt setate) ---
		if (soundButton != null)
		{
			// evităm adăugarea dublă
			soundButton.onClick.RemoveListener(ToggleSound);
			soundButton.onClick.AddListener(ToggleSound);
		}
		if (musicButton != null)
		{
			musicButton.onClick.RemoveListener(ToggleMusic);
			musicButton.onClick.AddListener(ToggleMusic);
		}

		// --- Aplicăm stările în AudioManager (dacă există) ---
		if (audioManager != null)
		{
			audioManager.SetSoundEnabled(_soundEnabled);
			audioManager.SetMusicEnabled(_musicEnabled);
		}
	}

	private void EnsureButtonImages()
	{
		// Dacă soundButton există, ne asigurăm că are Image și îl asignăm butonului
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
	}

	// Eliminăm listenerii la distrugere pentru siguranță
	private void OnDestroy()
	{
		if (soundButton != null) soundButton.onClick.RemoveListener(ToggleSound);
		if (musicButton != null) musicButton.onClick.RemoveListener(ToggleMusic);
	}

	// Funcții publice pentru OnClick (legate din Inspector)
	public void ToggleSound()
	{
		_soundEnabled = !_soundEnabled;
		PlayerPrefs.SetInt(soundKey, _soundEnabled ? 1 : 0);
		PlayerPrefs.Save();
		ApplySoundSprite();

		// Aplicăm imediat în AudioManager (dacă e setat)
		if (audioManager != null)
		{
			audioManager.SetSoundEnabled(_soundEnabled);
		}
		// TODO: aici poți adăuga logică reală pentru a opri/porni sunetul global
	}

	public void ToggleMusic()
	{
		_musicEnabled = !_musicEnabled;
		PlayerPrefs.SetInt(musicKey, _musicEnabled ? 1 : 0);
		PlayerPrefs.Save();
		ApplyMusicSprite();

		// Aplicăm imediat în AudioManager (dacă e setat)
		if (audioManager != null)
		{
			audioManager.SetMusicEnabled(_musicEnabled);
		}
		// TODO: aici poți adăuga logică reală pentru a opri/porni muzica
	}

	// Helpers pentru schimbat sprite pe buton / image
	private void ApplySoundSprite()
	{
		if (soundButton != null && soundButton.image != null)
		{
			soundButton.image.sprite = _soundEnabled ? soundOnSprite : soundOffSprite;
		}
		else
		{
			// fallback: căutăm o Image cu tag "SoundImage" (opțional)
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

	// mic helper pentru fallback; returnează prima Image găsită cu numele dat
	private Image TryFindImageByName(string name)
	{
		GameObject go = GameObject.Find(name);
		if (go != null) return go.GetComponent<Image>();
		return null;
	}
}
