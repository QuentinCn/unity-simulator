using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class CsvReader : MonoBehaviour
{
    public List<int[]> GetCsvInfos(string filePath)
    {
        try
        {
            string content = File.ReadAllText(filePath);

            if (content == "")
                return null;
            
            string pattern = @"CAMERA\n((?:-?\d+,-?\d+,-?\d+,-?\d+,-?\d+,-?\d+\n*)+)";
            Match match = Regex.Match(content, pattern);

            if (match.Success)
            {
                string allFile = match.Groups[0].Value;
                string cameraPositions = match.Groups[1].Value;

                if (content.Replace(allFile, "") != "")
                {
                    Debug.LogError("File format not right");
                    return null;
                }

                List<int[]> cameraPositionsIntArray = ParseLineData(cameraPositions);

                return cameraPositionsIntArray;
            }
            Debug.LogError("File format not right");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading Csv file: {ex.Message}");
            return null;
        }
    }

    private List<int[]> ParseLineData(string lineData)
    {
        List<int[]> result = new List<int[]>();
        string[] lines = lineData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string[] values = line.Split(',');
            int[] intValues = Array.ConvertAll(values, int.Parse);
            result.Add(intValues);
        }
        return result;
    }
}
