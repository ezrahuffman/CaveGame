using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] GameObject _settingsMenu;
    [SerializeField] GameObject _mainMenu;
    [SerializeField] Transform _buttonsParent;


    private void Awake()
    {
        foreach (Button button in _buttonsParent.GetComponentsInChildren<Button>())
        {
            button.onClick.AddListener(() => PlayButtonClick());
        }
    }

    

    private void PlayButtonClick()
    {
        //TODO: Play button click
    }

    public void OnStartButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    public void OnSettingsButtonClick()
    {
        SwitchMenu(_mainMenu, _settingsMenu);
    }

    public void OnQuitButtonClick()
    {
        Debug.Log("Quit Application");
        Application.Quit();
    }

    private void SwitchMenu(GameObject currMenu, GameObject nextMenu)
    {
        currMenu.SetActive(false);
        nextMenu.SetActive(true);
    }

    
}
