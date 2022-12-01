using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Explore, Battle, Dialog, Menu, Party, Bag, PowerShift, Shop, Cutscene, Pause, }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] Camera exploreCamera;

    [SerializeField] BattleSystem battleSystem;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] BagUI bagUI;

    private ChallengerController challenger;

    private MenuController menuController;

    private GameState state;
    private GameState prevState;
    private GameState stateBeforeShift;
    public GameState State { get { return state; } }

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        menuController = GetComponent<MenuController>();

        BeastDB.Init();
        SkillDB.Init();
        ItemDB.Init();
        QuestDB.Init();
        StatusEffectDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleWin += EndBattle;

        partyScreen.SetParty();

        DialogManager.Instance.OnOpenDialog += () => 
        {
            prevState = state;
            state = GameState.Dialog; 
        };
        DialogManager.Instance.OnDialogFinished += () =>
        { 
            if (state == GameState.Dialog) { state = prevState; } 
        };

        menuController.OnSelected += HandleMenu;
        menuController.OnBack += () => { state = GameState.Explore; };

        PowerShiftManager.Instance.OnStartShift += () => 
        {
            stateBeforeShift = state;
            state = GameState.PowerShift; 
        };
        PowerShiftManager.Instance.OnCompleteShift += () => 
        { 
            state = stateBeforeShift;

            partyScreen.UpdatePartyScreen();

            AudioManager.Instance.PlayMusic(CurrentScene.SceneMusic, fade: true);
        };

        ShopController.Instance.OnStartShop += () => { state = GameState.Shop; };
        ShopController.Instance.OnFinishShop += () => { state = GameState.Explore; };
    }

    private void Update()
    {
        if (state == GameState.Explore)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                state = GameState.Menu;

                menuController.OpenMenu();
            }
        }
        else if (state == GameState.Battle) { battleSystem.HandleUpdate(); }
        else if (state == GameState.Menu) { menuController.HandleUpdate(); }
        else if (state == GameState.Party)
        {
            Action onSelected = () => { };

            Action onBack = () =>
            {
                state = GameState.Menu;

                partyScreen.gameObject.SetActive(false);
                menuController.OpenMenu();

            };

            partyScreen.HandlePartyScreen(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                state = GameState.Menu;

                bagUI.gameObject.SetActive(false);
                menuController.OpenMenu();
            };

            bagUI.HandleUpdate(onBack);
        }
        else if (state == GameState.Shop) { ShopController.Instance.HandleUpdate(); }
    }

    private void HandleMenu(int selectedItem)
    {
        if (selectedItem == 0)
        {
            state = GameState.Party;

            partyScreen.gameObject.SetActive(true);
        }
        else if (selectedItem == 1)
        {
            state = GameState.Bag;

            bagUI.gameObject.SetActive(true);
        }
        else if (selectedItem == 2)
        {
            state = GameState.Explore;

            SavingSystem.Instance.Save("saveSlot1");
        }
        else if (selectedItem == 3)
        {
            state = GameState.Explore;

            SavingSystem.Instance.Load("saveSlot1");
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;

        battleSystem.gameObject.SetActive(true);
        exploreCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<BeastParty>();
        var wildBeast = CurrentScene.GetComponent<MapArea>().GetRandomWildBeast();
        var wildBeastCopy = new Beast(wildBeast.BeastBase, wildBeast.Level);
        battleSystem.StartBattle(playerParty, wildBeastCopy);
    }

    public void StartChallenge(ChallengerController challenger)
    {
        state = GameState.Battle;
        this.challenger = challenger;

        battleSystem.gameObject.SetActive(true);
        exploreCamera.gameObject.SetActive(false);

        partyScreen.UpdatePartyScreen();

        var playerParty = playerController.GetComponent<BeastParty>();
        var challengerParty = challenger.GetComponent<BeastParty>();
        battleSystem.StartChallenge(playerParty, challengerParty);
    }

    private void EndBattle(bool win)
    {
        if (challenger != null)
        {
            challenger.DisableChallenge();
            challenger = null;
        }

        state = GameState.Explore;

        battleSystem.gameObject.SetActive(false);
        exploreCamera.gameObject.SetActive(true);

        AudioManager.Instance.PlayMusic(CurrentScene.SceneMusic, fade: true);
    }

    public void StartCutscene(ChallengerController challenger)
    {
        state = GameState.Cutscene;
        StartCoroutine(challenger.TriggerChallenge(playerController));
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Pause;
        }
        else { state = prevState; }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }
}
