using System;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : SnakeSegment
{
    [SerializeField] int initialSnakeLength = 5;

    [SerializeField] SnakeSegment segmentPrefab;
    [SerializeField] BoxCollider2D gridArea;
    [SerializeField] FoodController basicFood;

    Transform critter = null;
    Bounds snakeBounds;
    bool isPaused = false;
    bool receivedInputThisFrame = false;
    bool gameIsOver = false;
    bool shouldOpenMouth = false;
    Vector2 nextDirection = Vector2.zero;
    Vector2 orientationThisFrame;

    void OnEnable()
    {
        GameEvents.Instance.OnFoodTriggerEntered += Grow;
        GameEvents.Instance.OnCritterDestroyed += Grow;
        GameEvents.Instance.OnCritterSpawned += SetCritterReference;
        GameEvents.Instance.OnCritterDestroyed += NullifyCritterReference;
        GameEvents.Instance.OnPauseButtonPressed += PauseGame;
        GameEvents.Instance.OnUnpauseButtonPressed += UnpauseGame;
    }

    void Awake()
    {
        direction = Vector2.right;
        Segments.Add(this);
        snakeBounds = gridArea.bounds;
    }

    void Update()
    {
        if (gameIsOver)
            return;

        HandleMovementInput();
        HandleCheatInput();
        HandlePauseInput();
    }

    void FixedUpdate()
    {
        receivedInputThisFrame = false;


        // Grow the snake to the starting length, without adding digestion coordinates
        InitialSnakeGrowth();

        // Move each segment to the position of the segment in front of it
        // starting at the tail.
        for (int i = Segments.Count - 1; i > 0; i--)
        {
            Segments[i].transform.position = Segments[i - 1].transform.position;
        }

        // Move the head
        this.transform.position = new Vector3(
            Mathf.Round(transform.position.x) + direction.x * 2,
            Mathf.Round(transform.position.y) + direction.y * 2,
            0.0f
            );

        if (nextDirection != Vector2.zero)
        {
            direction = nextDirection;
            nextDirection = Vector2.zero;
        }
        orientationThisFrame = direction; // Which way the snake is currently facing. Helps detect invalid inputs.

        HandleHeadGraphics();

        // When the snake reaches the edge of the map, wrap around to the other side
        WrapSnakeOnEdgeCollide();
    }

    void InitialSnakeGrowth()
    {
        if (Segments.Count < initialSnakeLength)
        {
            SnakeSegment newSegment = Instantiate(segmentPrefab);
            newSegment.transform.position = Segments[Segments.Count - 1].transform.position;
            Segments.Add(newSegment);

            newSegment.Segments = Segments;

            // Broadcast the current list of segments
            GameEvents.Instance.SegmentsListUpdated(Segments);
        }
    }

    void HandleHeadGraphics()
    {
        Vector3 _movementNextFrame =  direction * 2; 

        //shouldOpenMouth = (this.transform.position + _movementNextFrame) == basicFood.transform.position;

        shouldOpenMouth = false;
        if ((this.transform.position + _movementNextFrame) == basicFood.transform.position)
        {
            shouldOpenMouth = true;
        }
        if (critter != null && 
            ((this.transform.position + _movementNextFrame) == critter.position ||
            (this.transform.position + _movementNextFrame) == new Vector3(critter.position.x + 2, critter.position.y, 0)))
        {
            shouldOpenMouth = true;
        }

        if (direction == Vector2.right)
        {
            if (shouldOpenMouth)
                SwapToSegmentType(SegmentTypes.HeadRightUpEat);
            else
                SwapToSegmentType(SegmentTypes.HeadRightUp);
        }
        if (direction == Vector2.left)
        {
            if (shouldOpenMouth)
                SwapToSegmentType(SegmentTypes.HeadLeftDownEat);
            else
                SwapToSegmentType(SegmentTypes.HeadLeftDown);
        }
        if (direction == Vector2.up)
        {
            if (shouldOpenMouth)
            {
                Transform seg = SwapToSegmentType(SegmentTypes.HeadRightUpEat);
                seg.Rotate(new Vector3(0, 0, 90));
            }
            else
            {
                Transform seg = SwapToSegmentType(SegmentTypes.HeadRightUp);
                seg.Rotate(new Vector3(0, 0, 90));
            }
        }
        if (direction == Vector2.down)
        {
            if (shouldOpenMouth)
            {
                Transform seg = SwapToSegmentType(SegmentTypes.HeadLeftDownEat);
                seg.Rotate(new Vector3(0, 0, 90));
            }
            else
            {
                Transform seg = SwapToSegmentType(SegmentTypes.HeadLeftDown);
                seg.Rotate(new Vector3(0, 0, 90));
            }
        }
    }

    Transform SwapToSegmentType(GameObject segToInstantiate)
    {
        foreach (Transform child in this.transform)
            Destroy(child.gameObject);

        GameObject segment = Instantiate(segToInstantiate);
        segment.transform.position = this.transform.position;
        segment.transform.SetParent(this.transform);

        return segment.transform;
    }

    void WrapSnakeOnEdgeCollide()
    {
        if (transform.position.x > snakeBounds.max.x)
            transform.position = new Vector3(
                transform.position.x - snakeBounds.size.x - 2, 
                transform.position.y, 0);

        else if (transform.position.x < snakeBounds.min.x)
            transform.position = new Vector3(
                transform.position.x + snakeBounds.size.x + 2, 
                transform.position.y, 0);
        
        else if (transform.position.y > snakeBounds.max.y)
            transform.position = new Vector3(
                transform.position.x, 
                transform.position.y - snakeBounds.size.y - 2, 0);
        
        else if (transform.position.y < snakeBounds.min.y)
            transform.position = new Vector3(
                transform.position.x, 
                transform.position.y + snakeBounds.size.y + 2, 0);
    }

    void HandleMovementInput()
    {
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && orientationThisFrame != Vector2.down)
        {
            if (receivedInputThisFrame)
                nextDirection = Vector2.up;
            else
            {
                direction = Vector2.up;
                receivedInputThisFrame = true;
            }
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && orientationThisFrame != Vector2.right)
        {
            if (receivedInputThisFrame)
                nextDirection = Vector2.left;
            else
            {
                direction = Vector2.left;
                receivedInputThisFrame = true;
            }
        }
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && orientationThisFrame != Vector2.up)
        {
            if (receivedInputThisFrame)
                nextDirection = Vector2.down;
            else
            {
                direction = Vector2.down;
                receivedInputThisFrame = true;
            }
        }
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && orientationThisFrame != Vector2.left)
        {
            if (receivedInputThisFrame)
                nextDirection = Vector2.right;
            else
            {
                direction = Vector2.right;
                receivedInputThisFrame = true;
            }
        }
    }

    void HandleCheatInput()
    {
        if (receivedInputThisFrame)
            return;

        if (Input.GetKey(KeyCode.G))
        {
            Grow();
            receivedInputThisFrame = true;
        }
        if (Input.GetKey(KeyCode.P))
        {
            GameEvents.Instance.ScoreIncreased(7);
            receivedInputThisFrame = true;
        }
    }

    void HandlePauseInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (!isPaused)
        {
            GameEvents.Instance.PauseButtonPressed();
        }
        else if (isPaused)
        {
            GameEvents.Instance.UnpauseButtonPressed();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Debug.Log("Pausing game...");
    }

    void UnpauseGame()
    {
        isPaused = false;
        Debug.Log("Unpausing game...");
    }

    void Grow()
    {
        // Note the food's position so segments can check if they need to be digesting
        DigestPositions.Add(this.transform.position); 

        SnakeSegment newSegment = Instantiate(segmentPrefab);
        newSegment.transform.position = Segments[Segments.Count - 1].transform.position;
        Segments.Add(newSegment);

        newSegment.Segments = Segments;

        // Broadcast the current list of segments
        GameEvents.Instance.SegmentsListUpdated(Segments);
        GameEvents.Instance.DigestPositionsListUpdated(DigestPositions);
    }

    void SetCritterReference(Transform _critter) => critter = _critter;

    void NullifyCritterReference() => critter = null;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            GameEvents.Instance.PlayerDied();
            gameIsOver = true;
        }
    }

    void OnDisable()
    {
        if (GameEvents.Instance) // Prevents null reference error when closing game or changing scenes
        {
            GameEvents.Instance.OnFoodTriggerEntered -= Grow;
            GameEvents.Instance.OnCritterDestroyed -= Grow;
            GameEvents.Instance.OnCritterSpawned -= SetCritterReference;
            GameEvents.Instance.OnCritterDestroyed -= NullifyCritterReference;
            GameEvents.Instance.OnPauseButtonPressed -= PauseGame;
            GameEvents.Instance.OnUnpauseButtonPressed -= UnpauseGame;
        }
    }
}
