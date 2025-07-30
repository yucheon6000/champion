using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : BTComponent
{
    private new Rigidbody2D rigidbody2D;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.gravityScale = 1;
    }
}
