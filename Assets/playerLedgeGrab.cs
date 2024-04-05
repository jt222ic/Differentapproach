using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerLedgeGrab : MonoBehaviour
{
    // Start is called before the first frame update

    private bool GreenBox, RedBox;
    public float greenXOffset, greenYOffset, greenXSize, greenYSize;
    public float redXOffset, redYOffset, redXSize, redYSize;
    private Rigidbody2D rb;
    private float StartingGravity;
    public LayerMask layermask;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartingGravity = rb.gravityScale;
       
        
    }

    // Update is called once per frame
    void Update()
    {
        RedBox = Physics2D.OverlapBox(new Vector2(transform.position.x + (redXOffset * transform.localScale.x), transform.position.y + redYOffset), new Vector2(redXSize, redYSize), 0, layermask);
        GreenBox = Physics2D.OverlapBox(new Vector2(transform.position.x + (greenXOffset * transform.localScale.x), transform.position.y + greenYOffset), new Vector2(greenXSize, greenYSize), 0, layermask);


        
        if(GreenBox && !RedBox && NewPlayer.Instance.jumping)
        {
            Debug.Log("Grab is on");
            NewPlayer.Instance.grabbing = true;
        }

        if(NewPlayer.Instance.grabbing)
        {
            rb.gravityScale = 0;
            rb.velocity = new Vector2(0f, 0f);
        }
        
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (redXOffset * transform.localScale.x), transform.position.y + redYOffset), new Vector2(redXSize, redYSize));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (greenXOffset * transform.localScale.x), transform.position.y + greenYOffset), new Vector2(greenXSize, greenYSize));

    }
}
