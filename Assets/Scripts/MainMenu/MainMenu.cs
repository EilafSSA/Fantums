using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //
    public void PlayGame()
    {
        
        SceneManager.LoadSceneAsync(1); //Level1
    }

    public void QuitGame()
    {
        
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        //reference: https://youtu.be/YOaYQrN1oYQ
       
       Debug.Log(volume); 
    }

}
