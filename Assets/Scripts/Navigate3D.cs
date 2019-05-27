using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigate3D : MonoBehaviour
{

    float rotSpeed = 1.0f; //for rotation
    public float dragSpeed = 2;
    private Vector3 dragOrigin;

    float xValue;
    float xValueMinMax = 5.0f;
    float yValue;
    float yValueMinMax = 5.0f;
    float cameraSpeed = 10.0f;// Greater the lower speed
    Vector3 accelometerSmoothValue;

    float moveSpeed = 50.0f; //for move
    float camSens = 0.1f; //How sensitive it with mouse

    // Use this for initialization
    void Start()
    {
        //Get local rotation
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

    }

    // Update is called once per frame
    void Update()
    {
        //// Rotate
        //rotCamera(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), rotSpeed);

        // Move
        moveCamera();
    }

    /////////////**** Accelerometer Start****////////////////////////////////////////////////////////


    void cameraRotationAccelerometer()
    {
        //Set X Min Max
        if (xValue < -xValueMinMax)
            xValue = -xValueMinMax;

        if (xValue > xValueMinMax)
            xValue = xValueMinMax;

        //Set Y Min Max
        if (yValue < -yValueMinMax)
            yValue = -yValueMinMax;

        if (yValue > yValueMinMax)
            yValue = yValueMinMax;

        accelometerSmoothValue = lowpass();

        xValue += accelometerSmoothValue.x;
        yValue += accelometerSmoothValue.y;

        transform.rotation = new Quaternion(yValue, xValue, 0, cameraSpeed);
    }

    //Smooth Accelerometer
    public float AccelerometerUpdateInterval = 1.0f / 100.0f;
    public float LowPassKernelWidthInSeconds = 0.001f;
    public Vector3 lowPassValue = Vector3.zero;
    Vector3 lowpass()
    {
        float LowPassFilterFactor = AccelerometerUpdateInterval / LowPassKernelWidthInSeconds;//tweakable
        lowPassValue = Vector3.Lerp(lowPassValue, Input.acceleration, LowPassFilterFactor);
        return lowPassValue;
    }
    /////////////**** Accelerometer End****////////////////////////////////////////////////////////

    //Rot Parameters
    float mouseX;
    float mouseY;
    Quaternion localRotation;
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    void rotCamera(float horizontal, float verticle, float moveSpeed)
    {
        mouseX = horizontal;
        mouseY = -verticle;

        rotY += mouseX * moveSpeed;
        rotX += mouseY * moveSpeed;

        localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }

    // Move parameters
    void moveCamera(){
        Vector3 p = GetMoveInput();
        // Debug.Log("Camera" + p);
        p = p * moveSpeed;
        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;
        transform.Translate(p);
    }

    

    private Vector3 GetMoveInput()
    { 
        Vector3 p_Velocity = new Vector3();
        //if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        //{
        //    p_Velocity += new Vector3(0, 0, 1);
        //}
        //if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.S))
        //{
        //    p_Velocity += new Vector3(0, 0, -1);
        //}
        p_Velocity += new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * 5f);

        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        return p_Velocity;
    }
}
