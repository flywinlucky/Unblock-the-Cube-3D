using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public List<Cube> cubes = new List<Cube>();
    public int cubesCount;

    // Start is called before the first frame update
    void Start()
    {
        InitializeLevel();
    }

    public void InitializeLevel()
    {
        // Găsește toate componentele Cube din copiii obiectului curent
        foreach (Transform child in transform)
        {
            Cube cube = child.GetComponent<Cube>();
            if (cube != null)
            {
                cubes.Add(cube);
            }
        }

        // Setează numărul total de cuburi
        cubesCount = cubes.Count;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ShowChildsMaterialFocus();
        }
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
}
