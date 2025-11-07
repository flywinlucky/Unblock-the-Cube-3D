using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine.UI;

public class LevelsParser : MonoBehaviour {

	// Promenljiva u kojoj cemo da cuvamo podatke iz xmla
	public XmlDocument xmlDocument;
	public XmlNodeList packs;

	// Poslednji otkljucani nivo
	public int lastUnlockedPack;
	public int lastUnlockedWorld;
	public int lastUnlockedLevel;

	// Izabrani nivo za gameplay
	public static int selectedPack;
	public static int selectedWorld;
	public static int selectedLevel;

	// Prefabovi za pack, world i level
	public GameObject packPrefab;
	public GameObject worldTabPrefab;
	public GameObject levelPrefab;

	// Native prefab
//	public GameObject nativeAdPrefab;

	public static LevelsParser levelParser;

	void Awake()
	{
		xmlDocument = new XmlDocument();
		TextAsset xmlData = new TextAsset();
		xmlData = Resources.Load("WordSearch") as TextAsset;

		xmlDocument.LoadXml(xmlData.text);

		packs = xmlDocument.SelectNodes("/xml/category");

		if (GlobalVariables.stars == 0)
		{
			lastUnlockedPack = 0;
			lastUnlockedWorld = 0;
			lastUnlockedLevel = 0;

			if (!PlayerPrefs.HasKey("LastUnlockedPack"))
				PlayerPrefs.SetInt("LastUnlockedPack", 0);

			if (!PlayerPrefs.HasKey("LastUnlockedWorld"))
				PlayerPrefs.SetInt("LastUnlockedWorld", 0);

			if (!PlayerPrefs.HasKey("LastUnlockedLevel"))
				PlayerPrefs.SetInt("LastUnlockedLevel", 0);
		}
		else
		{
			lastUnlockedPack = PlayerPrefs.GetInt("LastUnlockedPack");
			lastUnlockedWorld = PlayerPrefs.GetInt("LastUnlockedWorld");
			lastUnlockedLevel = PlayerPrefs.GetInt("LastUnlockedLevel");
		}
//
//		// FIXMEvbrisi
//		int levelCounter = 0;
//		for (int i = 0; i < packs.Count; i++)
//		{
//			for (int j = 0; j < packs[i].ChildNodes.Count; j++)
//			{
//				levelCounter += packs[i].ChildNodes[j].ChildNodes.Count;
//			}
//		}
//
//		Debug.Log(levelCounter);
			
		levelParser = this;
		DontDestroyOnLoad(this);
	}

