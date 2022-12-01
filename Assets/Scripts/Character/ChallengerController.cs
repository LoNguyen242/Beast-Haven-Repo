using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengerController : MonoBehaviour, IInteractableObject, ISavable
{
    [SerializeField] GameObject fov;
    [SerializeField] GameObject emotion;

    [SerializeField] Dialog dialogBefore;
    [SerializeField] Dialog dialogAfter;

    [SerializeField] AudioClip challengerAppearMusic;

    private Character character;

    private bool challengeDisabled = false;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        RotateFOV(character.DefaultDir);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public void RotateFOV(FacingDirection dir)
    {
        float angle = 0;
        if (dir == FacingDirection.Down) { angle = 0f; }
        else if (dir == FacingDirection.Up) { angle = 180f; }
        else if (dir == FacingDirection.Left) { angle = 90f; }
        else if (dir == FacingDirection.Right) { angle = -90f; }

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public IEnumerator OnInteracted(Transform player)
    {
        character.LookToward(player.position);

        if (!challengeDisabled)
        {
            AudioManager.Instance.PlayMusic(challengerAppearMusic);

            emotion.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            emotion.SetActive(false);

            DialogManager.Instance.SetSpeaker(false, gameObject.name);
            yield return DialogManager.Instance.ShowDialog(dialogBefore);
            GameController.Instance.StartChallenge(this);
        }
        else
        {
            DialogManager.Instance.SetSpeaker(false, gameObject.name);
            yield return DialogManager.Instance.ShowDialog(dialogAfter);
        }
    }

    public IEnumerator TriggerChallenge(PlayerController player)
    {
        AudioManager.Instance.PlayMusic(challengerAppearMusic);

        emotion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        emotion.SetActive(false);

        var diff = player.transform.position - transform.position;
        var realDiff = diff - (2 * diff.normalized);
        var moveVec = new Vector2(Mathf.Round(realDiff.x), Mathf.Round(realDiff.y));
        yield return character.Move(moveVec);

        DialogManager.Instance.SetSpeaker(false, gameObject.name);
        yield return DialogManager.Instance.ShowDialog(dialogBefore);

        GameController.Instance.StartChallenge(this);
    }

    public void DisableChallenge()
    {
        challengeDisabled = true;
        fov.gameObject.SetActive(false);
    }

    public object CaptureState()
    {
        return challengeDisabled;
    }

    public void RestoreState(object state)
    {
        challengeDisabled = (bool)state;

        if (challengeDisabled) { fov.gameObject.SetActive(false); }
    }
}
