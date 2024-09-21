using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CatchPlayer : MonoBehaviour
{
    [SerializeField] private UnityEvent _onPlayerCaught;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _onPlayerCaught.Invoke();
        }
    }
}
