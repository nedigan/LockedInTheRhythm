using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _pauseUI;

    public void Pause()
    {
        _pauseUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void UnPause()
    {
        _pauseUI.SetActive(false);
        Time.timeScale = 1f;
    }

}
