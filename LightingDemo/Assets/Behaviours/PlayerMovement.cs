using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public Rigidbody2D m_rigidBody;
    public Animator m_animator;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        m_rigidBody.velocity = Vector2.zero;
        Vector3 newVel = Vector3.zero;
        newVel.x = Input.GetAxis("Horizontal");
        newVel.y = Input.GetAxis("Vertical");
        if (newVel != Vector3.zero)
        {
            float angle = Mathf.Atan2(newVel.y, newVel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        m_rigidBody.velocity = newVel;
        m_rigidBody.angularVelocity = 0f;
        m_animator.SetBool("Moving", m_rigidBody.velocity.sqrMagnitude != 0f);
    }
}
