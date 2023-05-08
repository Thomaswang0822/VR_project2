using System.Collections;
using System.Collections.Generic;
using System.IO;  // to parse
using UnityEngine;

public class AirRacing : MonoBehaviour
{
    // Public:
    public GameObject plane;
    public GameObject campus;
    public List<Vector3> checkPts;
    // should have been private, but we make it public to better switch viewpoint in pause mode
    public List<GameObject> checkPtObjs;
    public LineRenderer lineRenderer;

    public Color unfinishedColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);  // half-transparent red
    public Color targetColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);  // solid blue
    public Color finishedColor = new Color(0.0f, 1.0f, 0.0f, 0.1f);  // green

    // Private:
    private PlaneController planeController;
    private float sphR_foot = 100.0f;
    private float sphR;
    // previous and next checkPt sphere indicators: draw line and respawn
    private GameObject prevSph;
    private GameObject nextSph;
    private int nextIdx;

    // Consts:
    private const float inch2meter = 0.0254f;  // inch to meter
    private const float foot2meter = 0.3048f;
    private const string checkPt_fPath = "Assets/Scripts/Sample-track.txt";
    // private const float segLen = 0.5f;  // length of a segment in a dotted line
    // private const float segSpacing = 0.3f;  // spacing between segments
    private const int nSeg = 2;  // number of segments per dotted line

    
    // Start is called before the first frame update
    void Start()
    {
        // init checking points and Instantiate Spheres indicating check points
        checkPts = ParseFile(); 
        DrawCheckpoint();
        prevSph = checkPtObjs[0];
        nextSph = checkPtObjs[1];
        nextIdx = 1;

        planeController = plane.GetComponent<PlaneController>();
        plane.transform.position = prevSph.transform.position;

        //
        lineRenderer.material.color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        DrawLineInd();
        UpdateTarget();
    }

    // ********** Helper Functions **********
    List<Vector3> ParseFile(string filePath = checkPt_fPath)
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


    void DrawCheckpoint() {
        GameObject spherePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // set the color and transparency: half-transparent red
        spherePrefab.GetComponent<Renderer>().material.color = unfinishedColor;
        // set the radius to 30.0 feet in meter
        sphR = sphR_foot * foot2meter;
        spherePrefab.transform.localScale = Vector3.one * sphR; 

        foreach (Vector3 pos in checkPts)
        {
            GameObject sphere = Instantiate(spherePrefab, pos, Quaternion.identity);
            checkPtObjs.Add(sphere);
        }

        checkPtObjs[0].GetComponent<Renderer>().material.color = finishedColor;
        checkPtObjs[1].GetComponent<Renderer>().material.color = targetColor;

        Destroy(spherePrefab); // destroy the sphere prefab since it's no longer needed
    }

    /// <summary>
    /// 1. detect if user position is within the radius of nextSph
    /// 2. if yes: update prevSph and nextSph; update color
    /// </summary>
    void UpdateTarget() {
        // current Pos
        Vector3 currPos = plane.transform.position;
        
        // TODO: Check collider intersection instead?
        if (Vector3.Distance(currPos, nextSph.transform.position) <= sphR)
        {
            // update reference
            nextIdx++;
            prevSph = nextSph;
            nextSph = checkPtObjs[nextIdx];

            // update color
            prevSph.GetComponent<Renderer>().material.color = finishedColor;
            nextSph.GetComponent<Renderer>().material.color = targetColor;
        }
    }

    /// <summary>
    /// Draw a line indicator between 
    /// 1. currPos and target
    /// ? AND/OR
    /// 2. prev and target
    /// </summary>
    void DrawLineInd() {
        /* Vector3 startPos = prevSph.transform.position;
        Vector3 endPos = nextSph.transform.position;
        Vector3 direction = (endPos - startPos).normalized;
        float distance = (endPos - startPos).magnitude; 

        // length of a segment plus spacing between segments
        float unitLen = distance / nSeg;
        float segLen = 0.6f * unitLen;

        // Set the number of segments in the line renderer
        // add an extra segment (thus 2 pos) to make sure 
        // the line reaches the end
        lineRenderer.positionCount = nSeg * 2 + 2;

        // Set the positions of the vertices to create the dotted line
        for (int i = 0; i <= nSeg; i++)
        {
            Vector3 segStartPos = startPos + (direction * i * unitLen);
            Vector3 segEndPos = segStartPos + (direction * segLen);
            lineRenderer.SetPosition(i * 2, segStartPos);
            lineRenderer.SetPosition(i * 2 + 1, segEndPos);
        } */

        lineRenderer.SetPosition(0, prevSph.transform.position);
        lineRenderer.SetPosition(1, nextSph.transform.position);
    }
}
