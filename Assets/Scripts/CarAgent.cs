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
    public Camera camera;
    private LayerMask outsideLayer;
    private LayerMask insideLayer;
    
    private int _lastCheckpoint;
    private Rigidbody _carRigidbody;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private PrometeoCarController _carController;

    [Range(0, 1)] public float matrixScale = 0.5f;

    private void Start()
    {
        _lastCheckpoint = -1;
        _carRigidbody = GetComponent<Rigidbody>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _carController = GetComponent<PrometeoCarController>();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var throttle = actionBuffers.ContinuousActions[0]; // Throttle (forward/backward)
        var steering = actionBuffers.ContinuousActions[1]; // Steering (left/right)


        if (throttle > 0f)
        {
            _carController.GoForward();
            // AddReward(0.01f);
        }
        else if (throttle < 0f)
        {
            _carController.GoReverse();
            AddReward(-1f);
        }
        else
        {
            _carController.ThrottleOff();
            _carController.test();
        }

        if (steering < 0f)
            _carController.TurnLeft();
        else if (steering > 0f)
            _carController.TurnRight();
        else
            _carController.ResetSteeringAngle();

        // AddReward(throttle * 0.2f);
    }

    private void Update()
    {
        if (transform.position.y < -5f) EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Begin episode, cumulative reward = " + GetCumulativeReward());
        ResetCarPosition();
    }

    private void ResetCarPosition()
    {
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        _carRigidbody.velocity = Vector3.zero;
        _carRigidbody.angularVelocity = Vector3.zero;
        _lastCheckpoint = -1;
        _carController.ThrottleOff();
        _carController.test();
        _carController.ResetSteeringAngle();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var observationMatrix = Mask.GetObjectMatrix((int)(matrixScale * 1920), (int)(matrixScale * 1080), 150, camera, outsideLayer, insideLayer);

        foreach (var cell in observationMatrix) sensor.AddObservation(cell);

        sensor.AddObservation(_carRigidbody.velocity);
        sensor.AddObservation(_carRigidbody.rotation.eulerAngles);
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
            AddReward(-20.0f);
            Debug.Log("Touched line, cumulative reward = " + GetCumulativeReward());
            // ResetCarPosition();
            EndEpisode();
        }
        else if (other.CompareTag("CheckPoint"))
        {
            var otherId = int.Parse(other.name);

            if ((_lastCheckpoint < 0 && otherId == 1) || otherId == _lastCheckpoint + 1)
            {
                _lastCheckpoint = otherId;
                AddReward(10f);
                // Debug.Log("Passed checkpoint " + _lastCheckpoint + ", cumulative reward = " + GetCumulativeReward());
            }
            else
            {
                AddReward(-20f);
                Debug.Log("Old checkpoint " + other.name + ", cumulative reward = " + GetCumulativeReward());
                // EndEpisode();
            }
        }
    }
}