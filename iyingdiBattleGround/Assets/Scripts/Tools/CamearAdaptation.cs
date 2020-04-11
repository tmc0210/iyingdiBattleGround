using UnityEngine;

public class CamearAdaptation : MonoBehaviour
{
    private float horizontalFOV = 41f;
    void Awake()
    {
#if UNITY_STANDALONE
        float scale = 0.6f;
        int width = 750;
        int height = 1500;
        width = (int)(width * scale);
        height = (int)(height * scale);
        Screen.SetResolution(width, height, false);
#endif
#if UNITY_ANDROID
        Camera.main.fieldOfView = CalcVertivalFOV(horizontalFOV, Camera.main.aspect);
#endif
    }



    private float CalcVertivalFOV(float hFOVInDeg, float aspectRatio)
    {
        float hFOVInRads = hFOVInDeg * Mathf.Deg2Rad;
        float vFOVInRads = 2 * Mathf.Atan(Mathf.Tan(hFOVInRads / 2) / aspectRatio);
        float vFOV = vFOVInRads * Mathf.Rad2Deg;
        return vFOV;
    }
}