using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour, ITriggerableObject
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationPortal;

    private PlayerController player;

    public Transform SpawnPoint { get { return spawnPoint; } }

    public bool TriggerRepeatedly { get { return false; } }

    public void OnTriggered(PlayerController player)
    {
        this.player = player;

        player.Character.Anim.SetBool("isMoving", false);
        StartCoroutine(Teleport());
    }

    private IEnumerator Teleport()
    {
        GameController.Instance.PauseGame(true);

        yield return Fader.Instance.FadeIn(0.5f);

        var destPortal = FindObjectsOfType<Door>().First(x => x != this
        && x.destinationPortal == this.destinationPortal);
        player.transform.position = destPortal.SpawnPoint.position;

        GameController.Instance.PauseGame(false);

        yield return Fader.Instance.FadeOut(0.5f);
    }
}
