using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpaceGenerator : MonoBehaviour {

    public List<GameObject> meteorGOs = new List<GameObject>();

    float meteorCount = 20;
    float mapWidth = 10;
    float mapHeight = 6;

	/// <summary>
    /// Instantiate given number of asteroids on application start
    /// </summary>
	void Awake () {
        for (int i = 0; i < meteorCount; i++)
        {
            GameObject go = Instantiate(meteorGOs[Random.Range(0,meteorGOs.Count-1)]);
            go.transform.position = new Vector2(Random.Range(-mapWidth, mapWidth), Random.Range(-mapHeight, mapHeight));
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            float amount = Random.Range(2f, 5f);
            rb.AddForce(new Vector2(Random.Range(-amount, amount), Random.Range(-amount, amount)));
            rb.AddTorque(Random.Range(-1000f, 1000f), ForceMode2D.Force);
            go.transform.parent = transform;
        }
	}
}
