using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * WindowsPhoneBinding - za Windows Phone 8
 * Objekat za koji je vezana skripta ce biti nazvan WindowsPhoneBinding
 * Treba postoji po jedan objekat u svakoj sceni koja ce razmenjivati poruke sa windows phone-om.
 * */

public class WindowsPhoneBinding : MonoBehaviour {

	////////////////////////////////////////////////////
	//// Event Handler-i koji prosledjuju poruke windows phone-u.
	////
	//// Ne koristiti iz unitija.
	public static event EventHandler ShowAdInterstitialHandler;
	public static event EventHandler ShowAdBannerHandler;
	public static event EventHandler HideAdBannerHandler;
	
	public static event EventHandler PurchaseInAppHandler;
	public static event EventHandler RestoreInAppHandler;
	public static event EventHandler RequestPricesHandler;
	
	public static WindowsPhoneBinding windowsPhoneBinding;

	public static Dictionary<string, float> allPrices;
	
	static WindowsPhoneBinding instance;
	
	public static WindowsPhoneBinding Instance
	{
		get
		{
			if(instance == null)
				instance = GameObject.FindObjectOfType(typeof(WindowsPhoneBinding)) as WindowsPhoneBinding;
			
			return instance;
		}
	}
	
	void Awake()
	{
		name = "WindowsPhoneBinding";
		
		WindowsPhoneBinding.HideAdBanner ();
		
		if (windowsPhoneBinding == null)
		{
			DontDestroyOnLoad(gameObject);
			windowsPhoneBinding = this;
		}
		else if (windowsPhoneBinding != this)
		{
			Destroy(gameObject);
		}

		// "KupioDaNemaReklama" = RemoveAds inapp, tako da kad 
		// se kupi ovaj inapp staviti prefs za ovaj kljuc na vrednost razlicitu od 0  
		if (!PlayerPrefs.HasKey ("KupioDaNemaReklame"))
		{
			PlayerPrefs.SetInt ("KupioDaNemaReklame", 0);
		}
		
	}
	
	// Funkcija proverava da li je kupljen inapp za skidanje
	// reklama i prikazuje interstitial reklamu u odnosu na to
	public static void ShowAdInterstitial()
	{
		int removeAds = PlayerPrefs.GetInt("KupioDaNemaReklame");
		
		if(removeAds == 0)
		{
			if(ShowAdInterstitialHandler != null)
			{
				ShowAdInterstitialHandler(null, null);
			}
		}
		
	}
	
	// Funkcija proverava da li je kupljen inapp za skidanje
	// reklama i prikazuje banner u odnosu na to
	public static void ShowAdBanner()
	{
		int removeAds = PlayerPrefs.GetInt("KupioDaNemaReklame");
		
		if(removeAds == 0)
		{
			if(ShowAdBannerHandler != null)
			{
				ShowAdBannerHandler(null, null);
			}
		}
	}
	
	// Funkcija proverava da li je kupljen inapp za skidanje
	// reklama i sakriva banner u odnosu na to
	public static void HideAdBanner()
	{
		int removeAds = PlayerPrefs.GetInt("KupioDaNemaReklame");
		
		if(removeAds == 0)
		{
			if(HideAdBannerHandler != null)
			{
				HideAdBannerHandler(null, null);
			}
		}
	}

	// Saljemo nativu naziv inappa i na osnovu rezultata kupovine
	// cemo dobiti odgovor nazad u jednoj od sledecih funkcija
	public static void PurchaseInApp(string inAppId)
	{
		if(PurchaseInAppHandler != null)
		{
			PurchaseInAppHandler(inAppId, null);
		}
	}

	// Ovu funkciju popunjavamo u odnosu na potrebe aplikacije
	// ako je uspesna transakcija i kupljen inapp sa unetim idjem
	public static void InAppSuccessfullyPurchased(string inAppId)
	{
		Debug.Log("Inapp " + inAppId + "  successfully purchased!");
		GameObject.Find("Canvas/Panel/Message").GetComponent<Text>().text = "Inapp " + inAppId + "  successfully purchased!";
	}

	// Saljemo nativu naziv inappa i na osnovu rezultata
	// cemo dobiti odgovor da li je inapp vec kupljen ili ne
	// u nekoj od donjih funkcija
	public static void RestoreInApp(string inAppId)
	{
		if(RestoreInAppHandler != null)
		{
			RestoreInAppHandler(inAppId, null);
		}
	}

	// Ovu funkciju zovemo iz native-a ako je inApp kupljen i treba ga vratiti
	public static void InAppSuccessfullyRestored(string inAppId)
	{
		Debug.Log("Inapp " + inAppId + "  successfully restored!");
		GameObject.Find("Canvas/Panel/Message").GetComponent<Text>().text = "Inapp " + inAppId + "  successfully restored!";
	}

	// Pozivamo za uzimanje svih cena
	public static void GetAllPrices()
	{
		if(RequestPricesHandler != null)
		{
			RequestPricesHandler(null, null);
		}
	}

	// Setujemo sve cene u dict kako bi smo ih mogli koristiti uvek
	public static void SetAllPrices(string prices)
	{
		allPrices = new Dictionary<string, float>();

		string[] items = prices.Split(',');

		for (int i = 0; i < items.Length; i++)
		{
			allPrices.Add(items[i].Split('#')[0], float.Parse(items[i].Split('#')[1]));
		}

		GameObject.Find("Canvas/Panel/Message").GetComponent<Text>().text = "Prices successfully set!";
	}

	// Uzivanje jedne cene iz dicta, mora biti barem
	// jednom pozvana funkcija koja setuje sve cene
	public static float GetSinglePrice(string inAppId)
	{
		return allPrices[inAppId];
	}

	// Za testiranje uzimanja jedne cene
	public static void SinglePrice(string id)
	{
		GameObject.Find("Canvas/Panel/Message").GetComponent<Text>().text = "Cena za inapp:" + id + "   je  " + GetSinglePrice(id).ToString();
	}
}
