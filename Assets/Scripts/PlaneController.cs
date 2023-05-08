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

    // Start is called before the first frame update
    void Start()
    {
        ar = airRacingContainer.GetComponent<AirRacing>();
        rb = GetComponent<Rigidbody>();

        rb.detectCollisions = false;

        // For now, make the plane invisible.
        GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleDebugInput();
    }

    void HandleDebugInput() {
        throttle = 0.0f;
        roll = 0.0f;
        pitch = 0.0f;
        yaw = 0.0f;

        if (Input.GetKey("w")) {
            throttle = 1.0f;
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

    void FixedUpdate()
    {
        rb.AddForce(transform.forward * maxThrust * throttle);
        rb.AddTorque(transform.up * yaw * 10f);
        rb.AddTorque(transform.right * pitch * 10f);
        rb.AddTorque(-transform.forward * roll * 10f);
    }
}
