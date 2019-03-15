using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSize : MonoBehaviour
{
    public static float Duration = 10f;

    public static void Reset()
    {
        Ball.Instance.transform.localScale = Ball.Instance.DefaultSize;
    }

    public static void Grow()
    {
        Debug.Log("Grow!");

        Ball.Instance.transform.localScale = Ball.Instance.GrowSize;
    }

    public static void Shrink()
    {
        Ball.Instance.transform.localScale = Ball.Instance.ShrinkSize;
    }
}
