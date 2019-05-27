using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utility : MonoBehaviour
{
    // Specified event and drone in n=4 scenario
    private static List<int> DroneList = new List<int> { 1, 2, 3, 4 };
    private static List<int> EventList = new List<int> { 1, 2, 3, 4 };
    public static IEnumerator<int> DroneIt = DroneList.GetEnumerator();
    public static IEnumerator<int> EventIt = EventList.GetEnumerator();

    // Unity constant
    public static float DELTATIME;
    public static float AVGTIME;
    public static bool IS_RND_TAKEOFF = true;

    // game logic constant
    public const int EXIT_TIME = 6000;  // the total time that the game is running
    public static Dictionary<int, int> EVENT_INTERVALS = new Dictionary<int, int>() { { 5, 100 }, { 8, 100 }, { 10, 84 } , { 15, 76 }, { 20, 72 }};
    public static Dictionary<int, int> SEED = new Dictionary<int, int>() { { 5, 100 }, { 8, 100 }, { 10, 100 }, { 15, 42 }, { 20, 42 }};

    // drone logic constant
    public static readonly float BOUND_DIM = 1.4f, INTERACT_DIM = 2.3f, REPLAN_DIM = 2.2f;
    public static Vector3 COLLISION_BOUND = new Vector3(BOUND_DIM, BOUND_DIM, BOUND_DIM);
    public static Vector3 CUTOFF_INTERACT = new Vector3(INTERACT_DIM, INTERACT_DIM, INTERACT_DIM);
    public static Vector3 CUTOFF_REPLAN = new Vector3(REPLAN_DIM, REPLAN_DIM, REPLAN_DIM);

    public static float INTERACT_TIME = 1f;
    public static float DRONE_SPEED;

    public static Color Traj = new Color(1.0f, 0.1259f, 0.3736f, 1.0f);
    // shelf and event
    // private static Vector3 ShelfBasePos = new Vector3(-3f, 10.45f, -2.15f); // left-bottom corner of the shelf
    private static Vector3 ShelfBasePos = new Vector3(26.57f, 20.41f, 1.93f); // left-bottom corner of the shelf
    private static Vector3 ParkingBasePos = new Vector3(26.51615f, 17.572f, 20.04752f);
    // private static Vector3 ParkingBasePos = new Vector3(26.51615f, 17.572f, 24.01752f);
    private static float parkingInterval = 2.4f;
    // private static float horizonInterval = -2.0f;
    // private static float verticalInterval = 1.7f;
    private static float horizonInterval = -2.26f;
    private static float verticalInterval = 1.7f;

    public static Vector3[] shelves = InitShelves(ShelfBasePos, horizonInterval, verticalInterval, 4, 16);
    public static Vector3[] parking = InitParkingLot(ParkingBasePos, parkingInterval, parkingInterval, 4, 16);
    public static Color eventWaitingColor = new Color(255, 0, 0);
    public static Color eventIdleColor = new Color(255, 255, 255);
    //public static Material eventWaitingMat = Resources.Load("M_bear", typeof(Material)) as Material;
    //public static Material eventIdleMat = Resources.Load("M_pig", typeof(Material)) as Material;

    public static Vector3[] InitShelves(Vector3 basePos, float horizonInterval, float verticalInterval, int numLayer, int itemPerLayer){
        Vector3[] shelves = new Vector3[numLayer * itemPerLayer];
        for (int i = 0; i < numLayer; i++){
            for (int j = 0; j < itemPerLayer; j++){
                int curIdx = i * itemPerLayer + j;
                Vector3 curPos = new Vector3(basePos[0] + j * horizonInterval, basePos[1] + i * verticalInterval, basePos[2]);
                shelves[curIdx] = curPos;
            }
        }
        return shelves;
    }

    public static Vector3[] InitParkingLot(Vector3 basePos, float horizonInterval, float verticalInterval, int numLayer, int itemPerLayer)
    {
        Vector3[] ParkingLot = new Vector3[numLayer * itemPerLayer];
        for (int i = 0; i < numLayer; i++)
        {
            for (int j = 0; j < itemPerLayer; j++)
            {
                int curIdx = i * itemPerLayer + j;
                Vector3 curPos = new Vector3(basePos.x - j * horizonInterval, basePos.y, basePos.z - i * verticalInterval);
                ParkingLot[curIdx] = curPos;

            }
        }
        return ParkingLot;
    }

    //public static Vector3[] parking = new Vector3[]
    //{
    //    //[start, end] of the parking lot
    //    new Vector3(-0.241f, 7.914f, 11.1f),  new Vector3(-15.59f, 7.914f, 11.1f)
    //};

    public static bool IsLessThan(Vector3 a, Vector3 b)
    {
        return a.magnitude < b.x;
        // return Mathf.Abs(a.x) < Mathf.Abs(b.x) && Mathf.Abs(a.y) < Mathf.Abs(b.y) && Mathf.Abs(a.z) < Mathf.Abs(b.z);
    }

    public static bool IsMoreThan(Vector3 a, Vector3 b)
    {
        return a.magnitude >= b.x;
        // return Mathf.Abs(a.x) < Mathf.Abs(b.x) && Mathf.Abs(a.y) < Mathf.Abs(b.y) && Mathf.Abs(a.z) < Mathf.Abs(b.z);
    }

    public static float CalDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(Mathf.Pow(a[0] - b[0], 2.0f) + Mathf.Pow(a[1] - b[1], 2.0f) + Mathf.Pow(a[2] - b[2], 2.0f));
    }

    public static void DeleteChild(GameObject go, string name)
    {
        foreach (Transform child in go.transform)
        {
            if (child.gameObject.name == name)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }

    void Awake()
    {
        DELTATIME = Time.deltaTime;
        DRONE_SPEED = (INTERACT_DIM - BOUND_DIM) / INTERACT_TIME * DELTATIME;
        // cal average time
        int units = 16;
        int[] numOptions = new int[] { 5, 8, 10, 15, 20 };
        float[] times = new float[numOptions.Length];
        AVGTIME = 0f;

        for (int i = 0; i < numOptions.Length; i++)
        {
            int num = numOptions[i];
            int rowcapacity = 5;
            // int rowNeeded = num / units;
            int parkingInterval = units / rowcapacity;
            int rowNeeded = num / rowcapacity;
            // int parkingInterval = 1;
            for (int j = 0; j < num; j++)
            {
                // Debug.Log(parkinglot[parkingInterval * i]);
                Vector3 curPos = parking[parkingInterval * j];
                times[i] += CalAveTime(curPos, shelves, DELTATIME);
            }
            times[i] = times[i] / num;
            Debug.Log("times " + i + " " + times[i]);

            AVGTIME += times[i];
        }

        AVGTIME /= numOptions.Length;
    }

    public float CalAveTime(Vector3 curPos, Vector3[] shelf, float deltaTime)
    {
        // calculate average round trip time for the current drone
        float numFrame = 0;

        foreach (Vector3 shelfGrid in shelf)
        {
            float curDist = 2 * Utility.CalDistance(curPos, shelfGrid);
            numFrame += curDist / DRONE_SPEED;
        }

        return numFrame * deltaTime / shelf.Length;
    }

}
