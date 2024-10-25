using System.IO;
using UnityEngine;

public class CameraCapture
{
    public static void CaptureImage(string savePath, Camera cameraToCapture, int imageWidth, int imageHeight, int blurAmountPercent, int maxBlurPercent)
    { 
        RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        cameraToCapture.targetTexture = renderTexture;

        RenderTexture.active = renderTexture;
        cameraToCapture.Render();

        Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

        texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture.Apply();

        System.Random r = new System.Random();

        if (r.Next(0, 100) <= blurAmountPercent)
        {
            texture = TextureBlurrer.BlurTexture(texture, r.Next(0, maxBlurPercent));
        }
        
        byte[] imageBytes = texture.EncodeToPNG();

        File.WriteAllBytes(savePath, imageBytes);

        cameraToCapture.targetTexture = null;
        RenderTexture.active = null;
        // Destroy(renderTexture);
        // Destroy(texture);
    }
}