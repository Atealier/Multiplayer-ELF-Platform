using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaccadesModuleHandler
{
    void LastPlayerRemoved();
    void SetSaccadeScoreValue(int scoreValue);
    int GetAvtarBorderHighlightThreshold();
    int GetAvtarRoundParticleThreshold();
    int GetAvtarFlowParticleThreshold();
}
