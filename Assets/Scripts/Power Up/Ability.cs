using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public static float Begin(PowerUp.Abilities ability, Paddle paddle)
    {
        return Use(ability, paddle, true);
    }

    public static void End(PowerUp.Abilities ability, Paddle paddle)
    {
        Use(ability, paddle, false);
    }

    private static float Use(PowerUp.Abilities ability, Paddle paddle, bool begin)
    {
        // Switch of ability:
        switch (ability)
        {
            // If we are using the grow power up, call its respective function.
            case PowerUp.Abilities.Grow:
                if (begin)
                {
                    Grow.Begin(paddle);
                    return Grow.Duration;
                }

                Grow.End(paddle);
                return 0f;

            // If we are using the shrink power up, call its respective function.
            case PowerUp.Abilities.Shrink:
                if (begin)
                {
                    Shrink.Begin(paddle);
                    return Shrink.Duration;
                }

                Shrink.End(paddle);
                return 0f;
        }

        return 0f;
    }
}
