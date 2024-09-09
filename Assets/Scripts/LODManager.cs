using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODManager : MonoBehaviour
{
    [SerializeField] private VisionCone _visionConeScript;
    [SerializeField] private ConeDetection _coneDetectionScript;
    [SerializeField] private GameObject _scene;
    [SerializeField] private float _range = 50f;

    private Transform _player;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform; // Probably slow
    }

    // Update is called once per frame
    void Update()
    {
        bool enableScripts = (Vector3.Distance(transform.position, _player.position) < _range);
        _visionConeScript.ShowVisionCone = enableScripts;
        _coneDetectionScript.enabled = enableScripts;
    }
}
