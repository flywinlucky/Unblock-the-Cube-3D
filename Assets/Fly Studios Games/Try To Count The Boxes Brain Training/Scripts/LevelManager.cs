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
    public int activeDuration = 2;

    public void InitializeLevel()
    {
        // Golește lista de cuburi pentru a evita duplicarea
        cubes.Clear();

        // Găsește toate componentele Cube din copiii lui leveCubesRoot
        if (leveCubesRoot != null)
        {
            foreach (Transform child in leveCubesRoot)
            {
                Cube cube = child.GetComponent<Cube>();
                if (cube != null)
                {
                    cubes.Add(cube);
                }
            }
        }
        else
        {
            Debug.LogWarning("leveCubesRoot nu este setat în LevelManager.");
        }

        // Setează numărul total de cuburi
        cubesCount = cubes.Count;

    }

    public void ShowChildsMaterialFocus()
    {
        StartCoroutine(ShowChildsMaterialFocusRoutine());
    }

    private IEnumerator ShowChildsMaterialFocusRoutine()
    {
        foreach (Cube cube in cubes)
        {
            cube.SetGreenMaterial();
            yield return new WaitForSeconds(0.1f); // Așteaptă 0.3 secunde între fiecare schimbare
        }
    }

    public void StartActiveCountdown(int startValue, System.Action onComplete)
    {
        StartCoroutine(CountdownRoutine(startValue, onComplete));
    }

    private IEnumerator CountdownRoutine(int startValue, System.Action onComplete)
    {
        yield return new WaitForSeconds(activeDuration);
        ActivateLevelsCubesFlorrCell(false);
        ActivateSelfFlorrCell(false);
        onComplete?.Invoke();
    }

    public void ActivateSelfFlorrCell(bool state)
    {
        floorCells.SetActive(state);
    }
     public void ActivateLevelsCubesFlorrCell(bool state)
    {
        levelsCubes.SetActive(state);  
    }
}
