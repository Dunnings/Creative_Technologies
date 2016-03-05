using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour {

    public Rigidbody2D myRigid;
    public SpriteRenderer graphic;

    public Animator leftGun;
    public Animator rightGun;

    public Bullet[] bullets = new Bullet[50];
    int index = 0;

    float timeLastFired = 0f;
    bool left = true;
    private Vector3 targetPos;
	// Update is called once per frame
	void Update ()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        myRigid.velocity += new Vector2(x, y) * 0.2f;
        myRigid.velocity = myRigid.velocity * 0.95f;
        if (myRigid.velocity.magnitude > 0.1f)
        {
            graphic.transform.up = myRigid.velocity;
        }

        if (Input.GetButton("Submit"))
        {
            if(Time.time - timeLastFired > 0.15f)
            {
                Bullet go = bullets[index++];
                if(index >= bullets.Length)
                {
                    index = 0;
                }
                go.transform.position = graphic.transform.position + graphic.transform.right * (left?-0.37f:0.37f);
                go.GetComponent<Bullet>().vel = graphic.transform.up;
                go.transform.rotation = graphic.transform.rotation;
                Physics2D.IgnoreCollision(go.GetComponent<Collider2D>(), GetComponentInChildren<Collider2D>());
                go.Spawn(left ? new Color(0f, 0.8f, 1f, 0.05f) : new Color(1f, 0.6f, 0.0f, 0.05f));
                timeLastFired = Time.time;

                if (left)
                {
                    leftGun.Play("FireGun");
                }
                if (!left)
                {
                    rightGun.Play("FireGun");
                }

                left = !left;
            }
        }
    }

    void LateUpdate()
    {
        targetPos = graphic.transform.position;
        targetPos.z = -10f;
        Camera.main.transform.position = targetPos;
    }
}
