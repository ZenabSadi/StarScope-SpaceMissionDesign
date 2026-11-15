using UnityEngine;
using UnityEngine.UI;

public class OrbitController : MonoBehaviour
{
    [Header("Scene References")]
    public CelestialBodyData celestialBody;
    public Transform centralBody;
    public Transform spacecraft;
    public LineRenderer orbitLine;

    /*public Slider periapsisSlider;
    public Slider apoapsisSlider;
    public Slider inclinationSlider;*/


    //[Header("Planet Properties")]
    // Mars values by default
    //public float planetRadiusKm = 3389.5f;

    // Mars gravitational parameter, km^3/s^2
    //public float gravitationalParameter = 42828.375214f;

    [Header("Orbit Design")]
    public float periapsisAltitudeKm = 250f;
    public float apoapsisAltitudeKm = 1000f;
    public float inclinationDegrees = 45f;
    public float initialTrueAnomalyDegrees = 0f;

    [Header("Simulation")]
    // 1 Unity unit represents 1000 km
    public float kmPerUnityUnit = 1000f;

    // Simulated seconds per real second
    public float simulationSpeed = 600f;

    public int orbitSegments = 240;
    
    // Calculated orbital parameters
    [Header("Calculated Values (Read Only)")]
    public float semiMajorAxisKm;
    public float eccentricity;
    public float orbitalPeriodSeconds;
    public float periapsisVelocityKmPerSecond;
    public float apoapsisVelocityKmPerSecond;

    private double currentMeanAnomaly;
    private double initialMeanAnomaly;
    private double elapsedSimulationTime;

    private float meanMotion;


    void Start()
    {
        //periapsisAltitudeKm = celestialBody.minPeriapsisAltitudeKm;
        //apoapsisAltitudeKm = celestialBody.minApoapsisAltitudeKm;
        //inclinationDegrees = celestialBody.minInclinationDegrees;
        RebuildOrbit();
    }

    void Update()
    {
        if (spacecraft == null)
            return;

        // Advance simulated time
        elapsedSimulationTime +=
            Time.deltaTime * simulationSpeed;

        // Calculate mean anomaly
        currentMeanAnomaly =
            initialMeanAnomaly +
            meanMotion * elapsedSimulationTime;

        // Calculate spacecraft position
        Vector3 position =
            CalculatePositionFromMeanAnomaly(
                currentMeanAnomaly);

        spacecraft.position =
            centralBody.position + position;
    }
    public void RebuildOrbit()
    {
        if (celestialBody == null ||
           spacecraft == null ||
           orbitLine == null)
        {
            Debug.LogError(
                "Assign all scene references in the Inspector.");
            return;
        }

        if (kmPerUnityUnit <= 0f ||
            celestialBody.gravitationalParameter <= 0f ||
            celestialBody.planetRadiusKm <= 0f ||
            orbitSegments < 12)
        {
            Debug.LogError("celestialBody.gravitationalParameter = "+ celestialBody.gravitationalParameter);
            Debug.LogError("celestialBody.planetRadiusKm = " + celestialBody.planetRadiusKm);
            Debug.LogError("Invalid orbit settings.");
            return;
        }

        float rp =
            celestialBody.planetRadiusKm + periapsisAltitudeKm;

        float ra =
            celestialBody.planetRadiusKm + apoapsisAltitudeKm;

        if (periapsisAltitudeKm < 0f ||
            apoapsisAltitudeKm < periapsisAltitudeKm)
        {
            Debug.LogError(
                "Check periapsis and apoapsis altitudes.");
            return;
        }

        // Calculate semi-major axis
        semiMajorAxisKm = (rp + ra) / 2f;

        // Calculate eccentricity
        eccentricity = (ra - rp) / (ra + rp);

        if (eccentricity >= 1f || eccentricity < 0f)
        {
            Debug.LogError(
                "This example supports elliptical orbits only.");
            return;
        }

        // Calculate orbital period
        double mu = celestialBody.gravitationalParameter;
        double a = semiMajorAxisKm;

        orbitalPeriodSeconds =
            (float)(2.0 * System.Math.PI *
            System.Math.Sqrt(a * a * a / mu));

        // Calculate periapsis and apoapsis velocities
        periapsisVelocityKmPerSecond =
            Mathf.Sqrt(
                celestialBody.gravitationalParameter *
                (2f / rp - 1f / semiMajorAxisKm));

        apoapsisVelocityKmPerSecond =
            Mathf.Sqrt(
                celestialBody.gravitationalParameter *
                (2f / ra - 1f / semiMajorAxisKm));

        // Mean motion (radians per second)
        meanMotion =
            Mathf.Sqrt(
                celestialBody.gravitationalParameter /
                (semiMajorAxisKm *
                 semiMajorAxisKm *
                 semiMajorAxisKm));

        // Convert initial true anomaly to eccentric anomaly
        double trueAnomaly =
            initialTrueAnomalyDegrees * Mathf.Deg2Rad;

        double e = eccentricity;

        double eccentricAnomaly =
            2.0 * System.Math.Atan2(
                System.Math.Sqrt(1.0 - e) *
                System.Math.Sin(trueAnomaly / 2.0),

                System.Math.Sqrt(1.0 + e) *
                System.Math.Cos(trueAnomaly / 2.0)
            );

        // Convert eccentric anomaly to mean anomaly
        initialMeanAnomaly =
            eccentricAnomaly -
            e * System.Math.Sin(eccentricAnomaly);

        elapsedSimulationTime = 0;
        currentMeanAnomaly = initialMeanAnomaly;

        DrawOrbit();

        spacecraft.position =
            centralBody.position +
            CalculatePositionFromMeanAnomaly(
                currentMeanAnomaly);

        Debug.Log(
            $"Semi-major axis: {semiMajorAxisKm:F2} km\n" +
            $"Eccentricity: {eccentricity:F4}\n" +
            $"Orbital period: {orbitalPeriodSeconds / 60f:F2} minutes\n" +
            $"Periapsis velocity: {periapsisVelocityKmPerSecond:F3} km/s\n" +
            $"Apoapsis velocity: {apoapsisVelocityKmPerSecond:F3} km/s"
        );
    }

