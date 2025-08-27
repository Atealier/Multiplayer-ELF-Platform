using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSoundManager : MonoBehaviour
{
    public static GameSoundManager Instance { get; private set; }

    public AudioSource audioSource;
    public AudioSource bgMusicSource; // For background music
    
    // Sound clips
    public AudioClip chargingSound1;
    public AudioClip chargingSound2;
    public AudioClip chargingSound3;
    public AudioClip clashingSound;
    public AudioClip countdownSound;
    public AudioClip correctSound;
    public AudioClip halfStreakSound;
    public AudioClip fullStreakSound;
    public AudioClip bgMusic;
    public AudioClip victorySound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // Uncomment below if you want this to persist between scenes
            // DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }



    public void PlayBackgroundMusic()
    {
        if (bgMusic != null)
        {
            bgMusicSource.clip = bgMusic;
            bgMusicSource.loop = true;
            bgMusicSource.volume = 0.3f; // Adjust for the bg volume
            bgMusicSource.Play();
        }
    }


    public void PlayChargingSound1() { PlaySound(chargingSound1); }
    public void PlayChargingSound2() => PlaySound(chargingSound2);
    public void PlayChargingSound3() => PlaySound(chargingSound3);
    public void PlayClashingSound() => PlaySound(clashingSound);

    // In-Game
    public void PlayTimerSound() {
        
        
        PlaySound(countdownSound);

        //AudioSource.PlayClipAtPoint(countdownSound, Vector3.zero);
      /*  Debug.LogError("PLAYINGINSOUND");
        Debug.LogError("AudioSource is null? " + (audioSource == null));
        Debug.LogError("Clip is null? " + (countdownSound == null));*/
    }

    public void PlayCorrectAnswerSound() => PlaySound(correctSound);
    public void PlayHalfStreakSound() => PlaySound(halfStreakSound);
    public void PlayFullStreakSound() => PlaySound(fullStreakSound);
    public void PlayVictorySound() => PlaySound(victorySound);


    public void StopBackgroundMusic()
    { 
        Debug.LogWarning("STOPPING BGMUSIC");
        bgMusicSource.Stop();
        bgMusicSource.loop = false;
        bgMusicSource.clip = null;
    }
}
