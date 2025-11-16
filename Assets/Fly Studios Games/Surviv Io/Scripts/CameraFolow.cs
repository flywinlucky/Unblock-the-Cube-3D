using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFolow : MonoBehaviour
{
    public Transform playerPosition;

    [SerializeField]
    private float smoothTime = 0.3f;

    private Vector3 _velocity = Vector3.zero;
    private float _cameraZPosition;

    void Start()
    {
        _cameraZPosition = transform.position.z;
    }

    void LateUpdate()
    {
        if (playerPosition == null)
        {
            return;
        }

        Vector3 desiredPosition = new Vector3(
            playerPosition.position.x,
            playerPosition.position.y,
            _cameraZPosition
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _velocity,
            smoothTime
        );
    }
}