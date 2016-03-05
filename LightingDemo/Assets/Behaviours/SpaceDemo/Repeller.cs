using UnityEngine;
using System.Collections;

public class Repeller : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Asteroid")
        {
            other.GetComponent<Rigidbody2D>().AddForce((other.transform.position - transform.position).normalized * 25000f, ForceMode2D.Impulse);
        }
    }
}
