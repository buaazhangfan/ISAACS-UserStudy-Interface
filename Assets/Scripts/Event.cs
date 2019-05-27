using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event
{
    public int shelfId;
    public Vector3 pos;
    //public TrafficControl trafficControl;
    public GameObject gameObjectPointer;

    public Event(int shelfId, Vector3 pos)
    {
        this.shelfId = shelfId;
        this.pos = pos;
        // GameObject baseObject = GameObject.FindGameObjectWithTag("eventBase");
        GameObject baseObject = TrafficControl.worldobject.GetComponent<TrafficControl>().eventBaseObject;
        gameObjectPointer = Object.Instantiate(baseObject, pos, Quaternion.identity);
        gameObjectPointer.name = string.Concat("Event", shelfId.ToString());
        // gameObjectPointer.gameObject.tag = string.Concat("Event", shelfId.ToString());
        gameObjectPointer.transform.parent = TrafficControl.worldobject.transform;
    }

    public void markEvent(Material material){
        int idx = this.shelfId;
        GameObject curEvent = GameObject.Find(string.Concat("Event", idx.ToString()));
        // Debug.Log("Change color of event " + curEvent.name + " to " + material.name);
        // Material newMat = Resources.Load("M_bear", typeof(Material)) as Material;
        curEvent.GetComponent<Renderer>().material = material;
    }

    //public void markEvent(Color color){
    //    int idx = this.shelfId;
    //    GameObject curEvent = GameObject.Find(string.Concat("Event", idx.ToString()));
    //    Debug.Log("Current evnet is:" + curEvent.name);
    //    curEvent.GetComponent<Renderer>().material.color = color;
    //}

}

