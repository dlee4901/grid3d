using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitFilterView : LoggableBehaviour
{
    [SerializeField] private InteractableUI _button;
    [SerializeField] private GameObject _popup;
    [SerializeField] private TMP_InputField _searchField;
    [SerializeField] private List<InteractableUI> _costToggles;
    [SerializeField] private Color32 _onColor = new(255, 192, 64, 255);
    [SerializeField] private Color32 _offColor = new(0, 0, 0, 64);
    [SerializeField] private Color32 _onTextColor = new(32, 32, 48, 255);
    [SerializeField] private Color32 _offTextColor = new(255, 255, 255, 255);

    private bool[] _costs;

    public event Action Changed;

    private void Start()
    {
        _costs = new bool[_costToggles.Count];
        _button.OnClickCompleted += TogglePopup;
        _searchField.onValueChanged.AddListener(_ => Changed?.Invoke());
        for (var i = 0; i < _costToggles.Count; i++)
        {
            var index = i;
            _costToggles[i].OnClickCompleted += () => ToggleCost(index);
            RefreshCost(i);
        }
    }

    private void TogglePopup()
    {
        var open = !_popup.activeSelf;
        _popup.SetActive(open);
        if (!open) return;
        _searchField.Select();
        _searchField.ActivateInputField();
    }

    private void ToggleCost(int index)
    {
        _costs[index] = !_costs[index];
        RefreshCost(index);
        Changed?.Invoke();
    }

    public bool Matches(string unitId, int cost)
    {
        if (!unitId.Contains(_searchField.text.Trim(), StringComparison.OrdinalIgnoreCase)) return false;
        if (Array.IndexOf(_costs, true) < 0) return true;
        return _costs[Mathf.Clamp(cost, 1, _costs.Length) - 1];
    }

    private void RefreshCost(int index)
    {
        var on = _costs[index];
        _costToggles[index].GetComponent<Image>().color = on ? _onColor : _offColor;
        _costToggles[index].GetComponentInChildren<TMP_Text>().color = on ? _onTextColor : _offTextColor;
    }
}
