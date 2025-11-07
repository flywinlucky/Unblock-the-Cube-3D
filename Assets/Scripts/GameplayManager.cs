using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour {

	public GameObject wordHolder;
	public GameObject letterHolder;
	public GameObject emptyLetter;
	public GameObject selectedLetter;

	public GameObject targetWordsHolder;
	public GameObject targetWordsHolder1;
	public GameObject targetWordsHolder2;

	public List<string> targetWords;
	public List<string> offeredLetters;
	public List<string> offeredBonusLetters;
	public List<string> additionalWords;
	public List<string> solvedWords;

	public GameObject offeredLetterPrefab;
	public GameObject offeredLettersHolder;
	public GameObject selectedLettersHolder;

	public Sprite[] lettersSprites;

	[HideInInspector]
	public static bool gameFinished;

	public static bool isBonus;

	public GameObject loadingHolder;

	public Text coinsText;
	public Text coinsTextShop;

	// Promenljive za extra reci
	public Animator wordIntoCookieJarAnimator;
	public GameObject extraWordButton;

	public GameObject pausePopup;
	public GameObject shopMenu;
	public GameObject gameplayMenu;
	public GameObject levelFinishedPopup;
	public GameObject extraWordsPopup;

	public MenuManager menuManager;

	public List<GameObject> bonusLetters;

	public int bonusWordsCollecedOnThisLevel;

	// Bonus slova cemo da kreiramo u odnosu na redni broj packa FIXME

	public GameObject soundOffImageHolder;

	// Bloker za klikove
	public GameObject clicksBlocker;

	public static GameplayManager gameplayManager;

	void Awake()
	{
		// Pustamo loading depart animaciju ukoliko ima potrebe
		if (GlobalVariables.playLoadingDepartAtTheBegining)
		{
			gameFinished = true;
			loadingHolder.SetActive(true);
			loadingHolder.transform.GetChild(0).GetComponent<Animator>().Play("LoadingDepart", 0, 0);
			StartCoroutine("DisableLoadingHolder");
		}

		// Ako smo prvi put u gameplay sceni setujemo da smo usli tu
		if (!GlobalVariables.backFromGameplay)
		{
			GlobalVariables.playLoadingDepartAtTheBegining = true;
			GlobalVariables.backFromGameplay = true;
		}

		bonusWordsCollecedOnThisLevel = 0;

		coinsText.text = GlobalVariables.coins.ToString();
		coinsTextShop.text = GlobalVariables.coins.ToString();

		// Pustamo odgovarajucu muziku
		SoundManager.Instance.Stop_MenuMusic();
		SoundManager.Instance.Play_GameplayMusic();

		if (SoundManager.musicOn == 0)
			soundOffImageHolder.SetActive(true);
	}

	IEnumerator DisableLoadingHolder()
	{
		yield return new WaitForSeconds(0.9f);
		loadingHolder.SetActive(false);

		if (!isBonus)
			gameFinished = false;
	}

	void Start()
	{
		gameplayManager = this;

		// Ime selektovanog nivao
		GameObject.Find("LevelNameAndNumber").GetComponent<Text>().text =  /*LevelsParser.levelParser.packs[LevelsParser.selectedPack].Attributes["name"].Value + "   " + */LevelsParser.levelParser.packs[LevelsParser.selectedPack].ChildNodes[LevelsParser.selectedWorld].Attributes["title"].Value + " " + (LevelsParser.selectedLevel + 1).ToString();

		// Setujemo bonus na false, pa ako setovanje parametara resetuje onda radimo predvidjeno za bonus level
		if (LevelsParser.levelParser.packs[LevelsParser.selectedPack].ChildNodes[LevelsParser.selectedWorld].ChildNodes[LevelsParser.selectedLevel].Attributes["bonus"].Value == "yes")
			isBonus = true;
		else
			isBonus = false;

		// Uzimanje podataka o slovima i o recima
		// Setujemo parametre
		LevelsParser.levelParser.SetLevelParameters();

		// Proveravamo da li je spreman bonus
		if (GlobalVariables.bonusWordsCollected >= GlobalVariables.bonusWordsRequired)
			extraWordButton.transform.GetChild(0).GetComponent<Animator>().Play("ExtraWordsJarOpenAnimation", 0, 0);

		// Pozivamo funkciju za kreiranje targetovanih reci
		CreateTargetWords();
		CreateOfferedLetters();

		if (isBonus)
			StartCoroutine("SetBonusLetter");
	}

	public IEnumerator SetBonusLetter()
	{
		gameFinished = true;

		yield return new WaitForSeconds(1f);

		for (int i = 0; i < offeredBonusLetters.Count; i++)
		{
			offeredLettersHolder.transform.GetChild(offeredLetters.Count + offeredBonusLetters.Count - 1 - i).GetChild(0).GetComponent<Animator>().Play("BonusIdleToPosition", 0, 0);
		}

		offeredLetters.AddRange(offeredBonusLetters);

		yield return new WaitForSeconds(1f);

		gameFinished = false;
	}

	public void CreateTargetWords()
	{
		// Da li da kreiramo bonus slovca
		int numberOfBonusLetters = 0;
		int bonusLettersCreated = 0;

		if (Random.Range(0, 2) == 0)
		{
			// Da vidimo koliko cemo slovca da napravimo da budu bonus
			numberOfBonusLetters = Random.Range(1, LevelsParser.selectedPack + 1);
		}

		List<GameObject> allLetters = new List<GameObject>();

		if (!isBonus)
		{
			// Ako je broj trazenih reci veci od 3 onda delimo na dva holdera u suprotnom stavljamo u jedan
			if (targetWords.Count <= 3)
			{
				// Za svaku rec iz niza targetovanih reci kreiramo po jedan wordHolder objekat
				for (int i = targetWords.Count - 1; i >= 0; i--)
				{
					GameObject newWord = Instantiate(wordHolder, targetWordsHolder.transform) as GameObject;
					newWord.transform.localPosition = Vector3.zero;

					newWord.GetComponent<TargetWord>().word = targetWords[i];

					// Zatim za svako slovo iz reci kreiramo letterHolder objekat i popunjavamo ga
					char[] letters = targetWords[i].ToCharArray();

					for (int j = 0; j < letters.Length; j++)
					{
						GameObject newLetter = Instantiate(letterHolder, newWord.transform) as GameObject;

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(letters[j].ToString());

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().enabled = false;

						allLetters.Add(newLetter);
					}

					newWord.transform.localScale = Vector3.one;
				}
			}
			else
			{
				// Ukljucujemo odgovarajuce holdere
				targetWordsHolder.SetActive(false);
				targetWordsHolder1.SetActive(true);
				targetWordsHolder2.SetActive(true);

				// Najmanja sirina targetovanog slova
	//			float minLetterSize = 10000; // Postavljamo na brojku koja je sigurno mnogo veca od ostalih

				// Kreiramo prvu polovinu reci
				for (int i = targetWords.Count - 1; i > targetWords.Count / 2 - 1; i--)
				{
					GameObject newWord = Instantiate(wordHolder, targetWordsHolder1.transform) as GameObject;
					newWord.transform.localPosition = Vector3.zero;

					newWord.GetComponent<TargetWord>().word = targetWords[i];

					// Zatim za svako slovo iz reci kreiramo letterHolder objekat i popunjavamo ga
					char[] letters = targetWords[i].ToCharArray();

					for (int j = 0; j < letters.Length; j++)
					{
						GameObject newLetter = Instantiate(letterHolder, newWord.transform) as GameObject;

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(letters[j].ToString());

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().enabled = false;

						allLetters.Add(newLetter);
					}

					newWord.transform.localScale = Vector3.one;

	//				// Proveravamo i setujemo velicinu najmanjeg slova
	//				if (newWord.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x < minLetterSize)
	//				{
	//					minLetterSize = newWord.transform.GetChild(0).GetComponent<RectTransform>().rect.width;
	//				}
				}

				// Zatim i drugu
				for (int i = targetWords.Count / 2 - 1; i >= 0; i--)
				{
					GameObject newWord = Instantiate(wordHolder, targetWordsHolder2.transform) as GameObject;
					newWord.transform.localPosition = Vector3.zero;

					newWord.GetComponent<TargetWord>().word = targetWords[i];

					// Zatim za svako slovo iz reci kreiramo letterHolder objekat i popunjavamo ga
					char[] letters = targetWords[i].ToCharArray();

					for (int j = 0; j < letters.Length; j++)
					{
						GameObject newLetter = Instantiate(letterHolder, newWord.transform) as GameObject;

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(letters[j].ToString());

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().enabled = false;

						allLetters.Add(newLetter);
					}

					newWord.transform.localScale = Vector3.one;

	//				// Proveravamo i setujemo velicinu najmanjeg slova
	//				if (newWord.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x < minLetterSize)
	//				{
	//					minLetterSize = newWord.transform.GetChild(0).GetComponent<RectTransform>().rect.width;
	//				}
				}

	//			// Izjednacavamo velicine slova
	//			for (int i = 0; i < targetWordsHolder1.transform.childCount; i++)
	//			{
	//				for (int j = 0; j < targetWordsHolder1.transform.GetChild(i).childCount; j++)
	//				{
	//					targetWordsHolder1.transform.GetChild(i).GetChild(j).GetComponent<LayoutElement>().preferredWidth = minLetterSize;
	//				}
	//			}
	//
	//			for (int i = 0; i < targetWordsHolder2.transform.childCount; i++)
	//			{
	//				for (int j = 0; j < targetWordsHolder2.transform.GetChild(i).childCount; j++)
	//				{
	//					targetWordsHolder2.transform.GetChild(i).GetChild(j).GetComponent<LayoutElement>().preferredWidth = minLetterSize;
	//				}
	//			}

				// Biramo random onoliko slova koja ce biti bonus slova
				if (numberOfBonusLetters > 0)
				{
					while (bonusLettersCreated < numberOfBonusLetters)
					{
						int randomLetterIndex = Random.Range(0, allLetters.Count);

						if (!bonusLetters.Contains(allLetters[randomLetterIndex]))
						{
							bonusLetters.Add(allLetters[randomLetterIndex]);
							allLetters[randomLetterIndex].transform.Find("AnimationHolder").GetComponent<Animator>().Play("BonusLetterIdle", 0, 0);
							bonusLettersCreated += 1;
						}
					}
				}

				StartCoroutine("SetTargetWordLettersToSameSize");

				// Ako je neparan broj reci koje trazimo onda cemo da dodamo jos jednu praznu rec da bismo izjednacili kolone
				if (targetWords.Count % 2 == 1)
				{
					GameObject emptyWord = Instantiate(wordHolder, targetWordsHolder2.transform) as GameObject;
					emptyWord.transform.localPosition = Vector3.zero;
	//
					GameObject empty = Instantiate(emptyLetter, emptyWord.transform) as GameObject;
	//
					empty.transform.localScale = Vector3.one;
				}
			}
		}
		else // Bonuuuuuus!
		{
			List<string> allWords = new List<string>();
			allWords.AddRange(targetWords);
			allWords.AddRange(solvedWords);

			allWords.Sort((x, y) => x.Length.CompareTo(y.Length));

			// Ukljucujemo odgovarajuce holdere
			targetWordsHolder.SetActive(false);
			targetWordsHolder1.SetActive(true);
			targetWordsHolder2.SetActive(true);

			// Najmanja sirina targetovanog slova
			//			float minLetterSize = 10000; // Postavljamo na brojku koja je sigurno mnogo veca od ostalih

			// Kreiramo prvu polovinu reci
			for (int i = allWords.Count - 1; i > allWords.Count / 2 - 1; i--)
			{
				GameObject newWord = Instantiate(wordHolder, targetWordsHolder1.transform) as GameObject;
				newWord.transform.localPosition = Vector3.zero;

				newWord.GetComponent<TargetWord>().word = allWords[i];

				// Zatim za svako slovo iz reci kreiramo letterHolder objekat i popunjavamo ga
				char[] letters = allWords[i].ToCharArray(); // 

				if (targetWords.Contains(allWords[i]))
				{
					for (int j = 0; j < letters.Length; j++)
					{
						GameObject newLetter = Instantiate(letterHolder, newWord.transform) as GameObject;

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(letters[j].ToString());

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().enabled = false;

						allLetters.Add(newLetter);
					}
				}
				else
				{
					for (int j = 0; j < letters.Length; j++)
					{
						GameObject newLetter = Instantiate(letterHolder, newWord.transform) as GameObject;

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(letters[j].ToString());

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().enabled = true;

						newLetter.transform.Find("AnimationHolder").GetComponent<Animator>().Play("Solved", 0, 0);
					}

					newWord.GetComponent<TargetWord>().wordSolved = true;
				}


				newWord.transform.localScale = Vector3.one;

				//				// Proveravamo i setujemo velicinu najmanjeg slova
				//				if (newWord.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x < minLetterSize)
				//				{
				//					minLetterSize = newWord.transform.GetChild(0).GetComponent<RectTransform>().rect.width;
				//				}
			}

			// Zatim i drugu
			for (int i = allWords.Count / 2 - 1; i >= 0; i--)
			{
				GameObject newWord = Instantiate(wordHolder, targetWordsHolder2.transform) as GameObject;
				newWord.transform.localPosition = Vector3.zero;

				newWord.GetComponent<TargetWord>().word = allWords[i];

				// Zatim za svako slovo iz reci kreiramo letterHolder objekat i popunjavamo ga
				char[] letters = allWords[i].ToCharArray();

				if (targetWords.Contains(allWords[i]))
				{
					for (int j = 0; j < letters.Length; j++)
					{
						GameObject newLetter = Instantiate(letterHolder, newWord.transform) as GameObject;

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(letters[j].ToString());

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().enabled = false;

						allLetters.Add(newLetter);
					}
				}
				else
				{
					for (int j = 0; j < letters.Length; j++)
					{
						GameObject newLetter = Instantiate(letterHolder, newWord.transform) as GameObject;

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(letters[j].ToString());

						newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().enabled = true;

						newLetter.transform.Find("AnimationHolder").GetComponent<Animator>().Play("Solved", 0, 0);
					}

					newWord.GetComponent<TargetWord>().wordSolved = true;
				}

				newWord.transform.localScale = Vector3.one;

				//				// Proveravamo i setujemo velicinu najmanjeg slova
				//				if (newWord.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x < minLetterSize)
				//				{
				//					minLetterSize = newWord.transform.GetChild(0).GetComponent<RectTransform>().rect.width;
				//				}
			}

			//			// Izjednacavamo velicine slova
			//			for (int i = 0; i < targetWordsHolder1.transform.childCount; i++)
			//			{
			//				for (int j = 0; j < targetWordsHolder1.transform.GetChild(i).childCount; j++)
			//				{
			//					targetWordsHolder1.transform.GetChild(i).GetChild(j).GetComponent<LayoutElement>().preferredWidth = minLetterSize;
			//				}
			//			}
			//
			//			for (int i = 0; i < targetWordsHolder2.transform.childCount; i++)
			//			{
			//				for (int j = 0; j < targetWordsHolder2.transform.GetChild(i).childCount; j++)
			//				{
			//					targetWordsHolder2.transform.GetChild(i).GetChild(j).GetComponent<LayoutElement>().preferredWidth = minLetterSize;
			//				}
			//			}

			// Biramo random onoliko slova koja ce biti bonus slova
			if (numberOfBonusLetters > 0)
			{
				while (bonusLettersCreated < numberOfBonusLetters)
				{
					int randomLetterIndex = Random.Range(0, allLetters.Count);

					if (!bonusLetters.Contains(allLetters[randomLetterIndex]))
					{
						bonusLetters.Add(allLetters[randomLetterIndex]);
						allLetters[randomLetterIndex].transform.Find("AnimationHolder").GetComponent<Animator>().Play("BonusLetterIdle", 0, 0);
						bonusLettersCreated += 1;
					}
				}
			}

			StartCoroutine("SetTargetWordLettersToSameSize");

			// Ako je neparan broj reci koje trazimo onda cemo da dodamo jos jednu praznu rec da bismo izjednacili kolone
			if (targetWords.Count % 2 == 1)
			{
				GameObject emptyWord = Instantiate(wordHolder, targetWordsHolder2.transform) as GameObject;
				emptyWord.transform.localPosition = Vector3.zero;
				//
				GameObject empty = Instantiate(emptyLetter, emptyWord.transform) as GameObject;
				//
				empty.transform.localScale = Vector3.one;
			}
		}

		// Ako je bonus igra onda podesavamo i pronadjene reci :D
//		if (isBonus)
//		{
//			for (int i = 0; i < solvedWords.Count; i++)
//			{
//				GameObject newWord = Instantiate(wordHolder, targetWordsHolder.transform) as GameObject;
//				newWord.transform.localPosition = Vector3.zero;
//
//				newWord.GetComponent<TargetWord>().word = solvedWords[i];
//
//				// Zatim za svako slovo iz reci kreiramo letterHolder objekat i popunjavamo ga
//				char[] letters = solvedWords[i].ToCharArray();
//
//				for (int j = 0; j < letters.Length; j++)
//				{
//					GameObject newLetter = Instantiate(letterHolder, newWord.transform) as GameObject;
//
//					newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(letters[j].ToString());
//
//					newLetter.transform.Find("AnimationHolder/LetterImage").GetComponent<Image>().enabled = true;
//				}
//
//				newWord.transform.localScale = Vector3.one;
//			}
//		}
	}

	IEnumerator SetTargetWordLettersToSameSize()
	{
		yield return new WaitForEndOfFrame();

		float minLetterSize = 10000;

		if (targetWords.Count <= 3)
		{
			// Pronalazimo minimalnu velicinu slova
			for (int i = 0; i < targetWordsHolder.transform.childCount; i++)
			{
				if (targetWordsHolder.transform.GetChild(i).GetChild(0).GetComponent<RectTransform>().rect.width < minLetterSize)
					minLetterSize = targetWordsHolder.transform.GetChild(i).GetChild(0).GetComponent<RectTransform>().rect.width;
			}

			// Izjednacavamo velicine slova
			for (int i = 0; i < targetWordsHolder.transform.childCount; i++)
			{
				for (int j = 0; j < targetWordsHolder.transform.GetChild(i).childCount; j++)
				{
					targetWordsHolder.transform.GetChild(i).GetChild(j).GetComponent<LayoutElement>().preferredWidth = minLetterSize;
				}
			}
		}
		else
		{
			// Pronalazimo minimalnu velicinu slova
			for (int i = 0; i < targetWordsHolder1.transform.childCount; i++)
			{
				if (targetWordsHolder1.transform.GetChild(i).GetChild(0).GetComponent<RectTransform>().rect.width < minLetterSize)
					minLetterSize = targetWordsHolder1.transform.GetChild(i).GetChild(0).GetComponent<RectTransform>().rect.width;
			}

			for (int i = 0; i < targetWordsHolder2.transform.childCount; i++)
			{
				if (targetWordsHolder2.transform.GetChild(i).GetChild(0).GetComponent<RectTransform>().rect.width < minLetterSize)
					minLetterSize = targetWordsHolder2.transform.GetChild(i).GetChild(0).GetComponent<RectTransform>().rect.width;
			}

			// Izjednacavamo velicine slova
			for (int i = 0; i < targetWordsHolder1.transform.childCount; i++)
			{
				for (int j = 0; j < targetWordsHolder1.transform.GetChild(i).childCount; j++)
				{
					targetWordsHolder1.transform.GetChild(i).GetChild(j).GetComponent<LayoutElement>().preferredWidth = minLetterSize;
				}
			}

			for (int i = 0; i < targetWordsHolder2.transform.childCount; i++)
			{
				for (int j = 0; j < targetWordsHolder2.transform.GetChild(i).childCount; j++)
				{
					targetWordsHolder2.transform.GetChild(i).GetChild(j).GetComponent<LayoutElement>().preferredWidth = minLetterSize;
				}
			}
		}
	}

	public Sprite GetLetterSprite(string letter)
	{
		switch (letter)
		{
		case "a":
			return lettersSprites[0];
			break;
		case "b":
			return lettersSprites[1];
			break;
		case "c":
			return lettersSprites[2];
			break;
		case "d":
			return lettersSprites[3];
			break;
		case "e":
			return lettersSprites[4];
			break;
		case "f":
			return lettersSprites[5];
			break;
		case "g":
			return lettersSprites[6];
			break;
		case "h":
			return lettersSprites[7];
			break;
		case "i":
			return lettersSprites[8];
			break;
		case "j":
			return lettersSprites[9];
			break;
		case "k":
			return lettersSprites[10];
			break;
		case "l":
			return lettersSprites[11];
			break;
		case "m":
			return lettersSprites[12];
			break;
		case "n":
			return lettersSprites[13];
			break;
		case "o":
			return lettersSprites[14];
			break;
		case "p":
			return lettersSprites[15];
			break;
		case "q":
			return lettersSprites[16];
			break;
		case "r":
			return lettersSprites[17];
			break;
		case "s":
			return lettersSprites[18];
			break;
		case "t":
			return lettersSprites[19];
			break;
		case "u":
			return lettersSprites[20];
			break;
		case "v":
			return lettersSprites[21];
			break;
		case "w":
			return lettersSprites[22];
			break;
		case "x":
			return lettersSprites[23];
			break;
		case "y":
			return lettersSprites[24];
			break;
		case "z":
			return lettersSprites[25];
			break;
		default:
			return lettersSprites[0];
			break;
		}
	}

	public void CreateOfferedLetters()
	{
		float alpha = 360f / (offeredLetters.Count + offeredBonusLetters.Count);
		float firstAngle = 0;
		Vector3 startPosition = new Vector3(0, 250f, 0);

		for (int i = 0; i < offeredLetters.Count; i++)
		{
			GameObject letter = Instantiate(offeredLetterPrefab, offeredLettersHolder.transform) as GameObject;

			letter.transform.Find("AnimationHolder/LetterHolder").GetComponent<OfferedLetter>().letter = offeredLetters[i];
			letter.transform.Find("AnimationHolder/LetterHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(offeredLetters[i]);
			letter.transform.localScale = Vector3.one;
			letter.transform.localPosition = Vector3.zero;

			Quaternion r = letter.transform.rotation;
			r.eulerAngles = new Vector3(0, 0, firstAngle);
			letter.transform.rotation = r;

			Quaternion l = letter.transform.Find("AnimationHolder/LetterHolder").localRotation;
			l.eulerAngles = new Vector3(0, 0, -firstAngle);
			letter.transform.Find("AnimationHolder/LetterHolder").localRotation = l;

			firstAngle += alpha;

			// Kreiramo slovo za selektovanje
			GameObject sl = Instantiate(selectedLetter, selectedLettersHolder.transform) as GameObject;
			sl.GetComponent<Image>().sprite = GetLetterSprite(offeredLetters[i]);
			sl.transform.localScale = Vector3.one;
			sl.transform.localPosition = Vector3.zero;
			sl.name = offeredLetters[i];
			sl.SetActive(false);

//			letter.transform.localPosition = startPosition;
//			Vector3 nextPosition = new Vector3(startPosition.x * Mathf.Cos(alpha * Mathf.Deg2Rad) + startPosition.y + Mathf.Sin(alpha * Mathf.Deg2Rad), -startPosition.x * Mathf.Sin(alpha * Mathf.Deg2Rad) + startPosition.y + Mathf.Cos(alpha * Mathf.Deg2Rad), 0);
//			startPosition = nextPosition;
		}

		for (int i = 0; i < offeredBonusLetters.Count; i++)
		{
			GameObject letter = Instantiate(offeredLetterPrefab, offeredLettersHolder.transform) as GameObject;

			letter.transform.Find("AnimationHolder").GetComponent<Animator>().Play("BonusLetterIdle", 0, 0);

			letter.transform.Find("AnimationHolder/LetterHolder").GetComponent<OfferedLetter>().letter = offeredBonusLetters[i];
			letter.transform.Find("AnimationHolder/LetterHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(offeredBonusLetters[i]);
			letter.transform.localScale = Vector3.one;
			letter.transform.localPosition = Vector3.zero;

			Quaternion r = letter.transform.rotation;
			r.eulerAngles = new Vector3(0, 0, firstAngle);
			letter.transform.rotation = r;

			Quaternion l = letter.transform.Find("AnimationHolder/LetterHolder").localRotation;
			l.eulerAngles = new Vector3(0, 0, -firstAngle);
			letter.transform.Find("AnimationHolder/LetterHolder").localRotation = l;

			firstAngle += alpha;

			// Kreiramo slovo za selektovanje
			GameObject sl = Instantiate(selectedLetter, selectedLettersHolder.transform) as GameObject;
			sl.GetComponent<Image>().sprite = GetLetterSprite(offeredBonusLetters[i]);
			sl.transform.localScale = Vector3.one;
			sl.transform.localPosition = Vector3.zero;
			sl.name = offeredBonusLetters[i];
			sl.SetActive(false);

			//			letter.transform.localPosition = startPosition;
			//			Vector3 nextPosition = new Vector3(startPosition.x * Mathf.Cos(alpha * Mathf.Deg2Rad) + startPosition.y + Mathf.Sin(alpha * Mathf.Deg2Rad), -startPosition.x * Mathf.Sin(alpha * Mathf.Deg2Rad) + startPosition.y + Mathf.Cos(alpha * Mathf.Deg2Rad), 0);
			//			startPosition = nextPosition;
		}
	}

	public void ShuffleLetters()
	{
		if (!gameFinished)
			StartCoroutine("ShuffleLettersCoroutine");
	}

	IEnumerator ShuffleLettersCoroutine()
	{
		gameFinished = true;

		// Pustamo zvuk za shuffle
		SoundManager.Instance.Play_Sound(SoundManager.Instance.shuffle);

		// Pustamo animaciju za svako slovo
		for (int i = 0; i < offeredLettersHolder.transform.childCount; i++)
		{
			offeredLettersHolder.transform.GetChild(i).Find("AnimationHolder").GetComponent<Animator>().Play("Shuffle", 0, 0);
		}

		yield return new WaitForSeconds(0.4f);

		// FIXME za sada cu da namestim tako sto cu N (4) puta da zamenim mesta za dva random izabrana slova
		for (int i = 0; i < 4; i++)
		{
			int r1 = Random.Range(0, offeredLetters.Count);
			int r2 = Random.Range(0, offeredLetters.Count);

			if (r1 != r2)
			{
				string pomLetter = offeredLettersHolder.transform.GetChild(r1).Find("AnimationHolder/LetterHolder").GetComponent<OfferedLetter>().letter;
				offeredLettersHolder.transform.GetChild(r1).Find("AnimationHolder/LetterHolder").GetComponent<OfferedLetter>().letter = offeredLettersHolder.transform.GetChild(r2).Find("AnimationHolder/LetterHolder").GetComponent<OfferedLetter>().letter;
				offeredLettersHolder.transform.GetChild(r2).Find("AnimationHolder/LetterHolder").GetComponent<OfferedLetter>().letter = pomLetter;
				offeredLettersHolder.transform.GetChild(r1).Find("AnimationHolder/LetterHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(offeredLettersHolder.transform.GetChild(r1).Find("AnimationHolder/LetterHolder").GetComponent<OfferedLetter>().letter);
				offeredLettersHolder.transform.GetChild(r2).Find("AnimationHolder/LetterHolder/LetterImage").GetComponent<Image>().sprite = GetLetterSprite(pomLetter);
			}
		}

		yield return new WaitForSeconds(0.6f);

		gameFinished = false;
	}

	public void ShowHint()
	{
		if (!gameFinished && GlobalVariables.coins >= GlobalVariables.hintCost)
		{
			List<Animator> listOfLetters = new List<Animator>();

			// Prolazimo kroz sva slova i ako ima neko da nije ver prikazano pomocu hinta dodajemo ga u listu
			if (targetWords.Count <= 3)
			{
				for (int i = 0; i < targetWordsHolder.transform.childCount; i++)
				{
					if (!targetWordsHolder.transform.GetChild(i).GetComponent<TargetWord>().wordSolved)
					{
						// Prolazimo kroz sva slova da vidimo koja su prikazana i dodajemo ga u listu
						for (int j = 0; j < targetWordsHolder.transform.GetChild(i).childCount; j++)
						{
							if (targetWordsHolder.transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Empty"))
							{
								listOfLetters.Add(targetWordsHolder.transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<Animator>());
							}
						}
					}
				}

				if (listOfLetters.Count > 0)
				{
					// Biramo jedno random i pustamo animaciju za hint
					int r = Random.Range(0, listOfLetters.Count);

					listOfLetters[r].Play("Hinted", 0, 0);

					// Oduzimamo coine za hint
					GlobalVariables.globalVariables.AddCoins(-GlobalVariables.hintCost);
					coinsText.text = GlobalVariables.coins.ToString();
					coinsTextShop.text = GlobalVariables.coins.ToString();

					// Pustamo zvuk za hint
					SoundManager.Instance.Play_Sound(SoundManager.Instance.hint);
				}
			}
			else
			{
				for (int i = 0; i < targetWordsHolder1.transform.childCount; i++)
				{
					if (!targetWordsHolder1.transform.GetChild(i).GetComponent<TargetWord>().wordSolved)
					{
						// Prolazimo kroz sva slova da vidimo koja su prikazana i dodajemo ga u listu
						for (int j = 0; j < targetWordsHolder1.transform.GetChild(i).childCount; j++)
						{
							if (targetWordsHolder1.transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Empty"))
							{
								listOfLetters.Add(targetWordsHolder1.transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<Animator>());
							}
						}
					}
				}

				for (int i = 0; i < targetWordsHolder2.transform.childCount; i++)
				{
					if (!targetWordsHolder2.transform.GetChild(i).GetComponent<TargetWord>().wordSolved)
					{
						// Prolazimo kroz sva slova da vidimo koja su prikazana i dodajemo ga u listu
						for (int j = 0; j < targetWordsHolder2.transform.GetChild(i).childCount; j++)
						{
							if (targetWordsHolder2.transform.GetChild(i).GetComponent<TargetWord>().word != "" && targetWordsHolder2.transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Empty"))
							{
								listOfLetters.Add(targetWordsHolder2.transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<Animator>());
							}
						}
					}
				}

				if (listOfLetters.Count > 0)
				{
					// Biramo jedno random i pustamo animaciju za hint
					int r = Random.Range(0, listOfLetters.Count);

					listOfLetters[r].Play("Hinted", 0, 0);

					// FIXME za sada samo oduzimam coine
					GlobalVariables.globalVariables.AddCoins(-GlobalVariables.hintCost);
					coinsText.text = GlobalVariables.coins.ToString();
					coinsTextShop.text = GlobalVariables.coins.ToString();

					// Pustamo zvuk za hint
					SoundManager.Instance.Play_Sound(SoundManager.Instance.hint);
				}
			}
		}
		else
		{
			OpenShop();
		}
	}

	public void LevelFinished()
	{
		StartCoroutine("LevelFinishedCoroutine");	
	}

	IEnumerator LevelFinishedCoroutine()
	{
		gameFinished = true;
		yield return new WaitForSeconds(0.7f);
		int oldStarsNumber = GlobalVariables.stars;

		// Prikazujemo level finished ekran
		menuManager.ShowPopUpMenu(levelFinishedPopup);

		// Dodajemo coine
		GlobalVariables.globalVariables.AddCoins(targetWords.Count);

		// Pustamo zvuk za level fihish ekran
		SoundManager.Instance.Play_Sound(SoundManager.Instance.levelFinished);

		// Proveravamo da li je poslednji nivo i ako jeste onda pustamo dodavanje zvezdica
		// U suprotnom aktiviramo "Already completed" objekat
		if (LevelsParser.selectedPack == LevelsParser.levelParser.lastUnlockedPack &&
			LevelsParser.selectedWorld == LevelsParser.levelParser.lastUnlockedWorld &&
			LevelsParser.selectedLevel == LevelsParser.levelParser.lastUnlockedLevel)
		{
			levelFinishedPopup.transform.Find("AnimationHolder/AlreadyCompletedText").gameObject.SetActive(false);

			addStarsCor = AddStarsCoroutine(oldStarsNumber);

			StartCoroutine(addStarsCor);
		}
		else
		{
			levelFinishedPopup.transform.Find("AnimationHolder/StarNumberHolder").gameObject.SetActive(false);
		}

		// Pamtimo broj pronadjenih bonus reci
		PlayerPrefs.SetInt("BonusWordsCollected", GlobalVariables.bonusWordsCollected);
		PlayerPrefs.Save();

		LevelsParser.levelParser.CheckIfLastLevelWasFinished();
	}

	public IEnumerator addStarsCor = null;

	public bool addingStars = false;

	IEnumerator AddStarsCoroutine(int oldStarsNumber)
	{
		addingStars = true;
		Text starsText = levelFinishedPopup.transform.Find("AnimationHolder/StarNumberHolder/AnimationHolder/Text").GetComponent<Text>();
		starsText.text = oldStarsNumber.ToString();

		Animator starsAnimator = levelFinishedPopup.transform.Find("AnimationHolder/StarNumberHolder/AnimationHolder").GetComponent<Animator>();

		yield return new WaitForSeconds(0.2f);

		// Za sada stavljam da proces traje 2 sekunde
		float step = 1.4f / targetWords.Count;

		for (int i = 0; i < targetWords.Count; i++)
		{
			yield return new WaitForSeconds(step);
			starsText.text = (oldStarsNumber + i + 1).ToString();

			if (i == targetWords.Count - 1)
			{
				starsAnimator.Play("AddStar", 0, 0);
				SoundManager.Instance.Play_Sound(SoundManager.Instance.shopItemBought);
			}
			else
				SoundManager.Instance.Play_Sound(SoundManager.Instance.buttonClick);
		}

		addingStars = false;
	}

	public void NextLevelPressed()
	{
		StartCoroutine("LoadNextLevel");
	}

	public IEnumerator LoadNextLevel()
	{
		GameObject.Find("BtnNext").GetComponent<Button>().enabled = false;

		clicksBlocker.SetActive(true);
		gameFinished = false;

		loadingHolder.SetActive(true);

		if (addingStars)
		{
			StopCoroutine(addStarsCor);
			levelFinishedPopup.transform.Find("AnimationHolder/StarNumberHolder/AnimationHolder/Text").GetComponent<Text>().text = GlobalVariables.stars.ToString();
			levelFinishedPopup.transform.Find("AnimationHolder/StarNumberHolder/AnimationHolder").GetComponent<Animator>().Play("AddStar", 0, 0);
			addingStars = false;

			yield return new WaitForSeconds(0.7f);

			// Povecavamo nivo za 1 i loadujemo sledeci nivo
			LevelsParser.levelParser.IncrementLastSelectedLevel();

			GlobalVariables.playLoadingDepartAtTheBegining = true;

			loadingHolder.transform.GetChild(0).GetComponent<Animator>().Play("LoadingArrive", 0, 0);

			// Pustamo level finished interstitial
			if (!GlobalVariables.removeAdsOwned)
				AdsManager.Instance.ShowInterstitial();

			yield return new WaitForSeconds(0.9f);

			SceneManager.LoadScene("Level");
		}
		else
		{
			// Povecavamo nivo za 1 i loadujemo sledeci nivo
			LevelsParser.levelParser.IncrementLastSelectedLevel();

			GlobalVariables.playLoadingDepartAtTheBegining = true;

			loadingHolder.transform.GetChild(0).GetComponent<Animator>().Play("LoadingArrive", 0, 0);

			// Pustamo level finished interstitial
//			if (!GlobalVariables.removeAdsOwned)
			AdsManager.Instance.ShowInterstitial();

			yield return new WaitForSeconds(0.9f);

			SceneManager.LoadScene("Level");
		}
	}

	public void PauseGame()
	{
		// Samo ako vec nije predjen nivo moze da se pauzira
		if (!gameFinished)
		{
			// Posto na osnovu ove promenljive kontrolisemo i prst nju stavljamo na level finished true
			gameFinished = true;

			// Prikazujemo meni za pauzu
			menuManager.ShowPopUpMenu(pausePopup);

		}
	}

	public void ContinueGame()
	{
		menuManager.ClosePopUpMenu(pausePopup);

		gameFinished = false;
	}

	public void OpenShop()
	{
		// Shop takodje mozemo da kliknemo samo ukoliko level nije zavrsen
		if (!gameFinished)
		{
			gameFinished = true;

			menuManager.ShowPopUpMenu(shopMenu);
		}
	}

	public void CloseShop()
	{
		gameFinished = false;

		menuManager.ClosePopUpMenu(shopMenu);
	}

	public void HomeButtonClicked()
	{
		StartCoroutine("HomeButtonCoroutine");
	}

	IEnumerator HomeButtonCoroutine()
	{
		clicksBlocker.SetActive(true);
		loadingHolder.SetActive(true);

		// Posto je kliknuto home dugme oduzimamo extra reci koje smo nasli
		GlobalVariables.bonusWordsCollected -= bonusWordsCollecedOnThisLevel;

		GlobalVariables.playLoadingDepartAtTheBegining = true;

		loadingHolder.transform.GetChild(0).GetComponent<Animator>().Play("LoadingArrive", 0, 0);

		// Pustamo interstitial
//		if (!GlobalVariables.removeAdsOwned)
		AdsManager.Instance.ShowInterstitial();

		yield return new WaitForSeconds(0.9f);

		SceneManager.LoadScene("MainScene");
	}

	public void ExtraButtonClicked()
	{
		if (!gameFinished)
		{
			gameFinished = true;

			menuManager.ShowPopUpMenu(extraWordsPopup);

			// Ako nemam dovoljno reci sakrivamo claim dugme i popunjavamo kako treba podatke
			if (GlobalVariables.bonusWordsCollected < GlobalVariables.bonusWordsRequired)
			{
				extraWordsPopup.transform.Find("AnimationHolder/BtnClaim").gameObject.SetActive(false);
				extraWordsPopup.transform.Find("AnimationHolder/ProgressBarHolder/AnimationHolder/ProgressHolder/Text").GetComponent<Text>().text = 
					GlobalVariables.bonusWordsCollected.ToString() + "/" + GlobalVariables.bonusWordsRequired.ToString();

				extraWordsPopup.transform.Find("AnimationHolder/ProgressBarHolder/AnimationHolder/ProgressHolder/ProgressSprite").GetComponent<Image>().fillAmount = 
					(float)GlobalVariables.bonusWordsCollected / (float)GlobalVariables.bonusWordsRequired;
			}
			else
			{
				extraWordsPopup.transform.Find("AnimationHolder/BtnClaim").gameObject.SetActive(true);
				extraWordsPopup.transform.Find("AnimationHolder/ProgressBarHolder/AnimationHolder/ProgressHolder/Text").gameObject.SetActive(false);
				extraWordsPopup.transform.Find("AnimationHolder/ProgressBarHolder/AnimationHolder/ProgressHolder/ProgressSprite").GetComponent<Image>().fillAmount = 1f;
			}
		}
	}

	public void CloseBonusCoinsPopup()
	{
		if (gameFinished)
			gameFinished = false;
	}

	public void ClaimBonus()
	{
		// Dodajemo novcice i menjamo vrednosti za dalje igranje
		GlobalVariables.globalVariables.AddCoins(GlobalVariables.bonusCoinsAmount);
		coinsText.text = GlobalVariables.coins.ToString();

		extraWordsPopup.transform.Find("CoinsParticle").GetComponent<ParticleSystem>().Play();
		extraWordsPopup.transform.Find("AddCoinsHolder").GetComponent<Animator>().Play("AddCoinsAnimation", 0, 0);
		extraWordsPopup.transform.Find("AddCoinsHolder/TextCoinsNumber").GetComponent<Text>().text = "+" + GlobalVariables.bonusCoinsAmount.ToString();

		GlobalVariables.bonusWordsCollected = 0;
		GlobalVariables.bonusWordsRequired += 2;
		GlobalVariables.bonusCoinsAmount += 5;
		PlayerPrefs.SetInt("BonusWordsRequired", GlobalVariables.bonusWordsRequired);
		PlayerPrefs.SetInt("BonusWordsCollected", GlobalVariables.bonusWordsCollected);
		PlayerPrefs.SetInt("BonusCoinsAmount", GlobalVariables.bonusCoinsAmount);
		PlayerPrefs.Save();

		extraWordsPopup.transform.Find("AnimationHolder/BtnClaim").gameObject.SetActive(false);
		extraWordsPopup.transform.Find("AnimationHolder/ProgressBarHolder/AnimationHolder/ProgressHolder/Text").gameObject.SetActive(true);
		extraWordsPopup.transform.Find("AnimationHolder/ProgressBarHolder/AnimationHolder/ProgressHolder/Text").GetComponent<Text>().text = 
			GlobalVariables.bonusWordsCollected.ToString() + "/" + GlobalVariables.bonusWordsRequired.ToString();

		extraWordsPopup.transform.Find("AnimationHolder/ProgressBarHolder/AnimationHolder/ProgressHolder/ProgressSprite").GetComponent<Image>().fillAmount = 
			(float)GlobalVariables.bonusWordsCollected / (float)GlobalVariables.bonusWordsRequired;

		extraWordButton.transform.GetChild(0).GetComponent<Animator>().Play("ExtraWordsJarIdleAnimation", 0, 0);

		// Pustamo zvuk za uzimanje bonusa
		SoundManager.Instance.Play_Sound(SoundManager.Instance.claimExtraCoins);
	}

	public void PlayButtonClickSound()
	{
		// Pustamo zvuk za klik
		SoundManager.Instance.Play_Sound(SoundManager.Instance.buttonClick);
	}

	public void ToggleSound()
	{
		if (SoundManager.musicOn == 1)
		{
			SoundManager.musicOn = 0;
			SoundManager.soundOn = 0;
			PlayerPrefs.SetInt("SoundOn", 0);
			PlayerPrefs.SetInt("MusicOn", 0);
			PlayerPrefs.Save();

			soundOffImageHolder.SetActive(true);

			SoundManager.Instance.MuteAllSounds();
		}
		else
		{
			SoundManager.musicOn = 1;
			SoundManager.soundOn = 1;
			PlayerPrefs.SetInt("SoundOn", 1);
			PlayerPrefs.SetInt("MusicOn", 1);
			PlayerPrefs.Save();

			soundOffImageHolder.SetActive(false);

			SoundManager.Instance.UnmuteAllSounds();
		}
	}
}
