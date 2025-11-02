using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public Material grayMaterial;
    public Material greenMaterial;
    public MeshRenderer cubeMesh;

    // Start is called before the first frame update
    void Start()
    {
        cubeMesh.material = grayMaterial;
    }

    public void SetGreenMaterial()
    {
             cubeMesh.material = greenMaterial;
    }
}