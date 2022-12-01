using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour, ISavable
{
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;
    private Quest activeQuest;

    public QuestBase QuestToStart 
    { 
        get { return questToStart; }
        set { questToStart = value; }
    }
    public QuestBase QuestToComplete 
    { 
        get { return questToComplete; }
        set { questToComplete = value; }
    }
    public Quest ActiveQuest 
    { 
        get { return activeQuest; }
        set { activeQuest = value; }
    }

    public object CaptureState()
    {
        var saveData = new QuestGiverSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();
        if (questToStart != null) { saveData.questToStart = new Quest(questToStart).GetSaveData(); }
        if (questToComplete != null) { saveData.questToComplete = new Quest(questToComplete).GetSaveData(); }

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as QuestGiverSaveData;
        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).QuestBase : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).QuestBase : null;
        }
    }
}

[Serializable]
public class QuestGiverSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}
