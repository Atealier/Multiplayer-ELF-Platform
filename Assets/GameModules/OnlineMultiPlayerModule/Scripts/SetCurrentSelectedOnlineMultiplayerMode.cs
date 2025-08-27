using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SetCurrentSelectedOnlineMultiplayerMode : MonoBehaviour
{
    public OnlineMultiPlayerGameModule onlineMultiplayerGameModule;
    public GameObject[] GamePanels;
    public GameObject[] ErrorMsg;
    public Button HostBtn,JoinBtn;

    private void OnEnable()
    {
        Debug.Log("called set current selected multiplayer mode");
        StartCoroutine(SetOnlineMultiPlayerModuleWhenReady());

    }

    private IEnumerator SetOnlineMultiPlayerModuleWhenReady()
    {
        // Wait until GeneralModule is ready
        while (GeneralModule.Instance == null)
            yield return null;

        Debug.Log("called set current selected online multiplayer mode after waiting");
        GeneralModule.Instance.SetCurrentSelctedOnlineMultiplayerMode(onlineMultiplayerGameModule, GamePanels, ErrorMsg, HostBtn, JoinBtn);

    }
}
