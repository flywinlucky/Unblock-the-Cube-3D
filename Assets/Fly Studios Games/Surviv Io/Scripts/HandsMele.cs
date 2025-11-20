using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsMele : MonoBehaviour
{
    public Transform LeftHandPosition;
    public Transform RightHandPosition;
    public CircleCollider2D handColider2D;

    // Start is called before the first frame update
    void Start()
    {
        handColider2D = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
