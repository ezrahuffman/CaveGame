using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;
using Unity.Mathematics;

public class Block
{
    public List<int> triangles;
    public List<Vector3> vertices;
    public Node node { get; private set; }
    public Block(Node node) {
        triangles = new List<int>();
        vertices = new List<Vector3>();
        this.node = node;
    }
}

public class MapBlock : MonoBehaviour
{
    public SquareGrid squareGrid;
    float _squareSize;
    List<Vector3> vertices;
    List<int> triangles;
    public GameObject parentGO;
    List<Vector3> VerticiesCollide;
    SpriteRenderer m_Sr;
    LayerMask textureMask = LayerMask.NameToLayer("TextureLayer"); // this should only be used for rendering the mesh texture, not anything else
    public Material mapMaterial; 

    private static BindingFlags accessFlagsPrivate =
        BindingFlags.NonPublic | BindingFlags.Instance;

    private static int _subdivsions = 2;

    void TriangulateSquare(Square square,Block block)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(block, square.centerBot, square.botLeft, square.centerLeft);
                break;
            case 2:
                MeshFromPoints(block, square.centerRight, square.botRight, square.centerBot);
                break;
            case 4:
                MeshFromPoints(block, square.centerTop, square.topRight, square.centerRight);
                break;
            case 8:
                MeshFromPoints(block, square.topLeft, square.centerTop, square.centerLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(block, square.centerRight, square.botRight, square.botLeft, square.centerLeft);
                break;
            case 6:
                MeshFromPoints(block, square.centerTop, square.topRight, square.botRight, square.centerBot);
                break;
            case 9:
                MeshFromPoints(block, square.topLeft, square.centerTop, square.centerBot, square.botLeft);
                break;
            case 12:
                MeshFromPoints(block, square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            case 5:
                MeshFromPoints(block, square.centerTop, square.topRight, square.centerRight, square.centerBot, square.botLeft, square.centerLeft);
                break;
            case 10:
                MeshFromPoints(block, square.topLeft, square.centerTop, square.centerRight, square.botRight, square.centerBot, square.centerLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(block, square.centerTop, square.topRight, square.botRight, square.botLeft, square.centerLeft);
                break;
            case 11:
                MeshFromPoints(block, square.topLeft, square.centerTop, square.centerRight, square.botRight, square.botLeft);
                break;
            case 13:
                MeshFromPoints(block, square.topLeft, square.topRight, square.centerRight, square.centerBot, square.botLeft);
                break;
            case 14:
                MeshFromPoints(block, square.topLeft, square.topRight, square.botRight, square.centerBot, square.centerLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(block, square.topLeft, square.topRight, square.botRight, square.botLeft);
                break;
        }
    }

    void MeshFromPoints(Block block, params MapBlockNode[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2], block);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3], block);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4], block);
        }
        if (points.Length == 6)
        {
            CreateTriangle(points[0], points[4], points[5], block);
        }
    }

    void AssignVertices(MapBlockNode[] points)
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

    void CreateTriangle(MapBlockNode a, MapBlockNode b, MapBlockNode c, Block block)
    {
        block.triangles.Add(a.vertexIndex);
        block.triangles.Add(b.vertexIndex);
        block.triangles.Add(c.vertexIndex);
    }
    

    // Block Corners are the chunks and can be used to parent the sub-meshes too.
    public void GenerateMesh(Map map, List<Node> blockCorners)
    {

        int tilePerBlock = map.tilesPerBlock;
        
        Debug.Log($"blockCorners.size: {blockCorners.Count}");

        List<Block> blocks = new List<Block>();
        foreach(Node block in blockCorners)
        {
            blocks.Add(new Block(block));
        }

        squareGrid = new SquareGrid(map, map.vertexWidth, parentGO, _subdivsions);
        _squareSize = map.vertexWidth / _subdivsions; // TODO: check this actually works or remove it
        vertices = new List<Vector3>();
        triangles = new List<int>();


        // TODO: add the triangles created here (i.e. vertices and triangles to the submesh of the blocks
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                int divisor = tilePerBlock * _subdivsions; ;
                int blockX = x / divisor;
                int blockY = y / divisor;
                int blockIndex = (blockY * map.blockWidth) + blockX;
                //Debug.Log($"blockIndex({blockIndex}) = (blockY({blockY}) * map.blockWidth({map.blockWidth})) + blockX({blockX}) | x:{x}, y:{y}");
                Block block = blocks[blockIndex];

                // use the block node to asign the triangle to
                TriangulateSquare(squareGrid.squares[x, y], block);
            }
        }

        if (parentGO.GetComponent<MeshFilter>() == null)
        {
            int count  = 0;
            foreach (Block block in blocks)
            {

                Mesh mesh = new Mesh();

                mesh.vertices = vertices.ToArray();
                mesh.triangles = block.triangles.ToArray();
                
                mesh.RecalculateNormals();
                PolygonCollider2D polygonCollider2D = parentGO.AddComponent<PolygonCollider2D>();

                MakeCollisions(mesh, polygonCollider2D);
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();

                block.node.gameObject.AddComponent<MeshRenderer>().material = mapMaterial;
                block.node.gameObject.AddComponent<MeshFilter>().mesh = mesh;
                //map.gameObject.layer = textureMask;
                count++;
            }

            //Sprite sprite = ConvertMeshToSprite(mesh, 1024, 1024);
            //SpriteRenderer spriteRenderer = map.GetComponent<SpriteRenderer>();
            //if(spriteRenderer == null)
            //{
            //    spriteRenderer = map.gameObject.AddComponent<SpriteRenderer>();
            //}
            //spriteRenderer.sprite = sprite;
            //FindObjectOfType<GameController>().GetComponent<MeshFilter>().mesh = mesh;

            

        }


    }

    public Sprite ConvertMeshToSprite(Mesh mesh, int width, int height)
    {
        // Create a temporary RenderTexture
        var renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        renderTexture.Create();

        // Create a temporary camera
        var camera = new GameObject("MeshToSpriteCamera").AddComponent<Camera>();
        camera.transform.position = new Vector3(squareGrid.map.centerVertex.X, squareGrid.map.centerVertex.Y, -10);
        camera.orthographicSize = 140f;
        camera.orthographic = true;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 10f;
        camera.targetTexture = renderTexture;

        // Set up layer mask (optional)
        Debug.Log($"Texture mask: {textureMask.value}");
        camera.cullingMask = 1 << textureMask.value;

        // Render the mesh
        camera.Render();

        // Read pixels from RenderTexture
        RenderTexture.active = renderTexture;
        var pixelsRect = new Rect(0, 0, width, height);
        Texture2D spriteTexture = new Texture2D(width, height);
        spriteTexture.ReadPixels(pixelsRect, 0, 0);
        spriteTexture.Apply();

        // Clean up temporary objects
        //DestroyImmediate(camera.gameObject);
        //RenderTexture.active = null;
        //DestroyImmediate(renderTexture);

        // Create a sprite from the texture
        var sprite = Sprite.Create(spriteTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));

        return sprite;
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
    
    private static FieldInfo shapePathField = typeof(UnityEngine.Rendering.Universal.ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
    private static FieldInfo meshHashField = typeof(UnityEngine.Rendering.Universal.ShadowCaster2D).GetField("m_ShapePathHash", accessFlagsPrivate);

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
            var shadow = gO.AddComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
            shadow.selfShadows = true;
            shapePathField.SetValue(shadow, verts.ToArray());
            // invalidate the hash so it re-generates the shadow mesh on the next Update()
            meshHashField.SetValue(shadow, -1);
        }
        return collisionPoints;
    }
    #endregion


    public class MapBlockNode
    {
        public Vector3 pos;
        public int vertexIndex = -1;

        public MapBlockNode(Vector3 pos)
        {
            this.pos = pos;
        }
    }

    public class ControlNode : MapBlockNode
    {
        public bool active;
        public MapBlockNode above, right;

        public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos)
        {
            this.active = active;
            above = new MapBlockNode(pos + Vector3.up * squareSize / 2);
            right = new MapBlockNode(pos + Vector3.right * squareSize / 2);

        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, botRight, botLeft;
        public MapBlockNode centerTop, centerRight, centerBot, centerLeft;
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

        public SquareGrid(Map map, float squareSize, GameObject parentGO,int subDivisions)
        {
            this.map = map;
            _parentGO = parentGO;
            int nodeCountX = map.graphArray.Length * subDivisions;
            int nodeCountY = map.graphArray[0].nodes.Length * subDivisions;
            
            squareSize = squareSize / subDivisions;

            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(x * squareSize, y * squareSize, 0); 
                    
                    // NOTE: the y/subDivisions and x/subDivisions are used to get the correct index from the graph array because we the type of the real node.
                    controlNodes[x, y] = new ControlNode(pos, map.graphArray[y/subDivisions].nodes[x/subDivisions].Type == NodeType.wall, squareSize);
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
