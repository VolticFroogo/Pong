using Mirror;
using UnityEngine.UI;

public class Score : NetworkBehaviour
{
    public int Left = 0;
    public int Right = 0;
    public Text text;

    void OnGUI()
    {
        // Set the text.
        text.text = Left + " - " + Right;
    }

    public void Set(int left, int right)
    {
        Left = left;
        Right = right;
    }
}
