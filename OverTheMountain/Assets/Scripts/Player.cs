using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour {
    public float speed = 10F;

    private Rigidbody2D r2d;

    private void Awake()
    {
        r2d = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        float up = Input.GetAxis("Vertical");
        float right = Input.GetAxis("Horizontal");

        r2d.velocity = new Vector2(right * speed, up * speed);
    }
}
