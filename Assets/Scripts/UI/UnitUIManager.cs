using UnityEngine;
using UnityEngine.UI;

public class UnitUIManager : MonoBehaviour
{
    private int _id;
    private Image _image;
    //private bool _selected;

    public void Init(int id, Sprite sprite, Transform parent)
    {
        _id = id;
        _image = EngineUtil.GetOrAddComponent<Image>(gameObject);
        _image.sprite = sprite;
        transform.SetParent(parent);
        //_selected = false;
    }
}