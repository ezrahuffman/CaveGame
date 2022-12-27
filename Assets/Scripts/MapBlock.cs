using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Experimental.Rendering.Universal;

public class MapBlock : MonoBehaviour
{
    public SquareGrid squareGrid;
    float _squareSize;
    List<Vector3> vertices;
    List<int> triangles;
    public GameObject parentGO;
    List<Vector3> VerticiesCollide;
    SpriteRenderer m_Sr;

    private static BindingFlags accessFlagsPrivate =
        BindingFlags.NonPublic | BindingFlags.Instance;

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.centerBot, square.botLeft, square.centerLeft);
                break;
            case 2:
                MeshFromPoints(square.centerRight, square.botRight, square.centerBot);
                break;
            case 4:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.centerRight, square.botRight, square.botLeft, square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop, square.topRight, square.botRight, square.centerBot);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerBot, square.botLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            case 5:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBot, square.botLeft, square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.botRight, square.centerBot, square.centerLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.centerTop, square.topRight, square.botRight, square.botLeft, square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.botRight, square.botLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBot, square.botLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.botRight, square.centerBot, square.centerLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.botRight, square.botLeft);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length == 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        foreach (var point in points)
        {

            if (point.vertexIndex == -1)
            {
                // Vertex index has not been set
                point.vertexIndex = vertices.Count;
                vertices.Add(point.pos);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
    }

    public void GenerateMesh(Map map)
    {
        squareGrid = new SquareGrid(map, map.vertexWidth, parentGO);
        _squareSize = map.vertexWidth;
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        if (parentGO.GetComponent<MeshFilter>() == null)
        {

            Mesh mesh = new Mesh();
            //parentGO.AddComponent<MeshFilter>();
            //parentGO.AddComponent<MeshRenderer>();
            //parentGO.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = vertices.ToArray();
            triangles.Reverse();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            PolygonCollider2D polygonCollider2D = parentGO.AddComponent<PolygonCollider2D>();

            MakeCollisions(mesh, polygonCollider2D);
            map.mesh = mesh;

            
        }


    }

    

    #region Generate Collider
    void MakeCollisions(Mesh mesh, PolygonCollider2D PC2D)
    {
        VerticiesCollide = mesh.vertices.ToList();
        //List<Edge> edges = new List<Edge>();
        //List<Edge> outsideEdges = new List<Edge>();
        //List<List<Edge>> sortedOutsideEdges = new List<List<Edge>>();
        List<Edge> edges = GetEdges(mesh.triangles);
        List<Edge> outsideEdges = FindBoundary(edges, mesh.vertices);

        List<List<Edge>> sortedOutsideEdges = SortEdges(outsideEdges);
        print(sortedOutsideEdges.Count);
        Vector2[][] collisionPoints = convertEdgePathsToVector2Arrays(sortedOutsideEdges);
        PC2D.pathCount = collisionPoints.Length;

        for (int i = 0; i < collisionPoints.Length; i++)
        {
            PC2D.SetPath(i, collisionPoints[i]);
        }


    }

    public struct Edge
    {
        public int v1;
        public int v2;
        public int triangleIndex;
        public Edge(int aV1, int aV2, int aIndex)
        {
            v1 = aV1;
            v2 = aV2;
            triangleIndex = aIndex;
        }
    }

    public static List<Edge> GetEdges(int[] aIndices)
    {
        List<Edge> result = new List<Edge>();
        for (int i = 0; i < aIndices.Length; i += 3)
        {
            int v1 = aIndices[i];
            int v2 = aIndices[i + 1];
            int v3 = aIndices[i + 2];
            result.Add(new Edge(v1, v2, i));
            result.Add(new Edge(v2, v3, i));
            result.Add(new Edge(v3, v1, i));
        }
        return result;
    }

    public static List<Edge> FindBoundary(List<Edge> aEdges, Vector3[] verts)
    {
        List<Edge> result = new List<Edge>(aEdges);
        for (int i = result.Count - 1; i > 0; i--)
        {
            for (int n = i - 1; n >= 0; n--)
            {
                if (verts[result[i].v1] == verts[result[n].v2] && verts[result[i].v2] == verts[result[n].v1])
                {
                    result.RemoveAt(i);
                    result.RemoveAt(n);
                    i--;
                    break;
                }
            }
        }
        return result;
    }

    public List<List<Edge>> SortEdges(List<Edge> bEdges)
    {
        List<Edge> borderEdges = bEdges;

        List<List<Edge>> paths = new List<List<Edge>>();
        List<Edge> currentPath = new List<Edge>();

        while (borderEdges.Count > 0)
        {
            currentPath.Add(borderEdges[0]);
            borderEdges.RemoveAt(0);
            makePath(currentPath, borderEdges);
            paths.Add(currentPath);
            currentPath = new List<Edge>();
            print(borderEdges.Count);
        }
        return paths;
    }

    public void makePath(List<Edge> currentPath, List<Edge> borderEdges)
    {
        bool continuePath = true;

        List<Vector3> verts = VerticiesCollide;

        while (continuePath)
        {
            for (int i = 0; i < borderEdges.Count; i++)
            {
                if (verts[currentPath[currentPath.Count - 1].v2] == verts[borderEdges[i].v1] || verts[currentPath[currentPath.Count - 1].v1] == verts[borderEdges[i].v2])
                {
                    currentPath.Add(borderEdges[i]);
                    borderEdges.RemoveAt(i);
                    continuePath = true;
                    break;
                }
                else
                {
                    continuePath = false;
                }
            }
            if (borderEdges.Count == 0)
            {
                continuePath = false;
            }
        }
    }
    
    private static FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
    private static FieldInfo meshHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", accessFlagsPrivate);

    public Vector2[][] convertEdgePathsToVector2Arrays(List<List<Edge>> edgePaths)
    {
        Vector2[][] collisionPoints = new Vector2[edgePaths.Count][];
        

        for (int i = 0; i < edgePaths.Count; i++)
        {
            collisionPoints[i] = new Vector2[edgePaths[i].Count];
            GameObject gO = new GameObject("Shadow_" + i);
            gO.AddComponent<PolygonCollider2D>();
            List<Vector3> verts = new List<Vector3>();
            for (int j = 0; j < edgePaths[i].Count; j++)
            {
                collisionPoints[i][j] = VerticiesCollide[edgePaths[i][j].v1];
                verts.Add(collisionPoints[i][j]);
            }
            gO.GetComponent<PolygonCollider2D>().pathCount = 1;
            gO.GetComponent<PolygonCollider2D>().SetPath(0, collisionPoints[i]);
            
            //This is where the actual shadows are modified
            //TODO: need to make the cave walls into better shadow shapes, not sure about the best way to do this.
            var shadow = gO.AddComponent<ShadowCaster2D>();
            shadow.selfShadows = true;
            shapePathField.SetValue(shadow, verts.ToArray());
            // invalidate the hash so it re-generates the shadow mesh on the next Update()
            meshHashField.SetValue(shadow, -1);
        }
        return collisionPoints;
    }
    #endregion


    public class Node
    {
        public Vector3 pos;
        public int vertexIndex = -1;

        public Node(Vector3 pos)
        {
            this.pos = pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos)
        {
            this.active = active;
            above = new Node(pos + Vector3.up * squareSize / 2);
            right = new Node(pos + Vector3.right * squareSize / 2);

        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, botRight, botLeft;
        public Node centerTop, centerRight, centerBot, centerLeft;
        public int configuration; //possible 16 ways to arrange the square 

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode botRight, ControlNode botLeft)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.botRight = botRight;
            this.botLeft = botLeft;

            centerTop = topLeft.right;
            centerBot = botLeft.right;
            centerRight = botRight.above;
            centerLeft = botLeft.above;


            //TODO: there is for sure a cleaner way to do this
            if (topLeft.active)
            {
                configuration += 8;
            }
            if (topRight.active)
            {
                configuration += 4;
            }
            if (botRight.active)
            {
                configuration += 2;
            }
            if (botLeft.active)
            {
                configuration += 1;
            }
        }
    }

    public class SquareGrid
    {
        public Map map;
        public Square[,] squares;
        GameObject _parentGO;

        public SquareGrid(Map map, float squareSize, GameObject parentGO)
        {
            this.map = map;
            _parentGO = parentGO;
            int nodeCountX = map.graphArray.Length;
            int nodeCountY = map.graphArray[0].nodes.Length;

            


            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(x * squareSize, y * squareSize, 0); 
                    controlNodes[x, y] = new ControlNode(pos, map.graphArray[y].nodes[x].Type == NodeType.wall, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (squareGrid != null)
    //    {
    //        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
    //        {
    //            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
    //            {
    //                Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.pos, Vector3.one * .4f);
    //                Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].topRight.pos, Vector3.one * .4f);
    //                Gizmos.color = (squareGrid.squares[x, y].botRight.active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].botRight.pos, Vector3.one * .4f);
    //                Gizmos.color = (squareGrid.squares[x, y].botLeft.active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].botLeft.pos, Vector3.one * .4f);
    //                Gizmos.color = Color.grey;
    //                Gizmos.DrawCube(squareGrid.squares[x, y].centerTop.pos, Vector3.one * .15f);
    //                Gizmos.DrawCube(squareGrid.squares[x, y].centerRight.pos, Vector3.one * .15f);
    //                Gizmos.DrawCube(squareGrid.squares[x, y].centerBot.pos, Vector3.one * .15f);
    //                Gizmos.DrawCube(squareGrid.squares[x, y].centerLeft.pos, Vector3.one * .15f);

    //            }
    //        }
    //    }
    //}
}
