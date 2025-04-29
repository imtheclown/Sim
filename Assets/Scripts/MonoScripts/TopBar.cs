using UnityEngine;
using Unity.Entities;
using TMPro;
public class TopBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI fishCountText;
    private EntityManager entityManager;
    private Entity gameTime;
    private Entity boidManager;
    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        gameTime = ECSLookup.GetSingletonEntitySafe<GameTime>(entityManager);
        boidManager = ECSLookup.GetSingletonEntitySafe<BoidManager>(entityManager);
    }

    // Update is called once per frame
    void Update()
    {
        if (entityManager.Exists(gameTime) && entityManager.HasComponent<GameConfig>(gameTime))
        {
            var config = entityManager.GetComponentData<GameTime>(gameTime);
            dayText.text = $"Day {config.days}";
        }
        else
        {
            Debug.LogWarning("Game Time entity not found or missing component.");
        }
        if (entityManager.Exists(boidManager) && entityManager.HasComponent<BoidManager>(boidManager))
        {
            var config = entityManager.GetComponentData<BoidManager>(boidManager);
            fishCountText.text = $"{CMathUtils.FormatNumber(config.activeBoidCount)}/{CMathUtils.FormatNumber(config.boidSpawnedCount)}";
        }
        else
        {
            Debug.LogWarning("Boid Manager entity not found or missing component.");
        }
    }
}
