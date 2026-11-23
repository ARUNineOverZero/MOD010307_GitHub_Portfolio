using UnityEngine;

// A bubble that wobbles up towards the surface and pops after a few seconds.
public class ShoalBubble : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 1.5f;
    [SerializeField] private float lifetime = 3f;
    public ShoalSpawner Spawner { get; set; }

    private float age;

    private void Start()
    {
        // Bubbles at different depths, in among the fish.
        GetComponent<SpriteRenderer>().sortingOrder = Random.Range(0, 100);
    }

    private void OnEnable()
    {
        age = 0f;   // a pooled bubble is reused, so start its life again
    }

    private void Update()
    {
        age += Time.deltaTime;
        float wobble = Mathf.Sin(age * 6f) * 0.3f;
        transform.position += new Vector3(wobble, riseSpeed, 0f) * Time.deltaTime;

        if (age > lifetime) Spawner.ReleaseBubble(this);
    }
}
