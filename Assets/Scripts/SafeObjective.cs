using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Objective : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _objectiveText;
    [SerializeField] protected TextMeshProUGUI _description;
}
public class SafeObjective : Objective
{
    [SerializeField] private List<MusicalSafe> _musicalSafes = new List<MusicalSafe>();
    [SerializeField] private List<Image> _cassetteImages = new List<Image>();   


    // Update is called once per frame
    void Update()
    {
        int remainingSafes = _musicalSafes.Count(safe => !safe.Locked);
        for (int i = 0; i < remainingSafes; i++)
        {
            _cassetteImages[i].color = new Color(1, 1, 1, 1f);
        }
        _description.text = $"Remaining: {remainingSafes}";
    }
}
