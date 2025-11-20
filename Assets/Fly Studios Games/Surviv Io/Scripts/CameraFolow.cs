using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFolow : MonoBehaviour
{
    public BoxCollider2D arenaBounds;
    public Transform playerPosition;

    [SerializeField]
    private float smoothTime = 0.3f;

    private Vector3 _velocity = Vector3.zero;
    private float _cameraZPosition;
    private Camera _cam;

    void Start()
    {
        _cameraZPosition = transform.position.z;
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;
    }

    void FixedUpdate()
    {
        if (playerPosition == null)
            return;

        Vector3 desiredPosition = new Vector3(
            playerPosition.position.x,
            playerPosition.position.y,
            _cameraZPosition
        );

        if (arenaBounds != null && _cam != null && _cam.orthographic)
        {
            Bounds b = arenaBounds.bounds;
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float minX = b.min.x + halfW;
            float maxX = b.max.x - halfW;
            float minY = b.min.y + halfH;
            float maxY = b.max.y - halfH;

            if (b.size.x < halfW * 2f)
                desiredPosition.x = b.center.x;
            else
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

            if (b.size.y < halfH * 2f)
                desiredPosition.y = b.center.y;
            else
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _velocity,
            smoothTime
        );
    }

}