	public void SetPackWorlds()
	{
		// Pronalazimo objekat koji sadrzi packove
		Transform packsHolder = GameObject.Find("WorldPacksHolder").transform;

		// Za svaki pack prolazimo kroz sve svetove
		for (int i = 0; i < packs.Count; i++)
		{
			// Kreiramo holder za pack
			GameObject newPack = Instantiate(packPrefab, packsHolder) as GameObject;
			newPack.transform.localScale = Vector3.one;

			// Prolazimo kroz svaki svet
			for (int j = 0; j < packs[i].ChildNodes.Count; j++)
			{
				int numberOfStarsForThisWorld = 0;

				// Kreiramo tab za svaki svet iz ovog packa, takodje stavljamo i listener za pokretanje ovog sveta
				GameObject newWorld = Instantiate(worldTabPrefab, newPack.transform.GetChild(0).transform) as GameObject;
				newWorld.transform.localScale = Vector3.one;

				// Proveravamo da li je pack poslednji otkljucani
				if (i == lastUnlockedPack)
				{
					if (j > lastUnlockedWorld)
					{
						// Nije otkljucan, stavljamo transparenciju za ovaj tab
						newWorld.GetComponent<CanvasGroup>().alpha = 0.3f;
					}
					else if (j < lastUnlockedWorld) // Ovaj je zavrsem pa mu ukljucujemo badge
					{
						newWorld.transform.Find("AnimationHolder/BadgeImage").gameObject.SetActive(true);
					}
				}
				else if (i > lastUnlockedPack) // Podesavamo za zakljucane packove
				{
					// Ovde su svi nivoi sigurno zakljucani
					newWorld.GetComponent<CanvasGroup>().alpha = 0.3f;
				}
				else // Ostaju nam samo otkljucani packovi
				{
					// Ovde su svi nivoi otklucani pa im prikazujemo badgeve
					newWorld.transform.Find("AnimationHolder/BadgeImage").gameObject.SetActive(true);
				}

				// Setujemo indexe za klick na odabrani world
				newWorld.transform.GetChild(1).GetComponent<WorldLevelScript>().packIndex = i;
				newWorld.transform.GetChild(1).GetComponent<WorldLevelScript>().worldIndex = j;

				// Setujemo ime sveta i ponudjena slova
				newWorld.transform.Find("AnimationHolder/WorldName").GetComponent<Text>().text = packs[i].ChildNodes[j].Attributes["title"].Value;
				newWorld.transform.Find("AnimationHolder/LetterNumberText").GetComponent<Text>().text = packs[i].ChildNodes[j].Attributes["maxletters"].Value + " letters max";

				// Racunamo broj zvezdica za ovaj svet
				for (int k = 0; k < packs[i].ChildNodes[j].ChildNodes.Count; k++)
				{
					numberOfStarsForThisWorld += packs[i].ChildNodes[j].ChildNodes[k].SelectSingleNode("words").InnerText.Split(',').Length;
				}

				newWorld.transform.Find("AnimationHolder/StarNumber").GetComponent<Text>().text = numberOfStarsForThisWorld.ToString();

				// Dodajemo native ad ako je poslednji element iz ovog sveta // FIXME dodato da na svakom parnom imam native
//				if (i % 2 == 0 && j == packs[i].ChildNodes.Count - 1 && !GlobalVariables.removeAdsOwned)
//				{
//					GameObject nativeAd = Instantiate(nativeAdPrefab , newPack.transform.GetChild(0).transform) as GameObject;
//					nativeAd.transform.localScale = Vector3.one;
//					LevelSelectManager.levelSelectManager.listOfNativeAds.Add(nativeAd);
//				}
			}

			// Setujemo ime packa
			newPack.transform.Find("Tabs/TitleHolder").GetComponent<Text>().text = packs[i].Attributes["name"].Value;

			// Ako je ceo pack predjen onda prikazujemo i badge za njega
			if (i < lastUnlockedPack)
				newPack.transform.Find("Tabs/TitleHolder/WorldCompletedHolder").gameObject.SetActive(true);

			newPack.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(newPack.GetComponent<RectTransform>().anchoredPosition3D.x, newPack.GetComponent<RectTransform>().anchoredPosition3D.y, 0f);
		}

		// Setujemo tabove nakon zavrsenog frejma
		StartCoroutine(SetTabsAppropriately(packsHolder));
	}

	public void SetTabs(Transform packsHolder)
	{
		StartCoroutine(SetTabsAppropriately(packsHolder));
	}

