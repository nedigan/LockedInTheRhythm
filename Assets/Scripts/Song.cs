using System;
using System.Collections;
using System.Collections.Generic;
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
}

[Serializable]
public class Note
{
    public NoteValue Value;
}

public enum NoteValue
{
    WholeNote,
    HalfNote,
    QuarterNote,
    EighthNote,
    SixteenthNote,
    WholeRest,
    HalfRest,
    QuarterRest,
    EighthRest,
    SixteenthRest
}
