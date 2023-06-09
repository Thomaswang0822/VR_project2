using System.Collections;
using System.Collections.Generic;
using System.IO;  // to parse
using System.Linq;
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

    public TextAsset track;
    public GameObject plane;
    public GameObject campus;
    public List<Vector3> checkPts;
    // should have been private, but we make it public to better switch viewpoint in pause mode
    public List<GameObject> checkPtObjs;
    public LineRenderer lineRenderer;

    public Material unfinishedColor;
    public Material targetColor;
    public Material finishedColor;

    public TextMeshProUGUI hud;
    public GameObject miniMap;
    public GameObject nextLegend;  // a box in mini-map
    public GameObject nextNextLegend;  // a sphere in mini-map

    // Private:
    private GameState state = GameState.Waiting;
    private PlaneController planeController;
    private AudioController audioController;
    private float sphR_foot = 30.0f;
    private float sphR;
    // previous and next checkPt sphere indicators: draw line and respawn
    private GameObject prevSph;
    private GameObject nextSph;
    private int nextIdx;  // so to increment it and update nextSph
    private float r_miniMap;  // radius of minimap

    // Timer stuff
    private float elapsed = 0f;
    private float countdown = 10.0f;

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
        // checkPts = ParseFile(); 
        checkPts = ParseStr(); 
        DrawCheckpoint();
        prevSph = checkPtObjs[0];
        nextSph = checkPtObjs[1];
        nextIdx = 1;

        // move plane to first checkpoint
        planeController = plane.GetComponent<PlaneController>();
        OrientPlane();

        // init audio controller
        audioController = plane.GetComponent<AudioController>();
        // and play ready sound immediately
        audioController.onReady();

        lineRenderer.material.color = Color.green;

        r_miniMap = miniMap.transform.lossyScale.x * 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimers();
        UpdateGUI();

        DrawLineInd();
        // UpdateTarget();
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

    List<Vector3> ParseStr()
    {
//         string info = @"-84024 360 -85271
// -92678 2000 -85271
// -98122 5000 -96911
// -92678 4000 -85271
// -78619 1674 -85271
// -68341 100 -71901
// -78619 100 -83753
// -94793 1000 -88931";
        string info = track.text;

        List<Vector3> positions = new List<Vector3>();
        using (StringReader reader = new StringReader(info))
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
        // play crashing audio
        audioController.onCrash();
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
                // Play default audio only if current clip is not the default
                if (!audioController.src.clip.Equals(audioController.speed)) {
                    audioController.onDriving();
                }
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
        spherePrefab.GetComponent<Renderer>().material = unfinishedColor;
        spherePrefab.tag = "Sphere";
        spherePrefab.GetComponent<SphereCollider>().isTrigger = true;
        spherePrefab.GetComponent<SphereCollider>().enabled = true;

        // set the radius to 30.0 feet in meter
        sphR = sphR_foot * foot2meter;
        spherePrefab.transform.localScale = Vector3.one * sphR; 

        foreach (Vector3 pos in checkPts)
        {
            GameObject sphere = Instantiate(spherePrefab, pos, Quaternion.identity);
            checkPtObjs.Add(sphere);
        }

        checkPtObjs[0].GetComponent<Renderer>().material = finishedColor;
        checkPtObjs[1].GetComponent<Renderer>().material = targetColor;

        Destroy(spherePrefab); // destroy the sphere prefab since it's no longer needed
    }

    // The plane controller tells us when we should try to update.
    // We pass in the collider to double check that we're intersecting with the right game object.
    public void OnTargetUpdate(Collider collider) {
        // current Pos
        Vector3 currPos = plane.transform.position;
        
        // Sanity check! Make sure this is the right object
        if (collider.gameObject == nextSph)
        // if (plane.GetComponent<Collider>().bounds.Intersects(nextSph.GetComponent<Collider>().bounds))
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
            prevSph.GetComponent<Renderer>().material = finishedColor;
            nextSph.GetComponent<Renderer>().material = targetColor;

            // play bingo sound
            audioController.onPass();
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
        // Step 1: draw the mini-map frame
        // bottom mid point in world coordinate
        Vector3 worldBM = vrCam.ScreenToWorldPoint(new Vector3(0.5f * vrCam.pixelWidth, 0f, dist_miniMap));
        // move up a little bit
        miniMap.transform.position = worldBM + new Vector3(0f, 2f * r_miniMap, 0f);

        // Step 2: draw legends
        // center arrow lies under mini_map and thus requires no handle
        Vector3 center = miniMap.transform.position;

        if (nextIdx == checkPts.Count - 1) {
            // next target is destination, only draw next Legend at the radius
            nextLegend.transform.position = center + r_miniMap * (checkPts[nextIdx] - center).normalized;
            // safe destroy
            if (nextNextLegend != null) {
                Destroy(nextNextLegend);
            }
        }
        else {
            // pin nextNextLegend on the surface of the minimap
            // nextLegend adjust position (likely inside the minimap sphere) accordingly
            float dist = (checkPts[nextIdx+1] - center).magnitude;
            nextNextLegend.transform.position = center + r_miniMap * (checkPts[nextIdx+1] - center).normalized;
            nextLegend.transform.position = center + r_miniMap * (checkPts[nextIdx] - center) / dist;
        }
    }
}
