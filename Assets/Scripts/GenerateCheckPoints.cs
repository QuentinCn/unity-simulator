using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateCheckPoints : MonoBehaviour
{
    public int nbCheckpoint;
    public LineRenderer leftLineRenderer;
    public LineRenderer rightLineRenderer;
    
    private List<int> _checkpointsIndex;
    private int _id;

    void Start()
    {
        _id = 1;
        if (nbCheckpoint <= 0)
            return;
        
        if (nbCheckpoint > leftLineRenderer.positionCount || nbCheckpoint > rightLineRenderer.positionCount)
            nbCheckpoint = leftLineRenderer.positionCount < rightLineRenderer.positionCount ? leftLineRenderer.positionCount : rightLineRenderer.positionCount;

        for (int i = 0; i < nbCheckpoint - 1; i++)
        {
            int index = leftLineRenderer.positionCount / nbCheckpoint * (i + 1);
            
            if (index < leftLineRenderer.positionCount && index < rightLineRenderer.positionCount)
                AddCheckPoint(leftLineRenderer.GetPosition(index), rightLineRenderer.GetPosition(index));
        }
        AddCheckPoint(leftLineRenderer.GetPosition(leftLineRenderer.positionCount - 1), rightLineRenderer.GetPosition(rightLineRenderer.positionCount - 1), true);
    }

    private void AddCheckPoint(Vector3 vect1, Vector3 vect2, bool final = false)
    {
        GameObject newGameObject = new GameObject(final ? "0" : _id.ToString());
        _id++;
        BoxCollider newBox = newGameObject.AddComponent<BoxCollider>();
        newGameObject.layer = gameObject.layer;
        newGameObject.tag = gameObject.tag;
        newGameObject.transform.SetParent(gameObject.transform);
        newGameObject.transform.position = (vect1 + vect2) / 2;
        newBox.isTrigger = true;

        float size = Vector3.Distance(vect1, vect2);
        newBox.size = new Vector3(size, 1f, 1f);

        Vector3 direction = (vect2 - vect1).normalized;
        newBox.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
    }
}