	IEnumerator SetTabsAppropriately(Transform packsHolder)
	{
		yield return new WaitForEndOfFrame();

		for (int i = 0; i < packsHolder.childCount; i++)
		{
			packsHolder.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(packsHolder.GetChild(i).GetComponent<RectTransform>().sizeDelta.x, packsHolder.GetChild(i).GetChild(0).GetComponent<RectTransform>().sizeDelta.y);
		}

		packsHolder.GetComponent<ContentSizeFitter>().enabled = false;
		packsHolder.GetComponent<ContentSizeFitter>().enabled = true;

		//transform.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

		yield return new WaitForEndOfFrame();

		// Pozicioniranje scrollrecta na poslednje izabrani nivo/svet FIXME ovde treba da se napravi da se pozicionira 
		if (packsHolder.name == "LevelPacksHolder")
		{
			// Stavljam offset 200 cisto da pomerim na dole jos malo scroll rect
			if (selectedPack == lastUnlockedPack && selectedWorld == lastUnlockedWorld)
			{
				SnapTo(packsHolder.GetChild(0).GetChild(0).GetChild(selectedLevel + 1).GetComponent<RectTransform>(), 
					packsHolder.GetComponent<RectTransform>(), packsHolder.parent.GetComponent<ScrollRect>());
//				packsHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(packsHolder.GetComponent<RectTransform>().anchoredPosition.x, packsHolder.GetChild(0).GetChild(0).GetChild(selectedLevel + 1).GetComponent<RectTransform>().anchoredPosition.y - packsHolder.GetChild(0).GetChild(0).GetChild(selectedLevel + 1).GetComponent<RectTransform>().sizeDelta.y / 2f - packsHolder.GetComponent<VerticalLayoutGroup>().spacing + 200f);
			}
			else
			{
				packsHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(packsHolder.GetComponent<RectTransform>().anchoredPosition.x, -packsHolder.GetComponent<RectTransform>().sizeDelta.y / 2 + 200f);
			}

			// Ukljucujemo native reklamu
//			if (!GlobalVariables.removeAdsOwned)
//				packsHolder.GetChild(0).Find("Tabs/NativeAdHolder").GetComponent<LevelSelectNativeAd>().enabled = true;
		}
		else if (packsHolder.name == "WorldPacksHolder")
		{
//			packsHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(packsHolder.GetComponent<RectTransform>().anchoredPosition.x, (packsHolder.GetChild(selectedPack).GetComponent<RectTransform>().anchoredPosition.y - packsHolder.GetComponent<RectTransform>().sizeDelta.y / 2 + packsHolder.GetChild(selectedPack).GetChild(0).GetChild(selectedWorld + 1).GetComponent<RectTransform>().anchoredPosition.y));
//			packsHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(packsHolder.GetComponent<RectTransform>().anchoredPosition.x, 
//				packsHolder.GetComponent<RectTransform>().sizeDelta.y / 2f
//				- packsHolder.GetChild(selectedPack).GetComponent<RectTransform>().anchoredPosition.y
//				- packsHolder.GetChild(selectedPack).GetChild(0).GetChild(selectedWorld).GetComponent<RectTransform>().anchoredPosition.y
//				+ packsHolder.GetChild(selectedPack).GetComponent<RectTransform>().sizeDelta.y / 2f
//				+ packsHolder.GetChild(selectedPack).GetChild(0).GetChild(selectedWorld).GetComponent<RectTransform>().sizeDelta.y / 2f
//				+ packsHolder.GetComponent<VerticalLayoutGroup>().spacing);
			SnapTo(packsHolder.GetChild(selectedPack).GetChild(0).GetChild(selectedWorld).GetComponent<RectTransform>(), 
				packsHolder.GetComponent<RectTransform>(), packsHolder.parent.GetComponent<ScrollRect>());

//			// Ukljucujemo native reklame
//			if (!GlobalVariables.removeAdsOwned)
//			{
//				for (int i = 0; i < LevelSelectManager.levelSelectManager.listOfNativeAds.Count; i++)
//				{
//					LevelSelectManager.levelSelectManager.listOfNativeAds[i].GetComponent<LevelSelectNativeAd>().enabled = true;
//				}
//			}
		}
	}

