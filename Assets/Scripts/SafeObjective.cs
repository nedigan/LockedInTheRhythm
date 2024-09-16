using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SafeObjective : MonoBehaviour
{
    [SerializeField] private List<MusicalSafe> _musicalSafes = new List<MusicalSafe>();

    [SerializeField] private TextMeshProUGUI _objectiveText;
    [SerializeField] private TextMeshProUGUI _description;

    // Update is called once per frame
    void Update()
    {
        int remainingSafes = _musicalSafes.Count(safe => safe.Locked);
        _description.text = $"Remaining: {remainingSafes}";
    }
}
