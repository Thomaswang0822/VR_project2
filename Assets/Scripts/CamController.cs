using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ViewMode {
    FirstPerson, // Pilot, no visuals
    Cockpit,     // Pilot, w/ cockpit vis
    ThirdPerson, // Third person
}

public class CamController : MonoBehaviour
{
    public GameObject planeModel;
    public Transform behindView;
    public Transform cockpitView;
    public float speed;

    private ViewMode viewMode = ViewMode.ThirdPerson;
    private Vector3 target;
    private Quaternion targetRotation = Quaternion.identity;


    // Start is called before the first frame update
    void Start()
    {
        transform.forward = behindView.forward;
    }

    void CycleView()
    {
        switch (viewMode) {
            case ViewMode.FirstPerson:
                viewMode = ViewMode.Cockpit;
                planeModel.SetActive(true);
                break;

            case ViewMode.Cockpit:
                viewMode = ViewMode.ThirdPerson;
                break;

            case ViewMode.ThirdPerson:
                viewMode = ViewMode.FirstPerson;
                planeModel.SetActive(false);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleDebugInput();

        switch (viewMode) {
            case ViewMode.FirstPerson:
            case ViewMode.Cockpit:
                transform.position = Vector3.MoveTowards(transform.position, cockpitView.position, Time.deltaTime * speed);
                transform.rotation = cockpitView.rotation;
                break;

            case ViewMode.ThirdPerson:
                transform.position = Vector3.MoveTowards(transform.position, behindView.position, Time.deltaTime * speed);
                transform.rotation = behindView.rotation;
                break;
        }
    }

    void HandleDebugInput() {
        if (Input.GetKeyDown("v")) {
            CycleView();
        }
    }

    void FixedUpdate() {

    }
}
