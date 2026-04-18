using UnityEngine;
using TMPro;

public class WaveDemoController : MonoBehaviour
{
    public WaveSimulation sim;
    public TextMeshProUGUI modeText;

    [Header("Wave Source Settings")]
    public float sourceAmplitude = 0.02f;
    public float sourceFrequency = 1.6f;
    [Tooltip("Radius (in grid cells) around the source that receives a smoothed value")]
    public int sourceRadius = 2;

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

    void UpdateModeUI()
    {
        if (modeText != null)
            modeText.text = "Mode: " + currentMode.ToString();
    }
    public void SetSingleSource()
    {
        currentMode = DemoMode.SingleSource;
        timer = 0f;
        UpdateModeUI();
    }
    public void SetInterference()
    {
        currentMode = DemoMode.Interference;
        timer = 0f;
        UpdateModeUI();
    }

    public void SetDiffraction()
    {
        currentMode = DemoMode.Diffraction;
        timer = 0f;
        sim.ResetSimulation();
        sim.CreateSlit();
        UpdateModeUI();
    }

    public void ResetSimulationUI()
    {
        currentMode = DemoMode.None;
        timer = 0f;
        sim.ResetSimulation();
        UpdateModeUI();
    }
    void Update()
    {
       
        if (sim == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentMode = DemoMode.SingleSource;
            timer = 0f;
            UpdateModeUI();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentMode = DemoMode.Interference;
            timer = 0f;
            UpdateModeUI();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentMode = DemoMode.Diffraction;
            timer = 0f;
            sim.ResetSimulation();
            sim.CreateSlit(); // activate diffraction
            UpdateModeUI();
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

        // Distribute the source strength over a small radius with a smooth cosine falloff
        int r = Mathf.Max(0, sourceRadius);
        for (int oy = -r; oy <= r; oy++)
        {
            for (int ox = -r; ox <= r; ox++)
            {
                float dist = Mathf.Sqrt(ox * ox + oy * oy);
                if (dist > r) continue;

                float t = (r == 0) ? 0f : Mathf.Clamp01(dist / (float)r);
                // cosine falloff: 1 at center, 0 at radius
                float weight = Mathf.Cos(t * Mathf.PI) * 0.5f + 0.5f;

                int tx = source.x + ox;
                int ty = source.y + oy;

                sim.SetSource(tx, ty, strength * weight);
            }
        }
    }
}