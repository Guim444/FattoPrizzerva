using System.Collections;
using UnityEngine;

public class RingScript : MonoBehaviour
{
    public Collider colOutside, colInside;
    public PlayerController player;
    public float slowMultiplier = 0.1f;
    //public bool canExit;
    public void OnChildTriggerEnter(string ringName)
    {
        if (ringName == "OuterRing")
        {
            //to do
        }
        else if (ringName == "InnerRing")
        {
            //to do
        }
    }
    IEnumerator MoveBack()
    {
        Debug.Log("MOVING BACK");
        player.enabled = false;
        player.transform.position -= player.transform.forward * 5f;
        yield return new WaitForSeconds(0.1f);
        player.enabled = true;
        player.normalSpeed = 0;
    }
}
