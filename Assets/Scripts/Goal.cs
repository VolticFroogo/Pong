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

        // Find the score object
        var score = FindObjectOfType<Score>();

        // If we couldn't find the score object, return.
        if (!score)
            return;

        // Set the score to our local variables.
        var left = score.Left;
        var right = score.Right;

        // Add a point to the respective side.
        if (Left)
            left++;
        else
            right++;

        // Call set score on all clients.
        RpcSetScore(left, right);
    }

    [ClientRpc] // Server --> Client
    public void RpcSetScore(int left, int right)
    {
        // Find the score object.
        var score = FindObjectOfType<Score>();

        // If the score doesn't exist, return.
        if (!score)
            return;

        // Set the new score.
        score.Set(left, right);
    }
}