using UnityEngine;

public class Grow : MonoBehaviour
{
    public static float Duration = 15f;

    public static void Begin(Paddle paddle)
    {
        // Define the new scale we want.
        var scale = new Vector2(paddle.transform.localScale.x, paddle.GrowthScale);

        // Update the paddle to the new scale.
        paddle.transform.localScale = scale;
    }

    public static void End(Paddle paddle)
    {
        // Define the new scale we want.
        var scale = new Vector2(paddle.transform.localScale.x, paddle.DefaultScale);

        // Update the paddle to the new scale.
        paddle.transform.localScale = scale;
    }
}
