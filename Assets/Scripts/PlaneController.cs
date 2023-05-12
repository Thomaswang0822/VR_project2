using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{

    // Public
    public GameObject airRacingContainer;
    public GameObject leftGDContainer;
    public GameObject rightGDContainer;
    public GameObject camRig;
    public bool debugMode = false;

    // Private
    private AirRacing ar;
    private Rigidbody rb;
    private GestureDetector leftGD;
    private GestureDetector rightGD;
    private CamController camController;
    private string prevGesture = "bruh";

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
        leftGD = leftGDContainer.GetComponent<GestureDetector>();
        rightGD = rightGDContainer.GetComponent<GestureDetector>();

        camController = camRig.GetComponent<CamController>();

    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode) {
            HandleDebugInput();
        }
        
        HandleInput();
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
    /// 
    /// Design decision:
    /// 1. Right hand is used as throttle: thumb_up and fist are 
    ///     acc and dec
    /// 2. Left hand is used as orientation controller. The base state
    ///     is the vertical hand （palm inward).
    ///     - barrel roll is turning L/R
    ///     - finger tip up/down is pitching up/down
    /// </summary>
    void HandleInput() {
        throttle = 0.0f;
        roll = 0.0f;
        pitch = 0.0f;
        yaw = 0.0f;

        if (!ar.AcceptInput()) return;

        Gesture leftGesture = leftGD.Recognize();
        Gesture rightGesture = rightGD.Recognize();
        bool detected = !leftGesture.Equals(new Gesture()) || !rightGesture.Equals(new Gesture());
        if (!detected) { return; }


        switch (leftGesture.name)
        {
            // 2. up & down
            case "tipUp_L":
                pitch = 0.1f;
                break;
            case "tipDown_L":
                pitch = -0.1f;
                break;
            // 3. turn left & right
            case "palmUp_L":
                yaw = -0.1f;
                roll = -0.1f;
                break;
            case "palmDown_L":
                yaw = 0.1f;
                roll = 0.1f;
                break;
            default:
                Debug.Log("Gesture added but not handled properly: " + leftGesture.name);
                break;
        }

        switch (rightGesture.name)
        {
            // 1. acceleration & deceleration
            case "thumb_R":
                throttle = 1.0f;
                break;
            case "fist_R":
                // throttle = -0.5f;
                if (prevGesture != "fist_R") {
                    camController.CycleView();
                }
                break;
            // case "SOME NAME":
            //     camController.CycleView();
            //     break;
            default:
                Debug.Log("Gesture added but not handled properly: " + rightGesture.name);
                break;
        }

        prevGesture = System.String.Copy(rightGesture.name);
        // Debug.Log("pitch " + pitch);    
    }


    void FixedUpdate()
    {
        rb.AddForce(transform.forward * maxThrust * throttle);
        rb.AddTorque(transform.up * yaw * 10f);
        rb.AddTorque(transform.right * pitch * 10f);
        rb.AddTorque(-transform.forward * roll * 10f);
    }
}
