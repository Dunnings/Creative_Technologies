using UnityEngine;
using System.Collections;

public class DemoController : MonoBehaviour {

    public Material spriteMaterial;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (spriteMaterial.GetFloat("_SpriteLightness") == 1f)
            {
                spriteMaterial.SetFloat("_SpriteLightness", 0.25f);
            }
            else
            {
                spriteMaterial.SetFloat("_SpriteLightness", 1f);
            }
        }
    }
}
