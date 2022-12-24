//using UnityEngine;
//using System.Linq;
//using System.Collections.Generic;

//public class Temp_MapSmoothing : MonoBehaviour
//{
//    public SquareGrid squareGrid;
//    float _squareSize;
//    List<Vector3> vertices;
//    List<int> triangles;

//    void TriangulateSquare(Square square)
//    {
//        switch (square.configuration)
//        {
//            case 0:
//                break;

//            // 1 points:
//            case 1:
//                MeshFromPoints(square.centerBot, square.botLeft, square.centerLeft);
//                break;
//            case 2:
//                MeshFromPoints(square.centerRight, square.botRight, square.centerBot);
//                break;
//            case 4:
//                MeshFromPoints(square.centerTop, square.topRight, square.centerRight);
//                break;
//            case 8:
//                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
//                break;

//            // 2 points:
//            case 3:
//                MeshFromPoints(square.centerRight, square.botRight, square.botLeft, square.centerLeft);
//                break;
//            case 6:
//                MeshFromPoints(square.centerTop, square.topRight, square.botRight, square.centerBot);
//                break;
//            case 9:
//                MeshFromPoints(square.topLeft, square.centerTop, square.centerBot, square.botLeft);
//                break;
//            case 12:
//                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
//                break;
//            case 5:
//                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBot, square.botLeft, square.centerLeft);
//                break;
//            case 10:
//                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.botRight, square.centerBot, square.centerLeft);
//                break;

//            // 3 point:
//            case 7:
//                MeshFromPoints(square.centerTop, square.topRight, square.botRight, square.botLeft, square.centerLeft);
//                break;
//            case 11:
//                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.botRight, square.botLeft);
//                break;
//            case 13:
//                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBot, square.botLeft);
//                break;
//            case 14:
//                MeshFromPoints(square.topLeft, square.topRight, square.botRight, square.centerBot, square.centerLeft);
//                break;

//            // 4 point:
//            case 15:
//                MeshFromPoints(square.topLeft, square.topRight, square.botRight, square.botLeft);
//                break;
//        }
//    }

//    void MeshFromPoints(params Node[] points)
//    {
//        AssignVertices(points);

//        if (points.Length >= 3)
//        {
//            CreateTriangle(points[0], points[1], points[2]);
//        }
//        if (points.Length >= 4)
//        {
//            CreateTriangle(points[0], points[2], points[3]);
//        }
//        if (points.Length >= 5)
//        {
//            CreateTriangle(points[0], points[3], points[4]);
//        }
//        if (points.Length == 6)
//        {
//            CreateTriangle(points[0], points[4], points[5]);
//        }
//    }

//    void AssignVertices(Node[] points)
//    {
//        foreach(var point in points)
//        {
            
//            if(point.vertexIndex == -1)
//            {
//                // Vertex index has not been set
//                point.vertexIndex = vertices.Count;
//                vertices.Add(point.pos);
//            }
//        }
//    }

//    void CreateTriangle(Node a, Node b, Node c)
//    {
//        triangles.Add(a.vertexIndex);
//        triangles.Add(b.vertexIndex);
//        triangles.Add(c.vertexIndex);
//    }

//    public void GenerateMesh(Map map)
//    {
//        squareGrid = new SquareGrid(map, map.vertexWidth);
//        _squareSize = map.vertexWidth;
//        vertices = new List<Vector3>();
//        triangles = new List<int>();

//        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
//        {
//            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
//            {
//                TriangulateSquare(squareGrid.squares[x, y]);
//            }
//        }

//        Mesh mesh = new Mesh();
//        GetComponent<MeshFilter>().mesh = mesh;
//        mesh.vertices = vertices.ToArray();
//        triangles.Reverse();
//        mesh.triangles = triangles.ToArray();
//        mesh.RecalculateNormals();
//        UpdatePolygonCollider2D(GetComponent<MeshFilter>());

        
//    }

//    public static void UpdatePolygonCollider2D(MeshFilter meshFilter)
//    {
//        if (meshFilter.sharedMesh == null)
//        {
//            Debug.LogWarning(meshFilter.gameObject.name + " has no Mesh set on its MeshFilter component!");
//            return;
//        }

//        PolygonCollider2D polygonCollider2D = meshFilter.GetComponent<PolygonCollider2D>();
//        polygonCollider2D.pathCount = 1;

//        List<Vector3> vertices = new List<Vector3>();
//        meshFilter.sharedMesh.GetVertices(vertices);

//        var boundaryPath = EdgeHelpers.GetEdges(meshFilter.sharedMesh.triangles).FindBoundary().SortEdges();

//        Vector3[] yourVectors = new Vector3[boundaryPath.Count];
//        for (int i = 0; i < boundaryPath.Count; i++)
//        {
//            yourVectors[i] = vertices[boundaryPath[i].v1];
//        }
//        List<Vector2> newColliderVertices = new List<Vector2>();

//        for (int i = 0; i < yourVectors.Length; i++)
//        {
//            newColliderVertices.Add(new Vector2(yourVectors[i].x, yourVectors[i].y));
//        }

//        Vector2[] newPoints = newColliderVertices.Distinct().ToArray();

//        //EditorUtility.SetDirty(polygonCollider2D);

//        polygonCollider2D.SetPath(0, newPoints);
//        Debug.Log(meshFilter.gameObject.name + " PolygonCollider2D updated.");
//    }



//    public class Node
//    {
//        public Vector3 pos;
//        public int vertexIndex = -1;

//        public Node(Vector3 pos)
//        {
//            this.pos = pos;
//        }
//    }

//    public class ControlNode : Node
//    {
//        public bool active;
//        public Node above, right;

