using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Song : ScriptableObject
{
    [Header("Time Signature")]
    public int Numerator = 4;
    public int Denominator = 4;

    [Header("Tempo")]
    public int BPM = 120;

    [Header("Notes")]
    public List<Note> Notes = new List<Note>();

    public List<KeyValuePair<Note, float>> LoadSong()
    {
        float secondsPerBeat = 60f / (float)BPM;

        NoteValue notePerBeat;

        switch (Denominator)
        {
            case 1:
                notePerBeat = NoteValue.WholeNote; break;
            case 2:
                notePerBeat = NoteValue.HalfNote; break;
            case 4:
                notePerBeat = NoteValue.QuarterNote; break;
            case 8:
                notePerBeat = NoteValue.EighthNote; break;
            case 16:
                notePerBeat = NoteValue.SixteenthNote; break;
            default:
                notePerBeat = NoteValue.QuarterNote;
                Denominator = 4;
                Debug.LogWarning("Invalid or empty time signature denominator in song. Setting to 4");
                break;
        }

        List<KeyValuePair<Note, float>> noteSeconds = new List<KeyValuePair<Note, float>>();

        float currentAmountOfBeatsInBar = 0f;
        foreach (Note note in Notes)
        {
            float noteSize = ((float)notePerBeat / Mathf.Abs((float)note.Value)); // noteSize is the size relative to a beat. if beat is quarter note the eighth note would be 0.5
            float duration = secondsPerBeat * noteSize;

            if (currentAmountOfBeatsInBar + noteSize > (float)Numerator)
            {
                Debug.LogWarning("Invalid beats per bar... Consider fixing this.");
                // TODO : add rests if needed - otherwise just make sure you've set the song right!!!
            }

            noteSeconds.Add(new KeyValuePair<Note, float>(note, duration));
            currentAmountOfBeatsInBar += noteSize;

            if (currentAmountOfBeatsInBar == (float)Numerator)
                currentAmountOfBeatsInBar = 0f;
        }

        foreach (var kv in  noteSeconds)
        {
            Debug.Log($"Key: {kv.Key.Value} Value: {kv.Value}");
        }

        return noteSeconds;
    }
}

[Serializable]
public class Note
{
    public NoteValue Value;
    [Tooltip("Index on minigame track. 0 = Top, 3 = Bottom")]
    public int Index;
}

public enum NoteValue
{
    WholeNote = 1,
    HalfNote = 2,
    QuarterNote = 4,
    EighthNote = 8,
    SixteenthNote = 16,
    WholeRest = -1,
    HalfRest = -2,
    QuarterRest = -4,
    EighthRest = -8,
    SixteenthRest = -16,
}
