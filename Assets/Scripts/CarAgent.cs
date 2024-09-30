using System.Collections.Generic;
using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class CarAgent : Agent
{
    public float speed = 10f;
    public float turnSpeed = 100f;

    private int _lastCheckpoint;
    private Rigidbody _carRigidbody;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private void Start()
    {
        _lastCheckpoint = 0;
        _carRigidbody = GetComponent<Rigidbody>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var throttle = actionBuffers.ContinuousActions[0]; // Throttle (forward/backward)
        var steering = actionBuffers.ContinuousActions[1]; // Steering (left/right)
    
        var forwardMovement = transform.forward * throttle * speed * Time.deltaTime;
        _carRigidbody.MovePosition(_carRigidbody.position + forwardMovement);
    
        var turn = steering * turnSpeed * Time.deltaTime;
        var turnRotation = Quaternion.Euler(0f, turn, 0f);
        _carRigidbody.MoveRotation(_carRigidbody.rotation * turnRotation);
    }
    
    private void Update()
    {
        if (transform.position.y < -5f) ResetCarPosition();
    }
    
    public override void OnEpisodeBegin()
    {
        ResetCarPosition();
    }
    
    private void ResetCarPosition()
    {
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        _carRigidbody.velocity = Vector3.zero;
        _carRigidbody.angularVelocity = Vector3.zero;
        _lastCheckpoint = 0;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        int[,] observationMatrix = gameObject.GetComponent<CsvMatrix>().GetObjectMatrix();
        
        foreach (int cell in observationMatrix)
        {
            sensor.AddObservation(cell);
        }

        
        sensor.AddObservation(_carRigidbody.velocity);
        sensor.AddObservation(transform.position);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
    
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lines"))
        {
            AddReward(-1.0f);
            ResetCarPosition();
            Debug.Log("Car touched a warning line!");
        }
        else if (other.CompareTag("CheckPoint"))
        {
            int otherId = int.Parse(other.name);
            
            if (_lastCheckpoint < 0 || otherId == _lastCheckpoint + 1)
            {
                _lastCheckpoint = otherId;
                AddReward(1.0f);
                Debug.Log($"Car passed check point");
                if (otherId == 0)
                    ResetCarPosition();
            }
            else
            {
                AddReward(-1.0f);
                Debug.Log($"Car passed old check point");
                ResetCarPosition();
            }
        }
    }
}