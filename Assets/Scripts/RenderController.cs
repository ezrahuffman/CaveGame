using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderController : MonoBehaviour
{
    //TODO: change mapMaker to map
    private Map mapMaker;
    public Camera cam;
    Node[][] graph;

    Node[] centerArray;
    Node[] leftCol;
    Node[] rightCol;
    Node[] topRow;
    Node[] botRow;

    float xMax;
    float xMin;
    float yMax;
    float yMin;

    float objectHeight;
    float objectWidth;

    int graphWidth;
    int graphHeight;
    int tilesPerBlock;
    Dictionary<int, Node> shownNodes;


    public float rayLength;

    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        float timer = Time.realtimeSinceStartup;
        

        shownNodes = new Dictionary<int,Node>();


        //instantiate arrays
        centerArray = new Node[1];
        leftCol = new Node[3];
        rightCol = new Node[3];
        topRow = new Node[3];
        botRow = new Node[3];

        Debug.Log(gameObject.GetInstanceID());
        GameObject map_GO = GameObject.Find("map");
        if (!map_GO) { return; }
        mapMaker = map_GO.GetComponent<Map>();
        graph = mapMaker.Graph;
        graphWidth = mapMaker.blockWidth * mapMaker.tilesPerBlock;
        graphHeight = mapMaker.blockHeight * mapMaker.tilesPerBlock;
        tilesPerBlock = mapMaker.tilesPerBlock;


        //set middle block (top left node)

        Node centerNode = mapMaker.centerVertex;
        if(graph == null)
        {
            Debug.Log("there is no graph in renderer");
        }
        int centerI;
        int centerJ;
        if(mapMaker.blockHeight%2 == 0) {
            centerI = centerNode.I;
        }
        else
        {
            centerI = centerNode.I - tilesPerBlock/2; //change this
        }
        if(mapMaker.blockWidth%2 == 0)
        {
            centerJ = centerNode.J;
        }
        else
        {
            centerJ = centerNode.J - tilesPerBlock/2;
        }

        objectWidth = (centerNode.prefab.GetComponent<SpriteRenderer>().sprite.bounds.extents.x * centerNode.prefab.transform.localScale.x) * 2;
        objectHeight = (centerNode.prefab.GetComponent<SpriteRenderer>().sprite.bounds.extents.y * centerNode.prefab.transform.localScale.y) * 2;

        Node centerBlock = graph[centerI][centerJ];
        centerArray[0] = centerBlock;

       

        //set rows and collums
        //TODO: add check for small maps that can all be seen at once

        //left collumn
        leftCol[0] = graph[centerBlock.I + tilesPerBlock][centerBlock.J - tilesPerBlock];
        leftCol[1] = graph[centerBlock.I][centerBlock.J - tilesPerBlock];
        leftCol[2] = graph[centerBlock.I - tilesPerBlock][centerBlock.J - tilesPerBlock];

        //right collumn
        rightCol[0] = graph[centerBlock.I + tilesPerBlock][centerBlock.J + tilesPerBlock];
        rightCol[1] = graph[centerBlock.I][centerBlock.J + tilesPerBlock];
        rightCol[2] = graph[centerBlock.I - tilesPerBlock][centerBlock.J + tilesPerBlock];

        //top row
        topRow[0] = graph[centerBlock.I + tilesPerBlock][centerBlock.J - tilesPerBlock];
        topRow[1] = graph[centerBlock.I + tilesPerBlock][centerBlock.J];
        topRow[2] = graph[centerBlock.I + tilesPerBlock][centerBlock.J + tilesPerBlock];

        //bot row
        botRow[0] = graph[centerBlock.I - tilesPerBlock][centerBlock.J - tilesPerBlock];
        botRow[1] = graph[centerBlock.I - tilesPerBlock][centerBlock.J];
        botRow[2] = graph[centerBlock.I - tilesPerBlock][centerBlock.J + tilesPerBlock];

        //render first 9 blocks of 40X40
        Show(centerArray);
        Show(leftCol);
        Show(rightCol);
        Show(topRow);
        Show(botRow);

        SetMax(centerArray[0]);

        timer = Time.realtimeSinceStartup - timer;
        Debug.Log("Render took: " + timer + "s");
    }

    // Update is called once per frame
    void Update()
    {
        // If there is not a map, don't check for bounds
        if (!mapMaker)
        {
            return;
        }

        //find and check current window bounds against the edges of the current 40X40 block

        //get window width and height
        float windowWidth = cam.orthographicSize * cam.aspect;
        float windowHeight = windowWidth * (1 / cam.aspect);

        //Use window height and width to set visable bounds
        float windowRight = cam.transform.position.x + windowWidth;
        float windowLeft = cam.transform.position.x - windowWidth;
        float windowTop = cam.transform.position.y + windowHeight;
        float windowBot = cam.transform.position.y - windowHeight;



        ////check bounds
        ////Note: this is for debugging only and can be removed whenever

        //Vector3 topLineStart = new Vector3(xMin, yMax, centerArray[0].position.z);
        //Vector3 topLineEnd = new Vector3(xMax, topLineStart.y, topLineStart.z);

        //Debug.DrawLine(topLineStart, topLineEnd, Color.red);

        //Vector3 rightLineStart = new Vector3(xMax, yMax, topLineStart.z);
        //Vector3 rightLineEnd = new Vector3(xMax, yMin, rightLineStart.z);

        //Debug.DrawLine(rightLineStart, rightLineEnd, Color.red);

        //Vector3 botLineStart = new Vector3(xMin, yMin, rightLineEnd.z);
        //Vector3 botLineEnd = new Vector3(xMax, botLineStart.y, botLineStart.z);

        //Debug.DrawLine(botLineStart, botLineEnd, Color.red);

        //Vector3 leftLineStart = new Vector3(xMin, yMin, rightLineEnd.z);
        //Vector3 leftLineEnd = new Vector3(xMin, yMax, rightLineEnd.z);

        //Debug.DrawLine(leftLineStart, leftLineEnd, Color.red);

        //if window right > xMax
        if (windowLeft > xMax && rightCol[0].J < (graphWidth - tilesPerBlock))
        {
            //shift to the right
            ShiftRight();
        }

        //if window left < xMin
        if (windowRight < xMin && leftCol[0].J >= tilesPerBlock)
        {
            //shift to the left
            ShiftLeft();
        }

        //if  window top > yMax
        if (windowBot > yMax && rightCol[0].I < (graphHeight - tilesPerBlock))
        {
            //shift up
            ShiftUp();
            
        }

        //if window bot < yMin
        if (windowTop < yMin && botRow[0].I >= tilesPerBlock)
        {
            //Shift Down
            ShiftDown();
        }


        //Debug.DrawLine(centerArray[0].position, centerArray[0].position + (Vector3.right * xMax), Color.blue);

    }

    private void ShiftRight()
    {
        //remove current left col
        RemoveRow(leftCol);

        //reset left col
        leftCol[0] = topRow[1];
        leftCol[1] = centerArray[0];
        leftCol[2] = botRow[1];

        //reset top row
        topRow[0] = topRow[1];
        topRow[1] = topRow[2];
        topRow[2] = graph[topRow[2].I][topRow[2].J + tilesPerBlock];

        //reset bot row
        botRow[0] = botRow[1];
        botRow[1] = botRow[2];
        botRow[2] = graph[botRow[2].I][botRow[2].J + tilesPerBlock];

        //create new right col
        rightCol[0] = topRow[2];
        rightCol[1] = graph[centerArray[0].I][centerArray[0].J + (2* tilesPerBlock)];
        rightCol[2] = botRow[2];

        //reset center and max values
        centerArray[0] = graph[centerArray[0].I][centerArray[0].J + tilesPerBlock];
        SetMax(centerArray[0]);

        //render right col
        //Debug.Log("show right col");
        Show(rightCol);
    }

    private void ShiftLeft()
    {
        //remove current right col
        RemoveRow(rightCol);

        //reset right col
        rightCol[0] = topRow[1];
        rightCol[1] = centerArray[0];
        rightCol[2] = botRow[1];

        //reset top row
        topRow[2] = topRow[1];
        topRow[1] = topRow[0];
        topRow[0] = graph[topRow[0].I][topRow[0].J - tilesPerBlock];

        //reset bot row
        botRow[2] = botRow[1];
        botRow[1] = botRow[0];
        botRow[0] = graph[botRow[0].I][botRow[0].J - tilesPerBlock];

        //create new left col
        leftCol[0] = topRow[0];
        leftCol[1] = graph[centerArray[0].I][centerArray[0].J - (2 * tilesPerBlock)];
        leftCol[2] = botRow[0];

        //reset center and max values
        centerArray[0] = graph[centerArray[0].I][centerArray[0].J - tilesPerBlock];
        SetMax(centerArray[0]);

        //render left col
        //Debug.Log("show left col");
        Show(leftCol);
    }

    private void ShiftDown()
    {
        //remove current top row
        RemoveRow(topRow);

        //reset left
        leftCol[0] = leftCol[1];
        leftCol[1] = leftCol[2];
        leftCol[2] = graph[leftCol[2].I - tilesPerBlock][leftCol[2].J];

        //reset right
        rightCol[0] = rightCol[1];
        rightCol[1] = rightCol[2];
        rightCol[2] = graph[rightCol[2].I - tilesPerBlock][rightCol[2].J];

        //reset top
        topRow[0] = leftCol[0];
        topRow[1] = graph[leftCol[0].I][leftCol[0].J + tilesPerBlock];
        topRow[2] = rightCol[0];

        //create new bot
        botRow[0] = leftCol[2];
        botRow[1] = graph[leftCol[2].I][leftCol[2].J + tilesPerBlock];
        botRow[2] = rightCol[2];

        //reset center and max values
        centerArray[0] = graph[centerArray[0].I - tilesPerBlock][centerArray[0].J];
        SetMax(centerArray[0]);

        //render bot row
        //Debug.Log("show bot row");
        Show(botRow);
    }

    private void ShiftUp()
    {
        //Remove current bot row
        RemoveRow(botRow);

        //reset right
        rightCol[2] = rightCol[1];
        rightCol[1] = rightCol[0];
        rightCol[0] = graph[rightCol[0].I + tilesPerBlock][rightCol[0].J];

        //reset left
        leftCol[2] = leftCol[1];
        leftCol[1] = leftCol[0];
        leftCol[0] = graph[leftCol[0].I + tilesPerBlock][leftCol[0].J];

        //reset bot
        botRow[0] = leftCol[2];
        botRow[1] = graph[leftCol[2].I][leftCol[2].J + tilesPerBlock];
        botRow[2] = rightCol[2];

        //create top
        topRow[0] = leftCol[0];
        topRow[1] = graph[leftCol[0].I][leftCol[0].J + tilesPerBlock];
        topRow[2] = rightCol[0];

        //reset center and max values
        centerArray[0] = graph[centerArray[0].I + tilesPerBlock][centerArray[0].J];
        SetMax(centerArray[0]);

        //render top row
        //Debug.Log("show top row");
        Show(topRow);
    }

    private void SetMax(Node node)
    { 
        xMax = node.position.x + (tilesPerBlock * objectWidth);
        xMin = node.position.x;
        yMax = node.position.y + (tilesPerBlock * objectHeight);
        yMin = node.position.y;

        //Debug.DrawLine(node.position, node.position + (Vector3.right * xMax), Color.blue);
    }

    private void RemoveRow(Node[] row)
    {
        foreach(Node node in row)
        {
            //all nodes should be parented to corner node, so destroying corner should destroy all nodes
            node.gameObject.transform.parent.gameObject.SetActive(false);
            shownNodes.Remove(node.id);
        }
    }

    private void Show(Node[] nodeArray)
    {
        foreach (Node node in nodeArray)
        {
            if (!shownNodes.ContainsKey(node.id))
            {
                //instantiate each node game object in the associated 40X40 block
                //DrawBlock(node);
                Debug.Log("parent = " + node.gameObject.transform.parent.gameObject.name);
                node.gameObject.transform.parent.gameObject.SetActive(true);


                //Debug.Log($"animation: {}");
                shownNodes.Add(node.id, node);
            }
            else
            {
                //Debug.Log("Already shown");
            }
        }
    }

}
