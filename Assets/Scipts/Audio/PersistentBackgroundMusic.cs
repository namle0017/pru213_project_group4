using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(AudioSource))]
public class PersistentBackgroundMusic : MonoBehaviour
{
    private static readonly string[] DefaultAllowedScenes =
    {
        "Menu",
        "LevelSelect",
        "LevelSelect2",
        "LevelSelect3",
        "GarageScene",
        "DailyRewardScene",
        "SpinScene",
    };

    private static PersistentBackgroundMusic instance;

    [SerializeField] private string[] allowedSceneNames = DefaultAllowedScenes;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (instance != null && instance != this)
        {
            bool sameClip = instance.audioSource != null
                && audioSource != null
                && instance.audioSource.clip == audioSource.clip;

            if (sameClip || instance.audioSource == null || audioSource == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;

        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.Play();
        }

        if (!IsSceneAllowed(SceneManager.GetActiveScene().name))
        {
            DestroySelf();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!IsSceneAllowed(scene.name))
        {
            DestroySelf();
        }
    }

    private bool IsSceneAllowed(string sceneName)
    {
        string[] scenesToCheck = allowedSceneNames;
        if (scenesToCheck == null || scenesToCheck.Length == 0)
        {
            scenesToCheck = DefaultAllowedScenes;
        }

        for (int i = 0; i < scenesToCheck.Length; i++)
        {
            if (scenesToCheck[i] == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    private void DestroySelf()
    {
        if (instance == this)
        {
            instance = null;
        }

        Destroy(gameObject);
    }
}
