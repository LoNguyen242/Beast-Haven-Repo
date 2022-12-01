using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Create new Quest")]
public class QuestBase : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;

    [Header("Dialogues")]
    [SerializeField] Dialog startDialog;
    [SerializeField] Dialog inProgressDialog;
    [SerializeField] Dialog completeDialog;

    [Header("Requirements")]
    [SerializeField] ItemBase requiredItem;
    [SerializeField] int requiredCount;

    [Header("Rewards")]
    [SerializeField] ItemBase rewardItem;
    [SerializeField] int rewardCount;

    public string Name { get { return name; } }
    public string Description { get { return description; } }

    public Dialog StartDialog { get { return startDialog; } }
    public Dialog InProgressDialog 
    { get { return inProgressDialog?.Lines?.Count > 0 ? inProgressDialog : startDialog; } }
    public Dialog CompleteDialog { get { return completeDialog; } }

    public ItemBase RequiredItem { get { return requiredItem; } }
    public int RequiredCount { get { return requiredCount; } }

    public ItemBase RewardItem { get { return rewardItem; } }
    public int RewardCount { get { return rewardCount; } }
}
