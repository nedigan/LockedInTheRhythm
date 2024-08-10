using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderQueue : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private int _renderQueueIndex = 3000;
    // Start is called before the first frame update
    void Start()
    {
        if ( _material != null)
        {
            _material.renderQueue = _renderQueueIndex;
        }
    }
}
