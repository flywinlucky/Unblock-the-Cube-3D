using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AutoNextLevel : MonoBehaviour
{
    public float nextlevelDelaiTime;
    public UnityEvent onNextLevel;

    void OnEnable()
    {
        StartCoroutine(AutoNextLevelDelay());
    }
    private IEnumerator AutoNextLevelDelay()
    {
        yield return new WaitForSeconds(nextlevelDelaiTime);
        onNextLevel?.Invoke();
    }
}