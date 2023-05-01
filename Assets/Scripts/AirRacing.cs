using System.Collections;
using System.Collections.Generic;
using System.IO;  // to parse
using UnityEngine;

public class AirRacing : MonoBehaviour
{
    // Public:
    public GameObject campus;
    public List<Vector3> checkPts;
    // Private:
    private float sphR_foot = 30.0f;
    public List<GameObject> checkPt_Objs;
    // Consts:
    private const float inch2meter = 0.0254f;  // inch to meter
    private const float foot2meter = 0.3048f;
    private const string checkPt_fPath = "Assets/Scripts/Sample-track.txt";
    
    // Start is called before the first frame update
    void Start()
    {
        // init checking points
        checkPts = ParseFile();
        // Instantiate Spheres indicating check points
        draw_checkPt();
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // ********** Helper Functions **********
    List<Vector3> ParseFile(string filePath=checkPt_fPath)
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
        spherePrefab.GetComponent<Renderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        // set the radius to 30.0 feet in meter
        spherePrefab.transform.localScale = Vector3.one * sphR_foot * foot2meter; 

        foreach (Vector3 pos in checkPts)
        {
            GameObject sphere = Instantiate(spherePrefab, pos, Quaternion.identity);
            checkPt_Objs.Add(sphere);
        }

        Destroy(spherePrefab); // destroy the sphere prefab since it's no longer needed
    }

}
