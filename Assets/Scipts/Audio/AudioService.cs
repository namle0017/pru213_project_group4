using System.Collections.Generic;
using UnityEngine;

public static class AudioService
{
    private static readonly Dictionary<string, AudioClip> ClipCache = new Dictionary<string, AudioClip>();

    public static AudioClip LoadClip(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        if (ClipCache.TryGetValue(resourcePath, out AudioClip cachedClip))
        {
            return cachedClip;
        }

        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        ClipCache[resourcePath] = clip;
        return clip;
    }

    public static void PlayClip(string resourcePath, float volume = 1f)
    {
        PlayClip(LoadClip(resourcePath), volume);
    }

    public static void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            return;
        }

        GameObject audioObject = new GameObject("OneShotAudio_" + clip.name);
        Object.DontDestroyOnLoad(audioObject);

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = false;
        source.volume = Mathf.Clamp01(volume);
        source.clip = clip;
        source.Play();

        Object.Destroy(audioObject, clip.length + 0.25f);
    }

    public static void PlayButtonClick()
    {
        PlayClip(AudioPaths.ButtonClick, 1f);
    }

    public static void PlayBackClose()
    {
        PlayClip(AudioPaths.BackClose, 1f);
    }

    public static void PlayErrorNotEnoughCoin()
    {
        PlayClip(AudioPaths.ErrorNotEnoughCoin, 1f);
    }

    public static void PlayBuySuccess()
    {
        PlayClip(AudioPaths.BuySuccess, 1f);
    }

    public static void PlayUnlockSuccess()
    {
        PlayClip(AudioPaths.UnlockSuccess, 1f);
    }

    public static void PlayRewardPopup()
    {
        PlayClip(AudioPaths.RewardPopup, 1f);
    }
}
