using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone {
    public static Vector3 hoverShift = new Vector3(0f, 1.5f, 0f);
    public static int pauseTime = 100;

    public static float clickTime = 0;
    public static float wrongClickTime = 0;

    // drone properties
    public GameObject gameObjectPointer;
    public GameObject arrows1;
    public GameObject arrows2;
    public GameObject ring;
    public GameObject ring_2;
    public GameObject shatter;

    public int droneId;
    public bool isPaused;
    public int pauseCounter;
    public int status; // 0: parked, 1: takeoff, 2: to shelf, 3: to hover, 4: land, 5: collide
    public bool isCollided;
    public bool isWarning;

    public Vector3 parkingPos;
    public Vector3 hoverPos;
    public Vector3 eventPos;
    public Vector3 curPos;
    public Vector3 dstPos;
    public Vector3 direction;
    public int eventId;

    public Vector3 epsilon = new Vector3(0.1f, 0.1f, 0.1f);

    // public static float SPEED = Utility.DRONE_SPEED;   // private static readonly float SPEED = 0.07f;
    public float SPEED;
    private readonly Vector3 arrowOffset1 = new Vector3(0f, 0f, 0f);
    private readonly Vector3 arrowOffset2 = new Vector3(0f, 0f, 0.1412f);

    public Drone(int droneId, Vector3 initPos)
    {
        this.droneId = droneId;
        this.isPaused = false;
        this.parkingPos = this.curPos = initPos;
        this.hoverPos = this.parkingPos + hoverShift;
        this.status = 0;
        this.isWarning = false;

        // create game object
        // Debug.Log("Created new drone with id: " + droneId);
        GameObject baseObject = TrafficControl.worldobject.GetComponent<TrafficControl>().droneBaseObject;
        gameObjectPointer = Object.Instantiate(baseObject, initPos, Quaternion.identity);
        gameObjectPointer.GetComponent<DroneProperties>().classPointer = this;

        gameObjectPointer.name = string.Concat("Drone", droneId.ToString());
        gameObjectPointer.layer = 2;
        // gameObjectPointer.gameObject.tag = string.Concat("Drone", droneId.ToString());
        gameObjectPointer.transform.parent = TrafficControl.worldobject.transform;

        GameObject arrow1 = gameObjectPointer.GetComponent<DroneProperties>().Arrows;
        arrows1 = Object.Instantiate(arrow1, initPos, Quaternion.Euler(0f, -90f, 0f));
        arrows1.transform.parent = TrafficControl.worldobject.transform;
        GameObject arrow2 = gameObjectPointer.GetComponent<DroneProperties>().Arrows;
        arrows2 = Object.Instantiate(arrow2, initPos, Quaternion.Euler(0f, -270f, 0f));
        arrows2.transform.parent = TrafficControl.worldobject.transform;
        arrows1.SetActive(false);
        arrows2.SetActive(false);
        ring_2 = gameObjectPointer.transform.Find("warningSphere").gameObject;

    }

    public void AddEvent(Event e)
    {
        status = 1;
        dstPos = hoverPos;
        eventPos = e.pos;
        direction = Vector3.Normalize(dstPos - parkingPos);
        eventId = e.shelfId;
    }

    public void SetDronePause()
    {
        isPaused = true;
        pauseCounter = pauseTime;

        GameObject ring = this.gameObjectPointer.transform.Find("protectionSphere").gameObject;

        // Update user click info
        if (!isWarning)
            wrongClickTime++;
        clickTime++;
    }

    public void SetDroneRestart()
    {
        // Update user click info
        if (!isPaused)
            wrongClickTime++;
        clickTime++;

        isPaused = false;
    }

    /// <summary>
    /// Update the status of drone and Move
    /// </summary>
    /// <returns> 
    /// drone paused: 0; 
    /// end of to_shelf trip: 1;
    /// end of whole trip: 2;
    /// otherwise: -1 
    /// </returns>
    public int Move(){
        if (isPaused)
        {
            // if (pauseCounter-- == 0)
            // {
            //     isPaused = false;
            // }
            return 0;
        }
        // curPos = status == 0 ? gameObjectPointer.transform.position : gameObjectPointer.transform.position + direction * SPEED;
        // direction = Utility.shelves[eventId] - curPos;
        // direction = GameObject.Find("Event" + eventId.ToString()).transform.position - curPos;
        curPos = status == 0 ? curPos : curPos + direction * SPEED;
        // Debug.Log("Move drone " + droneId + " with dir: " + direction + " to pos: " + curPos);

        int flag = -1;
        if (status != 0 && Utility.IsLessThan(curPos - dstPos, epsilon))
        {
            // Debug.Log(status + " " + curPos + " " + dstPos + " " + hoverPos + " " + parkingPos + " " + eventPos);
        
            if (Utility.IsLessThan(dstPos - hoverPos, epsilon))
            {
                if (status == 1)  // end of takeoff
                {
                    Utility.DeleteChild(this.gameObjectPointer, "Line");
                    status = 2;
                    dstPos = eventPos;
                    // Debug.Log(droneId + "end of takeoff, now to event: " + eventPos + "cur: " + curPos);
                }
                else if (status == 3)  // end of to_hover trip
                {
                    Utility.DeleteChild(this.gameObjectPointer, "Line");
                    status = 4;
                    dstPos = parkingPos;
                }
            }
            else if (Utility.IsLessThan(dstPos - eventPos, epsilon))
            {   
                // cur_s = 2 --> 3
                // end of to_shelf trip
                status = 3;
                dstPos = hoverPos;
                flag = 1;
            }
            else
            {
                // end of whole trip
                status = 0;
                curPos = parkingPos;
                // Debug.Log(droneId + " end of whole trip");
                flag = 2;
            }
        }
        gameObjectPointer.transform.position = curPos;
        // gameObjectPointer.transform.rotation = Quaternion.identity;
        // gameObjectPointer.transform.rotation = TrafficControl.worldobject.transform.rotation;
        DisplayArrow();

        return flag;
    }

    public void RotateArrow()
    {
        Vector3 baseVector = Vector3.up;
        Vector3 basePoint = gameObjectPointer.transform.Find("pCube3").gameObject.transform.position;
        //Quaternion q = Quaternion.LookRotation(-direction, baseVector);
        Quaternion q = Quaternion.FromToRotation(new Vector3(0f, 0f, -1f), direction);
     // Debug.Log(q);
        arrows1.transform.rotation = q * Quaternion.Euler(0f, -90f, 0f);
        arrows2.transform.rotation = q * Quaternion.Euler(0f, -90f, 0f);
    }

    public void DisplayArrow()
    {
        RotateArrow();
        Vector3 basePoint = gameObjectPointer.transform.Find("pCube3").gameObject.transform.position;
        arrows1.transform.position = basePoint + arrowOffset1;
        arrows2.transform.position = curPos + arrowOffset2;


        if (status == 1)
        {
            arrows1.SetActive(true);
            arrows2.SetActive(false);
        }
        if (status == 2)
        {
            arrows1.SetActive(false);
            arrows2.SetActive(true);
        }
        if (gameObjectPointer.transform.position == parkingPos)
        {
            arrows1.SetActive(false);
            arrows2.SetActive(false);
        }
    }

    public void DroneCollideRender(bool collided)
    {
        ring = this.gameObjectPointer.transform.Find("protectionSphere").gameObject;
        if (collided == true){
            ring.GetComponent<MeshRenderer>().material = this.gameObjectPointer.GetComponent<DroneProperties>().collideMaterial;
        }
        else {
            ring.GetComponent<MeshRenderer>().material = this.gameObjectPointer.GetComponent<DroneProperties>().landingMaterial;
        }
    }

    public void CollideEffect()
    {
        shatter = this.gameObjectPointer.GetComponent<DroneProperties>().shatteredDrone;
        // Debug.Log("id" + this.droneId);
        // Debug.Log("status" + this.status);
        // Debug.Log("Collide Effect!");
        Utility.DeleteChild(this.gameObjectPointer, "Line");
        GameObject shatterObject = Object.Instantiate(shatter, this.gameObjectPointer.transform.position, this.gameObjectPointer.transform.rotation);
        shatterObject.tag = "shatter";
    }

    public void WarningRender(bool collided)
    {
        if (collided == true)
        {
            ring_2.GetComponent<MeshRenderer>().material = this.gameObjectPointer.GetComponent<DroneProperties>().warningMaterial;
        }
        else
        {
            ring_2.GetComponent<MeshRenderer>().material = this.gameObjectPointer.GetComponent<DroneProperties>().landingMaterial;
        }
    }

    public float CalAveTime(Vector3[] shelf, float deltaTime)
    {
        // calculate average round trip time for the current drone
        float numFrame = 0;

        foreach (Vector3 shelfGrid in shelf)
        {
            float curDist = 2 * Utility.CalDistance(this.parkingPos, shelfGrid);
            numFrame += curDist / SPEED; 
        }

        return numFrame * deltaTime / shelf.Length;
    }

}
