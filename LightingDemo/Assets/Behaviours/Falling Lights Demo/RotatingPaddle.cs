using UnityEngine;
using System.Collections;

public class RotatingPaddle : MonoBehaviour {

    public float m_speed = 10f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward, m_speed * Time.deltaTime);
	}
}
