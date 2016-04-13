using UnityEngine;
using System.Collections.Generic;

public class FallingLightSpawner : MonoBehaviour {

    public GameObject light;

    public float minX = -7f;
    public float maxX = 7f;
    public float minY = 5f;
    public float maxY = 6f;

    public float minDelay = 1f;
    public float maxDelay = 2f;

    private float timeToWait = 0f;
    private float timeLastDropped = 0f;

    public int maxLights = 100;

    private List<GameObject> allLights = new List<GameObject>();

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time - timeLastDropped > timeToWait)
        {
            timeLastDropped = Time.time;
            GameObject newGO = GameObject.Instantiate<GameObject>(light);
            newGO.transform.position = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
            newGO.transform.parent = transform;
            float h,s,v, a;
            Color.RGBToHSV(newGO.GetComponentInChildren<Shader_Light>().m_lightColor, out h, out s, out v);
            a = newGO.GetComponentInChildren<Shader_Light>().m_lightColor.a;
            h = Random.Range(0f, 1f);
            Color c = Color.HSVToRGB(h, s, v);
            c.a = a;
            newGO.GetComponentInChildren<Shader_Light>().m_lightColor = c;
            c.a = 0.5f;
            newGO.GetComponent<SpriteRenderer>().color = c;
            allLights.Add(newGO);
            timeToWait = Random.Range(minDelay, maxDelay);

            if (allLights.Count > maxLights)
            {
                Destroy(allLights[0]);
                OccluderManager.Instance.RemoveOccluder(allLights[0].GetComponent<LightOccluder>());
                allLights.RemoveAt(0);
            }
        }
	}
}
