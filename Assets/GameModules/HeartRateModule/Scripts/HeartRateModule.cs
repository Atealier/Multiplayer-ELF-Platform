using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartRateModule : MonoBehaviour, IBackButtonHandler
{
    public void OnBackButtonPressed()
    {
        HeartRateUIManager.Instance.GoToMenu();
    }

}
