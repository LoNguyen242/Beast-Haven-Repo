using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectState { None, Enable, Disable }

public class QuestObject : MonoBehaviour
{
    [SerializeField] QuestBase quesToCheck;
    [SerializeField] ObjectState onStart;
    [SerializeField] ObjectState onComplete;

    private QuestList questList;

    private void Start()
    {
        questList = QuestList.GetQuestList();
        questList.OnUpdated += UpdateObjectState;

        UpdateObjectState();
    }

    private void OnDestroy()
    {
        questList.OnUpdated -= UpdateObjectState;
    }

    public void UpdateObjectState()
    {
        if (onStart != ObjectState.None && questList.IsStarted(quesToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onStart == ObjectState.Enable)
                {
                    child.gameObject.SetActive(true);

                    var savable = child.GetComponent<SavableEntity>();
                    if (savable != null) { SavingSystem.Instance.RestoreEntity(savable); }
                }
                else if (onStart == ObjectState.Disable) { child.gameObject.SetActive(false); }
            }
        }

        if (onComplete != ObjectState.None && questList.IsCompleted(quesToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onComplete == ObjectState.Enable)
                {
                    child.gameObject.SetActive(true);
                    var savable = child.GetComponent<SavableEntity>();
                    if (savable != null) { SavingSystem.Instance.RestoreEntity(savable); }
                }
                else if (onComplete == ObjectState.Disable) { child.gameObject.SetActive(false); }
            }
        }
    }
}
