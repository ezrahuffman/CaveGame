using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;


[CreateAssetMenu(fileName = "MapMaker", menuName = "ScriptableObjects/MapMaker", order = 1)]
public class MapMaker : ScriptableObject
{
    public bool colorCode = false;

    public int testInt = 0;

    public List<Item> common;
    public List<Item> uncommon;
    public List<Item> rare;
    public List<Item> legendary;


    public Dictionary<string, Item> commonItems;
    public Dictionary<string, Item> uncommonItems;
    public Dictionary<string, Item> rareItems;
    public Dictionary<string, Item> legendaryItems;

    //TODO: might be better to convert these to percentages so they are independant of the number of total chests
    public int commonChests = 0;    //desired number of common chests
    public int uncommonChests = 0;  //desired number of uncommon chests
    public int rareChests = 0;      //desired number of rare chests
    public int legendaryChests = 0; //desired number of legendary chests
    private int wantedChests = 0;   //total number of desired chests

    public int maxPerChest = 1;     //maximum number of items in a single chest

    //current count for chests
    private int commonChestCount;    
    private int uncommonChestCount;  
    private int rareChestCount;      
    private int legendaryChestCount;

    public int tilesPerBlock;
    public int blockHeight;         //height in blocks of 'tilesPerBlock'
    public int blockWidth;          //width in blocks of 'tilesPerBlock'

    [SerializeField]
    private int height;             //how many nodes tall is the graph
    [SerializeField]
    private int width;              //how many nodes wide is the graph
    //public int paths;               //the number of paths to be created

    public int completePaths;       //number of complete paths to be created
    public int incompletePaths;     //number of incomplete paths to be created

    private GameController gameController;

    public int winningPaths;        //the number of possible winning paths
    [SerializeField]
    private int winningCount = 0;   //keep track of the winning paths

    private int compPathsCount = 0;
    //private int incompPathsCount = 0;

    private int loopcount = 0;
    public int maxLoops;
    public int smallestRoom = 0;
    public int secondRoom = 0;
    public int thirdRoom = 0; 

    [SerializeField]
    private Node centerVertex;      //center of the graph

    private float vertexWidth;       //how wide is the node
    private float vertexHeight;      //how tall is the node

    private int maxWeight;          //the maximum weight of the node

    public GameObject floor;        //game object of the floor
    public GameObject wall;         //wall prefab
    public GameObject endPrefab;    //end node prefab

    //TODO: Change this when more enemies are added to the game
    public GameObject EnemyPrefab;  //prefab of normal following enemy

    public GameObject lootChest;

    private Queue<Node> path;               //Queue containing path, excluding start node

    private Dictionary<int, Node> floorBlocks;         //list containing floor blocks
    private List<Node> blockCorners;        //list of the 40X40 block corners

    [System.Serializable]
    public class NodeList
    {
        [SerializeField]
        public Node[] nodes;
    }

    public NodeList[] graphArray;

    private GameObject aStar;

    public int[][] testArray;

    private bool isComplete;

    public GameObject[] enemies;

    List<int> gameIds = new List<int>();

