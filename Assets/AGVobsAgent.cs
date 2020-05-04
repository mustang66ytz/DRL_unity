using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

public class AGVobsAgent : Agent
{
	[Header("Target To drive Towards")]
	[Space(10)]
	public Transform Target;

	[Header("Body Parts")]
	[Space(10)]
	Rigidbody rBody;
	public Transform body;

	Vector3 m_DirToTarget;
	float m_MovingTowardsDot;
	float m_FacingDot;

	[Header("Reward Functions To Use")]
	[Space(10)]
	public bool rewardMovingTowardsTarget; // Agent should move towards target
	public bool rewardFacingTarget; // Agent should face the target
	public bool rewardUseTimePenalty; // Hurry up

	Quaternion m_LookRotation;
	Matrix4x4 m_TargetDirMatrix;

	internal Vector3 startPos;

	public override void Initialize()
	{
		rBody = GetComponent<Rigidbody>();
		m_DirToTarget = Target.position - body.position;
		startPos = new Vector3(0, 0.5f, 0);
	}
    
    public override void OnEpisodeBegin(){
		this.rBody.angularVelocity = Vector3.zero;
		this.rBody.velocity = Vector3.zero;
		this.transform.localPosition = startPos;	
		if (m_DirToTarget != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(m_DirToTarget);
		}
		transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f));

		// move the target to a new spot
		Target.localPosition = new Vector3(Random.value*28-40, 0.5f, Random.value*30-15);
    }

    public override void CollectObservations(VectorSensor sensor){
		// update pos to target
		m_DirToTarget = Target.position - body.position;
		m_LookRotation = Quaternion.LookRotation(m_DirToTarget);
		m_TargetDirMatrix = Matrix4x4.TRS(Vector3.zero, m_LookRotation, Vector3.one);

		// Forward & up to help with orientation
		var bodyForwardRelativeToLookRotationToTarget = m_TargetDirMatrix.inverse.MultiplyVector(body.forward);
		sensor.AddObservation(bodyForwardRelativeToLookRotationToTarget);

		var bodyUpRelativeToLookRotationToTarget = m_TargetDirMatrix.inverse.MultiplyVector(body.up);
		sensor.AddObservation(bodyUpRelativeToLookRotationToTarget);

    }
    
    public void MoveAgent(float[] act){
		var dirToGo = Vector3.zero;
		var rotateDir = Vector3.zero;
		var action = Mathf.FloorToInt(act[0]);
		switch(action){
			case 1:
			dirToGo = transform.forward*1f;
			break;
			case 2:
			dirToGo = transform.forward*-1f;
			break;
			case 3:
			rotateDir = transform.up*1f;	
			break;
			case 4:
			rotateDir = transform.up*-1f;
			break;
		}
		transform.Rotate(rotateDir, Time.deltaTime*150f);
		rBody.AddForce(dirToGo, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(float[] vectorAction){
        SetReward(-1f/3000);
		MoveAgent(vectorAction);
    }

	void FixedUpdate()
	{
		// Set reward for this step according to mixture of the following elements.
		if (rewardMovingTowardsTarget)
		{
			RewardFunctionMovingTowards();
		}

		if (rewardFacingTarget)
		{
			RewardFunctionFacingTarget();
		}

		if (rewardUseTimePenalty)
		{
			RewardFunctionTimePenalty();
		}
	}

	/// <summary>
	/// Reward moving towards target & Penalize moving away from target.
	/// </summary>
	void RewardFunctionMovingTowards()
	{
		m_MovingTowardsDot = Vector3.Dot(rBody.velocity, m_DirToTarget.normalized);
		AddReward(0.01f * m_MovingTowardsDot);
	}

	/// <summary>
	/// Reward facing target & Penalize facing away from target
	/// </summary>
	void RewardFunctionFacingTarget()
	{
		m_FacingDot = Vector3.Dot(m_DirToTarget.normalized, body.forward);
		AddReward(0.005f * m_FacingDot);
	}

	/// <summary>
	/// Existential penalty for time-contrained tasks.
	/// </summary>
	void RewardFunctionTimePenalty()
	{
		AddReward(-0.001f);
	}

	void OnCollisionEnter(Collision col){
		if(col.gameObject.CompareTag("Goal")){
			SetReward(1f);		    
		}
		EndEpisode();
    }

    public override float[] Heuristic()
    {
	var actionsOut = new float[1];
        actionsOut[0] = 0;
	if(Input.GetKey(KeyCode.D)){
	    actionsOut[0] = 3;
	}
        else if(Input.GetKey(KeyCode.W)){
	    actionsOut[0] = 1;
	}
	else if(Input.GetKey(KeyCode.A)){
	    actionsOut[0] = 4;
	}
	else if(Input.GetKey(KeyCode.S)){
	    actionsOut[0] = 2;
	}
	return actionsOut;
    }
}

