using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInteraction : MonoBehaviour
{
    //pivot is the center of the scene
    public GameObject pivot;
    //originalScale is the original localScale of the world
    public Vector3 originalScale;
    //This is the 1/10th of the originalScale of the world
    public Vector3 minScale;
    //This is the 10 times the originalScale of the world
    public Vector3 maxScale;
    //currentScale is the current localScale
    public Vector3 currentScale;
    //This tells us if the map is still moving or being dragged
    public enum MapState
    {
        IDLE, DRAGGING, MOVING, ROTATING
    }

    // current world
    public GameObject rotatingTable;

    // Rotation
    public LinkedList<float> angles;
    public bool handleHeldTrigger = false;
    public static MapState mapState;
    public OVRInput.Controller currentController;
    private Vector3 oldVec;
    private float movementAngle;
    public float movementAngleDecay = .95f;
    private float rotSpeed = 0.05f;  // Rotation speed(in rev/s)
    private float scalingSpeed = 0.05f;
    //This is the speed at which the map can be moved at
    public float speed = 3;

    // Start is called before the first frame update
    void Start()
    {
        //Pivot assignment
        pivot = GameObject.FindWithTag("Pivot");
        originalScale = transform.localScale;
        minScale = Vector3.Scale(originalScale, new Vector3(0.1F, 0.1F, 0.1F));
        maxScale = Vector3.Scale(originalScale, new Vector3(10F, 10F, 10F));
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            // SCALE WORLD - if both grip triggers are held
            ScaleWorld();
        }
        else
        {
            ControllerRotateWorld();
        }

        // MOVING WORLD
        MoveWorld();
    }

    private void MoveWorld()
    {
        float moveX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        float moveZ = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

        if (moveX != 0 || moveZ != 0)
        {
            // update map position based on input
            Vector3 position = transform.position;
            position.x -= moveX * speed * Time.deltaTime;
            position.z -= moveZ * speed * Time.deltaTime;
            transform.position = position;
            mapState = MapState.MOVING;
        }
        else if (mapState == MapState.MOVING)
        {
            mapState = MapState.IDLE;
        }


    }


    // Rotate the world based off of the right thumbstick
    private void ControllerRotateWorld()
    {
        float deltaX = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        // We only consider inputs above a certain threshold.
        if (Mathf.Abs(deltaX) > 0.2f)
        {
            mapState = MapState.IDLE; // Controller input overrides manual
            float angle = deltaX * rotSpeed * 360 * Time.fixedDeltaTime;
            Debug.Log("deltaX: " + deltaX);
            Debug.Log("rotSpeed:" + rotSpeed);
            Debug.Log("Time.fixedDeltaTime" + Time.fixedDeltaTime);
            transform.RotateAround(pivot.transform.position, Vector3.up, angle);
            if (rotatingTable)
            {
                rotatingTable.transform.RotateAround(pivot.transform.position, Vector3.up, angle);
            }
            mapState = MapState.ROTATING;
        }
        else if (mapState == MapState.ROTATING)
        {
            mapState = MapState.IDLE;
        }
    }

    private void ScaleWorld()
    {
        //Obtaining distance and velocity
        Vector3 d = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch) - OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Vector3 v = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch) - OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);

        //Calculating Scaling Vector
        float result = Vector3.Dot(v, d);

        //Adjusting result to slow scaling
        // float final_result = 1.0F + scalingSpeed * result;
        float final_result = 1.0F - scalingSpeed * result;

        Vector3 scalingFactor = Vector3.Scale(transform.localScale, new Vector3(final_result, final_result, final_result));

        //Checking Scaling Bounds
        if (scalingFactor.sqrMagnitude > minScale.sqrMagnitude && scalingFactor.sqrMagnitude < maxScale.sqrMagnitude)
        {
            Vector3 A = transform.position;
            Vector3 B = pivot.transform.position;
            B.y = A.y;

            Vector3 startScale = transform.localScale;
            Vector3 endScale = transform.localScale * final_result;

            Vector3 C = A - B; // diff from object pivot to desired pivot/origin

            // calc final position post-scale
            Vector3 FinalPosition = (C * final_result) + B;

            // finally, actually perform the scale/translation
            transform.localScale = endScale;
            transform.position = FinalPosition;
        }
    }
}
