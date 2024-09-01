using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraDetection : ConeDetection
{
    private static CameraDetection _cameraDetecting = null;
    public static UnityEvent NewCameraSpotted = new UnityEvent();

    public override void Update()
    {
        base.Update();

        if (DetectingPlayer && _cameraDetecting != this)
        {
            _cameraDetecting = this;
            NewCameraSpotted.Invoke();
        }
        else if (!DetectingPlayer && _cameraDetecting == this)
            _cameraDetecting = null;

       // Debug.Log(CameraDetecting);
    }
}
