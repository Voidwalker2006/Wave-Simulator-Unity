using UnityEngine;

public class WaveDemoController : MonoBehaviour
{
    public WaveSimulation sim;

        [Header("Single Pulse Settings")]
        public float singlePulseAmplitude = 5f;
        public Vector2Int singlePulsePosition = new Vector2Int(-1, -1);

        [Header("Double Slit Settings")]
        public int slitWallX = -1; // -1 means center
        public int slitSeparation = 6;
        public int slitHeight = 3;
        public float slitSourceFrequency = 3f;
        public float slitSourceAmplitude = 0.5f;

    public enum Mode
    {
        None,
        SinglePulse,
        Interactive,
        Diffraction
    }

    private Mode currentMode = Mode.None;

    void Update()
    {
        if (sim == null) return;

        // 🔹 MODE SWITCHING
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetSinglePulseMode();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetInteractiveMode();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetDiffractionMode();
        }

        // 🔹 CONTINUOUS SOURCE FOR DIFFRACTION (looks better)
        if (currentMode == Mode.Diffraction)
        {
            int wallX = (slitWallX >= 0) ? slitWallX : sim.size / 2;

            // slits were created at wallX; compute slit centers
            int centerY = sim.size / 2;
            int sep = Mathf.Abs(slitSeparation);
            int slit1Y = centerY - sep;
            int slit2Y = centerY + sep;

            int sourceX = Mathf.Clamp(wallX - 1, 1, sim.size - 2); // just before the wall

            float wave = Mathf.Sin(Time.time * slitSourceFrequency * Mathf.PI * 2f) * slitSourceAmplitude;

            sim.SetSource(sourceX, Mathf.Clamp(slit1Y, 1, sim.size - 2), wave);
            sim.SetSource(sourceX, Mathf.Clamp(slit2Y, 1, sim.size - 2), wave);
        }
    }

    // 🔹 MODE 1: SINGLE PULSE
    void SetSinglePulseMode()
    {
        currentMode = Mode.SinglePulse;
        sim.ResetSimulation();

        int cx = (singlePulsePosition.x >= 0) ? singlePulsePosition.x : sim.size / 2;
        int cy = (singlePulsePosition.y >= 0) ? singlePulsePosition.y : sim.size / 2;

        sim.AddImpulse(cx, cy, singlePulseAmplitude);
    }

    // 🔹 MODE 2: INTERACTIVE
    void SetInteractiveMode()
    {
        currentMode = Mode.Interactive;
        sim.ResetSimulation();
    }

    // 🔹 MODE 3: DIFFRACTION
    void SetDiffractionMode()
    {
        currentMode = Mode.Diffraction;

        sim.ResetSimulation();
        sim.CreateDoubleSlit(slitWallX, slitSeparation, slitHeight);

        int wallX = (slitWallX >= 0) ? slitWallX : sim.size / 2;
        int centerY = sim.size / 2;
        int sep = Mathf.Abs(slitSeparation);
        int slit1Y = centerY - sep;
        int slit2Y = centerY + sep;

        int sourceX = Mathf.Clamp(wallX - 1, 1, sim.size - 2);

        // give an initial coherent kick at both slits to seed the interference
        sim.AddImpulse(sourceX, Mathf.Clamp(slit1Y, 1, sim.size - 2), singlePulseAmplitude);
        sim.AddImpulse(sourceX, Mathf.Clamp(slit2Y, 1, sim.size - 2), singlePulseAmplitude);
    }

    // 🔹 USED BY CLICK SCRIPT
    public Mode GetMode()

    {
        return currentMode;
    }
}