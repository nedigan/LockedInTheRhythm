using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minigame : MonoBehaviour
{
    [SerializeField] private Song _song; // testing - play method will take song from safe
    [SerializeField] private float _speed = 5f; // testing
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private Transform[] _spawnPoints = new Transform[4];
    [SerializeField] private Transform[] _targetPoints = new Transform[4];
    [SerializeField] private Slider _slider;
    [SerializeField] private Image[] _trackIndicators = new Image[4];

    private List<GameObject> _currentNotes = new List<GameObject>();


    private List<KeyValuePair<Note, float>> _notes;

    private void OnEnable()
    {
        Play();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        DestroyNotes();
    }

    private void Update()
    {
        foreach (Image image in  _trackIndicators)
            image.color = Color.white;

        if (_slider.value >= 0 && _slider.value < 0.10f)
            _trackIndicators[3].color = Color.black;
        else if (_slider.value < 0.50f)
            _trackIndicators[2].color = Color.black;
        else if ( _slider.value < 0.90f)
            _trackIndicators[1].color = Color.black;
        else
            _trackIndicators[0].color = Color.black;
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
        _currentNotes.Add(note.gameObject);

        yield return new WaitForSeconds(_notes[index].Value);

        StartCoroutine(SpawnNote((index + 1) % _notes.Count)); // next note and wraps around
    }

    private void DestroyNotes()
    {
        foreach(GameObject note in _currentNotes)
        {
            Destroy(note);  
        }
        _currentNotes.Clear();  
    }
}
