using UnityEngine;
using System.Collections;

public class FlickerLightAlpha : MonoBehaviour {

    public Mesh_Light m_light;
    public float min = 0.01f;
    public float max = 0.04f;

    private float lastAlpha = 0f;
    private float targetAlpha;
    private float timeLastChanged = 0f;
    private float timeBetweenChange = 0.1f;

    // Use this for initialization
    void Start () {
        lastAlpha = m_light.m_lightColor.a;
	}
	
	// Update is called once per frame
	void Update () {
        if(Time.time - timeLastChanged > timeBetweenChange)
        {
            timeLastChanged = Time.time;
            lastAlpha = targetAlpha;
            targetAlpha = Random.Range(min, max);
        }

        m_light.m_lightColor.a = Mathf.Lerp(lastAlpha, targetAlpha, timeBetweenChange / (Time.time - timeLastChanged));
	}
}
