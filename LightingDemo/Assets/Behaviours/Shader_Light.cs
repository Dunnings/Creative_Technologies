using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Main class for shader lighting solution
/// Requires an attached camera
/// </summary>
[RequireComponent(typeof(Camera))]
public class Shader_Light : MonoBehaviour {
    //Which layers should be treated as light occluders
    public List<string> m_occluderLayers = new List<string> { "LightOccluder" };

    //Should the light falloff over duration
    public bool m_gradientFalloff = true;

    //Should the light simulate penumbra
    public bool m_penumbra = true;

    //Light colour
    public Color m_lightColor;

    //Size of light in Unity units
    public int m_lightSize = 20;

    //Size of the shadow map
    private int m_shadowSize = 512;

    private RenderTexture m_inputRenderTex;
    private RenderTexture m_middleRenderTex;

    //Light material
    private Material m_lightMaterial;

    //Light mesh renderer
    public MeshRenderer m_meshRenderer;

    //Light camera
    private Camera m_camera;

    //Shader phase one material
    private Material phaseOneMaterial;

    // Use this for initialization
    void Awake()
    {
        //Initalize render textures
        m_inputRenderTex = new RenderTexture(m_shadowSize, m_shadowSize, 0, RenderTextureFormat.ARGB32);
        m_middleRenderTex = new RenderTexture(m_shadowSize, 1, 0, RenderTextureFormat.ARGB32);

        //Initialize light material
        m_lightMaterial = new Material(Shader.Find("2DLighting/Deferred_Point_2"));
        m_lightMaterial.SetTexture("_DepthMap", m_middleRenderTex);
        m_lightMaterial.SetInt("_Penumbra", m_penumbra ? 1 : 0);
        m_lightMaterial.SetInt("_GradientFalloff", m_gradientFalloff ? 1 : 0);
        
        //Initialize phase one material
        phaseOneMaterial = new Material(Shader.Find("2DLighting/Deferred_Point_1"));
        phaseOneMaterial.SetFloat("_DistanceModifier", 0.001f);

        //Aquire camera component and initialize it
        m_camera = GetComponent<Camera>();
        m_camera.enabled = true;
        m_camera.targetTexture = m_inputRenderTex;

        //Create culling mask from given occluder layers
        int occluders = 0;
        for (int i = 0; i < m_occluderLayers.Count; i++)
        {
            occluders = occluders | (1 << LayerMask.NameToLayer(m_occluderLayers[i]));
        }
        m_camera.cullingMask = occluders;

        //Assign mesh material
        m_meshRenderer.material = m_lightMaterial;
    }

    /// <summary>
    /// Run every frame
    /// </summary>
    void Update()
    {
        //Update light colour
        m_lightMaterial.color = m_lightColor;

        //Update light size
        m_meshRenderer.transform.localScale = new Vector3(m_lightSize, m_lightSize);
        m_camera.orthographicSize = m_lightSize * 0.5f - 0.1f;
    }
    
    /// <summary>
    /// After the camera renders the scene
    /// </summary>
	void OnPostRender()
    {
        //Blit the phase one shader onto the render texture
        Graphics.Blit(m_inputRenderTex, m_middleRenderTex, phaseOneMaterial);
    }
}
