using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugHelper : MonoBehaviour {

    public Shader_Light light;
    public RawImage img;

	// Use this for initialization
	void Start () {
        img.texture = light.m_inputRenderTex;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
