using UnityEngine;
using System.Collections;

public class Repeller : MonoBehaviour {
    
    void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "Asteroid")
        {
            other.GetComponent<Rigidbody2D>().AddForce((other.transform.position - transform.position).normalized * 50000f * Time.deltaTime, ForceMode2D.Impulse);
        }
    }
}
