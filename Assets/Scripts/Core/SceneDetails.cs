using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;

    public bool IsLoaded { get; private set; }

    public AudioClip SceneMusic { get; private set; }

    private List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("Enter " + gameObject.name);

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            if (sceneMusic != null) { AudioManager.Instance.PlayMusic(sceneMusic, fade: true); }

            foreach (var scene in connectedScenes)
            { scene.LoadScene(); }

            var prevScene = GameController.Instance.PrevScene;
            if (prevScene != null)
            {
                var prevLoadedScenes = GameController.Instance.PrevScene.connectedScenes;
                foreach (var scene in prevLoadedScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                    { scene.UnoadScene(); }
                }

                if (!connectedScenes.Contains(prevScene)) { prevScene.UnoadScene(); }
            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.Instance.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnoadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.Instance.CaptureEntityStates(savableEntities);
            SceneManager.UnloadSceneAsync(gameObject.name);

            IsLoaded = false;
        }
    }

    private List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>()
            .Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }
}
