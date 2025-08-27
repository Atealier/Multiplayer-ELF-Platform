using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSaccadesModule : MonoBehaviour, IBackButtonHandler, IStartButtonHandler
{
    public void OnBackButtonPressed()
    {
        Debug.Log("On back pressend");
        OnlineMultiplayerUIManager.Instance.GoToMenu();
    }

    public void OnStartGameButtonPressed()
    {
        //
    }

}
