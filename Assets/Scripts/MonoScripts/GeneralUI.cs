using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

public class GeneralUI : MonoBehaviour
{
    [SerializeField] private Button expandBottomButton;
    [SerializeField] private Button unPauseButton;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject topBar;
    [SerializeField] private GameObject botBar;

    //ui states

    private EntityManager entityManager;
    private Entity gameConfigEntity;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        gameConfigEntity = ECSLookup.GetSingletonEntitySafe<GameConfig>(entityManager);

        // ✅ Register UI event
        unPauseButton.onClick.AddListener(HandleUnPause);
    }

    void HandleUnPause()
    {
        if (entityManager.Exists(gameConfigEntity) && entityManager.HasComponent<GameConfig>(gameConfigEntity))
        {
            var config = entityManager.GetComponentData<GameConfig>(gameConfigEntity);

            // 🔁 Modify your ECS config
            config.isPaused = false;
            titleScreen.SetActive(false);
            entityManager.SetComponentData(gameConfigEntity, config);
            topBar.SetActive(true);
            botBar.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameConfig entity not found or missing component.");
        }
    }
}
