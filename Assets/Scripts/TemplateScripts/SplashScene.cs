using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScene : MonoBehaviour
{
	int appStartedNumber;
	string sceneToLoad;
	// Use this for initialization
	void Start()
	{
		sceneToLoad = "MainScene";

		if (PlayerPrefs.HasKey("appStartedNumber"))
		{
			appStartedNumber = PlayerPrefs.GetInt("appStartedNumber");
		}
		else
		{
			appStartedNumber = 0;
		}

		appStartedNumber++;
		PlayerPrefs.SetInt("appStartedNumber", appStartedNumber);
        SceneManager.LoadScene(sceneToLoad);
	}
}