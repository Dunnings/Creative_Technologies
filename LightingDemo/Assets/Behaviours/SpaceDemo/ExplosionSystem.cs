using UnityEngine;
using System.Collections;

public class ExplosionSystem : MonoBehaviour {

    public static ExplosionSystem Instance;
    public ParticleSystem playerParticles;
    public ParticleSystem enemyParticles;

    // Use this for initialization
    void Awake () {
        Instance = this;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void EmitPlayer(Vector3 pos, int i = 1)
    {
        playerParticles.enableEmission = true;
        playerParticles.transform.position = pos;
        playerParticles.Emit(i);
    }
    public void EmitEnemy(Vector3 pos, int i = 1)
    {
        enemyParticles.enableEmission = true;
        enemyParticles.transform.position = pos;
        enemyParticles.Emit(i);
    }
}
