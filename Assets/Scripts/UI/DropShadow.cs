using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DropShadow : MonoBehaviour
{
    public enum Direction { Left, Right, Down, Up }

    [SerializeField] private Graphic _shadow;
    [SerializeField] private Shadow _effect;
    [SerializeField] private Direction _direction = Direction.Left;
    [SerializeField] private float _distance = 4f;
    [SerializeField] private Color32 _color = new(0, 0, 0, 128);
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Material[] _textMaterials = new Material[4];

    private Vector2 Offset => _distance * _direction switch
    {
        Direction.Left => Vector2.left,
        Direction.Right => Vector2.right,
        Direction.Down => Vector2.down,
        _ => Vector2.up
    };

    public void SetDirection(Direction direction)
    {
        _direction = direction;
        Apply();
    }

    private void OnEnable() => Apply();

#if UNITY_EDITOR
    private void OnValidate() => UnityEditor.EditorApplication.delayCall += () =>
    {
        if (this != null) Apply();
    };
#endif

    private void Apply()
    {
        if (_shadow != null)
        {
            _shadow.rectTransform.anchoredPosition = Offset;
            _shadow.color = _color;
        }
        if (_effect != null)
        {
            _effect.effectDistance = Offset;
            _effect.effectColor = _color;
        }
        var index = (int)_direction;
        if (_text != null && _textMaterials != null && index < _textMaterials.Length && _textMaterials[index] != null)
            _text.fontSharedMaterial = _textMaterials[index];
    }
}
