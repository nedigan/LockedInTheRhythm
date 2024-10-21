using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToonBoom.XMLWorkflowGame
{
public class PerryEvents : MonoBehaviour
{
    public GameObject Parent;
    //Holding is called by an Animation Event when when the object is supposed to be grabbed inside the animation.
    public void Holding()
    {
      PerryScript Script = Parent.GetComponent<PerryScript>();
      PerryUI UI = Script.UI.GetComponent<PerryUI>();
      Script.Holding = true;
      Script.Cube.GetComponent<Rigidbody2D>().isKinematic = true;
      Script.Cube.transform.SetParent(transform.GetChild(0));
      UI.GrabText.text = "Space : Drop";
    }

    //Dropping is called by an Animation Event when the object is supposed to leave your anchor inside the animation.
    public void Dropping()
    {
      PerryScript Script = Parent.GetComponent<PerryScript>();
      PerryUI UI = Script.UI.GetComponent<PerryUI>();
      Script.Holding = false;
      Script.Cube.transform.parent = null;
      Script.Cube.GetComponent<Rigidbody2D>().isKinematic = false;
      UI.GrabText.text = "Space : Grab";
    }

    //backToPlay is called by an Animation Event when an animation is done and you want to get control over the character again.
    public void backToPlay()
    {
      PerryScript Script = Parent.GetComponent<PerryScript>();
      Script.noControl = false;
    }
}
}