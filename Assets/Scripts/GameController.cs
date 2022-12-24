using UnityEngine;

public class GameController : MonoBehaviour
{

    //public Vector2 spawnPoint;
    public GameObject playerPrefab;
    public CameraController camController;
    public MapMaker mapMaker;
    public GameObject endMenu;
    public GameObject gameOverMenu;
    public GameObject torchUI;
    public GameObject lightUI;

    Vector2 currentSpawnPoint;


    public static GameController instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("You are trying to create a game controller instance, but one already exists!");
        }

       //GetComponent<Temp_MapSmoothing>().GenerateMesh(FindObjectOfType<Map>());
    }

    // Start is called before the first frame update
    public void Spawn(Vector2 spawnPoint)
    {
        //instantiate player at spawnPoint
        camController.PlayerObj = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        currentSpawnPoint = spawnPoint;
    }

    public void LevelComplete()
    {
        //pause game
        Time.timeScale = 0;

        //open end menu
        endMenu.SetActive(true);
        PauseMenuController.instance.OnEndMenuAwake();
    }

    public void GameOver()
    {
        //pause game
        Time.timeScale = 0;

        //open end menu
        gameOverMenu.SetActive(true);
    }


    public float WallWidth
    {
        get
        {
            GameObject floorPrefab = mapMaker.floor;
            float width = (floorPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x * floorPrefab.transform.localScale.x);
            return width;
        }
    }
}
