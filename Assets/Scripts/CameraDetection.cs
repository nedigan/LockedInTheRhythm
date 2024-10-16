using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

public class CameraDetection : ConeDetection
{
    private static CameraDetection _cameraDetecting = null;

    public static bool AnyCameraDetecting { get; private set; }

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


        AnyCameraDetecting = _cameraDetecting != null;

        if (DetectingPlayer) // CHANGE THIS MAYBE
        {
            EnemyAlert.CameraDetecting.Invoke();
        }
       // Debug.Log(CameraDetecting);
    }
}
