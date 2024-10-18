using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertText : MonoBehaviour
{
    private Animator _animator;
    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
    public void NewAlert(Vector3 octPos)
    {
        Vector2 pos = (Camera.main.WorldToViewportPoint(octPos));
        pos = new Vector2(Mathf.Clamp(pos.x, 0.1f, 0.9f), Mathf.Clamp(pos.y, 0.1f, 0.9f));
        transform.position = Camera.main.ViewportToScreenPoint(pos);

        if (!_animator.GetBool("Playing"))
            _animator.SetTrigger("StartAlertText");
    }
}
