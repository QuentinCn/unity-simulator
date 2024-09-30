using System.IO;
using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    public Camera cameraToCapture; // Assign your camera in the Inspector
    public int imageWidth = 1920; // Desired image width
    public int imageHeight = 1080; // Desired image height
    
    public void CaptureImage(string savePath)
    { 
        RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        cameraToCapture.targetTexture = renderTexture;

        RenderTexture.active = renderTexture;
        cameraToCapture.Render();

        Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

        texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture.Apply();

        byte[] imageBytes = texture.EncodeToPNG();

        File.WriteAllBytes(savePath, imageBytes);

        cameraToCapture.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        Destroy(texture);
    }
}