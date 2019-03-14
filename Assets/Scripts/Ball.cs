using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public float Speed = 30f;

    public AudioClip HitSound;

    private AudioSource Audio;

    private Rigidbody2D RB;

    private uint LastHit;

    private List<Vector2> States = new List<Vector2>()
    {
        new Vector2(-0.5f, -0.5f), // Left and up.
        new Vector2(-0.5f, 0.5f), // Left and down.
        new Vector2(0.5f, -0.5f), // Right and up.
        new Vector2(-0.5f, -0.5f) // Right and down.
    };

    void Awake()
    {
        // Initialise variables.
        RB = GetComponent<Rigidbody2D>();
        Audio = GetComponent<AudioSource>();

        // Set the audio player clip to our hit sound.
        Audio.clip = HitSound;
    }

    public override void OnStartServer()
    {
        // Set the ball to simulated.
        RB.simulated = true;

        // Reset the ball.
        Reset();

        PowerUpSpawner.Instance.Begin();
    }

    public void Update()
    {
        // If the ball is out of bounds, reset.
        if (transform.position.x > 20f
            || transform.position.x < -20f
            || transform.position.y > 10f
            || transform.position.y < -10f)

            Reset();
    }

    public void Reset()
    {
        // Set the ball to the middle of the screen.
        transform.position = new Vector2(0, 0);

        // Pick a random state.
        var state = Random.Range(0, States.Count);

        // Set the velocity to the state multiplied by the speed.
        RB.velocity = States[state] * Speed;
    }

    [ClientRpc] // Server --> Client
    private void RpcPlayHitSound()
    {
        // Play the audio clip.
        Audio.Play();
    }

    [ServerCallback] // Only run on server.
    void OnCollisionEnter2D(Collision2D col)
    {
        // Tell all clients to play hit sound.
        RpcPlayHitSound();

        // Get the paddle component (if it exists).
        var paddle = col.gameObject.GetComponent<Paddle>();

        // If we collided with a paddle:
        if (paddle)
        {
            // Set last hit to the paddle we just collided with.
            LastHit = paddle.netId;

            // Calculate the width of the paddle.
            var xOff = col.collider.bounds.size.x / 2;

            // If we are on the left, subtract it.
            // If we are on the right, add it.
            xOff = paddle.transform.position.x < 0 ? -xOff : xOff;

            // Calculate delta values.
            var dx = paddle.transform.position.x + xOff - transform.position.x;
            var dy = paddle.transform.position.y - transform.position.y;

            // Calculate angle from delta values.
            var rad = Mathf.Atan2(dy, dx);

            // Convert velocity to absolute.
            var absVelocityX = Mathf.Abs(RB.velocity.x);
            var absVelocityY = Mathf.Abs(RB.velocity.y);

            // Calculate power from absolute velocity.
            var power = absVelocityX + absVelocityY;

            // Calculate the angles.
            var angleX = -Mathf.Cos(rad);
            var angleY = -Mathf.Sin(rad);

            // Convert the angles to absolute values.
            var absAngleX = Mathf.Abs(angleX);
            var absAngleY = Mathf.Abs(angleY);

            // Calculate total angle from absolute angles.
            var total = absAngleX + absAngleY;

            // Calculate how much power each axis receives.
            var xPowerPercentage = angleX / total;
            var yPowerPercentage = angleY / total;

            // Caluclate the total power for each axis.
            var xPower = xPowerPercentage * power;
            var yPower = yPowerPercentage * power;

            // Round the power to two decimal places.
            // This is to correct floating point errors.
            var xPowerRounded = Mathf.Round(xPower * 100) / 100;
            var yPowerRounded = Mathf.Round(yPower * 100) / 100;

            // Set new velocity.
            RB.velocity = new Vector2(xPowerRounded, yPowerRounded);
        }
    }

    [ServerCallback] // Only run on server.
    void OnTriggerEnter2D(Collider2D col)
    {
        // Get the power up component (if it exists).
        var powerUp = col.gameObject.GetComponent<PowerUp>();

        // If we collided with a power up:
        if (powerUp)
        {
            // Tell our spawner that the power up was destroyed.
            PowerUpSpawner.Instance.Destroyed();

            // Tell our server somebody hit the power up.
            powerUp.Hit(LastHit);

            // Tell all clients somebody hit the power up.
            powerUp.RpcHit(LastHit);

            // Destroy the power up on all clients.
            NetworkServer.Destroy(col.gameObject);
        }
    }
}
