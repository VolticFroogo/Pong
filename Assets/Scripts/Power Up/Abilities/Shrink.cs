using UnityEngine;

public class Shrink : MonoBehaviour
{
    public static float Duration = 15f;

    public static void Begin(Paddle paddle)
    {
        // Get all of the opponents.
        var opponents = Paddle.FromTeam(!paddle.Left);

        // Iterate through each opponent:
        foreach (var opponent in opponents)
        {
            // Define the new scale we want.
            var scale = new Vector2(opponent.ShrinkScale.x, opponent.ShrinkScale.y);

            // Update the paddle to the new scale.
            opponent.transform.localScale = scale;
        }
    }

    public static void End(Paddle paddle)
    {
        // Get all of the opponents.
        var opponents = Paddle.FromTeam(!paddle.Left);

        // Iterate through each opponent:
        foreach (var opponent in opponents)
        {
            // Define the new scale we want.
            var scale = new Vector2(opponent.DefaultScale.x, opponent.DefaultScale.y);

            // Update the paddle to the new scale.
            opponent.transform.localScale = scale;
        }
    }
}