	public void SnapTo(RectTransform target, RectTransform contentPanel, ScrollRect scrollRect)
	{
		Canvas.ForceUpdateCanvases();

		contentPanel.anchoredPosition =
			(Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
			- (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
	}
		
	// Za selektovani svet setujemo parametre za levele
	public void SetWorldLevels(int packIndex, int worldIndex)
	{
		string worldName = packs[packIndex].ChildNodes[worldIndex].Attributes["title"].Value;

		// Pronalazimo objekat koji sadrzi packove
		Transform packsHolder = GameObject.Find("LevelPacksHolder").transform;

		// Kreiramo jedan pack holder za sve levele osim ako nije vec kreiran
		GameObject newPack;
		if (packsHolder.childCount == 0)
		{
			newPack = Instantiate(packPrefab, packsHolder) as GameObject;
			newPack.transform.localScale = Vector3.one;
		}
		else
		{
			newPack = packsHolder.GetChild(0).gameObject;
		}

		// U odnosu na to da li smo vec kreirali tabove za level imamo x slucaja
		// Ako nismo do sada kreirali tabove za levele - kreiramo ih sve za selektovani svet
		if (packsHolder.GetChild(0).GetChild(0).childCount == 1)
		{
			// Kreiramo holdere za level
			for (int i = 0; i < packs[packIndex].ChildNodes[worldIndex].ChildNodes.Count; i++)
			{
				// Kreiramo tab za svaki level iz selektovanog packa i sveta, takodje stavljamo i listener za pokretanje ovog levela
				GameObject newLevel = Instantiate(levelPrefab, newPack.transform.GetChild(0).transform) as GameObject;
				newLevel.transform.localScale = Vector3.one;
				newLevel.name = "Level" + (i + 1).ToString();

				// Proveravamo da li je pack poslednji otkljucani
				if (selectedPack == lastUnlockedPack)
				{
					// Poslednji svet u ovom packu
					if (selectedWorld == lastUnlockedWorld)
					{
						if (i >= lastUnlockedLevel) // Leveli koji nisu otkljucani
						{
							if (i > lastUnlockedLevel)
							{
								// Nije otkljucan, stavljamo transparenciju za ovaj tab
								newLevel.GetComponent<CanvasGroup>().alpha = 0.3f;
							}

							// Stavljamo text da level nije predjen
							newLevel.transform.Find("AnimationHolder/LevelSolvedText").GetComponent<Text>().text = "Not solved";
						}
						else if (i < lastUnlockedLevel) // Otkljucani
						{
							// Stavljamo text da je level predjen
							newLevel.transform.Find("AnimationHolder/LevelSolvedText").GetComponent<Text>().text = "Solved";
						}
					}
					else // Posto moze samo da se otvori svet koji je manji od trenutno otkljucanog onda sigurno stavljamo da je level predjen
					{
						// Stavljamo text da je level predjen
						newLevel.transform.Find("AnimationHolder/LevelSolvedText").GetComponent<Text>().text = "Solved";
					}
				}

				// Postavljamo ime nivoa
				newLevel.transform.Find("AnimationHolder/LevelNumberName").GetComponent<Text>().text = "Level " + (i + 1).ToString();

				// Setujemo event za klick na odabrani world
				newLevel.transform.GetChild(1).GetComponent<WorldLevelScript>().levelIndex = i;

//				// Ako je level trenutno izabrani level stavljamo i native ad odmah iza njega
//				if (i == selectedLevel && !GlobalVariables.removeAdsOwned)
//				{
//					GameObject nativeAd = Instantiate(nativeAdPrefab , newPack.transform.GetChild(0).transform) as GameObject;
//					nativeAd.transform.localScale = Vector3.one;
//					nativeAd.name = "NativeAdHolder";
//					LevelSelectManager.levelSelectManager.listOfNativeAds.Add(nativeAd);
//				}
			}
		}
		else // Vec postoje tabovi za levele
		{
			// Prvo pronalazimo native ad i postavljamo ga kao poslednje dete
			if (packsHolder.GetChild(0).GetChild(0).Find("NativeAdHolder") != null)
			{
				if (!GlobalVariables.removeAdsOwned)
					packsHolder.GetChild(0).GetChild(0).Find("NativeAdHolder").SetAsLastSibling();
				else
					Destroy(packsHolder.GetChild(0).GetChild(0).Find("NativeAdHolder").gameObject);
			}

			// Za selektovani svet proveravamo u odnosu na broj tabova koje smo kreirali da li trebamo da dodamo neke ili da iskljucimo
			if (packsHolder.GetChild(0).GetChild(0).childCount > packs[packIndex].ChildNodes[worldIndex].ChildNodes.Count + 1) // Ima vise tabova - 1 zbog title holdera
			{
				// Gasimo ostatak tabova
				for (int i = packs[packIndex].ChildNodes[worldIndex].ChildNodes.Count + 1; i < packsHolder.GetChild(0).GetChild(0).childCount /*- 1*/; i++)
				{
					packsHolder.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(false);	
				}
			}
			else if (packsHolder.GetChild(0).GetChild(0).childCount < packs[packIndex].ChildNodes[worldIndex].ChildNodes.Count + 1)
			{
				// Kreiramo tabove koji nedostaju
				for (int i = packsHolder.GetChild(0).GetChild(0).childCount; i < packs[packIndex].ChildNodes[worldIndex].ChildNodes.Count + 1; i ++)
				{
					GameObject newLevel = Instantiate(levelPrefab, newPack.transform.GetChild(0).transform) as GameObject;
					newLevel.transform.localScale = Vector3.one;
					newLevel.name = "Level" + (i + 1).ToString();

					// Setujemo event za klick na odabrani world
					newLevel.transform.GetChild(1).GetComponent<WorldLevelScript>().levelIndex = i;
				}
			}

			// Setujemo sve levele
			for (int i = 0; i < packs[packIndex].ChildNodes[worldIndex].ChildNodes.Count; i++)
			{
				// Proveravamo da li je pack poslednji otkljucani
				if (selectedPack == lastUnlockedPack)
				{
					// Poslednji svet u ovom packu
					if (selectedWorld == lastUnlockedWorld)
					{
						if (i >= lastUnlockedLevel) // Leveli koji nisu otkljucani
						{
							if (i > lastUnlockedLevel)
							{
								// Nije otkljucan, stavljamo transparenciju za ovaj tab
								packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).GetComponent<CanvasGroup>().alpha = 0.3f;
							}

							// Stavljamo text da level nije predjen
							packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).transform.Find("AnimationHolder/LevelSolvedText").GetComponent<Text>().text = "Not solved";
						}
						else if (i < lastUnlockedLevel) // Otkljucani
						{
							// Stavljamo text da je level predjen
							packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).transform.Find("AnimationHolder/LevelSolvedText").GetComponent<Text>().text = "Solved";
						}
					}
					else // Posto moze samo da se otvori svet koji je manji od trenutno otkljucanog onda sigurno stavljamo da je level predjen
					{
						// Sigurno je otkljucan otkljucan, stavljamo transparenciju za ovaj tab
						packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).GetComponent<CanvasGroup>().alpha = 1f;

						// Stavljamo text da je level predjen
						packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).transform.Find("AnimationHolder/LevelSolvedText").GetComponent<Text>().text = "Solved";
					}
				}
				else
				{
					// Sigurno je otkljucan otkljucan, stavljamo transparenciju za ovaj tab
					packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).GetComponent<CanvasGroup>().alpha = 1f;

