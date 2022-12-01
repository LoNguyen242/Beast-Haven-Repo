using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestList : MonoBehaviour, ISavable
{
    private List<Quest> quests = new List<Quest>();

    public event Action OnUpdated;

    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerController>().GetComponent<QuestList>();
    }

    public void AddQuest(Quest quest)
    {
        if (!quests.Contains(quest)) { quests.Add(quest); }

        OnUpdated?.Invoke();
    }

    public bool IsStarted(string questName)
    {
        var questState = quests.FirstOrDefault(q => q.QuestBase.Name == questName)?.State;
        return questState == QuestState.Started || questState == QuestState.Completed;
    }

    public bool IsCompleted(string questName)
    {
        var questState = quests.FirstOrDefault(q => q.QuestBase.Name == questName)?.State;
        return questState == QuestState.Completed;
    }

    public object CaptureState()
    {
        return quests.Select(q => q.GetSaveData()).ToList();
    }

    public void RestoreState(object state)
    {
        var saveData = state as List<QuestSaveData>;
        if (saveData != null) 
        { 
            quests = saveData.Select(q => new Quest(q)).ToList();
            OnUpdated?.Invoke();
        }
    }
}
