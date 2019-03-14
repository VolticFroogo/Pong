using UnityEngine;

public class Shrink : MonoBehaviour
{
    public static float Duration = 15f;

    public static void Begin(Paddle paddle)
    {
        // Get all of the opponents.
        var opponents = Paddle.FromTeam(!paddle.Left);

        // Iterate through each opponent:
        for (var i = 0; i < opponents.Length; i++)
        {
            // Define the new scale we want.
            var scale = new Vector2(opponents[i].ShrinkScale.x, opponents[i].ShrinkScale.y);

            // Update the paddle to the new scale.
            opponents[i].transform.localScale = scale;
        }
    }

    public static void End(Paddle paddle)
    {
        // Get all of the opponents.
        var opponents = Paddle.FromTeam(!paddle.Left);

        // Iterate through each opponent:
        for (var i = 0; i < opponents.Length; i++)
        {
            // Define the new scale we want.
            var scale = new Vector2(opponents[i].DefaultScale.x, opponents[i].DefaultScale.y);

            // Update the paddle to the new scale.
            opponents[i].transform.localScale = scale;
        }
    }
}
