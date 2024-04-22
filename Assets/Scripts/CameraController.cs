using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject playerObject;

    private Vector2 position;

    Camera cam;

    float xMax, xMin, yMax, yMin;

    float windowWidth;
    float windowHeight;

    Map mapMaker;
    Node[][] graph;
    int graphWidth;
    int graphHeight;
    int tilesPerBlock;

    public void Start()
    {
        cam = GetComponent<Camera>();

        //get window width and height
        windowWidth = cam.orthographicSize * cam.aspect;
        windowHeight = windowWidth * (1 / cam.aspect);

        // Test for map because some test seens don't have a map
        GameObject map_GO = GameObject.Find("map");
        if (map_GO)
        {
            mapMaker = map_GO.GetComponent<Map>();
            graph = mapMaker.Graph;
            tilesPerBlock = mapMaker.tilesPerBlock;
            graphWidth = mapMaker.blockWidth * tilesPerBlock;
            graphHeight = mapMaker.blockHeight * tilesPerBlock;
        }


        SetMax();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (playerObject != null)
        {
            //camera transform = player transform
            position = playerObject.transform.position;

            UpdatePosition(position);
            //transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
        else
        {
            //find player
            playerObject = GameObject.FindGameObjectWithTag("Player");

            Debug.Log($"playerobj: {playerObject.name}");

        }
    }

    public GameObject PlayerObj{
        set{ 
            playerObject = value;
        }
    }

    public void SetMax()
    {
        // If there is no map, don't limit the camera 
        if (!mapMaker)
        {
            xMax = float.MaxValue;
            xMin = float.MinValue;
            yMax = xMax;
            yMin = xMin;
            return;
        }


        Node centerNode = mapMaker.centerVertex;
        float objectWidth = (centerNode.prefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x * centerNode.prefab.transform.localScale.x);

        xMax = (graphHeight * objectWidth) - objectWidth/2;
        xMin = -(objectWidth/2);
        yMax = xMax;
        yMin = xMin;
        //xMax = xMax - (1/2 * WindowSize.x)
        xMax -= windowWidth;

        //xMin = xMin + (1/2 * WindowSize.x)
        xMin += windowWidth;


        //yMax = yMax - (1/2 * WindowSize.y)
        yMax -= windowHeight;

        //yMin = yMin + (1/2 * WindowSize.y)
        yMin += windowHeight;
    }

    void UpdatePosition(Vector3 playerPos)
    {
        if (playerPos.x < xMin)
        {
            playerPos.x = xMin;
        }
        if (playerPos.x > xMax)
        {
            playerPos.x = xMax;
        }
        if (playerPos.y < yMin)
        {
            playerPos.y = yMin;
        }
        if (playerPos.y > yMax)
        {
            playerPos.y = yMax;
        }

        transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
        Debug.Log($"playerpos = {playerPos}");
    }
}
