using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePortalManager : MonoBehaviour
{
    public static GamePortalManager Instance { get; set; }

    public GameObject GamePortalPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void OnGameModuleSelected(GameModule gameModule)
    {
        GeneralModule.Instance.SetCurrentSelctedMode(gameModule);
    }

    public void ReturnToGamePortalPanel()
    {
        GeneralModule.Instance.DisableCurrentSelectedModule();
        GamePortalPanel.SetActive(true);

    }
}
