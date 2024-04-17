using UnityEngine;
using UnityEditor;

public class PathExpansionTool
{
    [MenuItem("MyTools/ExpandPath")]
    static void OnClick()
    {
        //get reference to GameController
        GameObject mapObject = GameObject.Find("map");
        Map map;
        if (mapObject)
        {
            map = mapObject.GetComponent<Map>();
        }
        else
        {
            Debug.LogError("mapObject is NULL. You might be missing a 'map' object in the scene.");
            return;
        }


        //TODO: actually do something here, might need a driver() method or something like that
        map.GenerateDictionary(); //this doesn't have much cost asociated with it here because it only generates dictionary if it doesn't exist


        // expand path out by one node on each side
        foreach(var node in map.nodeGroups[NodeType.floor])
        {
            // check neighboring nodes
            Debug.Log("Sanity check");

            // check up, down, left and right neighbors and convert to floor
            // store new nodes in temp collections to be added to node group after 
        }

        // resmooth the path and regenerate mesh 
    }

   


}



