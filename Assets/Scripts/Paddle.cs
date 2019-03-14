using Mirror;
using System;
using UnityEngine;

public class Paddle : NetworkBehaviour
{
    public float Speed = 10f;

    private Rigidbody2D RB;

    public float DefaultScale;
    public float GrowthScale = 4f;
    public float ShrinkScale = 1.5f;

    public PowerUp.Abilities AbilitySlot = PowerUp.Abilities.None;
    public PowerUp.Abilities ActiveAbility = PowerUp.Abilities.None;

    public float AbilityEnd;

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
        DefaultScale = transform.localScale.y;
    }

    void FixedUpdate()
    {
        // If this paddle is our local player:
        if (isLocalPlayer)
        {
            // Handle the movement.
            Movement();
        }
    }

    void Update()
    {
        // If we are controlling this paddle, have an ability, and our player is trying to use it:
        if (isLocalPlayer && HasAbility() && UseAbilityDown()) 
            // Tell the server we want to use our ability.
            CmdUseAbility();

        // If we are the server, the paddle has an active ability, and the ability should end:
        if (isServer && HasActiveAbility() && Time.time > AbilityEnd)
            // Tell all clients we should end the ability.
            RpcEndAbility();
    }

    #region Ability

    [Command]
    private void CmdUseAbility()
    {
        // If according to us (server) the paddle (client) doesn't have any ability.
        // This could be because of lag or a malicious client; just ignore them.
        if (!HasAbility())
            return;

        // If a player is trying to activate the ability for another client, return.
        // This could be a glitch but is most likely a client with malicious intent.
        if (connectionToClient.playerController.netId != netId)
            return;

        // Tell all clients that this paddle is using their ability.
        RpcUseAbility();
    }

    [ClientRpc]
    private void RpcUseAbility()
    {
        // Set the active ability to our ability.
        ActiveAbility = AbilitySlot;

        // Reset the ability of this paddle.
        ResetAbility();

        // Use the ability and set its end time.
        AbilityEnd = Ability.Begin(ActiveAbility, this) + Time.time;

        // If this is our paddle using the ability:
        if (isLocalPlayer)
            // Tell our UI to reset our ability.
            PowerUpUI.Instance.Reset();
    }

    [ClientRpc]
    private void RpcEndAbility()
    {
        Debug.Log("End ability!");

        // End the ability.
        Ability.End(ActiveAbility, this);

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

    private bool UseAbilityDown()
    {
        // If we are pressing our ability button by keyboard, return true.
        if (Input.GetButtonDown("Ability"))
            return true;

        // Get the names of all connected controllers.
        var controllers = Input.GetJoystickNames();

        // If we have no controllers connected, return false.
        if (controllers.Length < 1)
            return false;

        // If we are pressing the ability button on a controller, return true.
        if ((controllers[0].Length == 19 && Input.GetButtonDown("Ability PS"))
            || (controllers[0].Length == 33 && Input.GetButtonDown("Ability Xbox")))
            return true;

        // Return false.
        return false;
    }

    #endregion

    private void Movement()
    {
        // Get the input from our vertical axis.
        float vertical = Input.GetAxisRaw("Vertical");

        // Set our X velocity to zero.
        // Set our Y velocity to our vertical axis multiplied by Speed.
        RB.velocity = new Vector2(0, vertical * Speed);
    }

    public static Paddle FromNetId(uint netId)
    {
        var paddles = FindObjectsOfType<Paddle>();

        return Array.Find(paddles, paddle => paddle.netId.Equals(netId));
    }
}
