using System.Collections;
using System.Collections.Generic;
using System.IO;  // to parse
using UnityEngine;
using TMPro;

// There are three states that the game can be in:
enum GameState {
    // The game hasn't started and we're waiting for countdown. No input should be accepted.
    Waiting,
    // The game has started. The time elapsed ticks up.
    Playing,
    // The player collided with the ground/building. Time ticks up while countdown is shown. No input should be accepted.
    Respawning,
    // The player has crossed the last checkpoint. The final time is displayed. No input should be accepted.
    Finished,
}

public class AirRacing : MonoBehaviour
{
    // Public:
    public Camera vrCam;

    public GameObject plane;
    public GameObject campus;
    public List<Vector3> checkPts;
    // should have been private, but we make it public to better switch viewpoint in pause mode
    public List<GameObject> checkPtObjs;
    public LineRenderer lineRenderer;

    public Color unfinishedColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);  // half-transparent red
    public Color targetColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);  // solid blue
    public Color finishedColor = new Color(0.0f, 1.0f, 0.0f, 0.1f);  // green

    public TextMeshProUGUI hud;
    public GameObject miniMap;

    // Private:
    private GameState state = GameState.Waiting;
    private PlaneController planeController;
    private float sphR_foot = 100.0f;
    private float sphR;
    // previous and next checkPt sphere indicators: draw line and respawn
    private GameObject prevSph;
    private GameObject nextSph;
    private int nextIdx;  // so to increment it and update nextSph
    private float r_miniMap;  // radius of minimap

    // Timer stuff
    private float elapsed = 0f;
    private float countdown = 3.0f;

    // Consts:
    private const float inch2meter = 0.0254f;  // inch to meter
    private const float foot2meter = 0.3048f;
    private const string checkPt_fPath = "Assets/Scripts/Sample-track.txt";
    // private const float segLen = 0.5f;  // length of a segment in a dotted line
    // private const float segSpacing = 0.3f;  // spacing between segments
    private const int nSeg = 2;  // number of segments per dotted line
    private const float dist_miniMap = 5.0f;  // distance from camera (in camera coord Z axis)

    
    // Start is called before the first frame update
    void Start()
    {
        // init checking points and Instantiate Spheres indicating check points
        checkPts = ParseFile(); 
        DrawCheckpoint();
        prevSph = checkPtObjs[0];
        nextSph = checkPtObjs[1];
        nextIdx = 1;

        // move plane to first checkpoint
        planeController = plane.GetComponent<PlaneController>();
        OrientPlane();

        lineRenderer.material.color = Color.green;

        r_miniMap = miniMap.transform.localScale.x;

        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimers();
        UpdateGUI();

        DrawLineInd();
        UpdateTarget();
        DrawMiniMap();
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

    public bool AcceptInput() {
        return state == GameState.Playing;
    }

    public void OnCollision() {
        OrientPlane();
        state = GameState.Respawning;
    }

    // Place plane at current idx. Point it towards next idx.
    void OrientPlane() {
        plane.transform.position = prevSph.transform.position;
        plane.transform.forward = nextSph.transform.position - prevSph.transform.position;
    }

    // Update timers.
    // We start in the Waiting state at the start of the game.
    // Once enough time has passed (3.0s), switch the state so that the player can start playing.
    // If we detect a collision with the ground or buildings, while playing, change the game state;
    // set to respawning and respawn the player somewhere else. Tick the elapsed timer down.
    // If the last checkpoint is cleared, we're finished!
    void UpdateTimers() {
        switch (state) {
            case GameState.Waiting:
                countdown -= Time.deltaTime;

                if (countdown <= 0.0f) {
                    countdown = 3.0f;
                    state = GameState.Playing;
                }

                break;

            case GameState.Playing:
                elapsed += Time.deltaTime;

                break;

            case GameState.Respawning:
                elapsed += Time.deltaTime;
                countdown -= Time.deltaTime;

                if (countdown <= 0.0f) {
                    countdown = 3.0f;
                    state = GameState.Playing;
                }

                break;

            case GameState.Finished:
                // Don't do anything
                break;
        }
    }

    // Update GUI
    void UpdateGUI() {
        switch (state) {
            case GameState.Waiting:
                hud.text = "Ready! " + countdown.ToString("F1") + "s";
                break;

            case GameState.Playing:
                hud.text = "Cleared " + (nextIdx - 1).ToString() + "/" + (checkPts.Count - 1).ToString() + "\n";
                hud.text += (Mathf.FloorToInt(elapsed / 60.0f)).ToString("D2") + ":" + System.String.Format("{0:00.0}", elapsed % 60.0f);

                break;

            case GameState.Respawning:
                hud.text = "Crashed! Respawning in " + countdown.ToString("F1") + "s\n";
                hud.text += "Cleared " + (nextIdx - 1).ToString() + "/" + (checkPts.Count - 1).ToString() + "\n";
                hud.text += (Mathf.FloorToInt(elapsed / 60.0f)).ToString("D2") + ":" + System.String.Format("{0:00.0}", elapsed % 60.0f);

                break;

            case GameState.Finished:
                hud.text = "Finished! ";
                hud.text += (Mathf.FloorToInt(elapsed / 60.0f)).ToString("D2") + ":" + System.String.Format("{0:00.0}", elapsed % 60.0f);
                break;
        }
    }

    void DrawCheckpoint() {
        GameObject spherePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // set the color and transparency: half-transparent red
        spherePrefab.GetComponent<Renderer>().material.color = unfinishedColor;
        spherePrefab.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");;
        spherePrefab.GetComponent<SphereCollider>().enabled = false;

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
            if (nextIdx == checkPts.Count - 1) {
                // If nextIdx the last checkpoint, set game state and return
                state = GameState.Finished;
                return;
            }

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
        lineRenderer.SetPosition(0, prevSph.transform.position);
        lineRenderer.SetPosition(1, nextSph.transform.position);
    }

    void DrawMiniMap() {
        // bottom mid point in world coordinate
        Vector3 worldBL = vrCam.ScreenToWorldPoint(new Vector3(0.5f * vrCam.pixelWidth, 0f, dist_miniMap));

        // move up a little bit
        miniMap.transform.position = worldBL + new Vector3(0f, 0.5f * r_miniMap, 0f);
    }
}
