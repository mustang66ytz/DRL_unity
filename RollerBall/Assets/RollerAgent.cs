using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

public class RollerAgent : Agent
{
    Rigidbody rBody;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;
    public float start_time;
    public override void OnEpisodeBegin(){
	if(this.transform.localPosition.y < 0){
	    // if agent fall, zero its momentum
	    this.rBody.angularVelocity = Vector3.zero;
	    this.rBody.velocity = Vector3.zero;
	    this.transform.localPosition = new Vector3(0, 0.5f, 0);	
	}
	// move the target to a new spot
	Target.localPosition = new Vector3(Random.value*8-4, 0.5f, Random.value*8-4);
	start_time = Time.time;
    }

    public override void CollectObservations(VectorSensor sensor){
        // target and agent positions
	sensor.AddObservation(Target.localPosition);
	sensor.AddObservation(this.transform.localPosition);
	// Agent velocity
	sensor.AddObservation(rBody.velocity.x);
	sensor.AddObservation(rBody.velocity.z);
    }

    public float speed = 10;
    public override void OnActionReceived(float[] vectorAction){
	//Action, size =2
	Vector3 controlSignal = Vector3.zero;
	controlSignal.x = vectorAction[0];
	controlSignal.z = vectorAction[1];
	rBody.AddForce(controlSignal*speed);

	// rewards
	float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // reached target
	if(distanceToTarget<1.42f){
	    SetReward(1.0f);
	    EndEpisode();
	}

	// fall off platform
	if(this.transform.localPosition.y<0){
	    EndEpisode();
	}

	if(Time.time-start_time>5.0f){
	    SetReward(-0.4f);
	    EndEpisode();	
	}
    }

    public override float[] Heuristic()
    {
        var actionsOut = new float[2];

        actionsOut[0] = Input.GetAxis("Horizontal");
	actionsOut[1] = Input.GetAxis("Vertical");
        return actionsOut;
    }

}
