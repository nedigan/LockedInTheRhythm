using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraDetection : ConeDetection
{
    private static CameraDetection _cameraDetecting = null;

    public override void Update()
    {
        base.Update();

        if (DetectingPlayer && _cameraDetecting != this)
        {
            _cameraDetecting = this;
            EnemyAlert.NewAlert.Invoke();
            Debug.Log("Camera DETECTING...");
        }
        else if (!DetectingPlayer && _cameraDetecting == this)
            _cameraDetecting = null;

        if (DetectingPlayer)
        {
            EnemyAlert.CameraDetecting.Invoke();
        }

       // Debug.Log(CameraDetecting);
    }
}
