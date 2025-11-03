using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public Material grayMaterial;
    public Material greenMaterial;
    public MeshRenderer cubeMesh;

    private void Awake()
    {
        if (cubeMesh == null)
        {
            Debug.LogError("CubeMesh is not assigned in " + gameObject.name);
        }
        if (grayMaterial == null || greenMaterial == null)
        {
            Debug.LogError("Materials are not assigned in " + gameObject.name);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (cubeMesh != null && grayMaterial != null)
        {
            cubeMesh.material = grayMaterial;
        }
    }

    public void SetGreenMaterial()
    {
        if (cubeMesh != null && greenMaterial != null)
        {
            cubeMesh.material = greenMaterial;
        }
    }
}