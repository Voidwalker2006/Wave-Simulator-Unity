using UnityEngine;

public class WaveDemoController : MonoBehaviour
{
    public WaveSimulation sim;

    [Header("Wave Source Settings")]
    public float sourceAmplitude = 0.02f;
    public float sourceFrequency = 1.6f;

    [Header("Source Positions")]
    public Vector2Int source1 = new Vector2Int(38, 50);
    public Vector2Int source2 = new Vector2Int(62, 50);

    private enum DemoMode
    {
        None,
        SingleSource,
        Interference,
        Diffraction
    }

    private DemoMode currentMode = DemoMode.None;
    private float timer = 0f;

    void Update()
    {
        if (sim == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentMode = DemoMode.SingleSource;
            timer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentMode = DemoMode.Interference;
            timer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentMode = DemoMode.Diffraction;
            timer = 0f;
            sim.ResetSimulation();
            sim.CreateSlit(); // 🔥 activate diffraction
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentMode = DemoMode.None;
            timer = 0f;
            sim.ResetSimulation();
        }

        timer += Time.deltaTime;

        if (currentMode == DemoMode.SingleSource)
        {
            EmitSource(source1);
        }
        else if (currentMode == DemoMode.Interference)
        {
            EmitSource(source1);
            EmitSource(source2);
        }
        else if (currentMode == DemoMode.Diffraction)
        {
            EmitSource(source1);
        }
    }

    void EmitSource(Vector2Int source)
    {
        float wave = Mathf.Sin(timer * sourceFrequency * Mathf.PI * 2f);
        float strength = wave * sourceAmplitude;

        sim.SetSource(source.x, source.y, strength);
    }
}