    public void Driver()
    {
        CreateItemDictionaries();

        winningCount = 0;


        float timer = Time.realtimeSinceStartup;
        //gameController = GetComponent<GameController>();

        wantedChests = commonChests + uncommonChests + rareChests + legendaryChests;


        height = blockHeight * tilesPerBlock;          //create in blocks of 'tilesPerBlock'
        width = blockWidth * tilesPerBlock;            //create in blocks of 'tilesPerBlock'

        path = new Queue<Node>();
        floorBlocks = new Dictionary<int, Node>();
        blockCorners = new List<Node>();

        //instantiate array of nodes for graph
        graphArray = new NodeList[height];
        testArray = new int[height][];
        for (int i = 0; i < height; i++)
        {
            testArray[i] = new int[width];
            graphArray[i] = new NodeList();
            graphArray[i].nodes = new Node[width];
        }

        //Get Height and Width of vertex
        vertexWidth = (floor.GetComponent<SpriteRenderer>().sprite.bounds.extents.x * floor.transform.localScale.x) * 2;
        vertexHeight = (floor.GetComponent<SpriteRenderer>().sprite.bounds.extents.y * floor.transform.localScale.y) * 2;

        maxWeight = (height * width) + 1;       //setting the max weight that a node can be (not sure exactly what this does, but seemed like a good distribution)
        int idCount = 0;
        //create nodes in a graph
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                //make new node
                Node node = new Node(vertexWidth * j, vertexHeight * i, i, j);

                //assign position of node
                node.position = new Vector3(node.X, node.Y, wall.transform.position.x);

                //game object = prefab
                node.prefab = wall;
                node.Type = NodeType.wall;
                node.color = Color.white;

                //randomize weight of node
                node.weight = Random.Range(1, maxWeight);

                //place node in graph
                graphArray[i].nodes[j] = node;

                //assign edges
                if (i == 0 || j == 0 || j == (width - 1) || i == (height - 1))
                {
                    node.edge = true;
                }

                if (i % tilesPerBlock == 0 && j % tilesPerBlock == 0)
                {
                    blockCorners.Add(node);
                }

                node.id = idCount;

                AssignNeighbors(node, i, j);

                idCount++;
            }
        }

        //assign center
        centerVertex = graphArray[height / 2].nodes[width / 2];

        //set the pathfinding graph to matchup with map graph
        GridGraph gridGraph = AstarPath.active.data.gridGraph;
        gridGraph.SetDimensions(width, height, vertexHeight);

        //center gridgraph to centerNode position shifted to the center of the node
        gridGraph.center = new Vector3(centerVertex.position.x - (vertexWidth / 2), centerVertex.position.y - (vertexHeight / 2), centerVertex.position.z);


        //mark center with green
        centerVertex.prefab = floor;
        centerVertex.Type = NodeType.floor;
        centerVertex.color = Color.green;


        //while the number of paths has not been met and the maximum number of loops has not been met
        compPathsCount = 0; //TODO: figure out why this is getting reset in the first place
        loopcount = 0;
        while (compPathsCount < completePaths && loopcount < maxLoops)
        {
            Debug.Log("ran make path");
            MakePath(centerVertex);
        }

        if (loopcount >= maxLoops)
        {
            Debug.Log("loop count exit");
        }

        //Find corridors
        Dictionary<int, Node> corridors = FindCorridors();
        MakeCorridor(corridors);

        //find rooms and sort them by size
        Dictionary<int, Node> roomBlocks = RemoveBlocks(floorBlocks, corridors);
        List<Queue<Node[]>> rooms = FindRooms(roomBlocks);

        //make rooms(just colors by size right now)
        MakeRooms(rooms);

        //Instaniate Graph
        Map map = MakeGraph();

        //Make the loot in rooms
        Dictionary<int, GameObject> loot = MakeLoot(rooms);


        FillChest(loot);

        enemies = AddEnemies(rooms);

        //set spawn for player
        Vector2 spawnPoint = new Vector2(CenterNode.X, CenterNode.Y);
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        gameController.Spawn(spawnPoint);

        //store information in gameObject
        StoreInfo(map);

        MapBlock mapBlock = new MapBlock();
        mapBlock.parentGO = map.gameObject;
        mapBlock.GenerateMesh(map);

        timer = Time.realtimeSinceStartup - timer;
        Debug.Log("MapMaker took: " + timer + "s");
    }

    private void StoreInfo(Map map)
    {
        //store graphArray (try just normal gameObjects first
        map.graphArray = graphArray;
        map.centerVertex = CenterNode;
        map.blockWidth = blockWidth;
        map.blockHeight = blockHeight;
        map.tilesPerBlock = tilesPerBlock;
        map.vertexWidth = vertexWidth;
        map.GenerateDictionary();   // sorts tiles 
        //TODO: GenerateDictionary passes over the entire map again. Could be optimized by sorting tiles another way
    }

    //create dictionaries from the item lists
    public void CreateItemDictionaries()
    {
        commonItems = new Dictionary<string, Item>();
        uncommonItems = new Dictionary<string, Item>();
        rareItems = new Dictionary<string, Item>();
        legendaryItems = new Dictionary<string, Item>();

        foreach (Item item in common)
        {
            commonItems.Add(item.name, item);
        }
        foreach (Item item in uncommon)
        {
            commonItems.Add(item.name, item);
        }
        foreach (Item item in rare)
        {
            commonItems.Add(item.name, item);
        }
        foreach (Item item in legendary)
        {
            commonItems.Add(item.name, item);
        }
    }

    //returns list - sublist
    //NOTE: this function does not modify the original list
    public Dictionary<int, Node> RemoveBlocks(Dictionary<int, Node> dict, Dictionary<int, Node> sublist)
    {
        Dictionary<int, Node> returnDict = new Dictionary<int, Node>();
        foreach(Node node in dict.Values)
        {
            if (!sublist.ContainsKey(node.id))   //add items that are in list but not sublist
            {
                returnDict.Add(node.id, node);
            }
        }
        return returnDict;
    }

    public void AssignNeighbors(Node node, int i, int j)
    {
        if (i != 0)
        {
            //assign up and down nodes
            Node downNode = graphArray[i - 1].nodes[j];
            node.down = downNode;
            node.AddNeighbor(downNode);
            node.down.up = node;
            node.down.AddNeighbor(node);
        }
        
        if (j != 0)
        {
            //assign left and right nodes
            Node leftNode = graphArray[i].nodes[j - 1];
            node.left = leftNode;
            node.AddNeighbor(leftNode);
            node.left.right = node;
            node.left.AddNeighbor(node);
        }
    }

    public void MakePath(Node s)
    {
        //while not at the edge && has neighbors
        path.Clear();
        isComplete = false;

        while (!s.edge)
        {
            //visit minimum weight neighbor (if not visited)
            Node minNeighbor = Min(s);

            //set neighbor to visited
            if (minNeighbor != null)
            {
                //never mark center node as visited (better if this can be removed)
                if (minNeighbor != centerVertex)
                {
                    minNeighbor.visited = true;
                }

                path.Enqueue(s);

                if (!floorBlocks.ContainsKey(s.id)) {
                    floorBlocks.Add(s.id, s);
                }


                //dont collapse center node
                if (s != centerVertex)
                {
                    s.Collapse();   //Might Break everthing
                }

                s = minNeighbor;
            }
            else
            {
                Debug.Log("minNeighbor is null");
                break;
            }

            s.prefab = floor;
            s.Type = NodeType.floor;
            //TODO: Make sure that the rest of tiles are assigned the correct type.
        }

        if (s.edge)
        {
            isComplete = true;
            compPathsCount++;
        } 


       if(!CheckPath(isComplete))
        {
            Debug.Log("shuffle");
            Shuffle(path);
        }

        EndPath(s);

        loopcount++;
    }

    //Randomize Path Weights
    private void Shuffle(Queue<Node> path)
    {
        foreach (Node node in path)
        {
            node.color = Color.green;
            node.visited = false;
            node.weight = Random.Range(1, maxWeight);
        }
    }


    private void EndPath(Node end)
    {
        //if end is an edge and winning count has been surpassed
        if (end.edge)
        {
            if (winningCount >= winningPaths)
            {

                end.prefab = wall;
                end.Type = NodeType.wall;
                end.color = Color.white;
                
            }
            else
            {
                end.prefab = endPrefab;
                end.Type = NodeType.end;
                end.color = Color.blue;

                //add end to floor blocks list
                if (!floorBlocks.ContainsKey(end.id))
                {
                    floorBlocks.Add(end.id, end);
                }

               

                winningCount++;
            }
            
        }
    }

    private bool CheckPath(bool complete)
    {
        if(complete && compPathsCount <= completePaths)
        {
            return true;
        }
        return false;
    }


    //returns null if all neighbors have been visited, otherwise returns smallest weighted node
    Node Min(Node node)
    {
        int min = -1;
        Node retNode = null;
        foreach(Node neighbor in node.neighbors.Values)
        {
            if ((min == -1 || neighbor.weight < min) && !neighbor.visited)
            {
                retNode = neighbor;
                min = neighbor.weight;
            }
        }

        return retNode;
    }



    //Find corridor blocks
    public Dictionary<int, Node> FindCorridors()
    {
        Dictionary<int, Node> corridorBlocks = new Dictionary<int, Node>();
        foreach(Node floor in floorBlocks.Values)
        {
            if (IsCorridor(floor))
            {
                corridorBlocks.Add(floor.id, floor);
            }
        }

        return corridorBlocks;
    }

    //Check if node is corridor
    private bool IsCorridor(Node node)
    {
        int consecNeighbors = 0;

        //what the indexes mean. The "x" marks the current node
        // 7 0 1
        // 6 x 2
        // 5 4 3

        //counts through the surounding 8 blocks, if 3 consecutive blocks are floor, then this is not a corridor
        //NOTE: counts to 10 because the first two blocks need to be counted twice
        for (int i = 0; i < 10; i++)
        {
            if (CheckCorridor(node, i))
            {
                consecNeighbors++;
            }
            else
            {
                consecNeighbors = 0;
            }

            if(consecNeighbors >= 3)
            {
                if (!CheckRow(node, i))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool CheckRow(Node node, int i)
    {
        switch (i)
        {
            case 3:
                bool two = CheckCorridor(node, 2);
                bool one = CheckCorridor(node, 1);
                return two && one;
            case 5:
                bool four = CheckCorridor(node, 4);
                bool three = CheckCorridor(node, 3);
                return four && three;
            case 7:
                bool six = CheckCorridor(node, 6);
                bool five = CheckCorridor(node, 5);
                return six && five;
            case 9://this is block index 1 (top right)
                bool eight = CheckCorridor(node, 8);
                bool seven = CheckCorridor(node, 7);
                return eight && seven;
            default:
                return false;
        }
    }

    private bool CheckCorridor(Node node,  int i)
    {
        switch (i%8)
        {
            case 0://top
                if (node.I < (height - 1) - 1)
                {
                    return graphArray[node.I + 1].nodes[node.J].visited;
                } else
                {
                    return false;
                }
            case 1://top right
                if (node.I < (height - 1) - 1 && node.J < (width - 1) - 1)
                {
                    return graphArray[node.I + 1].nodes[node.J + 1].visited;
                }
                else
                {
                    return false;
                }
            case 2://right
                if (node.J < (width - 1) - 1)
                {
                    return graphArray[node.I].nodes[node.J + 1].visited;
                }
                else
                {
                    return false;
                }
            case 3://bot right
                if (node.I > 0 && node.J < (width - 1) - 1)
                {
                    return graphArray[node.I - 1].nodes[node.J + 1].visited;
                }
                else
                {
                    return false;
                }
            case 4://bot
                if (node.I > 0)
                {
                    return graphArray[node.I - 1].nodes[node.J].visited;
                }
                else
                {
                    return false;
                }
            case 5://bot left
                if (node.I > 0 && node.J > 0)
                {
                    return graphArray[node.I - 1].nodes[node.J - 1].visited;
                }
                else
                {
                    return false;
                }
            case 6://left
                if (node.J > 0)
                {
                    return graphArray[node.I].nodes[node.J - 1].visited;
                }
                else
                {
                    return false;
                }
            case 7://top left
                if (node.I < (height - 1) - 1 && node.J > 0)
                {
                    return graphArray[node.I + 1].nodes[node.J - 1].visited;
                }
                else
                {
                    return false;
                }
            default:
                Debug.Log("when checking for corridor, index was out of range");
                return false;
        }
    }

    //Make Corridor
    public void MakeCorridor(Dictionary<int, Node> corridorBlocks)
    {
        foreach(Node node in corridorBlocks.Values)
        {
            //add content for corridor here

            //color corridor blocks for debugging
            //color this node
            node.color = Color.black;

        }
    }




    //Find Rooms and sort them
    public List<Queue<Node[]>> FindRooms(Dictionary<int, Node> nodes)
    {
        List<Queue<Node[]>> rooms = new List<Queue<Node[]>>();  //hash table** of rooms sorted by size


        //TODO: chane size of list to generalized version based on how many rooom sizes there are (not hardcoded to 4)
        int numRooms = 4;
        //instantiate rooms
        for(int i = 0; i < numRooms; i++)
        {
            Queue<Node[]> queue = new Queue<Node[]>();

            rooms.Add(queue);
        }


        List<Node> currentRoom = new List<Node>();
        while (nodes.Count != 0)                    //while nodes is not empty
        {
            Node node = nodes.First().Value;
            currentRoom.Clear();                    //clear current room

            SearchRoom(node, nodes, currentRoom);   //find the nodes in current room

            int index = RoomFunction(currentRoom);  //get index based on room size

            rooms[index].Enqueue(currentRoom.ToArray());    //add current room to list of rooms (queue of its given size)
        }

        return rooms;
    } 

    private void SearchRoom(Node node, Dictionary<int, Node> list, List<Node> currentRoom)
    {
        node.room = true;       //set this room to visited

        currentRoom.Add(node);  //add node to room
        list.Remove(node.id);      //remove node from list of nodes

        //check if in bounds
        if (node.I <= (height - 1) - 1)
        {
            Node top = graphArray[node.I + 1].nodes[node.J];
            //visit top neighbor if not already in room
            if (list.ContainsKey(top.id))
            {
                SearchRoom(top, list, currentRoom);
            }
        }

        //Check if in bounds
        if (node.J <= (width - 1) - 1) {
            Node right = graphArray[node.I].nodes[node.J + 1];
            //visit right neighbor if not already in room
            if (list.ContainsKey(right.id))
            {
                SearchRoom(right, list, currentRoom);
            }
        }

        //check if in bounds
        if (node.I > 0)
        {
            Node bot = graphArray[node.I - 1].nodes[node.J];
            //visit bot neighbor if not already in room
            if (list.ContainsKey(bot.id))
            {
                SearchRoom(bot, list, currentRoom);
            }
        }

        //check if in bounds
        if (node.J > 0)
        {
            Node left = graphArray[node.I].nodes[node.J - 1];
            //visit left neighbor if not already in rooms
            if (list.ContainsKey(left.id))
            {
                SearchRoom(left, list, currentRoom);
            }
        }
    }

    //Rooms Function (Sorts rooms based on their size)
    private int RoomFunction(List<Node> roomList)
    {
        //find a more generalized way to do this like a list or array of room sizes
        int count = roomList.Count;

        if(count <= smallestRoom)
        {
            return 0;
        } else if(count <= secondRoom)
        {
            return 1;
        } else if (count <= thirdRoom)
        {
            return 2;
        }
        else
        {
            //rooms larger than thirdroom
            return 3;
        }
    }

    //Make rooms (currently just colors by size)
    public void MakeRooms(List<Queue<Node[]>> rooms)
    {
        Color[] roomColors = { Color.green, Color.cyan, Color.grey, Color.magenta}; 

        //TODO: add room content here
        int size = 0;
        foreach (Queue<Node[]> queue in rooms)
        {
            Color color = roomColors[size];
            foreach(Node[] room in queue)
            {
                for(int i = 0; i < room.Length; i++)
                {
                    Node node = room[i];

                    //color node based on size
                    node.color = color;
                }
            }

            size++;
        }
    }

    public Dictionary<int, GameObject> MakeLoot(List<Queue<Node[]>> rooms)
    {
        Dictionary<int, GameObject> loot = new Dictionary<int, GameObject>();
        Queue<Node[]> smallRooms = rooms[0];
        foreach (Node[] room in smallRooms)
        {
            //select a random node in the room
            int index = Random.Range(0, room.Length);
            Node spawnNode = room[index];

            //place loot at that random node and parent so it is seen with the rest of graph
            GameObject chest = Instantiate(lootChest, spawnNode.position, Quaternion.identity);
            chest.GetComponent<SpriteRenderer>().enabled = true;

            Debug.Log("created chest");

            //parent chest to the node it is on top of so it has similar behavior
            chest.transform.parent = spawnNode.gameObject.transform.parent;

            //add chest to loot dictionary
            loot.Add(spawnNode.id, chest);
        }

        //return loot
        return loot;
    }

    public void FillChest(Dictionary<int, GameObject> loot)
    {
        //calculate relative number of chests based off percentage of total
        //TODO: clean this up (too many casts)
        float ratio = (float)commonChests / (float)wantedChests;
        commonChests = (int)(loot.Count * ratio);

        ratio = (float)uncommonChests / (float)wantedChests;
        uncommonChests = (int)(loot.Count * ratio);

        ratio = (float)rareChests / (float)wantedChests;
        rareChests = (int)(loot.Count * ratio);

        ratio = (float)legendaryChests / (float)wantedChests;
        legendaryChests = (int)(loot.Count * ratio);

        foreach (GameObject chest in loot.Values)
        {
            Dictionary<int, List<Item>> items = RandomLoot();
            chest.GetComponent<Chest>().ResetItems(items);
        }
    }

    private Dictionary<int, List<Item>> RandomLoot()
    {
        Dictionary<int, List<Item>> items = new Dictionary<int, List<Item>>();
        List<Item> pool = new List<Item>();
        if (legendaryChestCount < legendaryChests)
        {
            //add legendary loot to pool
            foreach(Item item in legendaryItems.Values)
            {
                pool.Add(item);
            }
        }
        if(rareChestCount < rareChests)
        {
            //add rare loot to pool
            foreach(Item item in rareItems.Values)
            {
                pool.Add(item);
            }
        }
        if(uncommonChestCount < uncommonChests)
        {
            //add uncommon loot to pool
            foreach (Item item in uncommonItems.Values)
            {
                pool.Add(item);
            }
        }
        //if there are not enough common chests or if the pool is empty
        if (commonChestCount < commonChests || pool.Count == 0)
        {
            //add common Chest to pool
            foreach (Item item in commonItems.Values)
            {
                pool.Add(item);
            }
        }

        //draw items from pool
        int numItems = Random.Range(1, maxPerChest);
        for (int i = 0; i < numItems; i++)
        {
            int index = Random.Range(0, pool.Count);
            Item item = Instantiate(pool[index]);
            pool.Remove(item);
            if (items.ContainsKey(item.GetInstanceID()) && item.IsStackable(ChestInventory.instance)){
                items[item.GetInstanceID()].Add(item);
            }
            else
            {
                List<Item> list = new List<Item>();
                list.Add(item);
                items.Add(item.GetInstanceID(), list);
            }


            if (legendaryItems.ContainsKey(item.name))
            {
                legendaryChestCount++;
            } else if (rareItems.ContainsKey(item.name))
            {
                rareChestCount++;
            }else if (uncommonItems.ContainsKey(item.name))
            {
                uncommonChestCount++;
            }
            else
            {
                commonChestCount++;
            }
        }

        return items;
    }

    private GameObject[] AddEnemies(List<Queue<Node[]>> rooms)
    {
        //spawn enemies in large rooms (might want to add some fields for where to spawn enemies)
        Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();
        Queue<Node[]> largeRooms = rooms[rooms.Count - 1];
        foreach (Node[] room in largeRooms)
        {
            //select a random node in the room
            int index = Random.Range(0, room.Length);
            Node spawnNode = room[index];

            //place Enemy at that random node and parent so it is seen with the rest of graph
            GameObject enemy = Instantiate(EnemyPrefab, spawnNode.position, Quaternion.identity);

            //TODO: set follow to false on spawn
            //enemy.GetComponent<SpriteRenderer>().enabled = true;

            Debug.Log("created chest");

            //parent enemy to the node it is on top of so it has similar behavior
            enemy.transform.parent = spawnNode.gameObject.transform.parent;

            //add enemy to enemies dictionary
            enemies.Add(spawnNode.id, enemy);
        }

        GameObject[] list = new GameObject[enemies.Values.Count];
        int i = 0;
        foreach (GameObject enemy in enemies.Values)
        {
            list[i] = enemy;
            i++;
        }
        return list;
    }

    public Map MakeGraph()
    {
        //create parent object to hold the entire map
        GameObject bigParent = new GameObject("map");
        bigParent.AddComponent<Map>();
        
        

        //create new dictionary for blocks that have already been drawn
        Dictionary<int, Node> drawnBlocks = new Dictionary<int, Node>();
        
        //create list to store parent nodes so they can be hidden after A* scanning
        List<GameObject> parents = new List<GameObject>();

        //draw blocks
        foreach (Node node in blockCorners)
        {
            //if block has not already been drawn, draw that block
            if (!drawnBlocks.ContainsKey(node.id))
            {
                drawnBlocks.Add(node.id, node);

                //draw block and add parent to list
                parents.Add(DrawBlock(node, bigParent));
            }
        }

        //scan the map
        var graphToScan = AstarPath.active.data.gridGraph;
        AstarPath.active.Scan(graphToScan);

        //hide all parent nodes
        foreach (GameObject parent in parents)
        {
            parent.SetActive(false);
        }

        return bigParent.GetComponent<Map>();
    }

    private GameObject DrawBlock(Node node, GameObject bigParent)
    {

        //create empty game object as parent
        GameObject parent = new GameObject();

        //set parent to single game object
        parent.transform.parent = bigParent.transform;
        //Draw the 40X40 block of nodes starting at the given node (current node is the bot left)
        for (int i = 0; i < tilesPerBlock; i++)
        {

            List<Node> nodeList = new List<Node>();
            for (int j = 0; j < tilesPerBlock; j++)
            {
                //get current node
                Node curNode = graphArray[node.I + i].nodes[node.J + j];
                //instantiate game object of node
                //curNode.gameObject = new GameObject();//Instantiate(curNode.prefab, curNode.position, Quaternion.identity);
                //TODO: the floor needs to be handled in a better way probably
                if (curNode.prefab == floor || curNode.prefab == endPrefab)
                {
                    curNode.gameObject = Instantiate(curNode.prefab, curNode.position, Quaternion.identity);
                }
                else
                {
                    curNode.gameObject = new GameObject();
                }

                //save object id so it can be recovered later
                curNode.objectId = curNode.gameObject.GetInstanceID();

                /*
                //change color of game object
                if (colorCode)
                {
                    curNode.gameObject.GetComponent<SpriteRenderer>().color = curNode.color;
                }
                */
                //parent node to current parent node
                curNode.gameObject.transform.parent = parent.transform;
                nodeList.Add(node);
            }
        }

        //TODO: This is also stupid fix the way that generate mesh takes in these values, or have an overload for nodelist and vertexWidth
        //blockMap.graphArray = nodeLists.ToArray();
        //blockMap.vertexWidth = vertexWidth;
        //block.parentGO = parent;
        //block.GenerateMesh(blockMap);
        return parent;
    }

    


    public Node CenterNode {
        get
        {
            return centerVertex;
        }
    }

    public GameObject[] Enemies
    {
        get
        {
            return enemies;
        }
    }

   

    //this should be obsolete
    public Node[][] Graph
    {
        get
        {
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
}
