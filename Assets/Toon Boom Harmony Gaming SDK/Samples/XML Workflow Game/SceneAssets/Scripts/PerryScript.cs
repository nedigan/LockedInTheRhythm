using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToonBoom.Harmony;

namespace ToonBoom.XMLWorkflowGame
{
    public class PerryScript : MonoBehaviour
    {
        //Event variables
        private bool isWalking;
        public bool canHold;
        public bool Holding;
        public bool noControl;

        //Stats variables
        private float speed = 4.0f;
        private float scale = 0.005f;
        private float Levelbounds = 5.4f;
        private int Palette;
        private int Eyes;
        private int Glasses;
        private int Headband;

        //Grabbable object
        public GameObject Cube;
        public GameObject UI;

        //Harmony Assets controllers
        public Animator PerryAnim;
        public HarmonyRenderer Renderer;

        // Start is called before the first frame update
        void Start()
        {
            //Remove collision between character and cube
            Physics2D.IgnoreCollision(Cube.GetComponent<BoxCollider2D>(), GetComponent<CapsuleCollider2D>());

            //Add a listener to the Palette Slider so we know when the slider value changes & get base values for the slider
            Palette = Mathf.RoundToInt(UI.GetComponent<PerryUI>().Palette.value);
            UI.GetComponent<PerryUI>().Palette.onValueChanged.AddListener(delegate { PaletteCheck(); });

            //Add a listener to each Skin Slider so we know when the sliders value changes & get base values for the sliders
            Eyes = Mathf.RoundToInt(UI.GetComponent<PerryUI>().Eyes.value);
            UI.GetComponent<PerryUI>().Eyes.onValueChanged.AddListener(delegate { EyesCheck(); });
            Glasses = Mathf.RoundToInt(UI.GetComponent<PerryUI>().Glasses.value);
            UI.GetComponent<PerryUI>().Glasses.onValueChanged.AddListener(delegate { GlassesCheck(); });
            Headband = Mathf.RoundToInt(UI.GetComponent<PerryUI>().Headband.value);
            UI.GetComponent<PerryUI>().Headband.onValueChanged.AddListener(delegate { HeadbandCheck(); });

            //Update the renderer with base values
            UpdateRenderer(true, true, true, true);
        }

        //Change and update the Renderer (Palette and Skins)
        public void UpdateRenderer(bool paletteChanged, bool eyesChanged, bool glassesChanged, bool headbandChanged)
        {
            if (paletteChanged)
            {
                switch (Palette)
                {
                    case 0:
                        Renderer.FindSpriteSheet("HD", "Default");
                        break;

                    case 1:
                        Renderer.FindSpriteSheet("HD", "Autumn");
                        break;
                }
            }

            //Setting the skins to the proper id
            if (eyesChanged)
            {
                switch (Eyes)
                {
                    case 0:
                        Renderer.GroupSkins.SetSkin(0, 3);
                        break;
                    case 1:
                        Renderer.GroupSkins.SetSkin(0, 4);
                        break;
                }
            }

            if (glassesChanged)
            {
                switch (Glasses)
                {
                    case 0:
                        Renderer.GroupSkins.SetSkin(1, 0);
                        break;
                    case 1:
                        Renderer.GroupSkins.SetSkin(1, 1);
                        break;
                    case 2:
                        Renderer.GroupSkins.SetSkin(1, 2);
                        break;
                }
            }

            if (headbandChanged)
            {
                switch (Headband)
                {
                    case 0:
                        Renderer.GroupSkins.SetSkin(2, 0);
                        break;
                    case 1:
                        Renderer.GroupSkins.SetSkin(2, 5);
                        break;
                }
            }

            //Updating the Renderer is important to see any change
            Renderer.UpdateRenderer();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            //This bypassForSkins is used to ensure we force the Skin update no matter what
            //It is important to reupdate the Renderer when the first pass was done with the bypass when you change Palette, or else the skins may not be properly updated
            // if(hasPaletteChanged)
            // {
            //   Renderer.UpdateRenderer();
            //   hasPaletteChanged = false;
            // }

            //Send raycasts so we know when we can grab the cube
            if (!noControl)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.GetChild(0).position, -Vector2.up);
                if (hit.collider != null && hit.collider.tag == "Cube")
                {
                    canHold = true;
                }
                else
                {
                    canHold = false;
                }
            }

            //Grab or Drop action if we can grab, or if we hold, the cube
            if (Input.GetKeyDown("space"))
            {
                if (!Holding && canHold)
                {
                    canHold = false;
                    noControl = true;
                    PerryAnim.Play("PerryPoireau_pickup_item");
                }

                else if (Holding)
                {
                    noControl = true;
                    PerryAnim.Play("PerryPoireau_drop_item");
                }
            }

            //Movement system
            if (!noControl)
            {
                if (Input.GetAxis("Horizontal") != 0)
                {
                    PerryAnim.SetBool("isWalking", true);

                    float translation = Input.GetAxis("Horizontal") * speed;
                    translation *= Time.deltaTime;

                    // Move translation along the object's z-axis
                    if (transform.position.x + translation > -Levelbounds && transform.position.x + translation < Levelbounds)
                    {
                        transform.Translate(translation, 0, 0);
                    }

                    if (Input.GetAxis("Horizontal") > 0)
                    {
                        transform.localScale = new Vector3(scale, scale, 1);
                    }
                    else
                    {
                        transform.localScale = new Vector3(-scale, scale, 1);
                    }
                }

                else
                {
                    PerryAnim.SetBool("isWalking", false);
                }
            }
        }

        //If the Palette Slider changes we update the renderer with the proper Palette
        public void PaletteCheck()
        {
            Palette = Mathf.RoundToInt(UI.GetComponent<PerryUI>().Palette.value);
            UpdateRenderer(true, false, false, false);
        }

        //If the Eyes Slider changes we update the renderer with the proper Eyes
        public void EyesCheck()
        {
            Eyes = Mathf.RoundToInt(UI.GetComponent<PerryUI>().Eyes.value);
            UpdateRenderer(false, true, false, false);
        }

        //If the Glasses Slider changes we update the renderer with the proper Glasses
        public void GlassesCheck()
        {
            Glasses = Mathf.RoundToInt(UI.GetComponent<PerryUI>().Glasses.value);
            UpdateRenderer(false, false, true, false);
        }

        //If the Headband Slider changes we update the renderer with the proper Headband
        public void HeadbandCheck()
        {
            Headband = Mathf.RoundToInt(UI.GetComponent<PerryUI>().Headband.value);
            UpdateRenderer(false, false, false, true);
        }
    }
}