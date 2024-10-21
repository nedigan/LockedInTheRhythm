using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToonBoom.TBGWorkflowGame
{
    public class SlyCollectable : MonoBehaviour
    {
        public bool Kickable = false;
        public bool Pickupable = false;
        public Rigidbody2D interactableReplacement;
        public SpriteRenderer highlightRenderer;
        void Start()
        {
            if (interactableReplacement != null)
                interactableReplacement.gameObject.SetActive(false);
            if (highlightRenderer != null)
                highlightRenderer.gameObject.SetActive(false);
        }

        public Rigidbody2D Pickup()
        {
            if (!Pickupable)
                return null;
            if (interactableReplacement != null)
            {
                interactableReplacement.gameObject.SetActive(true);
                gameObject.SetActive(false);
                return interactableReplacement;
            }
            return GetComponentInChildren<Rigidbody2D>();
        }
    }
}