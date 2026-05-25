using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void LoadTestingScene()
    {
        SceneManager.LoadScene("Testing");
    }
}