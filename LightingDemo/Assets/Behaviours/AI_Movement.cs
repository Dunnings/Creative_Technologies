using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Movement : MonoBehaviour {

    public List<Transform> path = new List<Transform>();
    public Rigidbody2D myRigid;
    public Animator myAnim;
    public int pathPos = 0;

    private float prevAngle = 0f;

	// Use this for initialization
	void Start () {
        myAnim.SetBool("Moving", true);
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 target = new Vector2(path[pathPos].position.x, path[pathPos].position.y);
	    if(Vector3.Distance(transform.position, target) < 0.5f)
        {
            pathPos++;
            if(pathPos >= path.Count -1)
            {
                pathPos = 0;
            }
        }
        myRigid.velocity = (new Vector3(target.x, target.y, 0f) - transform.position).normalized;
        float angle = Mathf.Atan2(myRigid.velocity.y, myRigid.velocity.x) * Mathf.Rad2Deg;
        float lerpedAngle = Mathf.Lerp(prevAngle, angle, Time.deltaTime * 3);
        transform.rotation = Quaternion.AngleAxis(lerpedAngle, Vector3.forward);
        prevAngle = lerpedAngle;
    }
}
