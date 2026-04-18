using UnityEngine;

public class WaveClickInput : MonoBehaviour
{
    public Camera cam;
    public WaveSimulation sim;
    public WaveDemoController demo;

    public float impulseStrength = 0.8f;
    public int rippleRadius = 3;
    public float falloff = 1f;

    void Update()
    {
        // try to auto-assign missing references
        if (cam == null)
            cam = Camera.main;
        if (sim == null)
            sim = FindObjectOfType<WaveSimulation>();
        if (demo == null)
            demo = FindObjectOfType<WaveDemoController>();

        if (demo == null || sim == null || cam == null)
        {
            // missing required references - nothing to do
            return;
        }

        // Disable clicks in other modes
        if (demo.GetMode() != WaveDemoController.Mode.Interactive)
            return;

        // register clicks on press
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 point = hit.point;

            int cx = Mathf.Clamp(Mathf.RoundToInt(point.x), 1, sim.size - 2);
            int cy = Mathf.Clamp(Mathf.RoundToInt(point.z), 1, sim.size - 2);

            AddRipple(cx, cy);
        }
    }

    void AddRipple(int centerX, int centerY)
    {
        for (int dx = -rippleRadius; dx <= rippleRadius; dx++)
        {
            for (int dy = -rippleRadius; dy <= rippleRadius; dy++)
            {
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= rippleRadius)
                {
                    float strength = impulseStrength * Mathf.Exp(-falloff * dist);
                    sim.AddImpulse(centerX + dx, centerY + dy, strength);
                }
            }
        }
    }
}