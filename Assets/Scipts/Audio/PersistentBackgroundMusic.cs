using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class PersistentBackgroundMusic : MonoBehaviour
{
    private static readonly string[] DefaultUiScenes =
    {
        "Menu",
        "LevelSelect",
        "LevelSelect2",
        "LevelSelect3",
        "GarageScene",
        "DailyRewardScene",
        "SpinScene",
    };

    private static readonly string[] DefaultGameplayScenes =
    {
        "GroundMap",
        "DessertMap",
        "ArcticMap",
        "MarsMap",
        "HighwayMap",
        "AlienMap",
        "MoonMap",
        "ForestMap",
    };

    private static PersistentBackgroundMusic instance;

    [SerializeField] private string[] uiSceneNames = DefaultUiScenes;
    [SerializeField] private string[] gameplaySceneNames = DefaultGameplayScenes;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.7f;

    private AudioSource audioSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject musicObject = new GameObject("PersistentBackgroundMusic");
        musicObject.AddComponent<AudioSource>();
        musicObject.AddComponent<PersistentBackgroundMusic>();
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ConfigureAudioSource();
        ApplyMusicForScene(SceneManager.GetActiveScene().name);
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
        ApplyMusicForScene(scene.name);
    }

    private void ConfigureAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = bgmVolume;
    }

    private void ApplyMusicForScene(string sceneName)
    {
        AudioClip targetClip = null;

        if (SceneExistsInList(sceneName, uiSceneNames, DefaultUiScenes))
        {
            targetClip = AudioService.LoadClip(AudioPaths.MenuBgm);
        }
        else if (SceneExistsInList(sceneName, gameplaySceneNames, DefaultGameplayScenes))
        {
            targetClip = AudioService.LoadClip(AudioPaths.GameplayBgm);
        }

        if (targetClip == null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            return;
        }

        if (audioSource.clip != targetClip)
        {
            audioSource.clip = targetClip;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private static bool SceneExistsInList(string sceneName, string[] configuredScenes, string[] fallbackScenes)
    {
        string[] scenesToCheck = configuredScenes;
        if (scenesToCheck == null || scenesToCheck.Length == 0)
        {
            scenesToCheck = fallbackScenes;
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
}
