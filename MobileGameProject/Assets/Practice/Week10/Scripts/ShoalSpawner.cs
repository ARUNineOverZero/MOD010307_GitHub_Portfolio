using UnityEngine;

// Keeps the reef full: whenever there are fewer than fishCount fish, it adds more.
// Fish ask it for bubbles too.
public class ShoalSpawner : MonoBehaviour
{
    [SerializeField] private ShoalFish fishPrefab;
    [SerializeField] private ShoalBubble bubblePrefab;
    [SerializeField] private Sprite[] fishSprites;   // one per colour
    [SerializeField] private int fishCount = 400;
    [SerializeField] private int spawnPerFrame = 20;

    private void Update()
    {
        int alive = FindObjectsByType<ShoalFish>(FindObjectsSortMode.None).Length;
        for (int i = 0; i < spawnPerFrame && alive < fishCount; i++, alive++)
            SpawnFish();
    }

    private void SpawnFish()
    {
        Vector2 position = new Vector2(Random.Range(-4f, 4f), Random.Range(-7.5f, 7.5f));
        ShoalFish fish = Instantiate(fishPrefab, position, Quaternion.identity);
        fish.GetComponent<SpriteRenderer>().sprite = fishSprites[Random.Range(0, fishSprites.Length)];
        fish.Spawner = this;
    }

    public void SpawnBubble(Vector3 position)
    {
        Instantiate(bubblePrefab, position, Quaternion.identity);
    }
}
