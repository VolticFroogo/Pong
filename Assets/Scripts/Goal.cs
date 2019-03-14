using Mirror;
using UnityEngine;

public class Goal : NetworkBehaviour
{
    public bool Left;

    [ServerCallback] // Only run on server.
    void OnCollisionEnter2D(Collision2D col)
    {
        // Get the ball component.
        var ball = col.gameObject.GetComponent<Ball>();

        // If it wasn't a collision with a ball, return.
        if (!ball)
            return;

        // Reset the ball.
        ball.Reset();

        // Set the score to our local variables.
        var left = Score.Instance.Left;
        var right = Score.Instance.Right;

        // Add a point to the respective side.
        if (Left)
            right++;
        else
            left++;

        // Call set score on all clients.
        Score.Instance.RpcSet(left, right);
    }
}