using UnityEngine;

public class WaveDemoController : MonoBehaviour
{
    public WaveSimulation sim;

    [Header("Wave Source Settings")]
    public float sourceAmplitude = 0.004f;
    public float sourceFrequency = 1.6f;

    [Header("Source Positions")]
    public Vector2Int source1 = new Vector2Int(38, 50);
    public Vector2Int source2 = new Vector2Int(62, 50);

    private enum DemoMode
    {
        None,
        SingleSource,
        Interference
    }

    private DemoMode currentMode = DemoMode.None;
    private float timer = 0f;

    void Update()
    {
        if (sim == null) return;

        // Controls
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentMode = DemoMode.SingleSource;
            timer = 0f;
            Debug.Log("Mode: Single Source");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentMode = DemoMode.Interference;
            timer = 0f;
            Debug.Log("Mode: Interference");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentMode = DemoMode.None;
            timer = 0f;
            sim.ResetSimulation();
            Debug.Log("Simulation Reset");
        }

        timer += Time.deltaTime;

        if (currentMode == DemoMode.SingleSource)
        {
            EmitPeriodicSource(source1);
        }
        else if (currentMode == DemoMode.Interference)
        {
            EmitPeriodicSource(source1);
            EmitPeriodicSource(source2);
        }
    }

    void EmitPeriodicSource(Vector2Int source)
    {
        // Smooth sinusoidal oscillation
        float wave = Mathf.Sin(timer * sourceFrequency * Mathf.PI * 2f);

        // Very small controlled amplitude
        float strength = wave * sourceAmplitude;

        int radius = 1;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= radius)
                {
                    float falloff = Mathf.Exp(-1.2f * dist);
                    sim.AddImpulse(source.x + dx, source.y + dy, strength * falloff);
                }
            }
        }
    }
}