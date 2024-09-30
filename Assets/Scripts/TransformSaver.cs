using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TextCore;

public class TransformSaver : MonoBehaviour
{
    public float timeOffset = 1f;
    public LineRenderer rightLine;
    public LineRenderer leftLine;
    public string path;
    public bool save;
    public bool overwrite = false;

    private Transform _cameraTransform;

    private string AddLineRendererPositions(LineRenderer line, string lineName)
    {
        string content = lineName + "\n";
        ;
        Vector3[] points = new Vector3[line.positionCount];
        line.GetPositions(points);
        for (int i = 0; i < points.Length; i++)
        {
            content += points[i].x + "," + points[i].y + "," + points[i].z + "\n";   
        }
        return content;
    }
    
    void Start()
    {
        _cameraTransform = gameObject.transform;
        
        // string content = AddLineRendererPositions(rightLine, "RIGHT_LINE");
        // content += "\n";
        // content += AddLineRendererPositions(leftLine, "LEFT_LINE");
        // content += "\n";

        CsvReader csvReader = gameObject.AddComponent<CsvReader>();
        List<int[]> oldContent = csvReader.GetCsvInfos(path);

        string content = "";

        if (oldContent == null || (overwrite && save))
        {
            File.WriteAllText(path, content);
        }
        
        if (save)
            StartCoroutine(DoActionEveryTimeOffset());
    }
    
    IEnumerator DoActionEveryTimeOffset()
    {
        string content = overwrite ? "CAMERA\n" : "";
        int batchSize = 10;
        int transformReady = 0;
        
        while (true)
        {
            if (transformReady >= batchSize)
            {
                File.AppendAllText(path, content);
                
                content = "";
                transformReady = 0;
            }
            
            content += (int)_cameraTransform.position.x + "," +
                       (int)_cameraTransform.position.y + "," +
                       (int)_cameraTransform.position.z + "," +
                       (int)_cameraTransform.eulerAngles.x + "," +
                       (int)_cameraTransform.eulerAngles.y + "," +
                       (int)_cameraTransform.eulerAngles.z + "\n";
            transformReady++;

            yield return new WaitForSeconds(timeOffset);
        }
    }
}
