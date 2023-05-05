using System.Collections;
using System.Collections.Generic;
using System.IO;  // to parse
using UnityEngine;

public class AirRacing : MonoBehaviour
{
    // Public:
    public Camera camera;
    public GameObject campus;
    public List<Vector3> checkPts;
    // should have been private, but we make it public to better switch viewpoint in pause mode
    public List<GameObject> checkPt_Objs;
    // Private:
    private float sphR_foot = 30.0f;
    private float sphR;
    // previous and next checkPt sphere indicators: draw line and respawn
    private GameObject prevSph;
    private GameObject nextSph;
    private int nextIdx;
    // Consts:
    private const float inch2meter = 0.0254f;  // inch to meter
    private const float foot2meter = 0.3048f;
    private const string checkPt_fPath = "Assets/Scripts/Sample-track.txt";
    private readonly Color unfinishedColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);  // half-transparent red
    private readonly Color targetColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);  // solid blue
    private readonly Color finishedColor = new Color(0.0f, 0.0f, 1.0f, 0.1f);  // very transparent blue

    
    // Start is called before the first frame update
    void Start()
    {
        // init checking points and Instantiate Spheres indicating check points
        checkPts = parse_file(); 
        draw_checkPt();
        prevSph = checkPt_Objs[0];
        nextSph = checkPt_Objs[1];
        nextIdx = 1;

        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // ********** Helper Functions **********
    List<Vector3> parse_file(string filePath=checkPt_fPath)
    {
        List<Vector3> positions = new List<Vector3>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] coords = line.Split(' ');
                Vector3 pos = new Vector3(
                    float.Parse(coords[0]),
                    float.Parse(coords[1]), 
                    float.Parse(coords[2])
                );
                positions.Add(pos * inch2meter);
            }
        }
        return positions;
    }


    void draw_checkPt() {
        GameObject spherePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // set the color and transparency: half-transparent red
        spherePrefab.GetComponent<Renderer>().material.color = unfinishedColor;
        // set the radius to 30.0 feet in meter
        sphR = sphR_foot * foot2meter;
        spherePrefab.transform.localScale = Vector3.one * sphR; 

        // Also draw a dummy sphere at start location
        GameObject dummy = Instantiate(spherePrefab, camera.transform.position, Quaternion.identity);
        dummy.GetComponent<Renderer>().material.color = finishedColor;
        checkPt_Objs.Add(dummy);

        foreach (Vector3 pos in checkPts)
        {
            GameObject sphere = Instantiate(spherePrefab, pos, Quaternion.identity);
            checkPt_Objs.Add(sphere);
        }

        Destroy(spherePrefab); // destroy the sphere prefab since it's no longer needed
    }

    /// <summary>
    /// 1. detect if user position is within the radius of nextSph
    /// 2. if yes: update prevSph and nextSph; update color
    /// </summary>
    void update_target() {
        // current Pos
        Vector3 currPos = camera.transform.position;
        
        if (Vector3.Distance(currPos, nextSph.transform.position) <= sphR)
        {
            // update reference
            nextIdx++;
            prevSph = nextSph;
            nextSph = checkPt_Objs[nextIdx];

            // update color
            prevSph.GetComponent<Renderer>().material.color = finishedColor;
            nextSph.GetComponent<Renderer>().material.color = targetColor;
        }
    }
}
