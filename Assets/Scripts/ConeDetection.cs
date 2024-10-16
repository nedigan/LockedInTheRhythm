using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ConeDetection : MonoBehaviour
{
    [SerializeField] private VisionCone _visionCone;
    [SerializeField] private LayerMask _obstructionMask;

    [SerializeField]private float _coneResolution;
    [SerializeField]private float _coneRange;
    [SerializeField] private float _coneAngle; // should be the same as the vision code most of the time

    public bool DetectingPlayer { get; protected set; }

    // Used to check if anything is detecting
    private static ConeDetection _anyDetection = null;
    public static bool AnythingDetecting { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        if (_visionCone != null)
        {
            _coneResolution = _visionCone.VisionConeResolution;
            _coneRange = _visionCone.VisionRange;
        }

        _coneAngle *= Mathf.Deg2Rad;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        DetectPlayer();

        if (DetectingPlayer && _anyDetection != this)
        {
            _anyDetection = this;
        }
        else if (!DetectingPlayer && _anyDetection == this)
            _anyDetection = null;

        AnythingDetecting = _anyDetection != null;
    }

    private void DetectPlayer()
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

            if (Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit, _coneRange, _obstructionMask))
            {
                //Debug.Log($"Detecting!!!!! {hit.collider.gameObject.name}");
                if (hit.collider.CompareTag("Player"))
                {
                    DetectingPlayer = true;
                    return;
                }
            }

            currentangle += angleIcrement;
        }
        DetectingPlayer = false;
    }
}
