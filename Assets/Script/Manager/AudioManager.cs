using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SEType
{
    None,
    Chaim,
    Mouse
}
public enum BGMType
{
    None,
    Timer,
    Main_Peace,
    Main,
    Main_War
}

public class AudioManager : SingletonManager<AudioManager>
{
    public Dictionary<BGMType, AudioSource> BGMAudioPlayerDict = new Dictionary<BGMType, AudioSource>()
    {
        {BGMType.Main, null},
        {BGMType.Timer, null},
    };

    public Dictionary<SEType, AudioSource> SEAudioPlayerDict = new Dictionary<SEType, AudioSource>()
    {
        {SEType.Chaim, null},
        {SEType.Mouse, null},
    };

    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySE(SEType type, AudioClip clip)
    {
        if (!SEAudioPlayerDict.ContainsKey(type)) return;
        SEAudioPlayerDict[type].PlayOneShot(clip);
    }

    /// <summary>
    /// SE停止
    /// </summary>
    public void StopSE(SEType type)
    {
        if (!SEAudioPlayerDict.ContainsKey(type)) return;
        SEAudioPlayerDict[type].Stop();
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    /// <param name="clip"></param>
    public void PlayBGM(BGMType type, AudioClip clip)
    {
        BGMAudioPlayerDict[type].clip = clip;
        BGMAudioPlayerDict[type].Play();
    }

    /// <summary>
    /// BGM停止
    /// </summary>
    public void StopBGM(BGMType type)
    {
        BGMAudioPlayerDict[type].Stop();
    }

    /// <summary>
    /// 辞書型にオーディオソースを追加
    /// </summary>
    /// <param name="type"></param>
    /// <param name="source"></param>
    public void AddSEAudioSource(SEType type, AudioSource source)
    {
        SEAudioPlayerDict[type] = source;
    }

    /// <summary>
    /// 辞書型にオーディオソースを追加
    /// </summary>
    /// <param name="type"></param>
    /// <param name="source"></param>
    public void AddBGMAudioSource(BGMType type, AudioSource source)
    {
        BGMAudioPlayerDict[type] = source;
    }
}
