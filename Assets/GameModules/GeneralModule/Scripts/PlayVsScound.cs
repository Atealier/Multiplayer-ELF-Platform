using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayVsScound : MonoBehaviour
{
    public void PlayCharacterClashSound()
    {
        GameSoundManager.Instance.PlayClashingSound();
    }
}

