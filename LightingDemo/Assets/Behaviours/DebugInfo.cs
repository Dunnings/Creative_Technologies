using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// This class is used to display debug information
/// </summary>
public class DebugInfo : MonoBehaviour {

    //Array of debug labels
    public Text[] DebugLabels;
    
    //Array of start sizes for dividers
    int[] DividerStartSize;

    //Last time updated the debug menu
    private float delayedUpdateLastTime = 0f;

    //Should be moved to a seperate class here for convenience
    public RenderTexture RenTex;

    //Last 30 frame times
    List<float> previousFrameSamples = new List<float>();

    private int frameSampleCount = 30;

    private KeyCode[] levelKeys = new KeyCode[4] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    
    void Start () {
        //Put here but should be moved to a light manager
        RenTex.height = Screen.height;
        RenTex.width = Screen.width;
	}

    void Update()
    {
        //Check if the user has pressed any keys to load levels
        for (int i = 0; i < levelKeys.Length; i++)
        {
            if (Input.GetKeyDown(levelKeys[i]))
            {
                //Load the appropriate level
                Application.LoadLevel(i);
            }
        }
        //If the user has pressed escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Exit the application
            Application.Quit();
        }
    }
	
	void LateUpdate () {
        //Every 0.2 seconds
        if (Time.time - delayedUpdateLastTime > 0.2f)
        {
            //Update relevant debug information
            float avFPS = 0f;
            for (int i = 0; i < previousFrameSamples.Count; i++)
            {
                avFPS += previousFrameSamples[i];
            }
            avFPS /= previousFrameSamples.Count;

            DebugLabels[0].text = avFPS.ToString("0.0");
            DebugLabels[1].text = FindObjectsOfType<Mesh_Light>().Length.ToString("0");
            DebugLabels[2].text = FindObjectsOfType<Shader_Light>().Length.ToString("0");
            DebugLabels[3].text = (OccluderManager.Instance.occludersInFrustrum.Length).ToString("0");
            int colliderCount = 0;
            for (int i = 0; i < OccluderManager.Instance.occludersInFrustrum.Length; i++)
            {
                colliderCount += OccluderManager.Instance.occludersInFrustrum[i].myColliders.Length;
            }
            DebugLabels[4].text = colliderCount.ToString("0");

            int pointCount = 0;
            for (int i = 0; i < OccluderManager.Instance.occludersInFrustrum.Length; i++)
            {
                for (int x = 0; x < OccluderManager.Instance.occludersInFrustrum[i].myColliders.Length; x++)
                {
                    pointCount += OccluderManager.Instance.occludersInFrustrum[i].myColliders[x].points.Length;
                }
            }
            DebugLabels[5].text = pointCount.ToString("0");

            //Set the time we last updated the debug menu to now
            delayedUpdateLastTime = Time.time;
        }

        previousFrameSamples.Add(1f / Time.deltaTime);
        if(previousFrameSamples.Count > frameSampleCount)
        {
            previousFrameSamples.RemoveAt(0);
        }
	}
}
