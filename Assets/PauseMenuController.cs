using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    bool isOpen = false;
    int mainMenuBuildIndex = 0;

    public GameObject pauseMenu;
    public GameObject equipMenu;
    public GameObject nextLevelButton;
    public Toggle relativeMovementToggle;

    public static PauseMenuController instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("There is already an instance of this but you are trying to create another one!");
        }
    }

    //TODO: add some fancier interactions for the menu loads and such

    // Disable the next level button if we are the last level
    // TODO: There should probably be a way to handle when the player has beat all the levels in the game
    public void OnEndMenuAwake()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("END menu awake");
        if (SceneManager.sceneCount <= sceneIndex + 1)
        {
            Debug.Log("set menu awake == false");
            nextLevelButton.SetActive(false);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePauseMenu();
        }
    }

    //This should pause the game as well
    public void TogglePauseMenu()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            // TODO: the player is probably already cached somewhere. If not, it should be.
            relativeMovementToggle.isOn = FindObjectOfType<PlayerController>().relativeMovement;
            PauseGame();
        }
        else
        {
            UnpauseGame();
        }
        pauseMenu.SetActive(isOpen);
    }

    //This currently pauses the game, think that is a good idea
    public void ToggleEquipMenu()
    {
        bool equipOpen = !equipMenu.activeInHierarchy;
        if (equipOpen)
        {
            PauseGame();
        }
        else
        {
            UnpauseGame();
        }
        equipMenu.SetActive(equipOpen);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
    }

    public void MainMenu_OnClick()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    public void Restart_OnClick()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel_OnClick()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (SceneManager.sceneCount > sceneIndex + 1)
        {
            Debug.LogError("There is no next level, this button should be disabled!");
            return;
        }

        SceneManager.LoadScene(sceneIndex + 1);
    }

    public void ToggleRelativeMovement(bool val)
    {
        FindObjectOfType<PlayerController>().relativeMovement = val;
    }
}
