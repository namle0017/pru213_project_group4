using UnityEngine;

public static class AudioSettingsController
{
    public const string AudioMutedKey = "AudioMuted";

    private static bool isInitialized;
    private static bool isMuted;

    public static bool IsMuted()
    {
        EnsureInitialized();
        return isMuted;
    }

    public static void SetMuted(bool muted)
    {
        EnsureInitialized();

        isMuted = muted;
        AudioListener.volume = muted ? 0f : 1f;

        PlayerPrefs.SetInt(AudioMutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void ToggleMute()
    {
        SetMuted(!IsMuted());
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedStateBeforeSceneLoad()
    {
        isInitialized = false;
        EnsureInitialized();
    }

    private static void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        isMuted = PlayerPrefs.GetInt(AudioMutedKey, 0) == 1;
        AudioListener.volume = isMuted ? 0f : 1f;
        isInitialized = true;
    }
}
