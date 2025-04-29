using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotBar : MonoBehaviour
{
    [SerializeField] private List<GameObject> bottomPanels;
    [SerializeField] private Button expandBottomButton;
    [SerializeField] private Button closeBottomButton;
    void Start()
    {
        expandBottomButton.onClick.AddListener(handleExpand);
        closeBottomButton.onClick.AddListener(handleClose);
    }
    public void ShowOnlyIndex(int index)
    {
        if (index < 0 || index >= bottomPanels.Count)
        {
            Debug.LogWarning("Index out of range!");
            return;
        }

        for (int i = 0; i < bottomPanels.Count; i++)
        {
            var showHideComponent = bottomPanels[i].GetComponent<SmoothShowHide>();
            if(showHideComponent == null){
                if(i == index){
                    ManualSetActive(i);
                    continue;
                }
                ManualSetInactive(i);
                continue;
            }
            if(i == index){
                showHideComponent.Show();
            }else{
                showHideComponent.Hide();
            }
        }
    }

    public void ManualSetInactive(int index){
        bottomPanels[index].SetActive(false);
    }

    public void ManualSetActive(int index){
        bottomPanels[index].SetActive(true);
    }

    public void handleExpand(){
        ShowOnlyIndex(1);
    }
    public void handleClose(){
        ShowOnlyIndex(0);
    }
}
