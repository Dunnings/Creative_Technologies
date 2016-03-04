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
	
	// Update is called once per frame
	void Update ()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        myRigid.velocity += new Vector2(x, y) * 0.1f;
        myRigid.velocity = myRigid.velocity * 0.95f;

        Vector2 dir = myRigid.velocity;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        graphic.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        Vector3 targetPos = Vector3.Lerp(Camera.main.transform.position, transform.position, Time.deltaTime);
        targetPos.z = -10f;
        Camera.main.transform.position = targetPos;

        if (Input.GetButton("Submit"))
        {
            if(Time.time - timeLastFired > 0.15f)
            {
                Bullet go = bullets[index++];
                if(index >= bullets.Length)
                {
                    index = 0;
                }
                go.transform.position = transform.position + graphic.transform.right * (left?-0.37f:0.37f);
                go.GetComponent<Bullet>().vel = graphic.transform.up;
                go.transform.rotation = graphic.transform.rotation;
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
}
