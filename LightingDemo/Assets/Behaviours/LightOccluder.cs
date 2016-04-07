using UnityEngine;
using System.Collections;

public class LightOccluder : MonoBehaviour {
    //All colliders attached to this light occluder
    public PolygonCollider2D[] myColliders;
    
    //All renderers attached to this light occluder
    public SpriteRenderer[] myRenderer;
}
