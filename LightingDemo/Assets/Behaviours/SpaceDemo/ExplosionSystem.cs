using UnityEngine;
using System.Collections;

public class ExplosionSystem : MonoBehaviour {

    public static ExplosionSystem Instance;
    public ParticleSystem particles;

	// Use this for initialization
	void Awake () {
        Instance = this;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Emit(Vector3 pos, int i = 1)
    {
        particles.enableEmission = true;
        particles.transform.position = pos;
        particles.Emit(i);
    }
}
