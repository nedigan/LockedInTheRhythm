using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public abstract class Objective : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _objectiveText;
    [SerializeField] protected TextMeshProUGUI _description;
}
public class SafeObjective : Objective
{
    [SerializeField] private List<MusicalSafe> _musicalSafes = new List<MusicalSafe>();


    // Update is called once per frame
    void Update()
    {
        int remainingSafes = _musicalSafes.Count(safe => safe.Locked);
        _description.text = $"Remaining: {remainingSafes}";
    }
}
