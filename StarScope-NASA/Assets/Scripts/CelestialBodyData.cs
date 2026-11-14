using UnityEngine;

public class CelestialBodyData : MonoBehaviour
{
    [Header("Basic Properties")]
    public string bodyName;
    // Mass in kg
    public float massKg;
    // Mean radius in km
    public float planetRadiusKm;
    // Gravitational parameter in km^3/s^
    public float gravitationalParameter;

    [Header("Orbital Hierarchy")]
    // The body this object orbits
    public CelestialBodyData parentBody;

    // Approximate SOI radius in km
    public double sphereOfInfluenceKm;
    [Header("Orbit Design Limits")]
    public float minPeriapsisAltitudeKm;
    public float maxPeriapsisAltitudeKm;

    public float minApoapsisAltitudeKm;
    public float maxApoapsisAltitudeKm;

    public float minInclinationDegrees = 0f;
    public float maxInclinationDegrees = 180f;

}
