using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void LoadTestingScene()
    {
        SceneManager.LoadScene("GroundMap");
    }

    // 1. Hàm dùng để chơi lại màn hiện tại
    public void RestartCurrentScene()
    {
        // GetActiveScene().name sẽ tự động lấy tên cái map bạn đang đứng để load lại nó
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Đảm bảo thời gian chạy bình thường khi chơi lại (phòng hờ bạn đang Pause game)
        Time.timeScale = 1f;
    }
    // 2. Hàm dùng để quay về Menu chính
    public void LoadMainMenu()
    {
        // Điền tên chính xác của Scene Menu vào đây (Mình giả sử tên nó là "Menu")
        SceneManager.LoadScene("Menu");

        Time.timeScale = 1f;
    }
    // 3. Hàm dùng để Thoát Game
    public void QuitGame()
    {
        Debug.Log("Quit Game!"); // Dòng này in ra console để test
        Application.Quit(); // Dòng này sẽ thực sự tắt game khi bạn build file .exe
    }
}
