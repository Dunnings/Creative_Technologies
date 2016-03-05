using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugInfo : MonoBehaviour {

    public Text[] DebugLabels;

    public Text[] Dividers;
    int[] DividerStartSize;

    private float delayedUpdateLastTime = 0f;
    
    void Start () {
        DividerStartSize = new int[Dividers.Length];
        for (int i = 0; i < Dividers.Length; i++)
        {
            DividerStartSize[i] = Dividers[i].text.Length;
        }
	}
	
	void LateUpdate () {
        if (Time.time - delayedUpdateLastTime > 0.2f)
        {
            DebugLabels[0].text = (1f / Time.deltaTime).ToString("0.0");

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
            delayedUpdateLastTime = Time.time;
        }

        for (int i = 0; i < Dividers.Length; i++)
        {
            Dividers[i].text = "";
            for (int x = 0; x < DividerStartSize[i] + (5-DebugLabels[i].text.Length); x++)
            {
                Dividers[i].text += "_";
            }
        }
	}
}
