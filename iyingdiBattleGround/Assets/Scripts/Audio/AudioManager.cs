using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region get singleton
    public static AudioManager instance = null;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    private AudioListener mAudioListener;

    public AudioSource MusicSource { get; private set; }
    public AudioSource SoundSource { get; private set; }
    /// <summary>
    /// 调整背景音乐音量
    /// </summary>
    public float MusicVolume
    {
        get => MusicSource.volume;
        set => MusicSource.volume = value;
    }
    /// <summary>
    /// 调整音效音量
    /// </summary>
    public float SoundVolume
    {
        get => SoundSource.volume;
        set => SoundSource.volume = value;
    }
    public bool IsMusicOn { get => !MusicSource.mute; }
    public bool IsSoundOn { get => !SoundSource.mute; }

    /// <summary>
    /// 初始化
    /// </summary>
    void OnEnable()
    {
        if (!MusicSource)
        {
            MusicSource = gameObject.AddComponent<AudioSource>();
        }
        if (!SoundSource)
        {
            SoundSource = gameObject.AddComponent<AudioSource>();
        }
        CheckAudioListener();
    }

    //场景中只能有一个AudioListener。
    private void CheckAudioListener()
    {
        if (!mAudioListener)
        {
            mAudioListener = FindObjectOfType<AudioListener>();
        }
        if (!mAudioListener)
        {
            mAudioListener = gameObject.AddComponent<AudioListener>();
        }
    }

    public void PlaySound(string name)
    {
        var clip = Resources.Load<AudioClip>(name);
        PlaySound(clip);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioClip">要播放的音效片段</param>
    public void PlaySound(AudioClip audioClip)
    {
        SoundSource.clip = audioClip;
        SoundSource.Play();
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isLoop"></param>
    public void PlayMusic(string name, bool isLoop)
    {
        var clip = Resources.Load<AudioClip>(name);

        MusicSource.clip = clip;
        MusicSource.Play();
        MusicSource.loop = isLoop;
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    public void StopMusic()
    {
        MusicSource.Stop();
    }

    /// <summary>
    /// 暂停播放背景音乐
    /// </summary>
    public void PauseMusic()
    {
        MusicSource.Pause();
    }

    /// <summary>
    /// 继续播放背景音乐
    /// </summary>
    public void ResumeMusic()
    {
        MusicSource.UnPause();
    }

    /// <summary>
    /// 背景音乐静音
    /// </summary>
    public void MusicOff()
    {
        MusicSource.Pause();
        MusicSource.mute = true;
    }

    /// <summary>
    /// 背景音乐取消静音
    /// </summary>
    public void MusicOn()
    {
        MusicSource.UnPause();
        MusicSource.mute = false;
    }

    /// <summary>
    /// 音效静音
    /// </summary>
    public void SoundOff()
    {
        var soundSources = GetComponents<AudioSource>();

        foreach (var item in soundSources)
        {
            if (item != MusicSource)
            {
                item.mute = true;
            }
        }
    }

    /// <summary>
    /// 音效取消静音
    /// </summary>
    public void SoundOn()
    {
        var soundSources = GetComponents<AudioSource>();

        foreach (var item in soundSources)
        {
            if (item != MusicSource)
            {
                item.mute = false;
            }
        }
    }
}


