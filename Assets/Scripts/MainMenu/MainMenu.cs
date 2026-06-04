using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("=== Options ===")]
    [SerializeField] private OptionsMenu optionsMenu;

    public void PlayGame()
    {
        
        SceneManager.LoadSceneAsync(1); //Level1
    }

    public void QuitGame()
    {
        
        Application.Quit();
    }

    public void OpenOptions()
    {
        if (optionsMenu != null) optionsMenu.OpenOptions();
    }

    public void CloseOptions()
    {
        if (optionsMenu != null) optionsMenu.CloseOptions();
    }

    public void SetVolume(float volume)
    {
        //reference: https://youtu.be/YOaYQrN1oYQ
       
       Debug.Log(volume); 
    }

}
