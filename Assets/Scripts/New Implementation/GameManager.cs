using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private Scene currentScene;

    private void Start()
    {
        currentScene = SceneManager.GetActiveScene();
    }

    public void SwitchScenes()
    {
        if (currentScene.name == "Gotta Move 1")
        {
            SceneManager.LoadScene("SampleScene");
        } 
        else if (currentScene.name == "SampleScene")
        {
            SceneManager.LoadScene("Gotta Move 1");
        } 
        else
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void SwitchScenesDistinct()
    {
        if (currentScene.name != "Distinct")
        {
            SceneManager.LoadScene("Distinct");
        }
        else
        {
            SceneManager.LoadScene("Gotta Move 1");
        }
    }
}
