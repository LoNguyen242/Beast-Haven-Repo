using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    private Character character;

    private Vector2 input;

    private ITriggerableObject currTrigger;

    public Character Character { get { return character; } }

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal") * 2;
            input.y = Input.GetAxisRaw("Vertical") * 2;

            if (input.x != 0) { input.y = 0; }

            if (input != Vector2.zero) { StartCoroutine(character.Move(input, CheckTriggerable)); }
        }

        character.Anim.SetBool("isMoving", character.IsMoving);

        if (Input.GetKeyDown(KeyCode.Z)) { StartCoroutine(Interact()); }
    }

    private void CheckTriggerable()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, Mathf.Epsilon,
            GameLayers.Instance.TriggerableLayers);
        ITriggerableObject triggerable = null;
        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<ITriggerableObject>();
            if (triggerable != null)
            {
                if (triggerable == currTrigger && !triggerable.TriggerRepeatedly) { break; }
                triggerable.OnTriggered(this);
                currTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currTrigger) { currTrigger = null; }
    }

    private IEnumerator Interact()
    {
        var faceDir = new Vector3(character.Anim.GetFloat("moveX"), character.Anim.GetFloat("moveY"));
        var interactPos = transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interactPos, 1f, GameLayers.Instance.Interactable);
        if (collider != null)
        {
            character.Anim.SetBool("isMoving", false);
            yield return collider.GetComponent<IInteractableObject>()?.OnInteracted(transform);
        }
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            beasts = GetComponent<BeastParty>().Beasts.Select(b => b.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        var position = saveData.position;
        transform.position = new Vector3(position[0], position[1]);
        GetComponent<BeastParty>().Beasts = saveData.beasts.Select(s => new Beast(s)).ToList();
    }
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<BeastSaveData> beasts; 
}