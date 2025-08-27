using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractorLinear : MonoBehaviour
{
    /* ───────────────────── public settings ───────────────────── */
    [Header("Target (choose ONE)")]
    public Transform targetWorld;
    public RectTransform targetUI;

    [Header("Canvas (auto‑found if left null)")]
    public Canvas rootCanvas;

    [Header("Timing (seconds)")]
    public float switchAfterSeconds = 2f;   // ← 2‑second mark

    [Header("Orbital Z")]
    public float orbitalZBefore = 10f;      // 0‑2 s
    public float orbitalZAfter = 0f;       // ≥2 s

    [Header("Speed")]
    public float speedBefore = 0f;          // 0‑2 s
    public float speedAfter = 0.00f;        // ≥2 s

    /* ───────────────────── private state ─────────────────────── */
    ParticleSystem ps;
    ParticleSystem.Particle[] buffer;
    RectTransform canvasRect;
    float timer;
    bool switched;   // tracks whether we already crossed 2 s

    /* ───────────────────── Unity lifecycle ───────────────────── */
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        // simulate in local (UI) space
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        // cache canvas
        if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();
        canvasRect = rootCanvas.transform as RectTransform;

        buffer = new ParticleSystem.Particle[ps.main.maxParticles];
        timer = 0f;
        switched = false;

        /* set the initial orbital Z = 10 */
        var vol = ps.velocityOverLifetime;
        vol.enabled = true;
        vol.orbitalZ = new ParticleSystem.MinMaxCurve(orbitalZBefore);
    }
    void LateUpdate()
    {
        if (!ps || (!targetWorld && !targetUI)) return;

        // ── 1) get current target position in canvas local space ─────────────── 
        Vector2 localTarget;
        if (targetUI)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(null, targetUI.position),
                null,
                out localTarget);
        }
        else
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(targetWorld.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screen, null, out localTarget);
        }

        // ── 2) timing switch ──────────────────────────────────────────────────
        timer += Time.deltaTime;

        if (!switched && timer >= switchAfterSeconds)
        {
            var vol = ps.velocityOverLifetime;
            vol.orbitalZ = new ParticleSystem.MinMaxCurve(orbitalZAfter);
            switched = true;
        }

        // ── 3) move live particles toward the current target position ─────────
        float currentSpeed = switched ? speedAfter : speedBefore;
        int alive = ps.GetParticles(buffer);
        float step = currentSpeed * Time.deltaTime;

        for (int i = 0; i < alive; i++)
        {
            // Convert the current frame's target position into particle system local space
            Vector3 psLocalTarget = transform.InverseTransformPoint(
                canvasRect.TransformPoint(localTarget)
            );

            // Preserve original z so we don’t break depth
            psLocalTarget.z = buffer[i].position.z;

            // Smoothly move the particle toward the live target
            buffer[i].position = Vector3.LerpUnclamped(buffer[i].position, psLocalTarget, step);
        }

        ps.SetParticles(buffer, alive);
    }


    public void ApplyOrbitalZAfter()
    {
        var ps = GetComponent<ParticleSystem>();
        var vol = ps.velocityOverLifetime;
        vol.orbitalZ = new ParticleSystem.MinMaxCurve(orbitalZAfter);
    }
}