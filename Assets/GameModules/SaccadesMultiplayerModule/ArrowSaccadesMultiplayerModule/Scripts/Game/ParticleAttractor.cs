using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractor : MonoBehaviour
{
    [Header("Target in World")]
    public Transform worldTarget; // lion_boy or any character

    [Header("Source in UI")]
    public RectTransform avatarUI; // UI RectTransform of avatar

    [Header("Conversion")]
    public Camera uiCamera; // UI camera (for Screen Space - Camera)

    private Transform emitterTransform;
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    [Header("Attraction Settings")]
    public float attractionSpeed = 5f;

    void Start()
    {
        emitterTransform = this.transform;
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void Update()
    {
        if (avatarUI == null || worldTarget == null || uiCamera == null)
            return;

        // 1. Convert UI avatar position to world space
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, avatarUI.position);
        screenPos.z = 1f; // Adjust depth as needed
        Vector3 worldPos = uiCamera.ScreenToWorldPoint(screenPos);

        // 2. Move particle system to match avatar's position in world space
        emitterTransform.position = worldPos;

        // 3. Attract each particle to the world target
        int aliveCount = ps.GetParticles(particles);
        float step = attractionSpeed * Time.deltaTime;

        for (int i = 0; i < aliveCount; i++)
        {
            particles[i].position = Vector3.LerpUnclamped(particles[i].position, worldTarget.position, step);
        }

        ps.SetParticles(particles, aliveCount);
    }
}
