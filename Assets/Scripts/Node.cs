using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    [SerializeField]
    int i;
    [SerializeField]
    int j;

    public Dictionary <int, Node> neighbors;        //list of neighbors

    public GameObject gameObject;       //game object of what this node is (ie. floor/wall)
    public GameObject prefab;

    public int objectId;

    public Color color;

    public Vector3 position;

    public bool visited = false;        //whether or not the node has been visited
    public bool edge = false;           //if the node is an edge
    public bool room = false;

    public int id;

    public int weight = 0;              //weight of the node

    //coordinates of node
    float x;
    float y;

    //Neighboring Nodes
    public Node left;  
    public Node right;
    public Node up;
    public Node down;
    private NodeType type;
    

    public Node(float x, float y, int i, int j)
    {
        this.i = i;
        this.j = j;

        neighbors = new Dictionary<int, Node>();

        this.x = x;
        this.y = y;

        left = null;
        right = null;
        up = null;
        down = null;
    }

    public void Collapse()
    {
        if (!edge)
        {

            //remove this node from graph vertically
            down.up = this.up;
            down.RemoveNeighbor(this);
            down.AddNeighbor(up);

            up.down = this.down;
            up.RemoveNeighbor(this);
            up.AddNeighbor(down);

            //remove this node from the graph horizontally
            right.left = this.left;
            right.RemoveNeighbor(this);
            right.AddNeighbor(left);

            left.right = this.right;
            left.RemoveNeighbor(this);
            left.AddNeighbor(right);
        }
    }
    
    public void AddNeighbor(Node node)
    {
        //if node is not already a neighbor, add it
        if (!neighbors.ContainsKey(node.id))
        {
            neighbors.Add(node.id, node);
        }

    }

    public void RemoveNeighbor(Node node)
    {
        //if node is a neighbor, remove it
        if (neighbors.ContainsKey(node.id))
        {
            neighbors.Remove(node.id);
        }
    }

    ~Node()
    {

    }


    //x coordinate of pos
    public float X
    {
        get
        {
            return x;
        }
    }

    //y coordinate
    public float Y
    {
        get
        {
            return y;
        }
    }

    //I index
    public int I
    {
        get
        {
            return i;
        }
    }

    //J index
    public int J
    {
        get
        {
            return j;
        }
    }

    public bool CheckNode
    {
        get
        {
            //check every other node (this should be generalized to block height and block width)
            if (i % 2 != 0 && j % 2 != 0 && !edge)
            {
                return true;
            }

            return false;
        }
    }

    public NodeType Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;
        }
    }
}

public enum NodeType
{
    floor, wall, end
}
