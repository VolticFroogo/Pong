using Mirror;
using System;
using UnityEngine;

public class Paddle : NetworkBehaviour
{
    public float Speed = 10f;

    private Rigidbody2D RB;

    [HideInInspector]
    public Vector2 DefaultScale;
    public Vector2 GrowthScale;
    public Vector2 ShrinkScale;

    [HideInInspector]
    public PowerUp.Abilities AbilitySlot = PowerUp.Abilities.None;
    [HideInInspector]
    public PowerUp.Abilities ActiveAbility = PowerUp.Abilities.None;

    [HideInInspector]
    public float AbilityEnd;

    [HideInInspector]
    public bool Left;

    [HideInInspector]
    public bool InvertedControls = false;

    void Awake()
    {
        // Initialise variables.
        RB = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // If we are the server set the position to the left, otherwise to the right.
        if (isServer)
            RB.position = new Vector2(-12.5f, RB.position.y);
        else
            RB.position = new Vector2(12.5f, RB.position.y);

        // Setup our default scale variable.
        DefaultScale = transform.localScale;
    }

    void FixedUpdate()
    {
        // If this paddle is our local player:
        if (isLocalPlayer)
            // Handle the movement.
            Movement();
    }

    void Update()
    {
        if (RB.position.x < 0)
            Left = true;
        else
            Left = false;

        // If we are controlling this paddle, have an ability, and our player is trying to use it:
        if (isLocalPlayer && HasAbility() && GetInput.ButtonDown("Ability")) 
            // Tell the server we want to use our ability.
            CmdUseAbility();

        // If we are controlling this paddle, have an ability, and our player is trying to use it:
        if (isLocalPlayer && GetInput.ButtonDown("Switch Team"))
            // Tell the server we want to switch team.
            CmdSwitchTeam();

        // If we are the server, the paddle has an active ability, and the ability should end:
        if (isServer && HasActiveAbility() && Time.time > AbilityEnd)
            // Tell all clients we should end the ability.
            RpcEndAbility(ActiveAbility);
    }

    #region Ability

    [Command]
    private void CmdUseAbility()
    {
        // If according to us (server) the paddle (client) doesn't have any ability.
        // This could be because of lag or a malicious client; tell them they have no ability.
        if (!HasAbility())
        {
            TargetNoAbility(connectionToClient);
            return;
        }

        // If a player is trying to activate the ability for another client, return.
        // This could be a glitch but is most likely a client with malicious intent.
        if (connectionToClient.playerController.netId != netId)
            return;

        // Tell all clients that this paddle is using their ability.
        RpcUseAbility(AbilitySlot);
    }

    [TargetRpc]
    private void TargetNoAbility(NetworkConnection connection)
    {
        // This will be called if a client thinks they have a power up but the server disagrees.
        // This will mainly happen because of lag so we will update the UI and ability.

        // Reset the ability.
        ResetAbility();

        // Tell our UI to reset our ability.
        PowerUpUI.Instance.Reset();
    }

    [ClientRpc]
    private void RpcUseAbility(PowerUp.Abilities ability)
    {
        // Set the active ability to our ability.
        ActiveAbility = ability;

        // Reset the ability of this paddle.
        ResetAbility();

        // Use the ability and set its end time.
        var delay = Ability.Begin(ActiveAbility, this, isServer);

        AbilityEnd = Time.time + delay;

        // If we're using a one time use ability, don't try to end it.
        if (delay == 0f)
            ResetActiveAbility();

        // If this is our paddle using the ability:
        if (isLocalPlayer)
            // Tell our UI to reset our ability.
            PowerUpUI.Instance.Reset();
    }

    [ClientRpc]
    private void RpcEndAbility(PowerUp.Abilities ability)
    {
        // Call the non-RPC end ability.
        EndAbility(ability);
    }

    public void EndAbility(PowerUp.Abilities ability)
    {
        // End the ability.
        Ability.End(ability, this, isServer);

        // Reset the active ability of this paddle.
        ResetActiveAbility();
    }

    public bool HasAbility()
    {
        // Return if our ability doesn't equal none.
        return AbilitySlot != PowerUp.Abilities.None;
    }

    public bool HasActiveAbility()
    {
        // Return if our ability doesn't equal none.
        return ActiveAbility != PowerUp.Abilities.None;
    }

    public void ResetAbility()
    {
        // Set our ability to none.
        AbilitySlot = PowerUp.Abilities.None;
    }

    public void ResetActiveAbility()
    {
        // Set our ability to none.
        ActiveAbility = PowerUp.Abilities.None;
    }

    #endregion

    #region Team

    [Command]
    private void CmdSwitchTeam()
    {
        // If a player is trying to switch team for another client, return.
        // This could be a glitch but is most likely a client with malicious intent.
        if (connectionToClient.playerController.netId != netId)
            return;

        // If the team that they are trying to join is full.
        if (AmountOnTeam(!Left) >= 2)
        {
            // Tell the client that the team is full and return.
            TargetTeamFull(connectionToClient);
            return;
        }

        // Tell all clients that this paddle is switching teams.
        RpcSwitchTeam();
    }

    [TargetRpc]
    private void TargetTeamFull(NetworkConnection connection)
    {
        // TODO: tell the client the team they tried to join is full.
        Debug.Log("Team full!");
    }

    [ClientRpc]
    private void RpcSwitchTeam()
    {
        // Invert the left boolean.
        Left = !Left;

        // Get the coordinates of the new position.
        var x = Left ? -12.5f : 12.5f;
        var y = transform.position.y;

        // Update the position.
        transform.position = new Vector2(x, y);
    }

    #endregion

    private void Movement()
    {
        // Get the input from our vertical axis.
        float vertical = Input.GetAxisRaw("Vertical");

        // If we have inverted controls, invert the vertical axis.
        if (InvertedControls)
            vertical = -vertical;

        // Set our X velocity to zero.
        // Set our Y velocity to our vertical axis multiplied by Speed.
        RB.velocity = new Vector2(0, vertical * Speed);
    }

    public static Paddle FromNetId(uint netId)
    {
        var paddles = FindObjectsOfType<Paddle>();

        return Array.Find(paddles, paddle => paddle.netId.Equals(netId));
    }

    public static Paddle[] FromTeam(bool left)
    {
        var paddles = FindObjectsOfType<Paddle>();

        return Array.FindAll(paddles, paddle => paddle.Left.Equals(left));
    }

    public static int AmountOnTeam(bool left)
    {
        return FromTeam(left).Length;
    }
}
