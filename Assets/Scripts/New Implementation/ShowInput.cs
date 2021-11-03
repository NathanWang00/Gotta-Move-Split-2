using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowInput : MonoBehaviour
{
    public Color off, on;
    private SpriteRenderer jumpSprite;
    private void Start()
    {
        jumpSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetButton("Jump"))
        {
            jumpSprite.color = on;
        }
        else
            jumpSprite.color = off;
    }
}
