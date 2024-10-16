using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(LookAtConstraint))]
public class SpriteLookAtCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LookAtConstraint constraint = GetComponent<LookAtConstraint>();

        ConstraintSource source = new();
        source.sourceTransform = Camera.main.transform;
        source.weight = 1.0f;

        constraint.AddSource(source);
        constraint.constraintActive = true;
    }
}
