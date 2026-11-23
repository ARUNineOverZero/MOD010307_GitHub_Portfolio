using UnityEngine;
using UnityEngine.Pool;

// Keeps the reef full: whenever there are fewer than fishCount fish, it adds more.
// Fish ask it for bubbles too.
public class ShoalSpawner : MonoBehaviour
{
    [SerializeField] private ShoalFish fishPrefab;
    [SerializeField] private ShoalBubble bubblePrefab;
    [SerializeField] private Sprite[] fishSprites;   // one per colour
    [SerializeField] private int fishCount = 400;
    [SerializeField] private int spawnPerFrame = 20;
    private ObjectPool<ShoalFish> fishPool;
    private ObjectPool<ShoalBubble> bubblePool;

    private void Awake()
    {
        fishPool = new ObjectPool<ShoalFish>(
            () => Instantiate(fishPrefab, transform), 
            fish => fish.gameObject.SetActive(true),  
            fish => fish.gameObject.SetActive(false),
            fish => Destroy(fish.gameObject),           
            true, fishCount, fishCount * 2);
        bubblePool = new ObjectPool<ShoalBubble>(
            () => Instantiate(bubblePrefab, transform),
            bubble => bubble.gameObject.SetActive(true),
            bubble => bubble.gameObject.SetActive(false),
            bubble => Destroy(bubble.gameObject),
            true, 200, 1000);
    }

    private void Update()
    {
        int alive = ShoalFish.All.Count;
        for (int i = 0; i < spawnPerFrame && alive < fishCount; i++, alive++)
            SpawnFish();
    }

    private void SpawnFish()
    {
        ShoalFish fish = fishPool.Get();
        fish.transform.position = new Vector2(Random.Range(-4f, 4f), Random.Range(-7.5f, 7.5f));
        fish.GetComponent<SpriteRenderer>().sprite = fishSprites[Random.Range(0, fishSprites.Length)];
        fish.Spawner = this;
    }

    public void ReleaseFish(ShoalFish fish)
    {
        fishPool.Release(fish);
    }

    public void SpawnBubble(Vector3 position)
    {
        ShoalBubble bubble = bubblePool.Get();
        bubble.transform.position = position;
        bubble.Spawner = this;
    }

    public void ReleaseBubble(ShoalBubble bubble)
    {
        bubblePool.Release(bubble);
    }
}
