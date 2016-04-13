using UnityEngine;
using System.Collections;

public class PickUp : MonoBehaviour {

    private float timeLastPeak = 0f;
    private float timeBetweenPeaks = 1.5f;
    private float offset = 0.2f;
    private bool upwards = true;

    private Vector3 startPos;

	// Use this for initialization
	void Start () {
        startPos = transform.position;
        timeLastPeak = Random.Range(0f, timeBetweenPeaks / 2f);
        upwards = Random.value > 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
        if(Time.time - timeLastPeak > timeBetweenPeaks)
        {
            timeLastPeak = Time.time;
            upwards = !upwards;
        }
        if (upwards)
        {
            transform.localPosition = startPos + new Vector3(0f, Mathf.Lerp(offset, -offset, (Time.time - timeLastPeak) / timeBetweenPeaks), 0f);
        }
        else
        {
            transform.localPosition = startPos + new Vector3(0f, Mathf.Lerp(-offset, offset, (Time.time - timeLastPeak) / timeBetweenPeaks), 0f);
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
