using UnityEngine;

public class Grow : MonoBehaviour
{
    public static float Duration = 15f;

    public static void Begin(Paddle paddle)
    {
        // Define the new scale we want.
        var scale = new Vector2(paddle.GrowthScale.x, paddle.GrowthScale.y);

        // Update the paddle to the new scale.
        paddle.transform.localScale = scale;
    }

    public static void End(Paddle paddle)
    {
        // Define the new scale we want.
        var scale = new Vector2(paddle.DefaultScale.x, paddle.DefaultScale.y);

        // Update the paddle to the new scale.
        paddle.transform.localScale = scale;
    }
}
