using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction2D : MonoBehaviour
{
    public Camera camera;
    public Ray ray;
    public RaycastHit hit;
    private LineRenderer laserLine;
    private float range;
    private int layerMask;
    private GameObject prevDrone;
    private GameObject prevRing;
    private GameObject curDrone;
    private GameObject curRing;


    // Start is called before the first frame update
    void Start()
    {
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = true;
        range = 100.0f;
        layerMask = 1 << 2;

        prevDrone = null;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 vector = new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane);
        Vector3 mousePos = camera.ScreenToWorldPoint(vector);

        ray.origin = mousePos;
        ray.direction = (mousePos - cameraPos).normalized;
        //Debug.Log(ray.origin);
        //Debug.Log(ray.direction);

        laserLine.enabled = false;
        laserLine.SetPosition(0, ray.origin);
        laserLine.SetPosition(1, ray.origin + ray.direction * range);

        // check hit
        if (Physics.Raycast(ray, out hit, 100, layerMask))
        {
            if (hit.collider != null && hit.collider.gameObject.name.Contains("Drone"))
            {
                curDrone = hit.collider.gameObject;

                // check mouse click
                if (Input.GetMouseButtonDown(0))
                {
                    if (!curDrone.GetComponent<DroneProperties>().classPointer.isPaused)
                    {
                        curDrone.GetComponent<DroneProperties>().classPointer.SetDronePause();
                    }
                    else
                    {
                        curDrone.GetComponent<DroneProperties>().classPointer.SetDroneRestart();
                    }
                }

                if (prevDrone == null)
                {
                    prevDrone = curDrone;
                }

                if (curDrone != prevDrone)
                {
                    // set previous drone game object to original
                    prevDrone.GetComponent<DroneProperties>().classPointer.DroneCollideRender(false);
                    prevDrone = curDrone;
                }
                //Debug.Log(curDrone.GetComponent<MeshRenderer>().material.name);
                curDrone.GetComponent<DroneProperties>().classPointer.DroneCollideRender(true);

            }
        }

        else
        {
            if (curDrone != null)
                curDrone.GetComponent<DroneProperties>().classPointer.DroneCollideRender(false);
        }
    }


}
