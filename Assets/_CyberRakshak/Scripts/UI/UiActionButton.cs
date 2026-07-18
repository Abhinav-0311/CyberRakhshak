using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CyberRakshak.Runtime;

public enum UiAction
{
    StartTraining,
    ContinueTraining,
    OpenMainSettings,
    CloseMainSettings,
    Exit,
    LoadTutorial,
    LoadLevelOne,
    ReturnMainMenu,
    Resume,
    OpenPauseSettings,
    ClosePauseSettings
}

[RequireComponent(typeof(Button))]
public sealed class UiActionButton : MonoBehaviour, IPointerClickHandler
{
    public UiAction action;

    public void OnPointerClick(PointerEventData eventData)
    {
        ExecuteAction();
    }

    public void ExecuteAction()
    {
        var navigator = FindFirstObjectByType<SceneNavigator>();
        var mainMenu = FindFirstObjectByType<MainMenuController>();
        var pause = FindFirstObjectByType<GameplayPauseController>();

        switch (action)
        {
            case UiAction.StartTraining:
                navigator?.StartTraining();
                break;
            case UiAction.ContinueTraining:
                navigator?.ContinueTraining();
                break;
            case UiAction.OpenMainSettings:
                mainMenu?.OpenSettings();
                break;
            case UiAction.CloseMainSettings:
                mainMenu?.CloseSettings();
                break;
            case UiAction.Exit:
                navigator?.QuitGame();
                break;
            case UiAction.LoadTutorial:
                navigator?.LoadTutorial();
                break;
            case UiAction.LoadLevelOne:
                navigator?.LoadLevelOne();
                break;
            case UiAction.ReturnMainMenu:
                if (pause != null) pause.ReturnToMainMenu();
                else navigator?.ReturnToMainMenu();
                break;
            case UiAction.Resume:
                pause?.Resume();
                break;
            case UiAction.OpenPauseSettings:
                pause?.OpenSettings();
                break;
            case UiAction.ClosePauseSettings:
                pause?.CloseSettings();
                break;
        }
    }
}