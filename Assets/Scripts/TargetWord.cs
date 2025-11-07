using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetWord : MonoBehaviour {

	public string word;
	public bool wordSolved;

	void Awake()
	{
		wordSolved = false;
	}

	// Prikazuje rec kada je pronadjena
	public void ShowWord()
	{
		StartCoroutine("ShowLettersPeriodically");

		wordSolved = true;
	}

	IEnumerator ShowLettersPeriodically()
	{
		// FIXME Za sada prolazimo kroz sva slova i ukljucujemo text komponentu
		for (int i = 0; i < transform.childCount; i++)
		{
			
			//			transform.GetChild(i).Find("AnimationHolder/LetterText").GetComponent<Text>().enabled = true;
//			transform.GetChild(i).GetChild(0).GetComponent<Animator>().SetTrigger("Solved");

			if (transform.GetChild(i).GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Empty"))
				transform.GetChild(i).GetChild(0).GetComponent<Animator>().Play("Solved", 0, 0);
			else if (transform.GetChild(i).GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Hinted"))
				transform.GetChild(i).GetChild(0).GetComponent<Animator>().Play("HintedSolved", 0, 0);
			else
				transform.GetChild(i).GetChild(0).GetComponent<Animator>().Play("BonusLetterSolved", 0, 0);

			if (GameplayManager.gameplayManager.bonusLetters.Contains(transform.GetChild(i).gameObject))
			{
				GlobalVariables.globalVariables.AddCoins(1);
				GameplayManager.gameplayManager.coinsText.text = GlobalVariables.coins.ToString();
				GameplayManager.gameplayManager.coinsTextShop.text = GlobalVariables.coins.ToString();

				// Pustamo zvuk za dodavanje jednog novcica
				SoundManager.Instance.Play_Sound(SoundManager.Instance.bonuscoin);
			}

			yield return new WaitForSeconds(0.12f);

			// FIXME za sada da bi sva slovca bila iste velicine
			//			transform.GetChild(i).Find("AnimationHolder/LetterImage").GetComponent<Image>().SetNativeSize();
			//			transform.GetChild(i).Find("AnimationHolder/LetterImage").transform.localScale = new Vector3(0.58f, 0.58f, 0.58f);
		}
	}
}
