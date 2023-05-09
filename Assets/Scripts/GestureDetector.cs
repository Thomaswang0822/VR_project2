using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public struct Gesture 
{
    public string name;
    public List<Vector3> fingerData;
    public UnityEvent onRecognized;
}

public class GestureDetector : MonoBehaviour
{
    // Public
    public OVRSkeleton skeleton;
    public List<Gesture> gestures;
    private List<OVRBone> fingerBones;

    // Private:
    private bool debugMode = true;
    // Start is called before the first frame update
    void Start()
    {    
        fingerBones = new List<OVRBone>(skeleton.Bones);
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            SaveGesture();
        }
    }

    /// <summary>
    /// Save a hand gesture in the List<Gesture> gestures;
    /// </summary>
    void SaveGesture() {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();
        foreach (var bone in fingerBones) {
            // use finger tip's local position relative to the finger root
            data.Add(skeleton.transform.InverseTransformPoint(
                bone.Transform.position
            ));
        }
        g.fingerData = data;
        gestures.Add(g);
    }


}
