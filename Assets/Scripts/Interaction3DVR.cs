using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.UnityEventHelper;
using VRTK;

public class Interaction3DVR : MonoBehaviour
{
    private GameObject curDrone;
    private GameObject prevDrone;
    public GameObject controllerRight;

    // Bit shift the index of the layer (8) to get a bit mask
    private int layerMask = 1 << 8;

    // Start is called before the first frame update
    void Start()
    {
        controllerRight = GameObject.FindGameObjectWithTag("controller_right");
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(controllerPos);

        RaycastHit destinationHit = controllerRight.GetComponent<VRTK_StraightPointerRenderer>().GetDestinationHit();
        if (controllerRight.GetComponent<VRTK_Pointer>().IsStateValid() && destinationHit.collider != null)
        {
            if (destinationHit.collider.gameObject.name.Contains("warningSphere"))
            {
                curDrone = destinationHit.collider.gameObject.transform.parent.gameObject;

                if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
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