					// Stavljamo text da je level predjen
					packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).transform.Find("AnimationHolder/LevelSolvedText").GetComponent<Text>().text = "Solved";
				}

				// Postavljamo ime nivoa
				packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).transform.Find("AnimationHolder/LevelNumberName").GetComponent<Text>().text = "Level " + (i + 1).ToString();

				// Aktiviramo objekat za svaki slucaj
				packsHolder.GetChild(0).GetChild(0).GetChild(i + 1).gameObject.SetActive(true);
			}

			// Setujemo native iza poslednje izabranog nivoa
			for (int i = selectedLevel + 1; i < packs[packIndex].ChildNodes[worldIndex].ChildNodes.Count; i++)
			{
				packsHolder.GetChild(0).GetChild(0).GetChild(selectedLevel + 2).SetAsLastSibling();
			}
		}

		//Setujemo ime sveta
		newPack.transform.Find("Tabs/TitleHolder").GetComponent<Text>().text = packs[packIndex].ChildNodes[worldIndex].Attributes["title"].Value;

		newPack.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(newPack.GetComponent<RectTransform>().anchoredPosition3D.x, newPack.GetComponent<RectTransform>().anchoredPosition3D.y, 0f);

		// Setujemo tabove nakon zavrsenog frejma
		StartCoroutine(SetTabsAppropriately(packsHolder));
	}

	// TODO NAPOMENA za proveru da li je poslednji level u tom svetu, tj. da li treba da se poveca jedan na world
	// Koristi se packs[indexPacka].ChildNodes[indexSveta].ChildNodes.Count  i proverava u odnosu na selected level
	// kao i u odnosu na last unlocked level, pa ako je selected == last i plus je index selektovanog 
	// levela == packs[indexPacka].ChildNodes[indexSveta].ChildNodes.Count - 1, tada povecavamo svet za 1 i proveravamo
	// da li je to poslednji svet u packu, pa ako jeste povecavamo i poslednji pack


	// Fukcija koja ce se stavljati na pokretanje sveta koja menja panel na level select i koja setuje levele za taj svet 
	public void WorldSelected()
	{
		// Prikazujemo level select menu
		LevelSelectManager.levelSelectManager.WorldSelected();
	}

	// Funkcija koja ce se stavljati za pokretanje levela
	public void LevelSelected()
	{
		// Prikazivanje tranzicije i pokretanje level scene
		LevelSelectManager.levelSelectManager.LevelSelected();
	}

	// Funkcija koja vraca ponudjena slova za odabrani nivo
	public void SetLevelParameters()
	{
		string letters = packs[selectedPack].ChildNodes[selectedWorld].ChildNodes[selectedLevel].SelectSingleNode("letters").InnerText;
		string words = packs[selectedPack].ChildNodes[selectedWorld].ChildNodes[selectedLevel].SelectSingleNode("words").InnerXml;
		string additionalWords = packs[selectedPack].ChildNodes[selectedWorld].ChildNodes[selectedLevel].SelectSingleNode("additional_words").InnerText;
		string solvedWords = packs[selectedPack].ChildNodes[selectedWorld].ChildNodes[selectedLevel].SelectSingleNode("solved_words").InnerText;

		GameplayManager.gameplayManager.offeredLetters.Clear();
		GameplayManager.gameplayManager.targetWords.Clear();
		GameplayManager.gameplayManager.additionalWords.Clear();
		GameplayManager.gameplayManager.solvedWords.Clear();

		if (packs[selectedPack].ChildNodes[selectedWorld].ChildNodes[selectedLevel].Attributes["bonus"].Value == "yes")
			GameplayManager.isBonus = true;

		GameplayManager.gameplayManager.offeredLetters = letters.Split(',').ToList();

		// Ako je bonus level dodajemo i additional slovca
		if (GameplayManager.isBonus)
		{
			GameplayManager.gameplayManager.offeredBonusLetters = packs[selectedPack].ChildNodes[selectedWorld].ChildNodes[selectedLevel].SelectSingleNode("additional_letters").InnerText.Split(',').ToList();
//			GameplayManager.gameplayManager.offeredLetters.AddRange(GameplayManager.gameplayManager.offeredBonusLetters);
		}

		GameplayManager.gameplayManager.targetWords = words.Split(',').ToList();
		GameplayManager.gameplayManager.additionalWords = additionalWords.Split(',').ToList();
		GameplayManager.gameplayManager.solvedWords = solvedWords.Split(',').ToList();
	}

	// Proveravamo da li je poslednji otkljucani nivo zavrsen i ako jeste setujemo nove promenljive
	public void CheckIfLastLevelWasFinished()
	{
		if (lastUnlockedPack == selectedPack && lastUnlockedWorld == selectedWorld && lastUnlockedLevel == selectedLevel)
		{
			// Proveravamo da li je poslednji u izabranom svetu
			if (selectedLevel < packs[selectedPack].ChildNodes[selectedWorld].ChildNodes.Count - 1)
			{
				// Nije poslednji i samo povecavamo da je poslednji otkljucani nivo za jedan veci
				lastUnlockedLevel += 1;
				PlayerPrefs.SetInt("LastUnlockedLevel", lastUnlockedLevel);
				PlayerPrefs.Save();

				Debug.Log("Level povecan");
			}
			else if (selectedWorld < packs[selectedPack].ChildNodes.Count - 1) // Level je poslednji, pa proveravamo da li je world poslednji
			{
				lastUnlockedLevel = 0;
				lastUnlockedWorld += 1;
				PlayerPrefs.SetInt("LastUnlockedLevel", lastUnlockedLevel);
				PlayerPrefs.SetInt("LastUnlockedWorld", lastUnlockedWorld);
				PlayerPrefs.Save();
				Debug.Log("World povecan");
			}
			else if (lastUnlockedPack < packs.Count - 1) // Ovde proveravamo samo da li ima jos packova, ako ima otkljucavamo sledeci pack
			{
				lastUnlockedPack += 1;
				lastUnlockedWorld = 0;
				lastUnlockedLevel = 0;
				PlayerPrefs.SetInt("LastUnlockedLevel", lastUnlockedLevel);
				PlayerPrefs.SetInt("LastUnlockedWorld", lastUnlockedWorld);
				PlayerPrefs.SetInt("LastUnlockedPack", lastUnlockedPack);
				PlayerPrefs.Save();
				Debug.Log("Pack povecan");
			}

			GlobalVariables.stars += GameplayManager.gameplayManager.targetWords.Count;
			PlayerPrefs.SetInt("Stars", GlobalVariables.stars);
			PlayerPrefs.Save();
		}
	}

	public void IncrementLastSelectedLevel()
	{
		// Ako ima jos nivoa u ovom svetu onda samo povecamo selektovani nivo za 1
		if (selectedLevel < packs[selectedPack].ChildNodes[selectedWorld].ChildNodes.Count - 1)
		{
			selectedLevel++;
		}
		else if (selectedWorld < packs[selectedPack].ChildNodes.Count - 1) // Nema vise nivoa, moramo da predjemo na sledeci
		{
			selectedWorld++;
			selectedLevel = 0;
		}
		else if (selectedPack < packs.Count - 1)
		{
			selectedPack++;
			selectedWorld = 0;
			selectedLevel = 0;
		}
	}
}
