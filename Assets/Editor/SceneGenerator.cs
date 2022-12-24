using UnityEngine;
using UnityEditor;


public class SceneGenerator
{
    public MapMaker mapMaker;

    // Start is called before the first frame update
    [MenuItem("MyTools/GenerateMap")]
    static void MakeMap()
    {
        //get reference to GameController
        GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        //get reference to mapMaker
        MapMaker mapMaker = gameController.mapMaker;

        //make map
        mapMaker.Driver();

        //mark mapMaker to be saved
        EditorUtility.SetDirty(mapMaker);
    }

    
}


