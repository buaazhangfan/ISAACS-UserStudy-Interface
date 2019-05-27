using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigate3DVR : MonoBehaviour
{
    //This is the speed at which the map can be moved at
    public float speed = 1000;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //MoveCamera();
    }

    private void MoveCamera()
    {
        float moveX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        float moveZ = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        float moveY = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        Vector3 position = transform.position;
        if (moveX != 0 || moveZ != 0)
        {
            // update map position based on input
            position.x -= moveX * speed * Time.deltaTime;
            position.z -= moveZ * speed * Time.deltaTime;
            transform.position = position;
        }

        if (moveY != 0)
        {
            position.y -= moveY * speed * Time.deltaTime;
            transform.position = position;
        }
    }
}
