using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class FootprintFade : MonoBehaviour, IComparable<FootprintFade>
{
    [SerializeField] private float _fadeTime = 2f; // fade time when at max tempo

    private Material _material;
    private Color _color;

    private bool _visted = false;
    private float _time;
    private float _t;
    public float OctaviusT // used in octavius' behaviour so that once he reaches a footprint it will always return 1 so it is ignored
    {
        get
        {
            if (_visted)
                return 1;
            else
                return _t;
        }
    }

    public int CompareTo(FootprintFade obj)
    {
        return OctaviusT.CompareTo(obj.OctaviusT);
    }

    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<Renderer>().material;
        _color = _material.GetColor("_EmissionColor");

        _fadeTime = _fadeTime / TempoSlider.Instance.Value;
    }
    
    public void Visted()
    {
        _visted = true; // prolly redundant now
    }

    private void Update()
    {
        _t = Mathf.Clamp01(_time / _fadeTime);

        Color newColor = Color.Lerp(_color, Color.black, _t);
        _material.SetColor("_EmissionColor", newColor);

        _time += Time.deltaTime;
        if (Mathf.Approximately(_t, 1f))
            Destroy(gameObject.transform.parent.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Octavius"))
        {
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
