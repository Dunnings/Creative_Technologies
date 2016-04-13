using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public Animator m_anim;
    public SpriteRenderer m_spriteRenderer;
    public Rigidbody2D m_rigid;

    private bool isGrounded = false;
    private int jumpCount = 0;
    private float maxXVel = 5;
    
	
	// Update is called once per frame
	void Update () {
            if (Input.GetAxis("Horizontal") > 0f)
            {
                m_spriteRenderer.flipX = false;
                m_anim.SetBool("Walking", true);
                m_rigid.velocity += Vector2.right * Time.deltaTime * 10f;
            }
            else if (Input.GetAxis("Horizontal") < 0f)
            {
                m_spriteRenderer.flipX = true;
                m_anim.SetBool("Walking", true);
                m_rigid.velocity += Vector2.left * Time.deltaTime * 10f;
            }
            else
            {
                m_anim.SetBool("Walking", false);
            }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2)
        {
            m_rigid.velocity = new Vector2(m_rigid.velocity.x, 0f);
            m_rigid.AddForce(Vector2.up * 9f, ForceMode2D.Impulse);
            jumpCount++;
            isGrounded = false;
            m_anim.SetBool("Jumping", true);
        }

        if (m_rigid.velocity.x > maxXVel)
        {
            m_rigid.velocity = new Vector2(maxXVel, m_rigid.velocity.y);
        }
        else if (m_rigid.velocity.x < -maxXVel)
        {
            m_rigid.velocity = new Vector2(-maxXVel, m_rigid.velocity.y);
        }

        if(m_rigid.velocity.y < 0f && m_anim.GetBool("Jumping"))
        {
            m_anim.SetBool("Jumping", false);
            m_anim.SetBool("Falling", true);
        }

        if(transform.position.y < -6f)
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        isGrounded = true;
        m_anim.SetBool("Falling", false);
        jumpCount = 0;
    }
}
