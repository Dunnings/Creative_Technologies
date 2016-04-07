using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OccluderManager : MonoBehaviour {

    //Singleton instance
    public static OccluderManager Instance;

    //How often should the occluders in frustrum be calculated
    //Higher = better performance
    public int framesPerUpdate = 10;

    //All occluders in the view frustrum
    public LightOccluder[] occludersInFrustrum;

    //Used to keep track of frames passed
    private int frameCount = 0;
    
    //Initialize singleton instance on Awake
	void Awake () {
        Instance = this;
	}

    //Update the Occluder list at the start
    void Start()
    {
        UpdateOccluderList();
    }
	
    //Update the occluder list every given frames
	void Update () {
        frameCount++;
        if(frameCount >= framesPerUpdate)
        {
            UpdateOccluderList();
            frameCount = 0;
        }
	}

    /// <summary>
    /// Finds all light occluders in the scene and compiles a list of the ones within the view frustrum
    /// </summary>
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
