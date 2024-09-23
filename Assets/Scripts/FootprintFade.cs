using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class FootprintFade : MonoBehaviour
{
    [SerializeField] private float _fadeTime = 10f;
    private Material _material;
    private Color _color;

    private float _time;

    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<Renderer>().material;
        _color = _material.GetColor("_EmissionColor");
    }

    private void Update()
    {
        float t = Mathf.Clamp01(_time / _fadeTime);

        Color newColor = Color.Lerp(_color, Color.black, t);
        _material.SetColor("_EmissionColor", newColor);

        _time += Time.deltaTime;
        if (Mathf.Approximately(t, 1f))
            Destroy(gameObject);
    }
}
