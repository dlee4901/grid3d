using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeletableDropdown : TMP_Dropdown
{
    public event Action<int> DeleteRequested;
    public Func<int, bool> CanDelete;

    private int _createdItems;

    protected override GameObject CreateDropdownList(GameObject template)
    {
        _createdItems = 0;
        return base.CreateDropdownList(template);
    }

    protected override DropdownItem CreateItem(DropdownItem itemTemplate)
    {
        var item = base.CreateItem(itemTemplate);
        var index = _createdItems++;
        var delete = item.GetComponentInChildren<Button>(true);
        var deletable = CanDelete == null || CanDelete(index);
        delete.gameObject.SetActive(deletable);
        if (!deletable)
        {
            var label = item.text.rectTransform;
            label.offsetMax = new Vector2(-label.offsetMin.x, label.offsetMax.y);
        }
        delete.onClick.AddListener(() =>
        {
            Hide();
            DeleteRequested?.Invoke(index);
        });
        return item;
    }
}
