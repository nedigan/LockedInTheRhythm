using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongLoader : MonoBehaviour
{
    [SerializeField] private Song _song;
    private List<KeyValuePair<Note, float>> _notes;
    private Image _image; // testing

    // Start is called before the first frame update
    void Start()
    {
        _notes = _song.LoadSong();
        _image = GetComponent<Image>(); // testing

        StartCoroutine(PlayNote(0));
    }

    IEnumerator PlayNote(int index)
    {
        _image.enabled = !_image.enabled;

        yield return new WaitForSeconds(_notes[index].Value);

        StartCoroutine(PlayNote((index + 1) % _notes.Count)); // next note and wraps around
    }
}
