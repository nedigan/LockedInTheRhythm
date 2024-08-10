using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicalSafe : Interactable
{
    [SerializeField] private Material _outlineMaterial;
    private float _scale;
    public void Awake()
    {
        _scale = _outlineMaterial.GetFloat("_Scale");
        Highlight(false);
    }
    public override void Highlight(bool highlight)
    {
        Debug.Log(_outlineMaterial.GetFloat("_Scale"));
        if (!highlight)
            _outlineMaterial.SetFloat("_Scale", 0);
        else
            _outlineMaterial.SetFloat("_Scale", _scale);
    }

    public override void Interact()
    {
        Debug.Log("Interacted with musical safe");
    }

    
}
