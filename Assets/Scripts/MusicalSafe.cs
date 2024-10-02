using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicalSafe : Interactable
{
    [SerializeField] private Material _outlineMaterial;
    [SerializeField] private GameObject _minigameUI;
    [SerializeField] private Minigame _minigame;
    private float _scale;
    public bool Locked { get; private set; } = true;
    public void Awake()
    {
        _scale = 1.1f;
        Highlight(false);
    }

    private void Update()
    {
        // Testing
        if (Input.GetKeyDown(KeyCode.Escape))
            _minigameUI.SetActive(false);
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
        if (!Locked) return; // If unlocked dont let the minigame begin
        Debug.Log("Interacted with musical safe");
        _minigameUI.SetActive(true); // testing
        _minigame.Play(this);
    }

    public void Unlock()
    {
        Locked = false;
        // Change the look of the safe??
    }
}
