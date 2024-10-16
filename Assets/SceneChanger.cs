using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private Animator _transition;
    [SerializeField] private float _transitionTime = 1f;
    public void GameOver()
    {
        StartCoroutine(LoadScene(2));
    }

    public void GameWin()
    {
        // TODO: create win screen
        StartCoroutine(LoadScene(3));
    }

    public void MainScreen()
    {
        StartCoroutine(LoadScene(0));
    }

    public void TryAgain()
    {
        StartCoroutine(LoadScene(1));
    }

    IEnumerator LoadScene(int sceneIndex)
    {
        _transition.SetTrigger("Start");

        yield return new WaitForSeconds(_transitionTime);

        SceneManager.LoadScene(sceneIndex);
    }
}
