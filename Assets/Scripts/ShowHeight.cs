using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHeight : MonoBehaviour
{
    private Text text;
    private Vector3 refPos;
    private RectTransform rectTransform;
    private GameObject sphere;
    private GameObject textGO;
    private Vector3 ParkingPos;
    private float heightOffset = 7.914f; 

    // Start is called before the first frame update
    void Start()
    {
        Font arial;
        arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        ParkingPos = this.GetComponent<DroneProperties>().classPointer.parkingPos;

        GameObject canvasGO = new GameObject();
        canvasGO.name = "Canvas" + this.name;
        canvasGO.AddComponent<Canvas>();
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        Canvas canvas;
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        textGO = new GameObject();
        textGO.name = "text" + this.name; 
        textGO.transform.parent = canvasGO.transform;
        textGO.AddComponent<Text>();

        // Set Text component properties.
        text = textGO.GetComponent<Text>();
        text.font = arial;
        text.text = "Height";
        text.fontStyle = FontStyle.Bold;
        text.fontSize = 10;
        text.alignment = TextAnchor.MiddleCenter;
        rectTransform = text.GetComponent<RectTransform>();
        sphere = this.transform.Find("Height").gameObject;
        refPos = Camera.main.WorldToScreenPoint(sphere.transform.position);
        rectTransform.position = refPos;
        textGO.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.position == ParkingPos)
        {
            textGO.SetActive(false);
            return;
        }

        textGO.SetActive(true);
        text.text = (this.transform.position.y - heightOffset).ToString("F2");
        refPos = Camera.main.WorldToScreenPoint(sphere.transform.position);
        rectTransform.position = refPos;

    }
}
