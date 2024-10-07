using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TutorialSafe : MusicalSafe
{
    private Animator _animator;
    public void Start()
    {
        _animator = GetComponent<Animator>();
    }
    public override void Unlock()
    {
        base.Unlock();
        _animator.SetTrigger("LowerDoor");
    }
}
