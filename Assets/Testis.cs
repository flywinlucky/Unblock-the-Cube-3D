using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Testis : MonoBehaviour {

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.x, transform.GetChild(i).GetChild(0).GetComponent<RectTransform>().sizeDelta.y);
			}

			GetComponent<ContentSizeFitter>().enabled = false;
			GetComponent<ContentSizeFitter>().enabled = true;

			//transform.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

			transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(transform.GetComponent<RectTransform>().anchoredPosition.x, -transform.GetComponent<RectTransform>().sizeDelta.y / 2);

		}
	}
}
