using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OccluderManager : MonoBehaviour {

    public static OccluderManager Instance;
    public int framesPerUpdate = 10;

    public LightOccluder[] occludersInFrustrum;

    private int frameCount = 0;
    
	void Awake () {
        Instance = this;
	}

    void Start()
    {
        UpdateOccluderList();
    }
	
	void Update () {
        frameCount++;
        if(frameCount >= framesPerUpdate)
        {
            UpdateOccluderList();
            frameCount = 0;
        }
	}

    private void UpdateOccluderList()
    {
        List<LightOccluder> allOccluders = new List<LightOccluder>(FindObjectsOfType<LightOccluder>());
        for (int i = 0; i < allOccluders.Count; )
        {
            bool anyRenderersVisible = false;
            for (int k = 0; k < allOccluders[i].myRenderer.Length; k++)
            {
                if (allOccluders[i].myRenderer[k].isVisible)
                {
                    anyRenderersVisible = true;
                    break;
                }
            }
            if (!anyRenderersVisible)
            {
                allOccluders.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
        occludersInFrustrum = allOccluders.ToArray();
    }
}
