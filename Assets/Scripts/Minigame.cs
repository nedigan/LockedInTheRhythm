using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame : MonoBehaviour
{
    [SerializeField] private Song _song; // testing - play method will take song from safe
    [SerializeField] private float _speed = 5f; // testing
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private Transform[] _spawnPoints = new Transform[4];
    [SerializeField] private Transform[] _targetPoints = new Transform[4];


    private List<KeyValuePair<Note, float>> _notes;

    // Start is called before the first frame update
    void Start()
    {
        Play();
    }

    public void Play(Song song = null)
    {
        if (song == null)
            song = _song;

        _notes = song.LoadSong();

        StartCoroutine(SpawnNote(0));
    }

    IEnumerator SpawnNote(int index)
    {
        NoteMovement note = Instantiate(_notePrefab, _spawnPoints[_notes[index].Key.Index].position, _notePrefab.transform.rotation, transform).GetComponent<NoteMovement>();
        note.Setup(_speed, _targetPoints[_notes[index].Key.Index].position);

        yield return new WaitForSeconds(_notes[index].Value);

        StartCoroutine(SpawnNote((index + 1) % _notes.Count)); // next note and wraps around
    }
}
