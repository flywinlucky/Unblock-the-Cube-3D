using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using YG;

public class ShopManager : MonoBehaviour
{

	public Text removeAdsPriceText;
	public Text smallPackPriceText;
	public Text mediumPackPriceText;
	public Text bigPackPriceText;

	// Main scene buttons
	public GameObject mainScreenCoinsHolder;
	public GameObject mainScreenStarsHolder;
	public GameObject levelSelectButtonsHolder;
	public GameObject worldSelectButtonsHolder;
	public GameObject mainSceneShopBackButtonHolder;
	public GameObject mainSceneShopCoinsHolder;

	// Level scene buttons
	public GameObject levelSceneCoinsHolder;
	public GameObject pauseButtonHolder;
	public GameObject levelSceneShopBackButtonHolder;
	public GameObject levelSceneShopCoinsHolder;

	// Animacija za dodavanje coina
	public GameObject addCoinsAnimationHolder;

	// Video nije dostupan popup
	public GameObject videoNotAvailablePopup;

	public void AddCoinsAnimation()
	{
		StartCoroutine("AddCoinsCoroutine");
	}

	IEnumerator AddCoinsCoroutine()
	{
		addCoinsAnimationHolder.SetActive(true);
		//		SoundManager.Instance.Play_Sound(SoundManager.Instance.claimExtraCoins);

		yield return new WaitForSeconds(2f);

		if (SceneManager.GetActiveScene().name == "MainScene")
			LevelSelectManager.levelSelectManager.RefreshStarsAndCoins();
		else if (SceneManager.GetActiveScene().name == "Level")
		{
			GameplayManager.gameplayManager.coinsText.text = GlobalVariables.coins.ToString();
			GameplayManager.gameplayManager.coinsTextShop.text = GlobalVariables.coins.ToString();
		}

		addCoinsAnimationHolder.SetActive(false);
	}

	public void WatchVideoForCoins()
	{
	  YG2.RewardedAdvShow("continue_game_over_popup", () =>
	  {
		 AddCoinsAfterVideoWatched();
	  });
	}

	public void AddCoinsAfterVideoWatched()
	{
		// Dodajemo coine i pustamo animaciju
		GlobalVariables.globalVariables.AddCoins(300000);
		addCoinsAnimationHolder.transform.Find("AnimationHolder/CoinsHolder/CoinsNumberTextShop").GetComponent<Text>().text = "+300000";
		AddCoinsAnimation();
	}
}