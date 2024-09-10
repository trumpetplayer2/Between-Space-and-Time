using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //Camera Follow Variables
    public float maxX = 7;
    public float minX = -7;
    public float maxY = 10;
    public float minY = 0;
    public Transform playerTracker;
    public float smoothSpeed = 0.125f;
    public Vector3 locationOffset;
    public Vector3 rotationOffset;
    //Camera Shake Variables
    public Transform cameraTransform;
    public float shakeAmount = 0.25f;
    public float decreaseSpeed = 1.0f;
    public float shakeDuration = 0f;
    public static CameraFollow instance;

    public void Awake()
    {
        instance = this;
    }

    public void FixedUpdate()
    {
        if (playerTracker == null) return;

        Vector3 tempTracker = new Vector3(playerTracker.position.x, playerTracker.position.y, playerTracker.position.z);

        if(playerTracker.position.x < minX)
        {
            tempTracker = new Vector3(minX, playerTracker.position.y, playerTracker.position.z);
        }else if(playerTracker.position.x > maxX)
        {
            tempTracker = new Vector3(maxX, playerTracker.position.y, playerTracker.position.z);
        }

        if (playerTracker.position.y < minY)
        {
            tempTracker = new Vector3(tempTracker.x, minY, playerTracker.position.z);
        }
        else if (playerTracker.position.y > maxY)
        {
            tempTracker = new Vector3(tempTracker.x, maxY, playerTracker.position.z);
        }
        //Check player relation to camera
        Vector3 desiredPosition = tempTracker + playerTracker.rotation * locationOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

        Quaternion desiredrotation = playerTracker.rotation * Quaternion.Euler(rotationOffset);
        Quaternion smoothedrotation = Quaternion.Lerp(transform.rotation, desiredrotation, smoothSpeed);
        transform.rotation = smoothedrotation;
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            cameraTransform.localPosition = cameraTransform.position + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseSpeed;
        }
    }
}
