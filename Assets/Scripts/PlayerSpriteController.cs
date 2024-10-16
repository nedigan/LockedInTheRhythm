using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteController : SpriteController
{
    public override void Update()
    {
        base.Update();
        if (ConeDetection.AnythingDetecting)
        {
            _spriteRenderer.color = new Color(1, 0.4784314f, 0.4784314f); // hacky asf
        }
        else
        {
            _spriteRenderer.color = Color.white;
        }
    }
}
