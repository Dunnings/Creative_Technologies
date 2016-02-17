using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DemoController : MonoBehaviour {

    public Slider R;
    public Slider G;
    public Slider B;
    public Slider A;
    public Toggle lightingToggle;

    public Material spriteMaterial;
    public Mesh_Light playerLight;

    bool OptionsPanelShown = false;
    bool isAnimatingOptionsPanel = false;
    float OpenPosition = 0f;
    float ClosedPosition = -200f;

	// Use this for initialization
	void Start ()
    {
        R.value = playerLight.lightColor.r;
        G.value = playerLight.lightColor.g;
        B.value = playerLight.lightColor.b;
        A.value = playerLight.lightColor.a;
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
        playerLight.lightColor.r = r;
    }

    public void ChangeLightG(float r)
    {
        playerLight.lightColor.g = r;
    }

    public void ChangeLightB(float r)
    {
        playerLight.lightColor.b = r;
    }

    public void ChangeLightA(float r)
    {
        playerLight.lightColor.a = r;
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
        }
        OptionsPanelShown = !OptionsPanelShown;
        isAnimatingOptionsPanel = false;
    }

    public void TogglePlayerLight(bool toggle)
    {
        playerLight.transform.gameObject.SetActive(toggle);
    }
}
