using System.Collections;
using UnityEngine;

public class AnimationSequenceController : MonoBehaviour
{
    public GameObject character1;
    public GameObject character2;

    void Start()
    {

    }

    private void OnEnable()
    {
        // Optionally, trigger the sequence manually if needed
        StartCoroutine(PlayAnimationSequence());
    }

     private IEnumerator PlayAnimationSequence()
     {
         Debug.Log("PLAYER ANIMATION INTRO");
         // Play first character's animation

      
         character1.SetActive(true);
         GameSoundManager.Instance.PlayChargingSound1();
         yield return new WaitForSeconds(3f); // Adjust timing to animation length
         character1.SetActive(false);
         // Play second character's animation


         character2.SetActive(true);
         GameSoundManager.Instance.PlayChargingSound2();
         yield return new WaitForSeconds(3f);
         character2.SetActive(false);

         // Play both animations together
         character1.SetActive(true);     
         character2.SetActive(true);
         yield return new WaitForSeconds(0.5f);
         GameSoundManager.Instance.PlayChargingSound3();

         yield return new WaitForSeconds(3f);
        // GameSoundManager.Instance.PlayClashingSound();

         
         character1.SetActive(false);   
         character2.SetActive(false);


     }


    private IEnumerator PlayAnimationSequenc1e()
    {
        Debug.Log("PLAYER ANIMATION INTRO");

        // === Phase 1: Character 1 Appears ===
        GameSoundManager.Instance.PlayChargingSound1();
        character1.SetActive(true);
        yield return new WaitForSeconds(3f); // Let animation play
        character1.SetActive(false);

        // === Phase 2: Character 2 Appears ===
        character2.SetActive(true);
        GameSoundManager.Instance.PlayChargingSound2();
        yield return new WaitForSeconds(3f);
        character2.SetActive(false);

        // === Phase 3: Both move to center ===
        character1.SetActive(true);
        character2.SetActive(true);

        // Start both characters' center-move animation here if you have one
        GameSoundManager.Instance.PlayChargingSound3();

        // ⏱️ Wait exactly until characters visually reach center
        yield return new WaitForSeconds(2.0f); // <-- adjust this to match timing of movement

        // 💥 Trigger clash sound when they visually meet
        GameSoundManager.Instance.PlayClashingSound();

        // Optional wait for visual impact reaction
        yield return new WaitForSeconds(1f);

        // Hide characters after clash
        character1.SetActive(false);
        character2.SetActive(false);
    }

}
