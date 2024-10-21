using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    [SerializeField] private MusicalSafe[] _safes;
    [SerializeField] private AudioClip[] _songs;
    private int _currentSongIndex = 0;
    private AudioSource _audioSource;
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>(); 
    }

    // Update is called once per frame
    void Update()
    {
        int numUnlockedSafes = _safes.Count((safe) => !safe.Locked);

        if (numUnlockedSafes !=  _currentSongIndex)
        {
            NextSong();
        }
    }

    void NextSong()
    {
        _currentSongIndex++;
        float time = _audioSource.time; // might need this
        _audioSource.clip = _songs[_currentSongIndex];
        _audioSource.Play();
        _audioSource.time = time;
        //_audioSource.Play();
    }

    public void Pause()
    {
        _audioSource.Pause();
    }

    public void UnPause()
    {
        _audioSource.UnPause();
    }
}
