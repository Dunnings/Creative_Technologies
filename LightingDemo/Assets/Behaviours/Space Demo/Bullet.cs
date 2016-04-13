using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public Vector3 vel;
    public SpriteRenderer renderer;

    public string ownerTag = "Player";

    public int lightSize = 3;

    private float startAlpha = 0f;

	// Use this for initialization
	void Awake ()
    {
        StartCoroutine(disable());
    }

    void Start()
    {
        startAlpha = GetComponentInChildren<Shader_Light>().m_lightColor.a;
    }

    IEnumerator disable()
    {
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
        yield return null;
    }

    // Update is called once per frame
    void Update () {
        if (alive)
        {
            transform.position += vel * Time.deltaTime * 7.5f;
        }
	}

    public void Spawn(Color col)
    {
        GetComponentInChildren<Shader_Light>().m_lightSize = lightSize;
        GetComponentInChildren<Shader_Light>().m_lightColor.a = startAlpha;
        gameObject.SetActive(true);
        alive = true;
        StartCoroutine(Life());
        renderer.enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (alive)
        {
            if (other.gameObject.tag != ownerTag)
            {
                if (other.GetComponent<Bullet>() != null)
                {
                    if (other.GetComponent<Bullet>().ownerTag == ownerTag)
                    {
                        return;
                    }
                }
                
                StartCoroutine(Dead());
                if (ownerTag == "Player")
                {
                    ExplosionSystem.Instance.EmitPlayer(transform.position);
                    other.GetComponent<Rigidbody2D>().AddForceAtPosition((other.transform.position - transform.position).normalized * 1000f, transform.position, ForceMode2D.Impulse);
                }
                else
                {
                    ExplosionSystem.Instance.EmitEnemy(transform.position);
                }
            }
        }
    }

    IEnumerator Life()
    {
        yield return new WaitForSeconds(3f);
        if (alive)
        {
            StartCoroutine(Dead());
            if (ownerTag == "Player")
            {
                ExplosionSystem.Instance.EmitPlayer(transform.position);
            }
            else
            {
                ExplosionSystem.Instance.EmitEnemy(transform.position);
            }
        }
    }

    bool alive = false;

    IEnumerator Dead()
    {
        if (alive)
        {
            alive = false;
            renderer.enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
            float f = 0f;
            while (f < 0.3f)
            {
                GetComponentInChildren<Shader_Light>().m_lightSize = lightSize + Mathf.FloorToInt((f / 0.3f) * 3f);
                GetComponentInChildren<Shader_Light>().m_lightColor.a = f / 0.6f;
                yield return new WaitForEndOfFrame();
                f += Time.deltaTime;
            }
            GetComponentInChildren<Shader_Light>().m_lightColor.a = 1f;
            f = 0f;
            while (f < 0.1f)
            {
                GetComponentInChildren<Shader_Light>().m_lightSize = 3 - Mathf.FloorToInt((f / 0.1f) * 3);
                GetComponentInChildren<Shader_Light>().m_lightColor.a = 0.5f - (f / 0.1f) * 0.5f;
                yield return new WaitForEndOfFrame();
                f += Time.deltaTime;
            }
            GetComponentInChildren<Shader_Light>().m_lightColor.a = 0f;
            gameObject.SetActive(false);
        }
    }
}
