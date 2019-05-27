#define IS_USER_STUDY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using UnityEngine.SceneManagement;


public class TrafficControl : MonoBehaviour
{
    public GameObject droneBaseObject;
    public GameObject eventBaseObject;
    public static GameObject worldobject;
    // private GameObject canvas;

    public static Dictionary<int, Drone> dronesDict = new Dictionary<int, Drone>();
    public static Dictionary<int, Event> eventsDict = new Dictionary<int, Event>();

    public OrderedSet<int> waitingEventsId = new OrderedSet<int>();
    public HashSet<int> ongoingEventsId = new HashSet<int>();

    public OrderedSet<int> availableDronesId = new OrderedSet<int>();
    public HashSet<int> workingDronesId = new HashSet<int>();

    public static Vector3[] shelves = Utility.shelves;
    public static Vector3[] parkinglot = Utility.parking;

    // public Vector3 doorPos;vv
    public int numDrones;
    // private int numCollision = 0;
    private int systemError = 0;
    private int userError = 0;
    private int timeCounter = 0;
    private int cleanCounter = 0;
    private int successEventCounter = 0;
    private int totalEventCounter = 0;

    private float AVE_TIME;
    private const int EXIT_TIME = Utility.EXIT_TIME;
    private int EVENT_INTERVAL;
    // public const int EVENT_FREQUENCY = 100;
    private const int CLEAN_INTERVAL = 100; // interval to clean the shattered drone

    private readonly double REPLAN_FAIL_RATE = 1;
    //private static Vector3[] parking = Utility.parking;

    public static int seed;
    private static System.Random rnd;
    private static Material eventIdleMat;
    private static Material eventWaitingMat;

    public int GenRandEvent()
    {
        int idx = -1;
        // Debug.Log("Contains" + waitingEventsId.Contains(0));
        while (idx == -1)
        {
            idx = rnd.Next(shelves.Length);
            // Debug.Log(idx);
            idx = waitingEventsId.Contains(idx) ? -1 : idx;
        }

        return idx;
    }

