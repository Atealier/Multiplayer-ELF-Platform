using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject ParticleSystem;
    [SerializeField] private GameObject Rope;
    [SerializeField] private GameObject teamWinParticle;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnEnable()
    {
        Rope.SetActive(true);
        
    }

    public void StartPullingAnimation()
    {
        animator.SetBool("pulling", true);
    }

    public void UpdateAnimationState(int scoreDifference, bool isWinner)
    {
        Debug.Log("scoreDifference " + scoreDifference);

        if(scoreDifference < 10)
        {
            animator.SetBool("diff10w", false);
            animator.SetBool("diff10l", false);
        }
        else if(scoreDifference == 10 && isWinner)
        {
            animator.SetBool("diff20l", false);
            animator.SetBool("diff30l", false);
            animator.SetBool("diff10l", false);
            animator.SetBool("diff10w", true);
            animator.SetBool("diff20w", false);
        }
        else if (scoreDifference == 10 && !isWinner)
        {
            animator.SetBool("diff10l", true);
            animator.SetBool("diff20l", false);
            animator.SetBool("diff30l", false);
            animator.SetBool("diff30w", false);
            animator.SetBool("diff20w", false);
            animator.SetBool("diff10w", false);
        }
        else if(scoreDifference == 20 && isWinner)
        {
            animator.SetBool("diff10w", true);
            animator.SetBool("diff20w", true);
            animator.SetBool("diff30w", false);
        }
        else if (scoreDifference == 20 && !isWinner)
        {            
            animator.SetBool("diff20l", true);
            animator.SetBool("diff30l", false);
        }
        else if(scoreDifference == 30 && isWinner)
        {
            animator.SetBool("diff10w", true);
            animator.SetBool("diff20w", true);
            animator.SetBool("diff30w", true);
        }
        else if (scoreDifference == 30 && !isWinner)
        {
            animator.SetBool("diff20l", true);
            animator.SetBool("diff30l", true);
        }
                
    }

   

    public void UpdateAnimationBasedWinner(string flag, string winTeam)
    {
        Rope.SetActive(false);
        Debug.Log("Called winnter animation");
        if (flag == "win")
        {
            animator.SetBool("win", true);

            if (winTeam == "Team1")
                teamWinParticle.SetActive(true);
            else if (winTeam == "Team2")
                teamWinParticle.SetActive(true);
            else if (winTeam == "Draw")
            {
                teamWinParticle.SetActive(true);
                teamWinParticle.SetActive(true);
            }
        }
        else
        {
            animator.SetBool("lose", true);
        }


        //  Deactivate all "Particle attractor 004 soft noise"
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true); // include inactive

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Particle attractor 004 soft noise" && obj.activeSelf)
            {
                obj.SetActive(false);
                Debug.Log("Deactivated: " + obj.name);
            }
        }

        //animator.SetBool("diff30l", false);
        //animator.SetBool("diff20l", false);
        //animator.SetBool("diff30w", false);
        //animator.SetBool("diff20w", false);
        //animator.SetBool("diff10w", false);
        //animator.SetBool("consecutive", false);
    }

    public void UpdateAnimationBasedConsecutiveAnswers()
    {   
        animator.SetBool("consecutive", true);
        ParticleSystem.SetActive(true);
        StartCoroutine(DisableParticleAfterDelay());
                        
    }
    public void UpdateAnimationBasedHalfConsecutiveAnswers()
    {
        if(GeneralModule.Instance.CurrentGameModule != GameModule.HeartRate)
        {
            animator.SetBool("consecutive", true);
        }
        StartCoroutine(DisableParticleAfterDelay());
    }


    private IEnumerator DisableParticleAfterDelay()
    {
        yield return new WaitForSeconds(8f);
        ParticleSystem.SetActive(false);
        animator.SetBool("consecutive", false);
    }

    private void OnDisable()
    {
        teamWinParticle.SetActive(false);
        
    }
}
