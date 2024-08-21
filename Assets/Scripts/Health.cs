using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private Image[] _images = new Image[3];
    private int _maxHealth = 3;
    [Range(0, 3)][SerializeField] private int _currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        _currentHealth = _maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _images.Length; i++)
        {
            if (i < _currentHealth)
            {
                _images[i].enabled = true;
            }
            else
                _images[i].enabled = false;
        }
    }

    public void LoseLife()
    {
        _currentHealth--;

        if (_currentHealth < 0 )
            _currentHealth = 0;
    }
}
