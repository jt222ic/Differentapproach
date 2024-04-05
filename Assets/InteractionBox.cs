using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionBox : MonoBehaviour
{
    // Start is called before the first frame updateb
 
    BoxCollider2D box;
  
    void Start()
    {

        box = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerStay2D(Collider2D other)
    {

        Debug.Log("TESTING");
        if (other.gameObject.name == "AttackHit")
        {
            NewPlayer.Instance.InterractionWithObject = true;
            box.isTrigger = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.GetComponent<NewPlayer>())
        {
            NewPlayer.Instance.InterractionWithObject = false;
           
            box.isTrigger = false;
        }
    }
}
