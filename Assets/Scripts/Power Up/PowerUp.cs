using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : NetworkBehaviour
{
    public enum Abilities
    {
        None,
        Grow,
        Shrink,
        Confusion
    }

    [SyncVar]
    private Abilities Ability;

    private Abilities CurrentSprite;

    public SpriteRenderer Renderer;

    public List<Sprite> Sprites;

    void Awake()
    {
        // Get our sprite renderer.
        Renderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (isServer)
            // Pick our ability skipping over none.
            Ability = (Abilities)Random.Range(1, Abilities.GetValues(typeof(Abilities)).Length);
    }

    void Update()
    {
        // If the sprite isn't set yet:
        if (CurrentSprite != Ability)
        {
            // Set our sprite corresponding to the ability we have.
            Renderer.sprite = Sprites[(int)Ability - 1];

            // Set our current sprite variable to our ability.
            CurrentSprite = Ability;
        }
    }

    [ClientRpc] // Server --> Client
    public void RpcHit(uint netId)
    {
        // Call the hit function.
        // This is split into two functions because the server needs to call hit without an RPC.
        Hit(netId);
    }

    public void Hit(uint netId)
    {
        // Get the paddle that deserves the power up.
        var paddle = Paddle.FromNetId(netId);

        // Set the paddle's ability to this new ability.
        paddle.AbilitySlot = Ability;

        // If we just got a power up:
        if (paddle.isLocalPlayer)
        {
            // Tell our UI we have a new power up.
            PowerUpUI.Instance.Set(Ability);
        }
    }
}