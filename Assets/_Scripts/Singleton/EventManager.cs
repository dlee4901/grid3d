using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Singleton;// { get; private set; }

    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public event EventHandler<OnStartGameEventArgs> OnStartGame;
    public class OnStartGameEventArgs : EventArgs {}
    public void StartGame()
    {
        Debug.Log("EventManager StartGame");
        OnStartGame?.Invoke(this, new OnStartGameEventArgs {});
    }

    // public event EventHandler<OnCreateGridEventArgs> OnCreateGrid;
    // public class OnCreateGridEventArgs : EventArgs
    // {
    //     public GridNetwork Grid;
    // }
    // public void CreateGrid(GridNetwork grid)
    // {
    //     Debug.Log("EventManager CreateGrid");
    //     OnCreateGrid?.Invoke(this, new OnCreateGridEventArgs {Grid = grid});
    // }
    //
    // public event EventHandler<OnSelectUnitEventArgs> OnSelectUnit;
    // public class OnSelectUnitEventArgs : EventArgs
    // {
    //     public UnitManager Unit;
    // }
    // public void SelectUnit(UnitManager unit)
    // {
    //     Debug.Log("EventManager SelectUnit");
    //     OnSelectUnit?.Invoke(this, new OnSelectUnitEventArgs {Unit = unit});
    // }

    // public event EventHandler<OnAlignCameraEventArgs> OnAlignCamera;
    // public class OnAlignCameraEventArgs : EventArgs
    // {
    //     public Vector3 StartPosition;
    //     public BoxCollider2D Confiner;
    // }

    // public event EventHandler<OnEntityPlaceEventArgs> OnUnitPlace;
    // public class OnEntityPlaceEventArgs : EventArgs {}

} 
 