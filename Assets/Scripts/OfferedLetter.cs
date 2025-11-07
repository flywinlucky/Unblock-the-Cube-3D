using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfferedLetter : MonoBehaviour {

	public string letter;

	public Image letterHolder;

	void Awake()
	{
		letterHolder = transform.Find("LetterImage").GetComponent<Image>();
	}
}
