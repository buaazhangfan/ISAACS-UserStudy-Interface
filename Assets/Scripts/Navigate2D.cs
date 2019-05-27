using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigate2D : MonoBehaviour
{

    float moveSpeed = 50.0f; //for move

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveCamera();
    }

    void moveCamera()
    {
        Vector3 p = GetMoveInput();

        p = p * moveSpeed;
        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;
        transform.Translate(p);
    }

    private Vector3 GetMoveInput()
    {
        Vector3 p_Velocity = new Vector3();

        p_Velocity += new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * 10f);

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
            p_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        return p_Velocity;
    }
}
