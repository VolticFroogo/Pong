using Mirror;
using UnityEngine;

public class Paddle : NetworkBehaviour
{
    public float Speed = 10f;

    private Rigidbody2D RB;

    void Awake()
    {
        // Initialise variables.
        RB = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // If we are the server set the position to the left, otherwise to the right.
        if (isServer)
            RB.position = new Vector2(-12.5f, RB.position.y);
        else
            RB.position = new Vector2(12.5f, RB.position.y);
    }

    void FixedUpdate()
    {
        // If this paddle is our local player:
        if (isLocalPlayer)
        {
            // Handle the movement.
            Movement();
        }
    }

    void Movement()
    {
        // Get the input from our vertical axis.
        float vertical = Input.GetAxisRaw("Vertical");

        // Set our X velocity to zero.
        // Set our Y velocity to our vertical axis multiplied by Speed.
        RB.velocity = new Vector2(0, vertical * Speed);
    }
}
