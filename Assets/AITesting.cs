using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AITesting : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _ai;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _ai.SetDestination(transform.position); // sets ai path to go to player
        }
    }
}
