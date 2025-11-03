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

    public void InitializeLevel()
    {
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
            yield return new WaitForSeconds(0.1f);
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
