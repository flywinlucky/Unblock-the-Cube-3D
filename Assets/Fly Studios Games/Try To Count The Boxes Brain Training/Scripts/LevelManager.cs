using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public List<Cube> cubes = new List<Cube>();
    public int cubesCount;
    public Transform leveCubesRoot;
    public GameObject levelsCubes;
    public GameObject floorCells;
    [Header("Animation")]
    public float activeDuration;

    public bool isEasy;
    public bool isNormal;
    public bool isHard;

    private AudioManager audioManager;
    public void InitializeLevel()
    {
        audioManager = FindObjectOfType<AudioManager>();

        cubes.Clear();

        if (leveCubesRoot == null)
        {
            Debug.LogWarning("leveCubesRoot is not set in LevelManager.");
            return;
        }

        foreach (Transform child in leveCubesRoot)
        {
            if (child.TryGetComponent(out Cube cube))
            {
                cubes.Add(cube);
            }
        }

        cubesCount = cubes.Count;

        if (isEasy)
        {
            activeDuration = 0.250f * cubesCount;
        }
        if (isNormal)
        {
            activeDuration = 0.390f * cubesCount;
        }
        if (isHard)
        {
            activeDuration = 0.550f * cubesCount;
        }

        Debug.Log("set active Duration : " + activeDuration);
    }

    public void ShowChildsMaterialFocus()
    {
        if (cubes.Count > 0)
        {
            StartCoroutine(ShowChildsMaterialFocusRoutine());
        }
    }

    private IEnumerator ShowChildsMaterialFocusRoutine()
    {
        foreach (Cube cube in cubes)
        {
            cube.SetGreenMaterial();
            audioManager.PlayCountShowRCubes();
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void StartActiveCountdown(System.Action onComplete)
    {
        if (activeDuration > 0)
        {
            StartCoroutine(CountdownRoutine(onComplete));
        }
        else
        {
            Debug.LogWarning("Active duration is not set or invalid.");
            onComplete?.Invoke();
        }
    }

    private IEnumerator CountdownRoutine(System.Action onComplete)
    {
        yield return new WaitForSeconds(activeDuration);
        ActivateLevelsCubesFlorrCell(false);
        ActivateSelfFlorrCell(false);
        onComplete?.Invoke();
    }

    public void ActivateSelfFlorrCell(bool state)
    {
        if (floorCells != null)
        {
            floorCells.SetActive(state);
        }
    }

    public void ActivateLevelsCubesFlorrCell(bool state)
    {
        if (levelsCubes != null)
        {
            levelsCubes.SetActive(state);
        }
    }
}