//        public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos)
//        {
//            this.active = active;
//            above = new Node(pos + Vector3.up * squareSize / 2);
//            right = new Node(pos + Vector3.right * squareSize / 2);

//        }
//    }

//    public class Square
//    {
//        public ControlNode topLeft, topRight, botRight, botLeft;
//        public Node centerTop, centerRight, centerBot, centerLeft;
//        public int configuration; //possible 16 ways to arrange the square 

//        public Square(ControlNode topLeft, ControlNode topRight, ControlNode botRight, ControlNode botLeft)
//        {
//            this.topLeft = topLeft;
//            this.topRight = topRight;
//            this.botRight = botRight;
//            this.botLeft = botLeft;

//            centerTop = topLeft.right;
//            centerBot = botLeft.right;
//            centerRight = botRight.above;
//            centerLeft = botLeft.above;


//            //TODO: there is for sure a cleaner way to do this
//            if (topLeft.active)
//            {
//                configuration += 8;
//            }
//            if (topRight.active)
//            {
//                configuration += 4;
//            }
//            if (botRight.active)
//            {
//                configuration += 2;
//            }
//            if (botLeft.active)
//            {
//                configuration += 1;
//            }
//        }
//    }

//    public class SquareGrid
//    {
//        public Map map;
//        public Square[,] squares;

//        public SquareGrid(Map map, float squareSize)
//        {
//            this.map = map;
//            int nodeCountX = map.graphArray.Length;
//            int nodeCountY = map.graphArray[0].nodes.Length;


//            float mapWidth = nodeCountX * squareSize;
//            float mapHeight = nodeCountY * squareSize;

//            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

//            for (int x = 0; x < nodeCountX; x++)
//            {
//                for (int y = 0; y < nodeCountY; y++)
//                {
//                    Vector3 pos = new Vector3(x * squareSize,y * squareSize ,0);
//                    controlNodes[x, y] = new ControlNode(pos, map.graphArray[y].nodes[x].gameObject.CompareTag("Wall"), squareSize);
//                }
//            }

//            //TODO: this might not be the right indices, it might be nodeCountX, nodeCountY
//            squares = new Square[nodeCountX - 1, nodeCountY - 1];
//            for (int x = 0; x < nodeCountX - 1; x++)
//            {
//                for (int y = 0; y < nodeCountY - 1; y++)
//                {
//                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
//                }
//            }
//        }
//    }

//    private void OnDrawGizmos()
//    {
//        if(squareGrid != null)
//        {
//            for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
//            {
//                for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
//                {
//                    Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
//                    Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.pos, Vector3.one * .4f);
//                    Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
//                    Gizmos.DrawCube(squareGrid.squares[x, y].topRight.pos, Vector3.one * .4f);
//                    Gizmos.color = (squareGrid.squares[x, y].botRight.active) ? Color.black : Color.white;
//                    Gizmos.DrawCube(squareGrid.squares[x, y].botRight.pos, Vector3.one * .4f);
//                    Gizmos.color = (squareGrid.squares[x, y].botLeft.active) ? Color.black : Color.white;
//                    Gizmos.DrawCube(squareGrid.squares[x, y].botLeft.pos, Vector3.one * .4f);
//                    Gizmos.color = Color.grey;
//                    Gizmos.DrawCube(squareGrid.squares[x, y].centerTop.pos, Vector3.one * .15f);
//                    Gizmos.DrawCube(squareGrid.squares[x, y].centerRight.pos, Vector3.one * .15f);
//                    Gizmos.DrawCube(squareGrid.squares[x, y].centerBot.pos, Vector3.one * .15f);
//                    Gizmos.DrawCube(squareGrid.squares[x, y].centerLeft.pos, Vector3.one * .15f);

//                }
//            }
//        }
//    }
//}

//    public static class EdgeHelpers
//    {
//        public struct Edge
//        {
//            public int v1;
//            public int v2;
//            public int triangleIndex;
//            public Edge(int aV1, int aV2, int aIndex)
//            {
//                v1 = aV1;
//                v2 = aV2;
//                triangleIndex = aIndex;
//            }
//        }

//        public static List<Edge> GetEdges(int[] aIndices)
//        {
//            List<Edge> result = new List<Edge>();
//            for (int i = 0; i < aIndices.Length; i += 3)
//            {
//                int v1 = aIndices[i];
//                int v2 = aIndices[i + 1];
//                int v3 = aIndices[i + 2];
//                result.Add(new Edge(v1, v2, i));
//                result.Add(new Edge(v2, v3, i));
//                result.Add(new Edge(v3, v1, i));
//            }
//            return result;
//        }

//        public static List<Edge> FindBoundary(this List<Edge> aEdges)
//        {
//            List<Edge> result = new List<Edge>(aEdges);
//            for (int i = result.Count - 1; i > 0; i--)
//            {
//                for (int n = i - 1; n >= 0; n--)
//                {
//                    if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
//                    {
//                        // shared edge so remove both
//                        result.RemoveAt(i);
//                        result.RemoveAt(n);
//                        i--;
//                        break;
//                    }
//                }
//            }
//            return result;
//        }
//        public static List<Edge> SortEdges(this List<Edge> aEdges)
//        {
//            List<Edge> result = new List<Edge>(aEdges);
//            for (int i = 0; i < result.Count - 2; i++)
//            {
//                Edge E = result[i];
//                for (int n = i + 1; n < result.Count; n++)
//                {
//                    Edge a = result[n];
//                    if (E.v2 == a.v1)
//                    {
//                        // in this case they are already in order so just continoue with the next one
//                        if (n == i + 1)
//                            break;
//                        // if we found a match, swap them with the next one after "i"
//                        result[n] = result[i + 1];
//                        result[i + 1] = a;
//                        break;
//                    }
//                }
//            }
//            return result;
//        }
//    }