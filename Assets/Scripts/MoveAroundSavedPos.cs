using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MoveAroundSavedPos : MonoBehaviour
{
    #region PublicVariables

    public string version = "";
    public LayerMask outsideLayer;
    public LayerMask insideLayer;
    public GameObject ground;

    #endregion

    #region Constant

    private static readonly string ConfigFileName = "config.txt";
    private static readonly string DirName = "Datasets";

    private static readonly Dictionary<string, Type> PotentialValues = new()
    {
        { "loop", typeof(int) },
        { "imageWidth", typeof(int) },
        { "imageHeight", typeof(int) },
        { "imageToMaskScale", typeof(float) },
        { "visionDepth", typeof(int) },
        { "posFilePath", typeof(string) },
        { "texture", typeof(string[]) },
        { "additionalRotationFieldOfVision", typeof(int) },
        { "numberAdditionalRotation", typeof(int) },
        { "blurAmountPercent", typeof(int) },
        { "maxBlurPercent", typeof(int) },
        { "cameraHeight", typeof(int) },
        { "cameraInclination", typeof(int) },
    };

    #endregion

    #region PrivateVariables

    private Dictionary<string, object> _configData = new();
    private int _id;
    private CameraCapture _cameraCapture;
    private string _imageDirPath;
    private string _maskDirPath;

    #endregion

    #region UnityMethods

    private void Start()
    {
        try
        {
            InitFromConfigFile();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        
        _imageDirPath = Path.Combine(DirName, version, "Images");
        if (!Directory.Exists(_imageDirPath))
            Directory.CreateDirectory(_imageDirPath);
        _maskDirPath = Path.Combine(DirName, version, "Masks");
        if (!Directory.Exists(_maskDirPath))
            Directory.CreateDirectory(_maskDirPath);
        
        var nbLoop = _configData.TryGetValue("loop", out var value) ? (int)value : 1;
        var textures = (Texture2D[])_configData["textureList"];
        var positions = (Vector3[])_configData["pos"];
        var rotations = (Vector3[])_configData["rotation"];

        var meshRenderers = Utils.GetChildrenOfType<MeshRenderer>(ground);

        var r = new System.Random();

        foreach (var i in Capture(nbLoop, textures, positions, rotations, meshRenderers, r))
        {
        }
    }

    private void Update()
    {
        EditorApplication.isPlaying = false;
    }

    #endregion

    #region Config

    private void GetLatestVersion()
    {
        var directories = Directory.GetDirectories(DirName);
        string[] latestVersion = { "0", "0", "0" };

        foreach (var directory in directories)
        {
            int[] dirVersion = { 0, 0, 0 };
            var i = 0;
            foreach (var se in directory.Split('\\')[^1].Split('.')) dirVersion[i++] = int.Parse(se);
            if (dirVersion[0] > int.Parse(latestVersion[0]))
            {
                latestVersion[0] = dirVersion[0].ToString();
                latestVersion[1] = dirVersion[1].ToString();
                latestVersion[2] = dirVersion[2].ToString();
            }
            else if (dirVersion[0] == int.Parse(latestVersion[0]) && dirVersion[1] > int.Parse(latestVersion[1]))
            {
                latestVersion[1] = dirVersion[1].ToString();
                latestVersion[2] = dirVersion[2].ToString();
            }
            else if (dirVersion[0] == int.Parse(latestVersion[0]) && dirVersion[0] == int.Parse(latestVersion[1]) &&
                     dirVersion[2] >= int.Parse(latestVersion[2]))
            {
                latestVersion[2] = dirVersion[2].ToString();
            }
        }

        version = string.Join(".", latestVersion);
    }

    private void InitFromConfigFile()
    {
        if (version == "")
            GetLatestVersion();
        if (!Directory.Exists(DirName) || !Directory.Exists(Path.Join(DirName, version)))
            throw new Exception("Directory doesn't exist, try generating the version");

        if (!File.Exists(Path.Join(DirName, version, ConfigFileName)))
            throw new Exception("Version invalid, try regenerating it");
        var content = Utils.GetCsvInfosFromRegex(Path.Join(DirName, version, ConfigFileName),
            @"(version:(?:\d+\.\d+\.\d+))\r?\nSETTINGS\r?\n((?:.+:.+\r?\n)+)DATA_AUGMENTATION\r?\n((?:.+\r?\n?)+)");

        FillSettingsPart(content);
        FillDataAugmentationPart(content);
    }

    private void FillSettingsPart(string[] content)
    {
        var settings = content[1].Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        var settingsDict = settings.Select(line => line.Split(':')).ToArray();

        FillDictionaryPart(settingsDict);

        var path = ((string)_configData["posFilePath"]).Substring(0, ((string)_configData["posFilePath"]).Length - 1);
        if (!File.Exists(path)) throw new Exception($"File {path} not found");
        var fileContent = File.ReadAllText(path).Split('\n');

        _configData["pos"] = new Vector3[fileContent.Length];
        _configData["rotation"] = new Vector3[fileContent.Length];

        var posArray = (Vector3[])_configData["pos"];
        var rotationArray = (Vector3[])_configData["rotation"];

        _configData.TryGetValue("cameraHeight", out object cameraHeight);
        _configData.TryGetValue("cameraInclination", out object cameraInclination);
        
        if (cameraHeight == null || cameraInclination == null)
            throw new Exception("CameraHeight or CameraInclination are null");
        
        for (var i = 0; i < fileContent.Length; i++)
        {
            var parts = fileContent[i].Replace("\n", "").Split(';');
            if (parts.Length != 4)
                continue;
            try
            {
                posArray[i] = new Vector3(int.Parse(parts[0]), (int)cameraHeight, int.Parse(parts[1]));
                rotationArray[i] = new Vector3((int)cameraInclination, int.Parse(parts[2]), int.Parse(parts[3]));
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    private void FillDictionaryPart(string[][] content)
    {
        for (var i = 0; i < content.Length; i++)
            if (PotentialValues.TryGetValue(content[i][0], out var targetType))
            {
                if (targetType.IsArray ||
                    (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>)))
                    _configData[content[i][0]] = content[i][1].Trim().Split(';');
                else
                    _configData[content[i][0]] = Convert.ChangeType(content[i][1], targetType);
            }
            else
            {
                _configData[content[i][0]] = content[i][1];
            }
    }

    private void FillDataAugmentationPart(string[] content)
    {
        var dataAugmentation = content[2].Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        var dataAugmentationDict = dataAugmentation.Select(line => line.Split(':')).ToArray();

        FillDictionaryPart(dataAugmentationDict);

        var allTextures = new List<Texture2D>(Resources.LoadAll<Texture2D>("Texture2D"));
        var remainingTextures = new List<Texture2D>();
        for (var i = 0; i < allTextures.Count; i++)
            foreach (var textureName in (string[])_configData["texture"])
                if (allTextures[i].name.Equals(textureName))
                {
                    remainingTextures.Add(allTextures[i]);
                    break;
                }

        _configData["textureList"] = remainingTextures.ToArray();
    }

    #endregion

    #region Capture

    private void CaptureAt(Vector3 position, Vector3 eulerAngles)
    {
        _configData.TryGetValue("blurAmountPercent", out object blurAmountPercent);
        _configData.TryGetValue("maxBlurPercent", out object maxBlurPercent);
        if (blurAmountPercent == null)
            blurAmountPercent = 0;
        if (maxBlurPercent == null)
            maxBlurPercent = 0;
        
        gameObject.transform.SetPositionAndRotation(position, Quaternion.Euler(eulerAngles));

        CameraCapture.CaptureImage(Path.Combine(_imageDirPath, $"{_id}.png"), this.GetComponent<Camera>(),
            (int)_configData["imageWidth"], (int)_configData["imageHeight"], (int)blurAmountPercent, (int)maxBlurPercent);
        Mask.SaveMask(Path.Combine(_maskDirPath, $"{_id}.png"),
            (int)((int)_configData["imageWidth"] * (float)_configData["imageToMaskScale"]),
            (int)((int)_configData["imageHeight"] * (float)_configData["imageToMaskScale"]),
            (int)_configData["visionDepth"], this.GetComponent<Camera>(), outsideLayer, insideLayer);

        _id++;
    }

    private IEnumerable<int> Capture(int nbLoop, Texture2D[] textures, Vector3[] positions, Vector3[] rotations,
        List<MeshRenderer> meshRenderers, System.Random r)
    {
        foreach (var texture2D in textures)
        {
            Utils.ApplyTextureOnMeshRenderers(meshRenderers, texture2D);
            for (var j = 0; j < nbLoop; j++)
            for (var i = 0; i < positions.Length && EditorApplication.isPlaying; i++)
            {
                CaptureAt(positions[i], rotations[i]);
                CaptureAdditionalRotation(positions[i], rotations[i], r);
                yield return -1;
            }
        }

        yield return 0;
    }

    #endregion

    #region DataAugmentation

    private void CaptureAdditionalRotation(Vector3 position, Vector3 rotation, System.Random r)
    {
        _configData.TryGetValue("numberAdditionalRotation", out object numberAdditionalRotation);
        _configData.TryGetValue("additionalRotationFieldOfVision", out object additionalRotationFieldOfVision);
        if (numberAdditionalRotation == null || additionalRotationFieldOfVision == null)
            return;
        
        for (int k = 0; k < (int)numberAdditionalRotation ; k++)
        {
            var newRotation = new Vector3(
                rotation.x,
                rotation.y + r.Next((int)additionalRotationFieldOfVision / -2,
                    (int)additionalRotationFieldOfVision / 2),
                rotation.z
            );
            CaptureAt(position, newRotation);
        }
    }

    #endregion

    #region Debug

    private void DisplayDictionary(bool displayArray = false)
    {
        foreach (var keyValuePair in _configData)
        {
            Debug.Log(keyValuePair.Key + ": " + keyValuePair.Value + " of type: " + keyValuePair.Value.GetType());
            if (keyValuePair.Value is Array || keyValuePair.Value is IList)
            {
                if (displayArray)
                {
                    foreach (var vector3 in (IEnumerable)keyValuePair.Value)
                    {
                        Debug.Log(vector3);
                    }    
                }
                
            }
        }
    }

    #endregion
}