using Mirror;
using UnityEngine.UI;

public class Score : NetworkBehaviour
{
    public int Left = 0;
    public int Right = 0;
    public Text text;

    // Singleton.
    public static Score Instance;

    void Awake()
    {
        // Initialise our variables.
        Instance = this;
    }

    void OnGUI()
    {
        // Set the text.
        text.text = Left + " - " + Right;
    }

    [ClientRpc] // Server --> Client
    public void RpcSet(int left, int right)
    {
        // Update the score to whatever the server tells us.
        Left = left;
        Right = right;
    }

    [ServerCallback] // Only run on server.
    void Update()
    {
        if (GetInput.ButtonDown("Reset"))
        {
            Left = 0;
            Right = 0;
        }
    }
}
