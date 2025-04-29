using Unity.Entities;
using UnityEngine;

public class UIManagerAuthoring: MonoBehaviour{
    class Baker:Baker<UIManagerAuthoring> {
        public override void Bake(UIManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new UIStates{
                showPauseScreen= true
            });
        }
    }
}

public struct UIStates: IComponentData{
    public bool showPauseScreen;
}