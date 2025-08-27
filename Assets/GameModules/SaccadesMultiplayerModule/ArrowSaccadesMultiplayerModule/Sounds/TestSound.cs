using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSound : MonoBehaviour
{
    public  AudioClip audion;

    // Start is called before the first frame update
    void Start()
    {   
        AudioSource.PlayClipAtPoint(audion, Vector3.zero);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
