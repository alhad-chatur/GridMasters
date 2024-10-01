using UnityEngine;

public class ResolutionChanger : MonoBehaviour
{
    public Camera projectionCamera;
    public float aspectRatio = 9.0f / 16.0f; // Portrait aspect ratio

    void Start()
    {
        // Calculate the desired FOV
        float desiredFOV = CalculateFOV(Screen.width, Screen.height);

        // Set the FOV of the projection camera
        projectionCamera.fieldOfView = desiredFOV;
    }

    float CalculateFOV(int screenWidth, int screenHeight)
    {
        // Calculate the aspect ratio of the screen
        float screenAspect = (float)screenWidth / (float)screenHeight;

        // Calculate the horizontal FOV based on grid dimensions and aspect ratio
        float hFOV = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(projectionCamera.fieldOfView * Mathf.Deg2Rad / 2) * aspectRatio);

        // Calculate the vertical FOV based on grid dimensions
        float vFOV = hFOV / screenAspect;

        return vFOV;
    }
}
