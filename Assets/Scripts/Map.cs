using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Map : MonoBehaviour
{
    //this needs to store all values needed durring runtime
    //MapMaker shouldn't be storing anything, just making the game
    
    //node array storing the graph
    //if this doesn't work then nodes will actually need "Node" game objects
    public MapMaker.NodeList[] graphArray;
    public Node[][] Graph
    {
        //TODO: add check to see if node array alreadyh exists so it doesn't need to be made every time
        get
        {
            int height = graphArray.Length;
            int width = graphArray[0].nodes.Length;
            //convert back to 2D array from NodeList objects
            Node[][] nodeArray = new Node[height][];
            for (int i = 0; i < height; i++)
            {
                nodeArray[i] = new Node[width];
                nodeArray[i] = graphArray[i].nodes;
            }
            return nodeArray;
        }
    }

    //node storing the center vertex
    public Node centerVertex;

    public int blockWidth;
    public int blockHeight;
    public int tilesPerBlock;
    public float vertexWidth;

}
