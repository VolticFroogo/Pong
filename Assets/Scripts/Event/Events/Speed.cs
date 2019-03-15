using UnityEngine;

public class Speed : MonoBehaviour
{
    public static void Use()
    {
        var velocity = Ball.Instance.RB.velocity;

        velocity.x *= 1.5f;
        velocity.y *= 1.5f;

        Ball.Instance.RB.velocity = velocity;
    }
}
