using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToonBoom.XMLWorkflowGame
{
    public class PerryUI : MonoBehaviour
    {
        public GameObject Character;
        public Text GrabText;
        public Slider Palette;
        public Slider Eyes;
        public Slider Glasses;
        public Slider Headband;

        // Update is called once per frame
        void Update()
        {
            //Only show the Grab text when the character can grab the cube.
            PerryScript Perry = Character.GetComponent<PerryScript>();
            if (Perry.canHold)
            {
                GrabText.enabled = true;
            }
            else
            {
                GrabText.enabled = false;
            }

        }
    }
}