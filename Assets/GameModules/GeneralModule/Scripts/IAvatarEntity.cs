public interface IAvatarEntity
{
    int GetAvatarIndex();
    int GetConsecutiveCorrectAnswers();
    string GetEntityName(); // for debugging/logs
    bool IsHostControlled(); // tells if it's host-controlled
    int GetJoiningNumber(); // to support team logic (even/odd etc.)
    bool ApplyGreenBorder(); // highlight border of avtar in game play character animation
    bool ActivateRoundParticles(); // enable particles around the avtar in game play character animation
    bool ActivateFlowParticles(); // enable particles flow from avtar to character in game play character animation
}
