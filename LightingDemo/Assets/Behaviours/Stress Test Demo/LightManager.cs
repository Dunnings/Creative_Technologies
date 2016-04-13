using UnityEngine;
using System.Collections.Generic;

public class LightManager : MonoBehaviour {

    public GameObject meshLight;
    public List<GameObject> meshLights = new List<GameObject>();
    public GameObject shaderLight;
    public List<GameObject> shaderLights = new List<GameObject>();

    private float meshRadius = 5f;
    private float shaderRadius = 2f;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            meshLights.Add(Instantiate(meshLight));
            meshLights[meshLights.Count - 1].GetComponentInChildren<Mesh_Light>().m_lightColor = RandomHue(meshLights[meshLights.Count - 1].GetComponentInChildren<Mesh_Light>().m_lightColor.a);
            for (int i = 0; i < meshLights.Count; i++)
            {
                PositionLight(meshRadius, meshLights.Count, meshLights[i], i, 0f);
            }
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            shaderLights.Add(Instantiate(shaderLight));
            shaderLights[shaderLights.Count - 1].GetComponentInChildren<Shader_Light>().m_lightColor = RandomHue(shaderLights[shaderLights.Count - 1].GetComponentInChildren<Shader_Light>().m_lightColor.a);
            for (int i = 0; i < shaderLights.Count; i++)
            {
                PositionLight(shaderRadius, shaderLights.Count, shaderLights[i], i, 0f);
            }
        }


        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (meshLights.Count > 0)
            {
                Destroy(meshLights[meshLights.Count - 1]);
                meshLights.RemoveAt(meshLights.Count - 1);
                for (int i = 0; i < meshLights.Count; i++)
                {
                    PositionLight(meshRadius, meshLights.Count, meshLights[i], i, 0f);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (shaderLights.Count > 0)
            {
                Destroy(shaderLights[shaderLights.Count - 1]);
                shaderLights.RemoveAt(shaderLights.Count - 1);
                for (int i = 0; i < shaderLights.Count; i++)
                {
                    PositionLight(shaderRadius, shaderLights.Count, shaderLights[i], i, 0f);
                }
            }
        }

        for (int i = 0; i < meshLights.Count; i++)
        {
            PositionLight(meshRadius, meshLights.Count, meshLights[i], i, Time.time);
        }
        for (int i = 0; i < shaderLights.Count; i++)
        {
            PositionLight(shaderRadius, shaderLights.Count, shaderLights[i], i, -Time.time);
        }
    }

    public void PositionLight(float radius, int lightCount, GameObject go, int index, float offset)
    {
        float theta = ((float)index / (float)lightCount) * (2f * Mathf.PI);
        theta += offset;
        go.transform.position = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), go.transform.position.z);
    }

    public Color RandomHue(float a)
    {
        Color c = Color.HSVToRGB(Random.value, 1f, 1f);
        c.a = a;
        return c;
    }
}
