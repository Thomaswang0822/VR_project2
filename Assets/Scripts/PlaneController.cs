using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{

    // Public
    public GameObject airRacingContainer;

    // Private
    private AirRacing ar;
    private Rigidbody rb;

    private float throttle;  // % of max engine thrust
    private float roll;      // tilt left to right
    private float pitch;     // tilt front to back
    private float yaw;       // turn left to right

    private float maxThrust = 200.0f;
    private bool debugMode = true;

    // Start is called before the first frame update
    void Start()
    {
        ar = airRacingContainer.GetComponent<AirRacing>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode) {
            HandleDebugInput();
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        // If the other has a "Sphere" tag...
        if (other.gameObject.CompareTag("Sphere")) {
            ar.OnTargetUpdate(other);
        } else {
            // Otherwise, signal a collision to the air racing controller
            ar.OnCollision();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
    }

    void HandleDebugInput() {
        throttle = 0.0f;
        roll = 0.0f;
        pitch = 0.0f;
        yaw = 0.0f;

        if (!ar.AcceptInput()) return;

        if (Input.GetKey("w")) {
            throttle = 1.0f;
        }

        if (Input.GetKey("e")) {
            roll = 1.0f;
        } else if (Input.GetKey("q")) {
            roll = -1.0f;
        }

        if (Input.GetKey("up")) {
            pitch = 1.0f;
        } else if (Input.GetKey("down")) {
            pitch = -1.0f;
        }

        if (Input.GetKey("left")) {
            yaw = -1.0f;
        } else if (Input.GetKey("right")) {
            yaw = 1.0f;
        }
    }

    /// <summary>
    /// Design Note: 
    /// In theory, aircraft has 3 rotational components, pitch, yaw, roll.
    /// But in reality, pitch solely controls "up and down"; 
    /// yaw + roll control "turning left and right" together. 
    /// 
    /// Thus, we handle 3 types of interations:
    /// 1. acceleration & deceleration
    /// 2. up & down
    /// 3. turn left & right
    /// </summary>
    void HandleInput() {
        throttle = 0.0f;
        roll = 0.0f;
        pitch = 0.0f;
        yaw = 0.0f;

        // 1. acceleration & deceleration

        // 2. up & down

        // 3. turn left & right
    }


    void FixedUpdate()
    {
        rb.AddForce(transform.forward * maxThrust * throttle);
        rb.AddTorque(transform.up * yaw * 10f);
        rb.AddTorque(transform.right * pitch * 10f);
        rb.AddTorque(-transform.forward * roll * 10f);
    }
}
