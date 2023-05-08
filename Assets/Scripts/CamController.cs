using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public Transform behindView;
    public Transform cockpitView;
    public float speed;

    private bool cockpit = false;
    private Vector3 target;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        target = cockpit ? cockpitView.position : behindView.position;
    }

    void FixedUpdate() {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        transform.forward = cockpit ? cockpitView.forward : behindView.forward;
    }
}
