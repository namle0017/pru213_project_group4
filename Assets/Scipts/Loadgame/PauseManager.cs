using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Kéo thả Panel Pause của bạn vào đây")]
    public GameObject pausePanel;

    private bool isPaused = false;

    // Hàm này sẽ gắn vào Nút Pause trên màn hình
    public void PauseGame()
    {
        Debug.Log("ĐÃ BẤM VÀO NÚT PAUSE!"); 
        isPaused = true;
        Time.timeScale = 0f; // Đóng băng thời gian

        if (pausePanel != null)
        {
            pausePanel.SetActive(true); // Bật giao diện Pause lên
        }
    }

    // Hàm này sẽ gắn vào Nút Resume (Chơi tiếp) trên màn hình Pause
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Chạy lại thời gian

        if (pausePanel != null)
        {
            pausePanel.SetActive(false); // Tắt giao diện Pause đi
        }
    }
}
