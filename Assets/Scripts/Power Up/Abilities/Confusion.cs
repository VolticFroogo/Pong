using UnityEngine;

public class Confusion : MonoBehaviour
{
    public static float Duration = 10f;

    public static void Begin(Paddle paddle)
    {
        // Get all of the opponents.
        var opponents = Paddle.FromTeam(!paddle.Left);

        // Iterate through each opponent:
        foreach (var opponent in opponents) 
            // Invert their controls.
            opponent.InvertedControls = true; 
    }

    public static void End(Paddle paddle)
    {
        // Get all of the opponents.
        var opponents = Paddle.FromTeam(!paddle.Left);

        // Iterate through each opponent:
        foreach (var opponent in opponents)
            // Fix their controls.
            opponent.InvertedControls = false;
    }
}
