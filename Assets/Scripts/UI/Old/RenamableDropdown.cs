using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RenamableDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _addOption;
    [SerializeField] private Button _deleteOption;

    [SerializeField] private int _maxTextLength = 16;
    [SerializeField] private int _maxEntries = 16;

    private TMP_Text _inputFieldPlaceholder;
    private RectTransform _dropdownTemplateTransform;

    void Start()
    {
        _inputField.gameObject.SetActive(false);
        //_inputField.DeactivateInputField();
        _inputField.onEndEdit.AddListener(OnRename);
        _addOption.onClick.AddListener(OnAddOption);
        _deleteOption.onClick.AddListener(OnDeleteOption);
        _inputFieldPlaceholder = _inputField.placeholder.GetComponent<TMP_Text>();
        _dropdownTemplateTransform = _dropdown.template.GetComponent<RectTransform>();
        RefreshDropdown();
    }

    public void OnAddOption()
    {
        if (_dropdown.options.Count >= _maxEntries) return;
        string newTeamDefaultName = "New Team " + (_dropdown.options.Count + 1);
        _dropdown.options.Add(new TMP_Dropdown.OptionData(newTeamDefaultName));
        _dropdown.value = _dropdown.options.Count - 1;
        RefreshDropdown();
        _dropdown.Hide();
        EnableRename(newTeamDefaultName);
    }

    private void OnDeleteOption()
    {
        if (_dropdown.options.Count == 1) return;
        int selectedIndex = _dropdown.value;
        _dropdown.options.RemoveAt(selectedIndex);
        _dropdown.value = _dropdown.options.Count - 1;
        RefreshDropdown();
    }

    private void OnRename(string inputText)
    {
        string newName = _inputFieldPlaceholder.text;
        if (ValidateInputText(inputText)) newName = inputText;
        RenameSelectedText(newName);
        _inputField.gameObject.SetActive(false);
    }

    private void RefreshDropdown()
    {
        _dropdown.RefreshShownValue();
        _dropdownTemplateTransform.sizeDelta = new Vector2(_dropdownTemplateTransform.sizeDelta.x, _dropdown.options.Count * 60);
    }

    public void SetOptions(List<string> options)
    {
        _dropdown.ClearOptions();
        _dropdown.AddOptions(options);
    }

    public void EnableRename(string placeholderText)
    {
        if (_dropdown.options.Count == 0) return;
        _inputField.gameObject.SetActive(true);
        _inputField.text = "";
        _inputField.Select();
        if (placeholderText != null && placeholderText != "") _inputFieldPlaceholder.text = placeholderText;
    }

    private bool ValidateInputText(string inputText)
    {
        return inputText.Length > 0 && inputText.Length <= _maxTextLength;
    }

    private void RenameSelectedText(string newName)
    {
        int selectedIndex = _dropdown.value;
        _dropdown.options[selectedIndex].text = newName;
        RefreshDropdown();
    }
}