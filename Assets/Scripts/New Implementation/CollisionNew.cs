using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionNew : MonoBehaviour
{

    [Header("Layers")]
    public LayerMask groundLayer;

    [Space]

    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public int wallSide;

    [Space]

    [Header("Collision")]

    public float collisionRadius = 0.01f;
    public Vector2 bottomOffset, rightOffset, leftOffset;
    private Color debugCollisionColor = Color.red;

    private BoxCollider2D body;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {  
        onGround = Physics2D.OverlapBox(new Vector2(body.bounds.center.x, body.bounds.center.y - body.bounds.size.y / 2),
            new Vector2(body.bounds.size.x - 0.05f, 0.02f), 0, groundLayer);
        onWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer) 
            || Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        wallSide = onRightWall ? -1 : 1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset, rightOffset, leftOffset };

        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);
    }
}
