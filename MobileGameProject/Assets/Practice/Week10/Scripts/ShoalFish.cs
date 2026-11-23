using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class ShoalFish : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float personalSpace = 0.4f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float bubblesPerSecond = 0.5f;
    [SerializeField] private float checkEvery = 0.25f;   
    
    public static readonly List<ShoalFish> All = new List<ShoalFish>();

    public ShoalSpawner Spawner { get; set; }

    private static readonly ProfilerMarker AvoidMarker = new ProfilerMarker("ShoalFish.AvoidNeighbours");
    private static readonly ProfilerMarker SwimMarker = new ProfilerMarker("ShoalFish.Swim");

    private SpriteRenderer spriteRenderer;
    private Vector2 velocity;
    private float age;
    private float nextCheck;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        All.Add(this);
        // A pooled fish is reused, so reset what a new fish would start with.
        age = 0f;
        velocity = Random.insideUnitCircle.normalized * speed;
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    private void Start()
    {
        velocity = Random.insideUnitCircle.normalized * speed;
        spriteRenderer.sortingOrder = Random.Range(0, 100);
        nextCheck = Time.time + Random.value * checkEvery;
    }

    private void Update()
    {
        if (Time.time >= nextCheck)
        {
            nextCheck = Time.time + checkEvery;
            AvoidMarker.Begin();
            AvoidNeighbours();
            AvoidMarker.End();
        }

        SwimMarker.Begin();
        Swim();
        SwimMarker.End();

        if (Random.value < bubblesPerSecond * Time.deltaTime) Spawner.SpawnBubble(transform.position);

        age += Time.deltaTime;        
        if (age > lifetime) Spawner.ReleaseFish(this);
    }

    private void AvoidNeighbours()
    {
        Vector2 here = transform.position;
        float spaceSquared = personalSpace * personalSpace;
        foreach (ShoalFish other in All)
        {
            if (other == this) continue;
            Vector2 away = here - (Vector2)other.transform.position;
            if (away.sqrMagnitude < spaceSquared)
                velocity += away * (4f * checkEvery);
        }
        velocity = velocity.normalized * speed;
    }

    private void Swim()
    {
        Vector3 position = transform.position + (Vector3)(velocity * Time.deltaTime);
        if (Mathf.Abs(position.x) > 4.2f) velocity.x = -Mathf.Sign(position.x) * Mathf.Abs(velocity.x);
        if (Mathf.Abs(position.y) > 7.6f) velocity.y = -Mathf.Sign(position.y) * Mathf.Abs(velocity.y);
        transform.position = position;
        spriteRenderer.flipX = velocity.x < 0;
    }
}