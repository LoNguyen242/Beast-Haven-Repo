using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState { None, Started, Completed }

[Serializable]
public class Quest
{
    public QuestBase QuestBase { get; private set; }
    public QuestState State { get; private set; }

    public Quest(QuestBase _questBase)
    {
        QuestBase = _questBase;
    }

    public Quest(QuestSaveData saveData)
    {
        QuestBase = QuestDB.GetObjectByName(saveData.name);
        State = saveData.state;
    }

    public QuestSaveData GetSaveData()
    {
        var saveData = new QuestSaveData()
        {
            name = QuestBase.name,
            state = State
        };

        return saveData;
    }

    public IEnumerator StartQuest()
    {
        State = QuestState.Started;

        DialogManager.Instance.SetSpeaker(true);
        yield return DialogManager.Instance.ShowDialogString("Quest started!");

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public IEnumerator CompleteQuest(Transform player)
    {
        State = QuestState.Completed;

        var bag = Bag.GetBag();
        if (QuestBase.RequiredItem != null)
        {
            bag.RemoveItem(QuestBase.RequiredItem, QuestBase.RequiredCount);

            DialogManager.Instance.SetSpeaker(true);
            yield return DialogManager.Instance.ShowDialogString
                ("The Protagonist gave away " + QuestBase.RequiredCount + " " + QuestBase.RequiredItem.Name + "!");
        }

        DialogManager.Instance.SetSpeaker(true);
        yield return DialogManager.Instance.ShowDialogString("Quest completed!");

        if (QuestBase.RewardItem != null)
        {
            bag.AddItem(QuestBase.RewardItem, QuestBase.RewardCount);

            AudioManager.Instance.PlaySFX(AudioID.Obtain, true);
            DialogManager.Instance.SetSpeaker(true);
            yield return DialogManager.Instance.ShowDialogString
                ("The Protagonist received " + QuestBase.RewardCount + " " + QuestBase.RewardItem.Name + "!");
        }

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public bool CanBeCompleted(int count)
    {
        var bag = Bag.GetBag();
        if (QuestBase.RequiredItem != null)
        {
            if (!bag.HasItem(QuestBase.RequiredItem) || count < QuestBase.RequiredCount) 
            { return false; }
        }

        return true;
    }
}

[Serializable]
public class QuestSaveData
{
    public string name;
    public QuestState state;
}
