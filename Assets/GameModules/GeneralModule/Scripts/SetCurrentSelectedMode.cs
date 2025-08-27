using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCurrentSelectedMode : MonoBehaviour
{
    public GameModule selectedGameModule;
    public OnlineMultiPlayerGameModule selectedOnlineMultiplayerGameModule;


    //private void OnEnable()
    //{
    //    Debug.Log("called set current selected mode");
    //    StartCoroutine(SetModuleWhenReady());

    //}

    //private IEnumerator SetModuleWhenReady()
    //{
    //    // Wait until GeneralModule is ready
    //    while (GeneralModule.Instance == null)
    //        yield return null;

    //    Debug.Log("called set current selected mode after waiting");
    //    GeneralModule.Instance.SetCurrentSelctedMode(gameModule);
    //}

    public void OnGameModuleSelected()
    {
        OnGameModuleSelected(selectedGameModule);
    }

    public void OnGameModuleSelected(GameModule gameModule)
    {
        GeneralModule.Instance.SetCurrentSelctedMode(gameModule);

        if(selectedOnlineMultiplayerGameModule != OnlineMultiPlayerGameModule.None)
        {
            GeneralModule.Instance.OnlineMultiplayerGameModulesUI[(int)selectedOnlineMultiplayerGameModule - 1].SetActive(true);
        }

        GamePortalManager.Instance.GamePortalPanel.SetActive(false);
    }

}
