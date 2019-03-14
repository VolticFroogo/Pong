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
        }

        return 0f;
    }
}
