using UnityEngine;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour {

    public Image W;
    public Image A;
    public Image S;
    public Image D;
    public Image SPACE;

    public Color DepressedColour;
    public Color DefaultColour;
	
	void Update ()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            SPACE.color = DefaultColour;
        }
        else
        {
            SPACE.color = DepressedColour;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            W.color = DefaultColour;
        }
        else
        {
            W.color = DepressedColour;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            A.color = DefaultColour;
        }
        else
        {
            A.color = DepressedColour;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            S.color = DefaultColour;
        }
        else
        {
            S.color = DepressedColour;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            D.color = DefaultColour;
        }
        else
        {
            D.color = DepressedColour;
        }
    }
}
