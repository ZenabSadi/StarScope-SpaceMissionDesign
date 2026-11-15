/*using UnityEngine;
using UnityEngine.UI;

public class OrbitUIController:MonoBehaviour
{
    public Slider periapsisSlider;
    public Slider apoapsisSlider;
    public Slider inclinationSlider;
    public CelestialBodyData celestialBody;

    [Header("Simulation")]
    // 1 Unity unit represents 1000 km
    public float kmPerUnityUnit = 1000f;

    // Simulated seconds per real second
    public float simulationSpeed = 600f;

    public int orbitSegments = 240;
}

*/
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class OrbitUIController : MonoBehaviour
{
    [Header("Scene References")]
    public CelestialBodyData celestialBody;
    public Transform centralBody;
    public Transform spacecraft;
    public LineRenderer orbitLine;

    [Header("UI Sliders")]
    public Slider periapsisSlider;
    public Slider apoapsisSlider;
    public Slider inclinationSlider;
    //public Slider semiMajorAxisSlider;
    //public Slider eccentricitySlider;
    //public Slider inclinationSlider;

    //[Header("Orbit Parameters")]
    //[Min(0.01f)]
    //public float periapsisAltitudeKm = 10f;

    //[Range(0f, 0.99f)]
    //public float apoapsisAltitudeKm = 0.3f;

    [Range(0f, 180f)]
    public float inclination = 0f;

    [Header("Orbit Drawing")]
    [Range(50, 500)]
    public int orbitSegments = 200;

    private float rp, ra;
    public float eccentricity, semiMajorAxis;
    //-------------------------------------------------
    private void Start()
    {

        rp = celestialBody.minPeriapsisAltitudeKm + celestialBody.planetRadiusKm;
        ra = celestialBody.minApoapsisAltitudeKm + celestialBody.planetRadiusKm;
        eccentricity = (ra - rp) / (ra + rp);
        semiMajorAxis = (rp + ra) / 2f;

        // Initialize slider values
        if (periapsisSlider != null)
        {
            periapsisSlider.value = celestialBody.minPeriapsisAltitudeKm;//periapsisAltitudeKm;
            periapsisSlider.onValueChanged.AddListener(SetRp);
        }

        if (apoapsisSlider != null)
        {
            apoapsisSlider.value = celestialBody.minApoapsisAltitudeKm;
            apoapsisSlider.onValueChanged.AddListener(SetRa);
        }

        if (inclinationSlider != null)
        {
            inclinationSlider.value = celestialBody.minInclinationDegrees;
            inclinationSlider.onValueChanged.AddListener(SetInclination);
        }

        DrawOrbit();
    }

    // --------------------------------
    // SLIDER CALLBACKS
    // --------------------------------
    //private float rp, ra;
    //public float eccentricity, semiMajorAxis;
    public void SetRp(float value)
    {
        rp = value+celestialBody.planetRadiusKm;
        eccentricity = (ra - rp) / (ra + rp);
        semiMajorAxis = (rp + ra) / 2f;
        DrawOrbit();
    }

    public void SetRa(float value)
    {
        eccentricity = (ra - rp) / (ra + rp);
        semiMajorAxis = (rp + ra) / 2f;
        ra = value + celestialBody.planetRadiusKm;
        DrawOrbit();
    }

    public void SetInclination(float value)
    {
        inclination = value;
        DrawOrbit();
    }

    // --------------------------------
    // ORBIT CALCULATION
    // --------------------------------

    public void DrawOrbit()
    {
        if (orbitLine == null || centralBody == null)
            return;

        orbitLine.positionCount = orbitSegments;

        for (int j = 0; j < orbitSegments; j++)
        {
            double M = 2.0 * System.Math.PI * j /
                       (orbitSegments - 1);

            Vector3 position = CalculatePositionFromMeanAnomaly(M);

            orbitLine.SetPosition(j, centralBody.position + position);
        }
    }

    // --------------------------------
    // KEPLER ORBIT EQUATION
    // --------------------------------

    private Vector3 CalculatePositionFromMeanAnomaly(double M)
    {
        double e = eccentricity;
        double E = M;

        // Solve Kepler's equation:
        // M = E - e * sin(E)
        if (e!=0)
        {
            for (int i = 0; i < 15; i++)
            {
                double f = E - e * System.Math.Sin(E) - M;

                double fPrime =
                    1.0 - e * System.Math.Cos(E);

                E -= f / fPrime;
            }
        }

        // Position in orbital plane

        double x =
            semiMajorAxis *
            (System.Math.Cos(E) - e);

        double y =
            semiMajorAxis *
            System.Math.Sqrt(1.0 - e * e) *
            System.Math.Sin(E);

        // Apply inclination around the X axis

        double angle = inclination * Mathf.Deg2Rad;

        double rotatedY =
            y * System.Math.Cos(angle);

        double rotatedZ =
            y * System.Math.Sin(angle);

        return new Vector3(
            (float)x,
            (float)rotatedY,
            (float)rotatedZ
        );
    }
}
