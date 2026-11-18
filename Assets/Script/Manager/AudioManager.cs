using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum SEType
{
    None,
    Chime,
    Click,
    Main,
    State,
    Country,
    Force,
    Diplomacy,
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
    /// <summary>
    /// プレイヤーが戦争中かどうか
    /// </summary>
    public bool PlayerInWar { get; private set; } = false;

    public Dictionary<BGMType, AudioSource> BGMAudioPlayerDict = new Dictionary<BGMType, AudioSource>()
    {
        {BGMType.Main, null},
        {BGMType.Timer, null},
    };

    public Dictionary<SEType, AudioSource> SEAudioPlayerDict = new Dictionary<SEType, AudioSource>()
    {
        {SEType.Chime, null},
        {SEType.Click, null},
    };

    private async Task Update()
    {
        if (BattleSystemManager.Instance.PlayerCountry.EnemyCountryList.Count > 0 && !PlayerInWar)  //プレイヤーが戦争状態になった瞬間
        {
            PlayerInWar = true;
            PlayBGM(BGMType.Main, BGMType.Main_War);
        }

        if (BattleSystemManager.Instance.PlayerCountry.EnemyCountryList.Count == 0 && PlayerInWar)  //プレイヤーが平時状態になった瞬間
        {
            PlayerInWar = false;
            PlayBGM(BGMType.Main, BGMType.Main_Peace);
        }
    }

    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="clip"></param>
    public async Task PlaySE(SEType source, SEType type)
    {
        if (!SEAudioPlayerDict.ContainsKey(source)) return;

        SEAudioPlayerDict[source].PlayOneShot(await AddressableManager.Instance.LoadSEClipAsync(type));
    }

    /// <summary>
    /// SE停止
    /// </summary>
    public void StopSE(SEType source)
    {
        if (!SEAudioPlayerDict.ContainsKey(source)) return;
        SEAudioPlayerDict[source].Stop();
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    /// <param name="source">流す場所</param>
    /// <param name="type">流す種類</param>
    public async Task PlayBGM(BGMType source, BGMType type)
    {
        BGMAudioPlayerDict[source].clip = await AddressableManager.Instance.LoadBGMClipAsync(type);
        BGMAudioPlayerDict[source].Play();
    }

    /// <summary>
    /// BGM停止
    /// </summary>
    public void StopBGM(BGMType source)
    {
        BGMAudioPlayerDict[source].Stop();
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
