using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public enum DestinationIdentifier { A, B, C, D, E }

public class Portal : MonoBehaviour, ITriggerableObject
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] int sceneToLoad = -1;

    private PlayerController player;

    public Transform SpawnPoint { get { return spawnPoint; } }

    public bool TriggerRepeatedly { get { return false; } }

    public void OnTriggered(PlayerController player)
    {
        this.player = player;

        player.Character.Anim.SetBool("isMoving", false);
        StartCoroutine(SwitchScene());
    }

    private IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);

        yield return Fader.Instance.FadeIn(0.5f);
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this 
        && x.destinationPortal == this.destinationPortal);
        player.transform.position = destPortal.SpawnPoint.position;

        GameController.Instance.PauseGame(false);

        yield return Fader.Instance.FadeOut(0.5f);

        Destroy(gameObject);
    }
}
