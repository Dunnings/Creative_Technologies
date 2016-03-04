using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public Vector3 vel;
    public SpriteRenderer renderer;

    public int lightSize = 3;

	// Use this for initialization
	void Awake ()
    {
        StartCoroutine(disable());
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
            transform.position += vel * Time.deltaTime * 5f;
        }
	}

    public void Spawn(Color col)
    {
        GetComponentInChildren<Shader_Light>().m_lightColor.a = 0.03f;
        gameObject.SetActive(true);
        alive = true;
        StartCoroutine(Life());
        renderer.enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (alive)
        {
            if (col.collider.gameObject.name != "Player")
            {
                GetComponentInChildren<Shader_Light>().m_lightSize += 2;
                col.collider.GetComponent<Rigidbody2D>().AddForceAtPosition((col.collider.transform.position - transform.position).normalized * 1000f, transform.position, ForceMode2D.Impulse);
                StartCoroutine(Dead());
                ExplosionSystem.Instance.Emit(transform.position);
            }
        }
    }

    IEnumerator Life()
    {
        yield return new WaitForSeconds(3f);
        if (alive)
        {
            StartCoroutine(Dead());
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
                GetComponentInChildren<Shader_Light>().m_lightColor.a = 1f - f / 0.1f;
                yield return new WaitForEndOfFrame();
                f += Time.deltaTime;
            }
            GetComponentInChildren<Shader_Light>().m_lightColor.a = 0f;
            gameObject.SetActive(false);
        }
    }
}