    public void initDrones(int num)
    {
        int units = 16;
        int rowcapacity = 5;
        int parkingInterval = units / rowcapacity;
        int rowNeeded = num / rowcapacity;
        for (int i = 0; i < num; i++)
        {
            // Debug.Log(parkinglot[parkingInterval * i]);
            Drone newDrone = new Drone(i, parkinglot[parkingInterval * i]);
            dronesDict.Add(i, newDrone);
            availableDronesId.Add(i);
        } 
    }
    // Use this for initialization
    void Start()
    {
        AVE_TIME = Utility.AVGTIME;
        // EVENT_INTERVAL = (int) AVE_TIME / numDrones;
        EVENT_INTERVAL = Utility.EVENT_INTERVALS[numDrones];
        Debug.Log("AVG TIME " + AVE_TIME + " Event interval: " + EVENT_INTERVAL);

        seed = Utility.SEED[numDrones];
        rnd = new System.Random(seed);

        worldobject = this.gameObject;
        // canvas = GameObject.Find("Canvas");
        // canvas.SetActive(false);
        dronesDict = new Dictionary<int, Drone>();
        initDrones(numDrones);

        // event init
        eventIdleMat = Resources.Load("M_bear", typeof(Material)) as Material;
        eventWaitingMat = Resources.Load("M_pig", typeof(Material)) as Material;

        for (int i = 0; i < shelves.Length; i++)
        {
            Event newEvent = new Event(i, shelves[i]);
            newEvent.markEvent(eventIdleMat);
            eventsDict.Add(i, newEvent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // canvas.SetActive(false);
        // Check if to generate new random event 
        if (timeCounter % EVENT_INTERVAL == 0)
        {
            if (waitingEventsId.Count + ongoingEventsId.Count < shelves.Length - 1)
            {
                int newIdx = GenRandEvent();
                Event newWaitingEvent = eventsDict[newIdx];
                newWaitingEvent.markEvent(eventWaitingMat);
                waitingEventsId.Add(newIdx);

                totalEventCounter++;
            }
        }

        if (cleanCounter == CLEAN_INTERVAL && GameObject.FindGameObjectsWithTag("shatter") != null) 
        {
            foreach (GameObject shatterObject in GameObject.FindGameObjectsWithTag("shatter"))
            {
                // Debug.Log("cleaning + " + shatterObject);
                Destroy(shatterObject);
            }
            cleanCounter = -1;
        }

        // while available drones, pop event
        // assign drone dst, to-do event, remove from avail, add to working drones
        // direction 
        if (availableDronesId.Count > 0 && waitingEventsId.Count > 0)
        {
            int e = waitingEventsId.Next();
            int d = Utility.IS_RND_TAKEOFF ? availableDronesId.NextRnd() : availableDronesId.Next();

            //if (numDrones == 4)
            //{
            //    e = Utility.EventIt.Current; d = Utility.DroneIt.Current;
            //    Utility.EventIt.MoveNext(); Utility.DroneIt.MoveNext();
            //    Debug.Log("e: " + e + " d: " + d);
            //}
            //else
            //{
            //    e = waitingEventsId.Next();
            //    d = Utility.IS_RND_TAKEOFF ? availableDronesId.NextRnd() : availableDronesId.Next();
            //}

            dronesDict[d].AddEvent(eventsDict[e]);
            // Debug.Log("assign event " + e + " to drone " + d + " with direction: " + dronesDict[d].direction);

            availableDronesId.Remove(d);
            workingDronesId.Add(d);
            waitingEventsId.Remove(e);
            ongoingEventsId.Add(e);
        }

        // apply force meanwhile check collision 
        foreach (int i in workingDronesId)
        {
            Vector3 force = new Vector3();
            bool replan = false;
            bool isWarning = false;
            foreach (int j in workingDronesId)
            {

                Vector3 delta = dronesDict[i].gameObjectPointer.transform.Find("pCube2").gameObject.transform.position - dronesDict[j].gameObjectPointer.transform.Find("pCube2").gameObject.transform.position;
                float dis = delta.magnitude;
                //Vector3 delta = dronesDict[i].curPos - dronesDict[j].curPos;
                //if (i != j && Utility.IsMoreThan(delta, CUTOFF_INTERACT))
                //{
                //    isWarning = false;
                //}

                if (i != j && Utility.IsLessThan(delta, Utility.CUTOFF_INTERACT))
                {
                    //Debug.Log("The Drone: " + i + "and " + j + "will warning");
                    // collide
                    isWarning = true;
                    if (Utility.IsLessThan(delta, Utility.COLLISION_BOUND))
                    {
                        isWarning = false;
                        //Debug.Log("drone: " + i + ", " + j + " collide dealing event: " + dronesDict[i].eventId + ", " + dronesDict[j].eventId);
                        userError++;
                        dronesDict[i].status = 5;
                        dronesDict[j].status = 5;
                        dronesDict[i].gameObjectPointer.GetComponent<DroneProperties>().classPointer.CollideEffect();
                        dronesDict[j].gameObjectPointer.GetComponent<DroneProperties>().classPointer.CollideEffect();

                    }
                    else
                    {
                        //Debug.Log("drone " + i + ", " + j + " system plan error");
                        systemError++;
                    }
                }

                else if (i != j && Utility.IsLessThan(delta, Utility.CUTOFF_REPLAN))
                {
                    //dronesDict[i].WarningEffect(false);
                    //dronesDict[j].WarningEffect(false);

                    // replan with certain failure rate
                    if (rnd.NextDouble() < 1 - REPLAN_FAIL_RATE)
                    {
                        // Debug.Log("replan for " + i);
                        force += delta;
                        replan = true;
                    }
                }

            }

            // update direction
            dronesDict[i].isWarning = isWarning;
            dronesDict[i].direction = replan ? Vector3.Normalize(force) : Vector3.Normalize(dronesDict[i].dstPos - dronesDict[i].curPos);
        }


        // check status
        // move every working drone
        for (int i = 0; i < numDrones; i++)
        {
            int status = dronesDict[i].status;
            if (status == 0)
            {
                dronesDict[i].WarningRender(false);
                continue;
            }
            else if (status == 5)
            {
                dronesDict[i].isPaused = false;
                dronesDict[i].WarningRender(false);
                dronesDict[i].curPos = dronesDict[i].parkingPos;
                // check the event can be added back
                int eid = dronesDict[i].eventId;
                // if (!waitingEventsId.Contains(eid) && !ongoingEventsId.Contains(eid))
                waitingEventsId.Add(eid);

                Event curEvent = eventsDict[dronesDict[i].eventId];
                curEvent.markEvent(eventWaitingMat);

                dronesDict[i].status = 0;
            }

            else if (status == 2 || status == 3)
            {
                if (dronesDict[i].isWarning == true)
                {
                    dronesDict[i].WarningRender(true);
                    //canvas.SetActive(true);
                }
                else
                {
                    dronesDict[i].WarningRender(false);
                }
            }

            int moveStatus = dronesDict[i].Move();
            if (moveStatus == 1)  // drone status 2 --> 3
            {
                Event curEvent = eventsDict[dronesDict[i].eventId];
                curEvent.markEvent(eventIdleMat);
                ongoingEventsId.Remove(dronesDict[i].eventId);
            }
            else if (moveStatus == 2)  // end of whole trip
            {
                successEventCounter++;
            }

            if (dronesDict[i].status == 0)
            {
                //dronesDict[i].HideArrow();
                workingDronesId.Remove(i);
                availableDronesId.Add(i);
                ongoingEventsId.Remove(dronesDict[i].eventId);
            }
        }

        // update counter
        timeCounter++;
        cleanCounter++;

#if IS_USER_STUDY
        if (timeCounter >= EXIT_TIME)
            QuitGame();
#endif
    }

    void OnApplicationQuit()
    {
        float successRate = successEventCounter / numDrones;
        float clickFPRate = (Drone.clickTime > 0) ? Drone.wrongClickTime / Drone.clickTime : 0;
        Debug.Log("Total user error: " + userError / 2 + "; Click FP rate: " + clickFPRate.ToString("0.000") + "; Performance: " + successRate);

        string filename = "Assets/Log/" + SceneManager.GetActiveScene().name + "_" + numDrones + ".txt"; 
        // write to log file
        StreamWriter fileWriter = new StreamWriter(filename, true);
        // fileWriter.AutoFlush = true;

        fileWriter.WriteLine("==========Basic Parameters==========");
        fileWriter.WriteLine("Interface " + SceneManager.GetActiveScene().name);
        fileWriter.WriteLine("FPS: " + 1 / Time.deltaTime);
        fileWriter.WriteLine("Drone speed: " + Utility.DRONE_SPEED);
        fileWriter.WriteLine("Seed: ", seed);
        fileWriter.WriteLine("Number of drones: " + numDrones);
        fileWriter.WriteLine("Average time: " + AVE_TIME);
        fileWriter.WriteLine("Event interval: " + EVENT_INTERVAL);
        fileWriter.WriteLine("Number of events: " + totalEventCounter);

        fileWriter.WriteLine("==========User Study Data==========");
        fileWriter.WriteLine("User error: " + userError / 2);
        fileWriter.WriteLine("Number success events: " + successEventCounter);
        fileWriter.WriteLine("Wrong clicks: " + Drone.wrongClickTime);
        fileWriter.WriteLine("Total clicks: " + Drone.clickTime);
        fileWriter.WriteLine(" ");

        fileWriter.Close();
        //}

        //catch (System.Exception e)
        //{
        //    Debug.LogError("cannot write into the file!");
        //}

    }

    public void QuitGame()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
