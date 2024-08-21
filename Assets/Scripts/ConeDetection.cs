using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ConeDetection : MonoBehaviour
{
    [SerializeField] private VisionCone _visionCone;
    [SerializeField] private LayerMask _detectionMask;

    private float _coneResolution;
    private float _coneRange;
    [SerializeField] private float _coneAngle; // should be the same as the vision code most of the time
    // Start is called before the first frame update
    void Start()
    {
        _coneResolution = _visionCone.VisionConeResolution;
        _coneRange = _visionCone.VisionRange;
        _coneAngle *= Mathf.Deg2Rad;
    }

    // Update is called once per frame
    void Update()
    {
        DetectObject();
    }

    private void DetectObject()
    {
        float currentangle = -_coneAngle / 2;
        float angleIcrement = _coneAngle / (_coneResolution - 1);
        float sine;
        float cosine;

        for (int i = 0; i < _coneResolution; i++)
        {
            sine = Mathf.Sin(currentangle);
            cosine = Mathf.Cos(currentangle);
            Vector3 raycastDirection = (transform.forward * cosine) + (transform.right * sine);

            if (Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit, _coneRange, _detectionMask))
            {
                Debug.Log($"Detecting!!!!! {hit.collider.gameObject.name}");
            }

            currentangle += angleIcrement;
        }
    }
}
