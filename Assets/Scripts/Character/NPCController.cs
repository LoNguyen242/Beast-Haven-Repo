using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState { Idle, Walk, Talk }

public class NPCController : MonoBehaviour, IInteractableObject
{
    [SerializeField] Dialog dialog;
    [SerializeField] GameObject emotion;

    private Character character;
    private ItemGiver itemGiver;
    private BeastGiver beastGiver;
    private QuestGiver questGiver;
    private Shopkeeper shopkeeper;

    [SerializeField] List<Vector2> movePattern;
    [SerializeField] float timeBetweenPattern;
    private float idleTimer = 0f;
    private int currentPattern = 0;

    private NPCState state;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        beastGiver = GetComponent<BeastGiver>();
        questGiver = GetComponent<QuestGiver>();
        shopkeeper = GetComponent<Shopkeeper>();
    }

    private void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= timeBetweenPattern)
            {
                if (movePattern.Count > 0) { StartCoroutine(Walk()); }
                idleTimer = 0;
            }
        }

        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walk;

        var oldPos = transform.position;
        yield return character.Move(movePattern[currentPattern]);
        if (transform.position != oldPos) { currentPattern = (currentPattern + 1) % movePattern.Count; }

        state = NPCState.Idle;
    }

    public IEnumerator OnInteracted(Transform player)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Talk;

            emotion.SetActive(true);
            character.LookToward(player.position);
            yield return new WaitForSeconds(0.5f);
            emotion.SetActive(false);

            if (itemGiver != null && itemGiver.CanBeGiven())
            { yield return itemGiver.GiveItem(player.GetComponent<PlayerController>()); }
            else if (beastGiver != null && beastGiver.CanBeGiven())
            { yield return beastGiver.GiveBeast(player.GetComponent<PlayerController>()); }
            else if (questGiver != null)
            {
                if (questGiver.QuestToStart != null)
                {
                    questGiver.ActiveQuest = new Quest(questGiver.QuestToStart);
                    yield return questGiver.ActiveQuest.StartQuest();
                    DialogManager.Instance.SetSpeaker(false, gameObject.name);
                    yield return DialogManager.Instance.ShowDialog(questGiver.ActiveQuest.QuestBase.StartDialog);
                    questGiver.QuestToStart = null;

                    Debug.Log(questGiver.ActiveQuest.QuestBase.Name + " started!");

                    if (questGiver.ActiveQuest.CanBeCompleted
                        (player.GetComponent<Bag>().GetItemCount(questGiver.ActiveQuest.QuestBase.RequiredItem)))
                    {
                        Debug.Log(questGiver.ActiveQuest.QuestBase.Name + " completed!");

                        yield return questGiver.ActiveQuest.CompleteQuest(player);
                        DialogManager.Instance.SetSpeaker(false, gameObject.name);
                        yield return DialogManager.Instance.ShowDialog(questGiver.ActiveQuest.QuestBase.CompleteDialog);
                        questGiver.ActiveQuest = null;
                    }
                }
                else if (questGiver.QuestToComplete != null)
                {
                    var quest = new Quest(questGiver.QuestToComplete);
                    yield return quest.CompleteQuest(player);
                    DialogManager.Instance.SetSpeaker(false, gameObject.name);
                    yield return DialogManager.Instance.ShowDialog(questGiver.ActiveQuest.QuestBase.CompleteDialog);
                    questGiver.QuestToComplete = null;

                    Debug.Log(quest.QuestBase.Name + " completed!");
                }
                else if (questGiver.ActiveQuest != null)
                {
                    if (questGiver.ActiveQuest.CanBeCompleted
                        (player.GetComponent<Bag>().GetItemCount(questGiver.ActiveQuest.QuestBase.RequiredItem)))
                    {
                        Debug.Log(questGiver.ActiveQuest.QuestBase.Name + " completed!");

                        yield return questGiver.ActiveQuest.CompleteQuest(player);
                        DialogManager.Instance.SetSpeaker(false, gameObject.name);
                        yield return DialogManager.Instance.ShowDialog(questGiver.ActiveQuest.QuestBase.CompleteDialog);
                        questGiver.ActiveQuest = null;
                    }
                    else
                    {
                        DialogManager.Instance.SetSpeaker(false, gameObject.name);
                        yield return DialogManager.Instance.ShowDialog
                            (questGiver.ActiveQuest.QuestBase.InProgressDialog);
                    }
                }
            }
            else if (shopkeeper != null) { yield return shopkeeper.Trade(); }
            else
            {
                DialogManager.Instance.SetSpeaker(false, gameObject.name);
                yield return DialogManager.Instance.ShowDialog(dialog);
            }

            character.SetFacingDirection(character.DefaultDir);
            idleTimer = 0f;

            state = NPCState.Idle;
        }
    }
}
