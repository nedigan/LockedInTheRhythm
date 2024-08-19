using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    [SerializeField] private Transform _movementTransform;
    private SpriteRenderer _spriteRenderer;

    [Header("Testing")]
    [SerializeField] private Sprite[] _sprites = new Sprite[4];
    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        SetDirection(); // test 
    }

    void SetDirection()
    {
        Vector3 forward = _movementTransform.forward;

        if (Mathf.Abs(forward.z) > Mathf.Abs(forward.x))
        {
            if (forward.z > 0)
            {
                // forward sprite
                _spriteRenderer.sprite = _sprites[1];
            }
            else
            {
                // backward sprite
                _spriteRenderer.sprite = _sprites[0];
            }
        }
        else
        {
            if (forward.x > 0)
            {
                // right sprite
                _spriteRenderer.sprite = _sprites[3];
            }
            else
            {
                // left sprite
                _spriteRenderer.sprite = _sprites[2];
            }
        }
    }
}
