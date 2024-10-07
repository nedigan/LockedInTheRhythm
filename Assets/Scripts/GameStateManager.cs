using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainState,
    EndState

}
public interface IHandleGameState
{
    void ChangeState(GameState state);
}
public class GameStateManager : MonoBehaviour
{
    [SerializeField] private SceneChanger _changer;
    [SerializeField] private List<MusicalSafe> _safes = new List<MusicalSafe>();


    private List<IHandleGameState> _stateHandlers = new List<IHandleGameState>();
    private GameState _currentState;

    private void Start()
    {
        _stateHandlers = Find<IHandleGameState>();
    }
    public void GameOver()
    {
        // Do something...

        _changer.GameOver();
    }

    public void GameWin()
    {
        Debug.Log("WINNER!");
        // _changer.GameWin(); TODO
    }

    private void Update()
    {
        if (_currentState == GameState.MainState)
        {
            // Condition to switch state
            if (_safes.Count((safe) => !safe.Locked) == _safes.Count)
            {
                _currentState = GameState.EndState;

                foreach (var handler in _stateHandlers)
                {
                    handler.ChangeState(GameState.EndState);
                }
            }
        }
    }

    private List<T> Find<T>()
    {
        List<T> interfaces = new List<T>();
        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (var rootGameObject in rootGameObjects)
        {
            T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>();
            foreach (var childInterface in childrenInterfaces)
            {
                interfaces.Add(childInterface);
            }
        }

        return interfaces;
    }
}
