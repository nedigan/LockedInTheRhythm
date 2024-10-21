#if ENABLE_UNITY_2D_ANIMATION && ENABLE_UNITY_COLLECTIONS

using System.Collections.Generic;
using System.Linq;
using ToonBoom.TBGRenderer;
using UnityEngine;
using UnityEngine.U2D.IK;
using UnityEngine.UI;

namespace ToonBoom.TBGWorkflowGame
{
    [DefaultExecutionOrder(-2)] // Should run before SpriteSkin.
    [RequireComponent(typeof(IKManager2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(TBGRenderer.TBGRenderer))]
    public class SlyController : MonoBehaviour
    {
        [System.Serializable]
        public class MovementParams
        {
            public int tracesCount = 16;
            public float traceWidth = 0.25f;
            public float traceHeight = 4;
            public float desiredHeight = 4;
            public float legHeight = 0.5f;
            public float maxSpeed = 10;
            public float acceleration = 10;
            public float deceleration = 10;
            public AnimationCurve slopeToSpeedMultiplier = AnimationCurve.Linear(0, 1, 1, 1);
        }
        [System.Serializable]
        public class AnimationParams
        {
            [System.Serializable]
            public class Aim
            {
                public Transform aimingPivot;
                public AnimationCurve aimAngleToPivotAngle = AnimationCurve.Linear(0, 0, 1, 1);
                public AnimationCurve throwTimeToAimWeight = AnimationCurve.Linear(0, 0, 1, 1);
            }

            public string throwState = "Sly_Fox-P_sly_throw";
            public int throwLayer = 1;
            public string kickState = "Sly_Fox-P_sly_kick";
            public int kickLayer = 0;
            public Aim aim;
        }

        [System.Serializable]
        public class DebugControls
        {
            public KeyCode kick;
            public KeyCode aim;
        }

        [System.Serializable]
        public class EffectsParams
        {
            [System.Serializable]
            public class Leg
            {
                public Transform ankleTarget;
                public Transform ankleReference;
                public Transform foot;
                public float footRotationOffset = 180f;
            }
            [System.Serializable]
            public class Apron
            {
                public Transform frontKnee;
                public Transform backKnee;
                public Transform apronPivot;
                public Transform apronEnd;
            }

            public bool EnableGroundEffects = true;
            public bool EnableApronEffects = true;
            public Leg frontLeg;
            public Leg backLeg;
            public Apron apron;
            public float traceStartOffset = 3f;
            public float maxGroundNormalBend = .25f;
            public Slider PaletteSlider;
            public Slider ShirtSlider;
            public Slider PantsSlider;
            public Slider ApronSlider;
        }
        [System.Serializable]
        public class Interaction
        {
            public Transform pickupPoint;
            public Transform kickPoint;
            public Vector2 kickForce;
            public Vector2 throwForce;
            public Transform pickupHoldingPoint;
            public Text uiText;
            public string throwPrompt = "Throw (Click)";
            public string pickupPrompt = "Pickup (Click)";
            public string kickPrompt = "Kick (Click)";
        }

        [System.Serializable]
        public class CurrentState
        {
            [System.Serializable]
            public class Foot
            {
                public Vector2? collisionNormal;
            }

            public float velocity = 0;
            public int facing = 1;
            public Vector2 aimDirection = Vector2.right;
            public Foot frontFoot;
            public Foot backFoot;
            public enum Action
            {
                None,
                Aiming,
                Throwing,
                Kicking,
            }
            public Action action = Action.None;
            public Rigidbody2D target;
            public SpriteRenderer highlight;
        }

        public MovementParams movementParams;
        public AnimationParams animationParams;
        public DebugControls debugControls;
        public EffectsParams effectsParams;
        public Interaction interaction;
        public CurrentState currentState;
        private IKManager2D ikManager;
        private Animator animator;
        private TBGRenderer.TBGRenderer tbgRenderer;
        private Vector3 startingScale;
        void Awake()
        {
            // Find required sibling components.
            ikManager = GetComponent<IKManager2D>();
            animator = GetComponent<Animator>();
            tbgRenderer = GetComponent<TBGRenderer.TBGRenderer>();

            startingScale = transform.localScale;

            // Disable the IKManager2D, we will manually call IKManager2D.UpdatgeManager() in LateUpdate().
            ikManager.enabled = false;
        }

        // Caching lists to avoid Garbage Collections.
        private List<RaycastHit2D> foundCollisions = new List<RaycastHit2D>();
        private List<float> smoothedGroundHeights = new List<float>();

        public float MoveTowards(float current, float target)
        {
            if (current < target)
            {
                return Mathf.Min(current + movementParams.acceleration * Time.deltaTime, target);
            }
            else
            {
                return Mathf.Max(current - movementParams.deceleration * Time.deltaTime, target);
            }
        }

        // Event handler for kicking and throwing events created manually for these animations.
        public void OnSlyKick()
        {
            if (currentState.target == null)
                return;
            currentState.target.velocity = new Vector2(currentState.facing, 1) * interaction.kickForce;
            currentState.target = null;
        }

        public void OnSlyThrow()
        {
            if (currentState.target == null)
                return;
            currentState.target.bodyType = RigidbodyType2D.Dynamic;
            // Throw item in aiming direction
            currentState.target.AddForce((currentState.aimDirection + Vector2.up * interaction.throwForce.y).normalized * interaction.throwForce.x, ForceMode2D.Impulse);
            currentState.target = null;
        }

        public void UpdateInteractionPrompt(SpriteRenderer highlight, CurrentState.Action actionToPerform)
        {
            // Show interaction text depending on the action.
            switch (actionToPerform)
            {
                case CurrentState.Action.None:
                    interaction.uiText.text = "";
                    break;
                case CurrentState.Action.Aiming:
                    interaction.uiText.text = interaction.pickupPrompt;
                    break;
                case CurrentState.Action.Throwing:
                    interaction.uiText.text = interaction.throwPrompt;
                    break;
                case CurrentState.Action.Kicking:
                    interaction.uiText.text = interaction.kickPrompt;
                    break;
            }

            // Find screenspace position of highlight or character, to place the interaction text.
            Vector3 screenPos = Camera.main.WorldToScreenPoint(highlight != null ? highlight.transform.position : transform.position);
            interaction.uiText.transform.position = screenPos;
            interaction.uiText.rectTransform.anchoredPosition = new Vector2(screenPos.x, screenPos.y);

            // Update the highlight
            if (currentState.highlight != null)
                currentState.highlight.gameObject.SetActive(false);
            currentState.highlight = highlight;
            if (currentState.highlight != null)
                currentState.highlight.gameObject.SetActive(true);
        }

        // Execute before SpriteSkins run (bone deformation).
        void LateUpdate()
        {
            // Palette / Skin sliders.
            {
                tbgRenderer.SetPalette(effectsParams.PaletteSlider.value > 0.5f ? "SLY-Day" : "SLY-Night");
                tbgRenderer.SetSkinForGroup(0, (ushort)(effectsParams.PantsSlider.value + 1));
                tbgRenderer.SetSkinForGroup(1, (ushort)(effectsParams.ShirtSlider.value + 1));
                tbgRenderer.SetSkinForGroup(2, (ushort)(effectsParams.ApronSlider.value + 1));
            }

            UpdateInteractionPrompt(null, CurrentState.Action.None);

            // Aiming behaviour.
            {
                // Aim while holding an item.
                if (currentState.action == CurrentState.Action.Aiming)
                {
                    // Restart throwing animation, once key is released the animation will be able to follow through to the end.
                    animator.Play(animationParams.throwState, animationParams.throwLayer, 0);

                    // Throw on click.
                    UpdateInteractionPrompt(null, CurrentState.Action.Throwing);
                    if (Input.GetMouseButtonDown(0))
                    {
                        currentState.action = CurrentState.Action.Throwing;
                    }
                }
                // Move aiming pivot while aiming, time it to the cursor angle.
                if (currentState.action == CurrentState.Action.Aiming || currentState.action == CurrentState.Action.Throwing)
                {

                    // Determine cursor angle from player.
                    var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    // Set aim direction state.
                    currentState.aimDirection = new Vector2(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y);

                    // Set aim pivot based on cursor angle and animation curve.
                    {
                        var cursorAngle = Mathf.Atan2(currentState.aimDirection.y, Mathf.Abs(currentState.aimDirection.x)) * Mathf.Rad2Deg;
                        var pivotAngle = animationParams.aim.aimAngleToPivotAngle.Evaluate(cursorAngle / 90f) * 90f;
                        // Aiming is blended based on whether we are actively aiming, or if depending on the time of the throwing animation.
                        var aimWeight = currentState.action == CurrentState.Action.Aiming
                            ? 1 // While aiming, aim weight is always 1.
                            : animationParams.aim.throwTimeToAimWeight.Evaluate(animator.GetCurrentAnimatorStateInfo(animationParams.throwLayer).normalizedTime);
                        animationParams.aim.aimingPivot.localRotation = Quaternion.Euler(0, 0, pivotAngle * aimWeight) * animationParams.aim.aimingPivot.localRotation;
                        currentState.facing = (int)Mathf.Sign(mousePosition.x - transform.position.x);
                    }

                    // Force held item to hand.
                    if (currentState.target != null)
                    {
                        currentState.target.transform.rotation = interaction.pickupHoldingPoint.rotation;
                        currentState.target.transform.position = interaction.pickupHoldingPoint.position;
                    }

                    // Trace the cursor angle.
                    Debug.DrawLine(transform.position, mousePosition, Color.red);
                }
            }

            // Interacting behaviour.
            {

                // While idle, character can collect an item or kick.
                if (currentState.action == CurrentState.Action.None)
                {
                    // Trace from character origin to pickup pivot.
                    var pickupTrace = Physics2D.Raycast(transform.position, interaction.pickupPoint.position - transform.position, 100);
                    if (pickupTrace.collider != null)
                    {
                        // For collectables, first 'collect' them and handle returned interactable object.
                        var collectable = pickupTrace.collider.GetComponentInParent<SlyCollectable>();
                        if (collectable != null && collectable.Pickupable)
                        {
                            UpdateInteractionPrompt(collectable.highlightRenderer, CurrentState.Action.Aiming);
                            if (Input.GetMouseButtonDown(0))
                            {
                                var rigidbody = collectable.Pickup();
                                if (rigidbody != null)
                                {
                                    // Attach rigidbody from trace to pickup pivot on character.
                                    rigidbody.bodyType = RigidbodyType2D.Static;
                                    currentState.target = rigidbody;
                                    currentState.action = CurrentState.Action.Aiming;
                                }
                            }
                        }
                    }

                    // Trace from character origin to kick pivot, if we find a kickable object, kick it.
                    if (currentState.action != CurrentState.Action.Kicking)
                    {
                        var kickTrace = Physics2D.Raycast(transform.position, interaction.kickPoint.position - transform.position, 100);
                        if (kickTrace.collider != null)
                        {
                            var collectable = kickTrace.collider.GetComponentInParent<SlyCollectable>();
                            if (collectable != null && collectable.Kickable)
                            {
                                UpdateInteractionPrompt(collectable.highlightRenderer, CurrentState.Action.Kicking);
                                if (Input.GetMouseButtonDown(0))
                                {
                                    currentState.target = kickTrace.collider.GetComponentInParent<Rigidbody2D>();
                                    currentState.action = CurrentState.Action.Kicking;
                                    animator.Play(animationParams.kickState, animationParams.kickLayer, 0);
                                }
                            }
                        }
                    }
                }
                // Stop aiming if we are no longer throwing (trigger "Empty" state).
                if (currentState.action == CurrentState.Action.Throwing
                    && !animator.GetCurrentAnimatorStateInfo(animationParams.throwLayer).IsName(animationParams.throwState))
                {
                    currentState.action = CurrentState.Action.None;
                }
                // Stop kicking action if we are done the kicking animation (trigger "Empty" state).
                if (currentState.action == CurrentState.Action.Kicking
                    && !animator.GetCurrentAnimatorStateInfo(animationParams.kickLayer).IsName(animationParams.kickState))
                {
                    currentState.action = CurrentState.Action.None;
                }
            }

            // Trace down from the top of the player on different X positions.
            // For all collisions found, use the lowest collision as the ground collision.
            // This will accentuate IK when the player is on a slope.
            foundCollisions.Clear();
            for (int i = 0; i < movementParams.tracesCount; i++)
            {
                float x = Mathf.Lerp(-movementParams.traceWidth, movementParams.traceWidth, (float)i / (movementParams.tracesCount - 1));
                Vector3 origin = transform.position + transform.right * x;
                var hit = FirstNonCollectableHit(Physics2D.RaycastAll(origin, -transform.up, movementParams.traceHeight));
                if (hit.collider != null)
                {
                    foundCollisions.Add(hit);
                }
            }

            // Between found collisions, determine average collision height.
            // We weight the collisions based on x proximity to the player origin.
            var averageCollisionHeight = 0f;
            {
                var totalWeight = 0f;
                for (int i = 0; i < foundCollisions.Count; i++)
                {
                    var hit = foundCollisions[i];
                    var weight = GetGroundSmoothingWeight(hit);
                    totalWeight += weight;
                    averageCollisionHeight += hit.point.y * weight;
                }
                averageCollisionHeight /= totalWeight;
            }

            // Calculate percieved collision heights by taking existing found collisions,
            // and blending them with the average collision height, based on x proximity to the player origin.
            smoothedGroundHeights.Clear();
            for (int i = 0; i < foundCollisions.Count; i++)
            {
                var hit = foundCollisions[i];
                var weight = GetGroundSmoothingWeight(hit);
                smoothedGroundHeights.Add(Mathf.Lerp(averageCollisionHeight, hit.point.y, weight));
            }

            // Get the lowest point of all found collisions.
            if (smoothedGroundHeights.Count > 0)
            {
                var lowestHeight = float.MaxValue;
                var heighestHeight = float.MinValue;
                for (int i = 0; i < smoothedGroundHeights.Count; i++)
                {
                    Debug.DrawLine(foundCollisions[i].point, new Vector2(foundCollisions[i].point.x, smoothedGroundHeights[i]), Color.red);
                    if (smoothedGroundHeights[i] < lowestHeight)
                    {
                        lowestHeight = smoothedGroundHeights[i];
                    }
                    if (smoothedGroundHeights[i] > heighestHeight)
                    {
                        heighestHeight = smoothedGroundHeights[i];
                    }
                }
                // Debug lowest / highest floors as horizontal line.
                Debug.DrawLine(new Vector2(transform.position.x - movementParams.traceWidth, lowestHeight), new Vector2(transform.position.x + movementParams.traceWidth, lowestHeight), Color.green);
                Debug.DrawLine(new Vector2(transform.position.x - movementParams.traceWidth, heighestHeight), new Vector2(transform.position.x + movementParams.traceWidth, heighestHeight), Color.blue);

                // Reposition the player's y position relative to the lowest point.
                transform.position = new Vector3(transform.position.x, Mathf.Max(lowestHeight, heighestHeight - movementParams.legHeight) + movementParams.desiredHeight, transform.position.z);
            }

            // Take horizontal input, set target velocity, use deceleration when absolute target speed is lower than current absolute speed, otherwise use acceleration.
            var targetVelocity = currentState.action == CurrentState.Action.Kicking || currentState.action == CurrentState.Action.Throwing
                ? 0 // Halt movement when kicking or throwing.
                : Input.GetAxis("Horizontal") * movementParams.maxSpeed;
            if (Mathf.Abs(targetVelocity) < Mathf.Abs(currentState.velocity))
            {
                currentState.velocity = Mathf.MoveTowards(currentState.velocity, targetVelocity, movementParams.deceleration * Time.deltaTime);
            }
            else
            {
                currentState.velocity = Mathf.MoveTowards(currentState.velocity, targetVelocity, movementParams.acceleration * Time.deltaTime);
            }

            // Get average slope from existing found collisions.
            var averageSlope = 0f;
            {
                var totalWeight = 0f;
                for (int i = 0; i < foundCollisions.Count; i++)
                {
                    var hit = foundCollisions[i];
                    var weight = GetGroundSmoothingWeight(hit);
                    totalWeight += weight;
                    averageSlope += hit.normal.x * weight;
                }
                averageSlope = averageSlope / totalWeight;
            }
            var slopeMultiplier = movementParams.slopeToSpeedMultiplier.Evaluate(averageSlope * -currentState.facing);

            // Move the player using velocity and slope multiplier.
            transform.position += transform.right * currentState.velocity * slopeMultiplier * Time.deltaTime;

            // Set player facing direction.
            if (currentState.action == CurrentState.Action.None
                && Mathf.Abs(currentState.velocity) > 0)
            {
                currentState.facing = (int)Mathf.Sign(currentState.velocity);
            }
            transform.localScale = Vector3.Scale(startingScale, new Vector3(currentState.facing, 1, 1));

            // Pass stats on to animator
            var targetSpeed = Mathf.Abs(targetVelocity);
            animator.SetFloat("Speed", targetSpeed);

            // Trace from feet to ground per-feetReference, and set the feetTarget's position to the hit point.
            if (effectsParams.EnableGroundEffects)
            {
                AnimateLeg(effectsParams.frontLeg, currentState.frontFoot);
                AnimateLeg(effectsParams.backLeg, currentState.backFoot);
            }

            // Manually update IKManager2D.
            ikManager.UpdateManager();

            // After IKManager2D has updated the targets' position, finalize the feetTarget's rotation
            if (effectsParams.EnableGroundEffects)
            {
                AnimateFoot(effectsParams.frontLeg, currentState.frontFoot);
                AnimateFoot(effectsParams.backLeg, currentState.backFoot);
            }

            // Ensure that apron never goes below knees.
            if (effectsParams.EnableApronEffects)
            {
                var scale = new Vector2(currentState.facing, 1);
                var pivotPosition = effectsParams.apron.apronPivot.position;
                var leftKneeReference = Vector2.Scale(scale, effectsParams.apron.frontKnee.position - pivotPosition);
                var rightKneeReference = Vector2.Scale(scale, effectsParams.apron.backKnee.position - pivotPosition);
                var apronReference = Vector2.Scale(scale, effectsParams.apron.apronEnd.position - pivotPosition);
                var apronAngle = Mathf.Atan2(apronReference.y, apronReference.x) * Mathf.Rad2Deg;
                var highestKneeAngle = Mathf.Max(Mathf.Atan2(leftKneeReference.y, leftKneeReference.x), Mathf.Atan2(rightKneeReference.y, rightKneeReference.x)) * Mathf.Rad2Deg;
                if (highestKneeAngle > apronAngle)
                {
                    var angleDelta = highestKneeAngle - apronAngle;
                    effectsParams.apron.apronPivot.RotateAround(pivotPosition, Vector3.forward, angleDelta * currentState.facing);
                }
            }
        }

        private float GetGroundSmoothingWeight(RaycastHit2D hit)
        {
            return Mathf.Pow(1 - Mathf.Abs(hit.point.x - transform.position.x) / movementParams.traceWidth, 2);
        }

        private void AnimateLeg(EffectsParams.Leg leg, CurrentState.Foot foot)
        {
            var hit = FirstNonCollectableHit(Physics2D.RaycastAll(
                 origin: leg.foot.position + transform.up * effectsParams.traceStartOffset,
                 direction: -transform.up,
                 distance: effectsParams.traceStartOffset));
            // Filter out hits that are collectable (Linq is Garbage-y).
            if (hit.collider != null)
            {
                // Move foot ahead of target calculation to get best contact between toes and the ground.
                AnimateFoot(leg, foot);
                // Retarget ankle to the hit point.
                leg.ankleTarget.position = leg.ankleTarget.position
                    .SetXY(hit.point + Vector2.one * (leg.ankleReference.position - leg.foot.position));

                // Debug this collision with a normal.
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.yellow);
                // Set the feet rotation to the normal average ground rotation.
                foot.collisionNormal = hit.normal;
            }
            else
            {
                // If no collision is found, set the feet rotation to the normal average ground rotation.
                foot.collisionNormal = null;
                leg.ankleTarget.position = leg.ankleReference.position;
            }
        }

        private void AnimateFoot(EffectsParams.Leg leg, CurrentState.Foot foot)
        {
            // Limit normal by maxFootRotation.
            if (foot.collisionNormal == null)
                return;
            var normal = (Vector2)foot.collisionNormal;
            normal.x = Mathf.Clamp(normal.x, -effectsParams.maxGroundNormalBend, effectsParams.maxGroundNormalBend);

            // Set the feet rotation to the collision normal.
            leg.foot.rotation = Quaternion.AngleAxis(leg.footRotationOffset, Vector3.forward)
                    * Quaternion.FromToRotation(Vector3.up, normal);
        }

        private RaycastHit2D FirstNonCollectableHit(RaycastHit2D[] hits)
        {
            for (int i = 0; i < hits.Length; i++)
                if (hits[i].collider.GetComponentInParent<SlyCollectable>() == null)
                    return hits[i];
            return default;
        }
    }
    public static class SlyVector3Ext
    {
        public static Vector3 SetXY(this Vector3 vector, Vector2 xy)
        {
            vector.x = xy.x;
            vector.y = xy.y;
            return vector;
        }
    }
}

#endif