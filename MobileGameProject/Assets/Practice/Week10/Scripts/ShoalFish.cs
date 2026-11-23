using UnityEngine;

// One fish: swims around the reef, keeps its distance from the others,
// blows the odd bubble and swims off (is removed) after a few seconds.
public class ShoalFish : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float personalSpace = 0.4f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float bubblesPerSecond = 0.5f;

    public ShoalSpawner Spawner { get; set; }

    private Vector2 velocity;
    private float age;

    private void Start()
    {
        velocity = Random.insideUnitCircle.normalized * speed;
        // Fish at different depths, so they swim in front of and behind each other.
        GetComponent<SpriteRenderer>().sortingOrder = Random.Range(0, 100);
    }

    private void Update()
    {
        AvoidNeighbours();
        Swim();

        if (Random.value < bubblesPerSecond * Time.deltaTime) Spawner.SpawnBubble(transform.position);

        age += Time.deltaTime;
        if (age > lifetime) Destroy(gameObject);
    }

    // Steer away from every fish that is too close.
    private void AvoidNeighbours()
    {
        ShoalFish[] everyFish = FindObjectsByType<ShoalFish>(FindObjectsSortMode.None);
        foreach (ShoalFish other in everyFish)
        {
            if (other == this) continue;
            float distance = Vector2.Distance(transform.position, other.transform.position);
            if (distance < personalSpace)
                velocity += (Vector2)(transform.position - other.transform.position) * (4f * Time.deltaTime);
        }
        velocity = velocity.normalized * speed;
    }

    // Move, and turn back at the edges of the reef.
    private void Swim()
    {
        Vector3 position = transform.position + (Vector3)(velocity * Time.deltaTime);
        if (Mathf.Abs(position.x) > 4.2f) velocity.x = -Mathf.Sign(position.x) * Mathf.Abs(velocity.x);
        if (Mathf.Abs(position.y) > 7.6f) velocity.y = -Mathf.Sign(position.y) * Mathf.Abs(velocity.y);
        transform.position = position;
        GetComponent<SpriteRenderer>().flipX = velocity.x < 0;
    }
}
