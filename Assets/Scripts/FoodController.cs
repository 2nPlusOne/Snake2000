using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour
{
    [SerializeField] BoxCollider2D gridArea;
    [SerializeField] SnakeHead snake;
    [SerializeField] CritterTypesSO critterTypes;

    [SerializeField] Transform critter;
    [SerializeField] int maxCritterLifespan = 30; // In fixed timesteps

    Bounds bounds;
    public int critterLifeRemaining { get; private set; }

    void OnEnable()
    {
        // Subscribe to events here
    }

    void Start()
    {
        RandomizeFoodPosition();
    }

    void FixedUpdate()
    {
        if (critterLifeRemaining <= 0 && critter != null)
        {
            GameEvents.Instance.CritterDestroyed();
            Destroy(critter.gameObject);
            critter = null;
        }
        else critterLifeRemaining = Mathf.Clamp(critterLifeRemaining - 1, 0, maxCritterLifespan);
    }

    // TO DO: add a maximum number of tries before the while loop gives up (500 should be enough)
    void RandomizeFoodPosition()
    {
        bounds = gridArea.bounds;

        // Populate a new list containing the position of each snake segment
        List<Vector3> segmentPositions = (from seg in snake.Segments select seg.transform.position).ToList<Vector3>();

        Vector3 newFoodPosition = segmentPositions[0]; // Initialize to the position of the snake head

        while (segmentPositions.Contains(newFoodPosition))
        { // If newFoodPosition would overlap a snake segment, generate a new position.
            float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
            float y = UnityEngine.Random.Range(bounds.min.y, bounds.max.y);
            newFoodPosition = new Vector3(RoundToNearestEvenInteger(x), RoundToNearestEvenInteger(y), 0);
        }
        transform.position = newFoodPosition; // Set the food's position
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            RandomizeFoodPosition();
            GameEvents.Instance.FoodTriggerEntered();
            GameEvents.Instance.ScoreIncreased(7); // Regular food adds 7 points multiplied by combo level
            if (RollForCritterSpawn())
                HandleCritterSpawning();
        }
    }

    bool RollForCritterSpawn()
    {
        if (UnityEngine.Random.value <= .07f) // 7% chance to spawn a critter (high point food)
        {
            return true;
        }
        else return false;
    }

    void HandleCritterSpawning()
    {
        critter = Instantiate(critterTypes.Critters[UnityEngine.Random.Range(0, critterTypes.Critters.Count)]);
        Debug.Log("Spawning " + critter.name);

        bool foundValidCritterSpawnPosition = LookForValidCritterSpawnPosition(critter);
        if (foundValidCritterSpawnPosition)
        {
            critterLifeRemaining = maxCritterLifespan;
            GameEvents.Instance.CritterSpawned(critter);
        }
    }

    bool LookForValidCritterSpawnPosition(Transform _critter)
    {
        bounds = gridArea.bounds;

        // Populate a new list containing the position of each snake segment
        List<Vector3> segmentPositions = (from seg in snake.Segments select seg.transform.position).ToList<Vector3>();

        Vector3 newCritterLeftPos = segmentPositions[0]; // Initialize to the position of the snake head
        Vector3 newCritterRightPos = new Vector3(newCritterLeftPos.x + 2, newCritterLeftPos.y, 0);

        int attempts = 0;
        // Keep trying to spawn until a valid spawn is found or the maximum number of attempts have been made.
        while (IsInvalidCritterPosition(newCritterLeftPos, newCritterRightPos, segmentPositions) && attempts < 500)
        {
            attempts += 1;
            float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x - 2);
            float y = UnityEngine.Random.Range(bounds.min.y, bounds.max.y);
            newCritterLeftPos = new Vector3(RoundToNearestEvenInteger(x), RoundToNearestEvenInteger(y), 0);
            newCritterRightPos = new Vector3(newCritterLeftPos.x + 2, newCritterLeftPos.y, 0);
        }

        // If the maximum number of attempts have been made, abort spawning the critter
        if (attempts >= 500)
        {
            Destroy(_critter.gameObject);
            return false;
        }
        else
        {
            _critter.position = newCritterLeftPos; // Otherwise, set the critter's position and return true
            return true;
        }
    }

    bool IsInvalidCritterPosition(Vector3 leftPos, Vector3 rightPos, List<Vector3> segPositions)
    {
        if (segPositions.Contains(leftPos) || segPositions.Contains(rightPos) ||
            leftPos == this.transform.position || rightPos == this.transform.position)
        {
            return true;
        }

        else return false;
    }

    int RoundToNearestEvenInteger(float num) => (int)Math.Round(num / 2, MidpointRounding.AwayFromZero) * 2;

    void OnDisable()
    {
        // Unsubscribe from events here
    }
}
