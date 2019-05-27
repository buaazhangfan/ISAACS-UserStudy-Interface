using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{

    protected Transform xFormCamera;
    protected Transform xFormParent;

    protected Vector3 localRot;
    protected float cameraDistance = 10f;

    public float mouseSensitivity = 4f;
    //public float scrollSensitivity = 2f;
    public float orbitSpeed = 10f;
    //public float scrollSpeed = 6f;
    //public bool cameraDisabled = false;

    // Start is called before the first frame update
    void Start()
    {
        this.xFormCamera = this.transform;
        this.xFormParent = this.transform.parent;

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(1))
        {
            orbitSpeed = 10f;
            transform.RotateAround(xFormParent.position, Vector3.up, Input.GetAxis("Mouse X") * orbitSpeed);
            transform.RotateAround(xFormParent.position, Vector3.up, Input.GetAxis("Mouse Y") * orbitSpeed);
        }

        //if (!Input.GetMouseButton(1))
        //    // transform.RotateAround(xFormParent.position, Vector3.up, orbitSpeed * Time.deltaTime);
        //else
        //{

        //}
        orbitSpeed = 0;
    }

}
