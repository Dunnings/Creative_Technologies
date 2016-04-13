using UnityEngine;
using System.Collections;

public class AIShipController : MonoBehaviour {

    public Rigidbody2D myRigid;
    public SpriteRenderer graphic;

    public Animator leftGun;
    public Animator rightGun;

    public Bullet[] bullets = new Bullet[50];
    int index = 0;

    float timeLastFired = 0f;
    bool left = true;
    private Vector3 targetPos;

    public Rigidbody2D targetPlayer;

    Vector3 towardPlayer;
    // Update is called once per frame
    void Update ()
    {
        towardPlayer = (targetPlayer.transform.position + new Vector3(targetPlayer.velocity.x, targetPlayer.velocity.y) * 0.75f) - transform.position;
        myRigid.velocity = towardPlayer.normalized;
        if (towardPlayer.magnitude < 2f)
        {
            myRigid.velocity *= 0.1f;
        }

        Vector2 dir = myRigid.velocity * 0.8f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        graphic.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

        if (towardPlayer.magnitude < 10f)
        {
            if (Time.time - timeLastFired > 0.25f)
            {
                Bullet go = bullets[index++];
                if (index >= bullets.Length)
                {
                    index = 0;
                }
                go.transform.position = graphic.transform.position + graphic.transform.right * (left ? -0.37f : 0.37f);
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

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + new Vector3(myRigid.velocity.x, myRigid.velocity.y), Vector3.one * 1.1f);
    }
}
