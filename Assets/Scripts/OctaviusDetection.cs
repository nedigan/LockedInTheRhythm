using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OctaviusDetection : ConeDetection
{

    [SerializeField] private LayerMask _footprintLayer;
    [SerializeField] private float _footprintSearchRange;


    public bool DetectingFootprint { get; protected set; }
    public List<FootprintFade> FootprintsInRange { get; protected set; } = new List<FootprintFade>();  

    public override void Update()
    {
        base.Update();
        DetectFootprint();
    }

    private void DetectFootprint()
    {
        DetectingFootprint = false;
        // Get all colliders within the radius
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, _footprintSearchRange, _footprintLayer);
        FootprintsInRange.Clear();

        // Loop through colliders and check for the specific component
        foreach (Collider collider in collidersInRange)
        {
            FootprintFade component = collider.GetComponent<FootprintFade>();
            if (component != null)
            {
                DetectingFootprint = true;
                FootprintsInRange.Add(component);
            }
        }
    }
}
