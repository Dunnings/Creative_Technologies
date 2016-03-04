using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Shader_Light : MonoBehaviour {


    public List<string> m_occluderLayers = new List<string> { "LightOccluder" };

    public bool m_gradientFalloff = true;
    public bool m_penumbra = true;

    public Color m_lightColor;
    public int m_lightSize = 20;
    private int m_shadowSize = 512;

    public RenderTexture m_inputRenderTex;
    public RenderTexture m_middleRenderTex;
    public Material m_material;

    public MeshRenderer m_meshRenderer;

    private Camera cam;

    // Use this for initialization
    void Awake()
    {
        m_inputRenderTex = new RenderTexture(m_shadowSize, m_shadowSize, 0);
        m_middleRenderTex = new RenderTexture(m_shadowSize, 1, 0);
        m_material = new Material(Shader.Find("2DLighting/Deferred_Point_2"));
        m_material.SetTexture("_DepthMap", m_middleRenderTex);
        m_material.SetInt("_Penumbra", m_penumbra ? 1 : 0);
        m_material.SetInt("_GradientFalloff", m_gradientFalloff ? 1 : 0);

        cam = GetComponent<Camera>();
        cam.targetTexture = m_inputRenderTex;
        int occluders = 0;
        for (int i = 0; i < m_occluderLayers.Count; i++)
        {
            occluders = occluders | (1 << LayerMask.NameToLayer(m_occluderLayers[i]));
        }
        cam.cullingMask = occluders;
        m_meshRenderer.material = m_material;
    }

    void Update()
    {

        m_material.color = m_lightColor;

        m_meshRenderer.transform.localScale = new Vector3(m_lightSize, m_lightSize);
        cam.orthographicSize = m_lightSize * 0.5f - 0.1f;
    }
    
	void OnPostRender()
    {
        Graphics.Blit(m_inputRenderTex, m_middleRenderTex, new Material(Shader.Find("2DLighting/Deferred_Point_1")));
    }
}
