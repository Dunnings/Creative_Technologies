using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DemoController : MonoBehaviour {

    public Slider R;
    public Slider G;
    public Slider B;
    public Slider A;
    public Toggle lightingToggle;
    public Toggle meshToggle;
    public Toggle shaderToggle;

    public Material spriteMaterial;
    public Mesh_Light meshLight;
    public Shader_Light shaderLight;

    bool OptionsPanelShown = false;
    bool isAnimatingOptionsPanel = false;
    float OpenPosition = 0f;
    float ClosedPosition = -180f;

	// Use this for initialization
	void Start ()
    {
        R.value = meshLight.lightColor.r;
        G.value = meshLight.lightColor.g;
        B.value = meshLight.lightColor.b;
        A.value = meshLight.lightColor.a;
        lightingToggle.isOn = spriteMaterial.GetFloat("_ScreenSpaceLighting") == 1f;
    }
	
	// Update is called once per frame
	void Update ()
    {

    }

    public void ToggleLighting(bool toggle)
    {
        if (toggle)
        {
            spriteMaterial.SetFloat("_ScreenSpaceLighting", 1f);
        }
        else
        {
            spriteMaterial.SetFloat("_ScreenSpaceLighting", 0f);
        }
    }

    public void ChangeLightR(float r)
    {
        meshLight.lightColor.r = r;
        shaderLight.m_lightColor.r = r;
    }

    public void ChangeLightG(float r)
    {
        meshLight.lightColor.g = r;
        shaderLight.m_lightColor.g = r;
    }

    public void ChangeLightB(float r)
    {
        meshLight.lightColor.b = r;
        shaderLight.m_lightColor.b = r;
    }

    public void ChangeLightA(float r)
    {
        meshLight.lightColor.a = r;
        shaderLight.m_lightColor.a = r;
    }
    
    public void ToggleOptionsPanel()
    {
        if (!isAnimatingOptionsPanel)
        {
            StartCoroutine(ToggleOptionsPanelAnimation());
        }
    }

    IEnumerator ToggleOptionsPanelAnimation()
    {
        isAnimatingOptionsPanel = true;
        if (OptionsPanelShown)
        {
            float d = 0f;
            while(d < 0.5f)
            {
                transform.position = new Vector3(Mathf.Lerp(OpenPosition, ClosedPosition, d / 0.5f), transform.position.y, transform.position.z);
                d += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
                transform.position = new Vector3(ClosedPosition, transform.position.y, transform.position.z);
        }
        else
        {
            float d = 0f;
            while (d < 0.5f)
            {
                transform.position = new Vector3(Mathf.Lerp(ClosedPosition, OpenPosition, d / 0.5f), transform.position.y, transform.position.z);
                d += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            transform.position = new Vector3(OpenPosition, transform.position.y, transform.position.z);
        }
        OptionsPanelShown = !OptionsPanelShown;
        isAnimatingOptionsPanel = false;
    }

    public void LightingModeChanged(int mode)
    {
        switch (mode)
        {
            case 0:
                meshLight.transform.gameObject.SetActive(true);
                shaderLight.transform.gameObject.SetActive(false);
                break;
            case 1:
                meshLight.transform.gameObject.SetActive(false);
                shaderLight.transform.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}
