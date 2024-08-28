using UnityEngine;

public class SpotlightRandomMovement : MonoBehaviour
{
    public float minAngle = 35f; // Minimum angle in degrees
    public float maxAngle = 65f; // Maximum angle in degrees
    public float speed = 2f; // Speed of the spotlight movement

    private float targetAngle; // The target angle to move towards

    void Start()
    {
        SetRandomTargetAngle(); // Set an initial random target angle
    }

    void Update()
    {
        // Smoothly rotate the spotlight towards the target angle
        float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * speed);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, angle, transform.eulerAngles.z);

        // Check if the spotlight is close to the target angle, then set a new random target angle
        if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) < 0.1f)
        {
            SetRandomTargetAngle();
        }
    }

    // Set a new random target angle between minAngle and maxAngle
    void SetRandomTargetAngle()
    {
        targetAngle = Random.Range(minAngle, maxAngle);
    }
}
