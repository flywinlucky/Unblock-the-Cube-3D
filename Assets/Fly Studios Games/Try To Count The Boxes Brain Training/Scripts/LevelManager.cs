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
    public float activeDuration = 5f;

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

        StartCoroutine(ActiveDelayInitialization());
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

    public  IEnumerator ActiveDelayInitialization()
    {
        levelsCubes.SetActive(false);
        yield return new WaitForSeconds(1);
        levelsCubes.SetActive(true);
        yield return new WaitForSeconds(activeDuration);
        levelsCubes.SetActive(false);
    }
}