    void DrawOrbit()
    {
        Vector3[] points =
            new Vector3[orbitSegments + 1];

        float a = semiMajorAxisKm;
        float e = eccentricity;

        for (int j = 0; j <= orbitSegments; j++)
        {
            float E =
                2f * Mathf.PI * j / orbitSegments;

            // Position in the orbital plane
            float x =
                a * (Mathf.Cos(E) - e);

            float z =
                a * Mathf.Sqrt(1f - e * e) *
                Mathf.Sin(E);

            Vector3 orbitalPosition =
                new Vector3(x, 0f, z);

            // Rotate the orbital plane by inclination
            Quaternion inclinationRotation =
                Quaternion.Euler(
                    inclinationDegrees, 0f, 0f);

            orbitalPosition =
                inclinationRotation * orbitalPosition;

            // Convert km to Unity units
            points[j] =
                centralBody.position +
                orbitalPosition / kmPerUnityUnit;
        }

        orbitLine.positionCount = points.Length;
        orbitLine.SetPositions(points);
        orbitLine.loop = true;
        orbitLine.useWorldSpace = true;
    }

    Vector3 CalculatePositionFromMeanAnomaly(double M)
    {
        double e = eccentricity;

        // Solve Kepler's equation:
        // M = E - e * sin(E)

        double E = M;

        if (e != 0) 
        { 

            for (int i = 0; i < 10; i++)
            {
                double f =
                    E - e * System.Math.Sin(E) - M;

                double fPrime =
                    1.0 - e * System.Math.Cos(E);

                E -= f / fPrime;
            }
        }

        // Calculate position in orbital plane
        double x =
            semiMajorAxisKm *
            (System.Math.Cos(E) - e);

        double z =
            semiMajorAxisKm *
            System.Math.Sqrt(1.0 - e * e) *
            System.Math.Sin(E);

        Vector3 position =
            new Vector3((float)x, 0f, (float)z);

        // Apply inclination
        Quaternion rotation =
            Quaternion.Euler(
                inclinationDegrees, 0f, 0f);

        position = rotation * position;

        // Convert km to Unity units
        return position / kmPerUnityUnit;
    }

}
