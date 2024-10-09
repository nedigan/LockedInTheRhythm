using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Minigame : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private Song _song; // testing - play method will take song from safe
    [SerializeField] private float _speed = 5f; // testing
    [SerializeField] private float _hitNoteOffset = 10f;

    private float _hitPositionX;
    private int _combo = 0;
    private int _highestCombo = 0;

    private bool _playing = false;

    // used in calculating stamina
    private int _amountOfNotes = 0;

    private MusicalSafe _currentSafe = null;


    [Header("References")]
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private Transform[] _spawnPoints = new Transform[4];
    [SerializeField] private Transform[] _targetPoints = new Transform[4];
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private Health _health;
    [SerializeField] private OctaviusBehaviour _octaviusBehaviour; // for setting alert level and alerting him to player location
    [SerializeField] private PlayerMovement _playerMovement; // for refilling stamina

    [SerializeField] private Image[] _trackIndicators = new Image[4]; // lines on the minigame to indicate which track is selected
    private int _currentTrackIndex = 0;

    [Header("Debugging")]
    [SerializeField] private Transform _canvasTransform; // for showing where a note is marked as correct


    private List<NoteMovement> _currentNotes = new List<NoteMovement>();
    private List<KeyValuePair<Note, float>> _notes;

    private void Start()
    {
        // testing - just uses the first target points x positions for ease
        _hitPositionX = _targetPoints[0].position.x +  _hitNoteOffset;
    }

    private void OnDrawGizmos()
    {
        _hitPositionX = _targetPoints[0].position.x + _hitNoteOffset;
        Gizmos.matrix = _canvasTransform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(new Vector3(_hitPositionX, 0), new Vector3(10,500,10));
    }

    private void OnEnable()
    {
        Time.timeScale = 0.0f;
        ResetMinigame();
        //Play(); This is now called from the musical safe
    }

    private void OnDisable()
    {
        Time.timeScale = 1.0f;
        StopAllCoroutines();
        DestroyNotes();
    }

    private void Update()
    {
        foreach (Image image in  _trackIndicators)
            image.color = Color.white;

        if (_slider.value >= 0 && _slider.value < 0.10f)
            _currentTrackIndex = 3; // player is selecting bottom track
        else if (_slider.value < 0.50f)
            _currentTrackIndex = 2; // player is selecting second from bottom
        else if (_slider.value < 0.90f)
            _currentTrackIndex = 1; // player is selecting second from top
        else
            _currentTrackIndex = 0; // player is selecting top track

        if (_currentNotes.Count == 0) // TODO: this will be called if there is a long break
            EndMinigame(false);

        _trackIndicators[_currentTrackIndex].color = Color.gray; // set players track to black - prolly change to something prettier

        CheckCorrectNotes();
        //Debug.Log("AHAHAH");
    }

    private void CheckCorrectNotes()
    {
        // notes that have been destroyed
        List<NoteMovement> toRemove = new List<NoteMovement>();
        foreach (var note in _currentNotes)
        {
            if (note.transform.position.x > _hitPositionX && _currentTrackIndex == note.TrackIndex)
            // Player has correctly positioned the toggle thing
            {
                toRemove.Add(note);
                note.wasHit = true; // just for redundancy, makes sure they dont remove themselves

                _combo++;
                _comboText.text = $"Combo: x{_combo}";

                if (_combo > _highestCombo)
                    _highestCombo = _combo;
            }
        }

        foreach (var note in toRemove)
        {
            _currentNotes.Remove(note);
            Destroy(note.gameObject);
        }
    }

    public void Play(MusicalSafe safe, Song song = null)
    {
        if (_playing) return;

        if (song == null)
            song = _song;

        _notes = song.LoadSong();
        _currentSafe = safe;
        _playing = true;

        StartCoroutine(SpawnNote(0));
    }

    IEnumerator SpawnNote(int index)
    {
        if ((int)_notes[index].Key.Value > 0) // if note is not a rest
        {
            NoteMovement note = Instantiate(_notePrefab, _spawnPoints[_notes[index].Key.Index].position, _notePrefab.transform.rotation, transform).GetComponent<NoteMovement>();
            note.Setup(_speed, _targetPoints[_notes[index].Key.Index].position, _notes[index].Key.Index, this);

            _amountOfNotes++;
        }

        Debug.Log($"Note duration: {_notes[index].Value}");
        
        yield return new WaitForSecondsRealtime(_notes[index].Value);

        index++;
        if (index < _notes.Count)
            StartCoroutine(SpawnNote(index)); 
    }

    private void DestroyNotes() // used in on disable to remove all notes for reset
    {
        foreach(var note in _currentNotes)
        {
            Destroy(note.gameObject);
        }
        _currentNotes.Clear(); 
    }
    public void MissedNote(NoteMovement note)
    {
        Debug.Log(note.ToString());
        _currentNotes.Remove(note);
        _combo = 0;
        _health.LoseLife();
        _comboText.text = $"Combo: x{_combo}";
        Destroy(note.gameObject);

        if (_health.CurrentHealth == 0)
        {
            EndMinigame(true);
        }
    }

    private void ResetMinigame()
    {
        _health.ResetHealth();
        _combo = 0;
        _highestCombo = 0;
        _amountOfNotes = 0;
        _currentSafe = null;
        _playing = false;
    }

    private void EndMinigame(bool failed)
    {
        if (_currentSafe is not TutorialSafe)
        {
            // Set alert level of octavius based on lives remaining
            switch (_health.CurrentHealth)
            {
                case 0: // Lost all lives
                    _octaviusBehaviour.SetAlertLevel(AlertLevel.Level3); // set to maximum alertness
                    EnemyAlert.NewAlert.Invoke();
                    break;
                case 1: // 1 life remaining
                    _octaviusBehaviour.SetAlertLevel(AlertLevel.Level2);
                    EnemyAlert.NewAlert.Invoke();
                    break;
                case 2:
                    _octaviusBehaviour.SetAlertLevel(AlertLevel.Level1);
                    EnemyAlert.NewAlert.Invoke();
                    break;
            }
        }

        if (!failed)
            // If didnt fail the minigame, unlock the safe
        {
            _currentSafe.Unlock();
        }

        // Calculate Stamina
        float staminaPercentage = (float)_highestCombo / (float)_amountOfNotes; // stamina in range 0 to 1. For slider
        _playerMovement.SetStamina(staminaPercentage);

        gameObject.SetActive(false);
    }

    public void AddNote(NoteMovement note)
    {
        _currentNotes.Add(note);
    }
}
