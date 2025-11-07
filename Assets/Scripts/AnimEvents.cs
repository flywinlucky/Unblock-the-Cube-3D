using UnityEngine;
using System.Collections;
using UnityEngine.UI;

///<summary>
///<para>Scene:All/NameOfScene/NameOfScene1,NameOfScene2,NameOfScene3...</para>
///<para>Object:N/A</para>
///<para>Description: Sample Description </para>
///</summary>

public class AnimEvents : MonoBehaviour {

	public void LoadScene()
	{
		Application.LoadLevel("Loading");
	}

	public void DeactivateLetters()
	{
		// FIXME za sad cu samo da iskljucim sva slova i holder za njih (onaj deo kad se selektuju slovca)
		for (int i = 0; i < transform.GetChild(0).childCount; i++)
		{
			transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
		}
	}

	public void PlayChildElementParticle()
	{
		transform.GetChild(0).GetComponent<ParticleSystem>().Play();
	}

	public void StopChildElementParticle()
	{
		transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
	}

	public void PlayLoadingArriveSound()
	{
		SoundManager.Instance.Play_Sound(SoundManager.Instance.loadingArrive);
	}

	public void PlayLoadingDepartSound()
	{
		SoundManager.Instance.Play_Sound(SoundManager.Instance.loadingDepart);
	}
}
