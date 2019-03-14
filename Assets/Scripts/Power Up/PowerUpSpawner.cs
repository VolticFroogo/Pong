using Mirror;
using UnityEngine;

public class PowerUpSpawner : NetworkBehaviour
{
    // Minimum and maximum spawn delays.
    public float SpawnDelayMin = 5f;
    public float SpawnDelayMax = 15f;

    // Time to spawn the next power up.
    private float NextSpawn = 0f;

    // A boolean indicating whether there is currently a power up in play.
    private bool Exists = false;

    // Singleton.
    public static PowerUpSpawner Instance;

    // Boundaries to spawn the power up between.
    public Vector2 Boundaries;

    // Object to spawn in.
    public GameObject Spawn;

    public void Begin()
    {
        SetNextSpawn();
    }

    void Awake()
    {
        // Initialise variables.
        Instance = this;
    }

    [ServerCallback] // Only run on server.
    void Update()
    {
        // If it's time to spawn a new power up and one doesn't already exist:
        if (!Exists && NextSpawn < Time.time)
        {
            // Let our spawner know the power up is in play.
            Exists = true;

            // Generate a random position from our boundaries.
            var x = Random.Range(-Boundaries.x, Boundaries.x);
            var y = Random.Range(-Boundaries.y, Boundaries.y);

            // Convert the coordinates into a vector.
            var position = new Vector2(x, y);

            // Instantiate the power up on the server.
            var spawn = Instantiate(Spawn, position, transform.rotation);

            // Tell all clients to spawn the power up.
            NetworkServer.Spawn(spawn);
        }
    }

    public void Destroyed()
    {
        // Let our spawner know the power up is out of play.
        Exists = false;

        // Queue up the next power up.
        SetNextSpawn();
    }

    private void SetNextSpawn()
    {
        // Generate a random delay for the power up.
        var delay = Random.Range(SpawnDelayMin, SpawnDelayMax);

        // Set the next spawn to our current time plus the delay.
        NextSpawn = Time.time + delay;
    }
}
