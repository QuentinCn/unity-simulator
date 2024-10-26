using System.IO;
using UnityEngine;

public class Mask
{
    static int[,] CreateObjectMatrix(int matrixWidth, int matrixHeight, float rayDistance, Camera cameraToCapture, LayerMask object1Layer, LayerMask object2Layer)
    {
        int[,] objectMatrix = new int[matrixWidth, matrixHeight];
        for (int y = 0; y < matrixHeight; y++)
        {
            for (int x = 0; x < matrixWidth; x++)
            {
                float normalizedX = (float)x / (float)matrixWidth;
                float normalizedY = (float)y / (float)matrixHeight;

                Ray ray = cameraToCapture.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0));

                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
                {
                    if ((object1Layer.value & (1 << hit.collider.gameObject.layer)) > 0)
                    {
                        objectMatrix[x, y] = 1;
                    }
                    else if ((object2Layer.value & (1 << hit.collider.gameObject.layer)) > 0)
                    {
                        objectMatrix[x, y] = 2;
                    }
                    else
                    {
                        objectMatrix[x, y] = 0;
                    }
                }
                else
                {
                    objectMatrix[x, y] = 0;
                }
            }
        }
        return objectMatrix;
    }

    static int[,] MirrorMatrixY(int[,] matrix, int matrixWidth, int matrixHeight)
    {
        for (int y = 0; y < matrixHeight / 2; y++)
        {
            for (int x = 0; x < matrixWidth; x++)
            {
                (matrix[x, y], matrix[x, matrixHeight - y - 1]) = (matrix[x, matrixHeight - y - 1], matrix[x, y]);
            }
        }
        return matrix;
    }
    
    public static int[,] GetObjectMatrix(int matrixWidth, int matrixHeight, float rayDistance, Camera cameraToCapture, LayerMask object1Layer, LayerMask object2Layer)
    {
        return MirrorMatrixY(CreateObjectMatrix(matrixWidth, matrixHeight, rayDistance, cameraToCapture, object1Layer, object2Layer), matrixWidth, matrixHeight);
    }

    public static void SaveMask(string saveFilePath, int matrixWidth, int matrixHeight, float rayDistance, Camera cameraToCapture, LayerMask object1Layer, LayerMask object2Layer)
    {
        int[,] objectMatrix = CreateObjectMatrix(matrixWidth, matrixHeight, rayDistance, cameraToCapture, object1Layer, object2Layer);

        Texture2D texture = new Texture2D(matrixWidth, matrixHeight, TextureFormat.RGBA32, false);

        for (int y = 0; y < matrixHeight; y++)
        {
            for (int x = 0; x < matrixWidth; x++)
            {
                Color color;
                switch (objectMatrix[x, y])
                {
                    case 0:
                        color = Color.black;
                        break;
                    case 1:
                        color = Color.white;
                        break;
                    case 2:
                        color = Color.red;
                        break;
                    default:
                        color = Color.clear;
                        break;
                }
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();

        string directory = Path.GetDirectoryName(saveFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(saveFilePath, bytes);
    }
}
