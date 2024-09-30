using System.IO;
using UnityEngine;

public class CsvMatrix : MonoBehaviour
{
    public Camera cameraToCapture;
    public LayerMask object1Layer;
    public LayerMask object2Layer;
    public float rayDistance = 100f;
    public int imageWidth;
    public int imageHeight;

    private int[,] _objectMatrix;

    void CreateObjectMatrix()
    {
        _objectMatrix = new int[imageWidth, imageHeight];
        for (int y = 0; y < imageHeight; y++)
        {
            for (int x = 0; x < imageWidth; x++)
            {
                float normalizedX = (float)x / (float)imageWidth;
                float normalizedY = (float)y / (float)imageHeight;

                Ray ray = cameraToCapture.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0));

                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
                {
                    if ((object1Layer.value & (1 << hit.collider.gameObject.layer)) > 0)
                    {
                        _objectMatrix[x, y] = 1;
                    }
                    else if ((object2Layer.value & (1 << hit.collider.gameObject.layer)) > 0)
                    {
                        _objectMatrix[x, y] = 2;
                    }
                    else
                    {
                        _objectMatrix[x, y] = 0;
                    }
                }
                else
                {
                    _objectMatrix[x, y] = 0;
                }
            }
        }
    }

    public int[,] GetObjectMatrix()
    {
        CreateObjectMatrix();
        return _objectMatrix;
    }

    void MirrorMatrixY(int[,] matrix)
    {
        for (int y = 0; y < imageHeight / 2; y++)
        {
            for (int x = 0; x < imageWidth; x++)
            {
                (matrix[x, y], matrix[x, imageHeight - y - 1]) = (matrix[x, imageHeight - y - 1], matrix[x, y]);
            }
        }
    }

    public void SaveMatrixToCsv(string saveFilePath)
    {
        CreateObjectMatrix();
        MirrorMatrixY(_objectMatrix);

        using (StreamWriter writer = new StreamWriter(saveFilePath))
        {
            for (int y = 0; y < imageHeight; y++)
            {
                string line = "";
                for (int x = 0; x < imageWidth; x++)
                {
                    line += _objectMatrix[x, y].ToString();

                    if (x < imageWidth - 1)
                    {
                        line += ",";
                    }
                }

                writer.WriteLine(line);
            }
        }
    }}
