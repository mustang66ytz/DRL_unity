using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObstacle1 : MonoBehaviour
{
	Rigidbody rBody;
	public float speed = 0.3f;
	Vector3 dirToGo = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
	transform.localPosition = new Vector3(0f, 2.5f, 6.5f);
    }
	
    // Update is called once per frame
    void FixedUpdate()
    {
	if(transform.position.x>5){
		// move left
		dirToGo = transform.forward*1f*speed;
		rBody.AddForce(dirToGo, ForceMode.VelocityChange);
		
	}
	if(dirToGo == Vector3.zero){
		dirToGo = transform.forward*-1f*speed;
	}        
	else if(transform.position.x<-30){
		// move right
		dirToGo = transform.forward*-1f*speed;
		rBody.AddForce(dirToGo, ForceMode.VelocityChange);
	}
	else{
		rBody.transform.position += dirToGo;
	}
    }
}
