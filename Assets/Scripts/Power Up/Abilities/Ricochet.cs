using UnityEngine;

public class Ricochet : MonoBehaviour
{
    public static void Use()
    {
        var velocity = Ball.Instance.RB.velocity;

        velocity.x = -velocity.x;
        velocity.y = -velocity.y;

        Ball.Instance.RB.velocity = velocity;
    }
}
