using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MoveAroundSavedPos : MonoBehaviour
{
    public string filePath = "";
    public string dirToSave = "";

    private List<int[]> _fileContent;
    private int _id;
    private int _i;
    private CameraCapture _cameraCapture;
    private CsvMatrix _csvMatrix;
    
    void Start()
    {
        CsvReader csvReader = gameObject.AddComponent<CsvReader>();
        _fileContent = csvReader.GetCsvInfos(filePath);
        
        _i = 0;
        _id = Directory.EnumerateFiles(dirToSave).Count();
        
        _cameraCapture = gameObject.GetComponent<CameraCapture>();
    }

    void Update()
    {
        Vector3 position = new Vector3(_fileContent[_i][0], _fileContent[_i][1], _fileContent[_i][2]);
        Vector3 eulerAngles = new Vector3(_fileContent[_i][3], _fileContent[_i][4], _fileContent[_i][5]);

        gameObject.transform.SetPositionAndRotation(position, Quaternion.Euler(eulerAngles));
        
        _cameraCapture.CaptureImage(dirToSave + "/" + _id + ".png");
        _csvMatrix.SaveMatrixToCsv(dirToSave + "/" + _id + ".csv");
        
        _id++;
        _i++;

        if (_i >= _fileContent.Count && EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            Debug.Log("Play mode stopped.");
        }
    }
}
