using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FingerControls : MonoBehaviour {

	// Pomocne promenljive za pomeranje
	private RectTransform canvasRect;
	private Vector2 screenPosition;
	private Vector2 viewportPosition;

	public List<OfferedLetter> listOfSelectedLetters;

	public bool holdingDownFinger;

	public List<string> finishedWords;
	public List<string> finishedAdditionalWords;

	// Sluzi nam da brojimo koliko reci je koriznik nasao zaredom da bismo mu prikazali poruku
	public int consecutiveWordsFound;
	public Animator consecutiveWordsAnimator;
	public GameObject[] consecutiveWordsSprites;

	public LineRenderer line;

	void Awake()
	{
		canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
		listOfSelectedLetters = new List<OfferedLetter>();
		finishedWords = new List<string>();
		finishedAdditionalWords = new List<string>();
		consecutiveWordsFound = 0;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			transform.SetAsLastSibling();
			holdingDownFinger = true;
			GetComponent<BoxCollider2D>().enabled = true;

			if (!GameplayManager.gameFinished)
				transform.GetChild(0).GetComponent<ParticleSystem>().Play();
		}

		if (Input.GetMouseButton(0) && !GameplayManager.gameFinished && holdingDownFinger)
		{
			SetObjectPositionToCanvasPosition(gameObject, Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}

		if (Input.GetMouseButtonUp(0))
		{
			holdingDownFinger = false;
			GetComponent<BoxCollider2D>().enabled = false;
			CheckIfSelectedLettersAreValid();
			transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
		}
	}

	public void SetObjectPositionToCanvasPosition(GameObject obj, Vector3 worldPosition)
	{
		viewportPosition = Camera.main.WorldToViewportPoint(worldPosition);

		screenPosition = new Vector2(viewportPosition.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f, viewportPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);
//		screenPosition = new Vector2(viewportPosition.x / 2 * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f, viewportPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);

		obj.GetComponent<RectTransform>().anchoredPosition = screenPosition;  
	}

	public Vector3 GetObjectPositionInCanvasPosition(RectTransform rt)
	{
//		viewportPosition = Camera.main.WorldToViewportPoint(worldPosition);
//
//		return screenPosition = new Vector2(viewportPosition.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f, viewportPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);

		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);

		Vector3 worldPosition = new Vector3((corners[0].x + corners[3].x) / 2, (corners[0].y + corners[1].y) / 2, 0);

		return worldPosition;
	}

	public void OnTriggerEnter2D(Collider2D coll)
	{
		if (!GameplayManager.gameFinished && coll.tag == "Letter" && !listOfSelectedLetters.Contains(coll.GetComponent<OfferedLetter>()))
		{
			// Ako je lista prazna, tj. ovo je prvo slovo ukljucujemo holder za selektovana slova
			if (listOfSelectedLetters.Count == 0)
			{
				for (int i = 0; i < GameplayManager.gameplayManager.selectedLettersHolder.transform.childCount; i++)
				{
					GameplayManager.gameplayManager.selectedLettersHolder.transform.GetChild(i).gameObject.SetActive(false);
				}

				GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("SelectedLettersVisibleIdle", 0, 0);
			}

			listOfSelectedLetters.Add(coll.GetComponent<OfferedLetter>());

			// Ukljucujemo selektovano slovo i setujemo ga kao poslednje dete
			GameplayManager.gameplayManager.selectedLettersHolder.transform.Find(coll.GetComponent<OfferedLetter>().letter).gameObject.SetActive(true);
			GameplayManager.gameplayManager.selectedLettersHolder.transform.Find(coll.GetComponent<OfferedLetter>().letter).SetAsLastSibling();

			// Pustamo zvuk za selektovanje slova
			SoundManager.Instance.Play_Sound(SoundManager.Instance.letterSelected);

//			line.GetComponent<Main>().AddPoint(new Vector3(coll.transform.position.x, coll.transform.position.y, 0));
		}
		else if (coll.tag == "Letter" && listOfSelectedLetters.Contains(coll.GetComponent<OfferedLetter>()) && listOfSelectedLetters.IndexOf(coll.GetComponent<OfferedLetter>()) == listOfSelectedLetters.Count - 1)
		{
			listOfSelectedLetters.RemoveAt(listOfSelectedLetters.Count - 1);

			// Iskljucujemo slovo
			GameplayManager.gameplayManager.selectedLettersHolder.transform.Find(coll.GetComponent<OfferedLetter>().letter).gameObject.SetActive(false);

			// Ako je poslednje slovo izbaceno onda gasimo i holder za selektovana slovca
			if (listOfSelectedLetters.Count == 0)
			{
				GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("SelectedLettersIdle", 0, 0);

				// Tada i cistimo listu tacaka za iscravanje bezijerove krive
				line.GetComponent<Main>().ClearPoints();
			}
		}
	}

	public void ExtraWordFound()
	{
		// Dodajemo 1 na broj pronadjenih dodatnih bonus reci
		GlobalVariables.bonusWordsCollected += 1;
		GameplayManager.gameplayManager.bonusWordsCollecedOnThisLevel += 1;

		// Proveravamo da li je tegla puna i pustamo animaciju za punu teglicu ako jeste
		if (GlobalVariables.bonusWordsCollected >= GlobalVariables.bonusWordsRequired)
			StartCoroutine("PlayFullJarAnimation");

		GameplayManager.gameplayManager.wordIntoCookieJarAnimator.Play("ExtraWordFound", 0, 0);
		GameplayManager.gameplayManager.extraWordButton.transform.GetChild(0).GetComponent<Animator>().Play("JarExtraWord", 0, 0);
	}

	IEnumerator PlayFullJarAnimation()
	{
		yield return new WaitForSeconds(1.6f);
		GameplayManager.gameplayManager.extraWordButton.transform.GetChild(0).GetComponent<Animator>().Play("ExtraWordsJarOpenAnimation", 0, 0);
	}
		
	public void CheckIfSelectedLettersAreValid()
	{
		if (listOfSelectedLetters.Count > 1)
		{
			string word = "";

			for (int i = 0; i < listOfSelectedLetters.Count; i++)
				word += listOfSelectedLetters[i].letter;

			// Proveravamo prvo da li smo rec vec jednom nasli
			if (finishedWords.Contains(word))
			{
//				Debug.Log("Ovu rec smo vec pronasli:  " + word);
				GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("AlreadyFoundWord", 0, 0);
			}
			else if (GameplayManager.gameplayManager.targetWords.Contains(word)) // Nova trazena rec
			{
				finishedWords.Add(word);

//				Debug.Log("Nova rec:   " + word);
				GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("CorrectWord", 0, 0);

				// Pustamo zvuk za pronadjenu rec
				SoundManager.Instance.Play_Sound(SoundManager.Instance.wordSolved);

				if (GameplayManager.gameplayManager.targetWords.Count <= 3)
				{
					// Pronalazimo rec i prikazujemo je
					for (int i = 0; i < GameplayManager.gameplayManager.targetWords.Count; i++)
					{
						if (GameplayManager.gameplayManager.targetWordsHolder.transform.GetChild(i).GetComponent<TargetWord>().word == word)
							GameplayManager.gameplayManager.targetWordsHolder.transform.GetChild(i).GetComponent<TargetWord>().ShowWord();
					}
				}
				else
				{
					bool wordFound = false;

					// Pronalazimo rec i prikazujemo je ako se nalazi u prvoj grupi
					for (int i = 0; i < GameplayManager.gameplayManager.targetWordsHolder1.transform.childCount; i++)
					{
						if (GameplayManager.gameplayManager.targetWordsHolder1.transform.GetChild(i).GetComponent<TargetWord>().word == word)
						{
							GameplayManager.gameplayManager.targetWordsHolder1.transform.GetChild(i).GetComponent<TargetWord>().ShowWord();
							wordFound = true;
							break;
						}
					}

					// Ako nije pronadjena u prvoj grupi trazimo je u drugoj grupi
					if (!wordFound)
					{
						for (int i = 0; i < GameplayManager.gameplayManager.targetWordsHolder2.transform.childCount; i++)
						{
							if (GameplayManager.gameplayManager.targetWordsHolder2.transform.GetChild(i).GetComponent<TargetWord>().word == word)
							{
								GameplayManager.gameplayManager.targetWordsHolder2.transform.GetChild(i).GetComponent<TargetWord>().ShowWord();
								wordFound = true;
								break;
							}
						}
					}
				}

				// Proveravamo da li smo pronasli sve reci
				if (finishedWords.Count == GameplayManager.gameplayManager.targetWords.Count)
				{
					GameplayManager.gameFinished = true;
					GameplayManager.gameplayManager.LevelFinished();
				}
				else
				{
					// Ako nije game finished povecavamo consecutive words found i ako treba prikazujemo poruku
					consecutiveWordsFound += 1;

					if (consecutiveWordsFound % 3 == 0)
					{
						consecutiveWordsAnimator.Play("GameTextAnimation", 0, 0);
						int randomConsWord = Random.Range(0, consecutiveWordsSprites.Length);

						for(int i = 0; i < consecutiveWordsSprites.Length; i++)
						{
							if (i != randomConsWord)
								consecutiveWordsSprites[i].SetActive(false);
							else
								consecutiveWordsSprites[i].SetActive(true);
						}
					}
				}
			}
			else if (finishedAdditionalWords.Contains(word)) // Vec pronadjena additional
			{
//				Debug.Log("Ovu additional rec smo vec pronasli:  " + word);
				GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("AlreadyFoundWord", 0, 0);
			}
			else if (GameplayManager.gameplayManager.additionalWords.Contains(word)) // Nova additional rec
			{
				finishedAdditionalWords.Add(word);

				// Samo ako je poslednji nivo dodajemo additional reci
				if (LevelsParser.selectedPack == LevelsParser.levelParser.lastUnlockedPack && LevelsParser.selectedWorld == LevelsParser.levelParser.lastUnlockedWorld && LevelsParser.selectedLevel == LevelsParser.levelParser.lastUnlockedLevel)
				{
					ExtraWordFound();

//					Debug.Log("Nova additional rec:   " + word);
					GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("AdditionalWord", 0, 0);
				}

				// Dodajemo jednu rec u reci za consecutve
				consecutiveWordsFound += 1;

				if (consecutiveWordsFound % 3 == 0)
				{
					consecutiveWordsAnimator.Play("GameTextAnimation", 0, 0);
					int randomConsWord = Random.Range(0, consecutiveWordsSprites.Length);

					for(int i = 0; i < consecutiveWordsSprites.Length; i++)
					{
						if (i != randomConsWord)
							consecutiveWordsSprites[i].SetActive(false);
						else
							consecutiveWordsSprites[i].SetActive(true);
					}
				}

				// Pustamo zvuk za pronadjenu rec
				SoundManager.Instance.Play_Sound(SoundManager.Instance.wordSolved);
			}
			else if (GameplayManager.gameplayManager.solvedWords.Contains(word)) // Vec pronadjena  / bonus nivo
			{
//				Debug.Log("Ovu rec smo vec pronasli:  " + word);
				GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("AlreadyFoundWord", 0, 0);
			}
			else // Ova rec ne postoji
			{
				GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("WrongWord", 0, 0);

				// Prekidamo consecutive row
				consecutiveWordsFound = 0;

				// Pustamo zvuk za pogresnu rec
				SoundManager.Instance.Play_Sound(SoundManager.Instance.wrongWord);
			}
		}
		else
		{
			// Samo iskljucujemo selektovano slovo i holder za selektovana slova
			if (listOfSelectedLetters.Count > 0)
			{
				GameplayManager.gameplayManager.selectedLettersHolder.transform.parent.GetComponent<Animator>().Play("WrongWord", 0, 0);

				// Prekidamo consecutive row
				consecutiveWordsFound = 0;

				// Pustamo zvuk za pogresnu rec
				SoundManager.Instance.Play_Sound(SoundManager.Instance.wrongWord);
			}
		}

		// Na kraju cistimo listu selektovanih slova
		listOfSelectedLetters.Clear();

		// Brisemo sve linije FIXME verovatno ce da bude neka animacija
		line.positionCount = 0;
	}
}
