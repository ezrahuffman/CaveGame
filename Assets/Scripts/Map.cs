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
    public Mesh mesh;

    public void Start()
    {
        //int 
        gameObject.AddComponent<SpriteRenderer>();
        //gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(new Texture2D((int)mesh.bounds.size.x, (int)mesh.bounds.size.y), new Rect(centerVertex.position, mesh.bounds.size), centerVertex.position);
        var texture2D = new Texture2D((int)mesh.bounds.size.x, (int)mesh.bounds.size.y);
        gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 1);
        ConvertMeshToSprite(mesh);
    }

    void ConvertMeshToSprite(Mesh mesh)
    {
        //mesh = bottom.GetComponent<MeshFilter>().mesh;

        Sprite sprite1 = GetComponent<SpriteRenderer>().sprite;

        Vector2[] copiedVerticies = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            copiedVerticies[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
        }
      
        for (int i = 0; i < copiedVerticies.Length; ++i)
        {
            copiedVerticies[i] = (copiedVerticies[i] * sprite1.pixelsPerUnit) + sprite1.pivot;
        }
        ushort[] copiedTriangels = new ushort[mesh.triangles.Length];
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            copiedTriangels[i] = (ushort)mesh.triangles[i];
        }
        sprite1.OverrideGeometry(copiedVerticies, copiedTriangels);
    }

}
