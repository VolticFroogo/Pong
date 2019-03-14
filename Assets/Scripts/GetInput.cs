using UnityEngine;

public class GetInput : MonoBehaviour
{
    public static bool ButtonDown(string name)
    {
        // If we are pressing our ability button by keyboard, return true.
        if (Input.GetButtonDown(name))
            return true;

        // Get the names of all connected controllers.
        var controllers = Input.GetJoystickNames();

        // If we have no controllers connected, return false.
        if (controllers.Length < 1)
            return false;

        // If we are pressing the ability button on a controller, return true.
        if ((controllers[0].Length == 19 && Input.GetButtonDown(name + " PS"))
            || (controllers[0].Length == 33 && Input.GetButtonDown(name + " Xbox")))
            return true;

        // Return false.
        return false;
    }
}
