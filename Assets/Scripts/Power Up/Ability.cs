using UnityEngine;

public class Ability : MonoBehaviour
{
    public static float Begin(PowerUp.Abilities ability, Paddle paddle, bool isServer)
    {
        // If they already currently have an active ability, end it.
        if (paddle.HasActiveAbility())
            paddle.EndAbility(paddle.ActiveAbility);

        return Use(ability, paddle, true, isServer);
    }

    public static void End(PowerUp.Abilities ability, Paddle paddle, bool isServer)
    {
        Use(ability, paddle, false, isServer);
    }

    private static float Use(PowerUp.Abilities ability, Paddle paddle, bool begin, bool isServer)
    {
        // Switch of ability:
        switch (ability)
        {
            // If we are using the grow power up, call its respective function.
            case PowerUp.Abilities.Grow:
                if (begin)
                    Grow.Begin(paddle);
                else
                    Grow.End(paddle);

                return Grow.Duration;

            // If we are using the shrink power up, call its respective function.
            case PowerUp.Abilities.Shrink:
                if (begin)
                    Shrink.Begin(paddle);
                else
                    Shrink.End(paddle);

                return Shrink.Duration;

            // If we are using the confusion power up, call its respective function.
            case PowerUp.Abilities.Confusion:
                if (begin) 
                    Confusion.Begin(paddle);
                else
                    Confusion.End(paddle);

                return Confusion.Duration;

            // As ricochet is one time use, return zero as the duration and only use on begin.
            case PowerUp.Abilities.Ricochet:
                // Only run on server as this is change to the ball's velocity which the server controls.
                if (isServer && begin)
                    Ricochet.Use();

                return 0f;
        }

        return 0f;
    }
}
