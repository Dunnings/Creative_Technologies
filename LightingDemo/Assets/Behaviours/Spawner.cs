using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

    public GameObject go;
    private float time = 0f;
    public float spawnDelay = 0.75f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;

        if (time > spawnDelay)
        {
            GameObject g = Instantiate(go);
            g.transform.position = transform.position + Random.Range(-9f, 9f) * Vector3.right;
            g.transform.parent = transform;
            time = 0f;
            Destroy(g, 10f);
        }
	}
}
