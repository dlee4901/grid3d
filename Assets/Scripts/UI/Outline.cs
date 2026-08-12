using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class Outline : MonoBehaviour
{
    [SerializeField] private Color _color = new Color32(255, 255, 255, 64);
    [SerializeField] private List<Image> _lines;
    [SerializeField] private List<Image> _corners;
    
    private void Start()
    {
        foreach (var line in _lines) line.color = _color;
        foreach (var line in _corners) line.color = _color;
    }
    
    public void SetTransformsDiamond(float linePosition, float cornerPosition, float lineLength, float width=2f)
    {
        var cornerNormals = new List<(int, int)>{(0, 1), (1, 0), (0, -1), (-1, 0)};
        for (var i = 0; i < _lines.Count; i++)
        {
            var line = _lines[i];
            var x = i % 4 < 2 ? linePosition : -linePosition;
            var y = (i % 4 + 1) % 4 < 2 ? linePosition : -linePosition;
            line.rectTransform.anchoredPosition = new Vector2(x, y);
            line.rectTransform.sizeDelta = new Vector2(lineLength, width);
            
            var corner = _corners[i];
            x = cornerPosition * cornerNormals[i % 4].Item1;
            y = cornerPosition * cornerNormals[i % 4].Item2;
            corner.rectTransform.anchoredPosition = new Vector2(x, y);
            corner.rectTransform.sizeDelta = new Vector2(width, width);
        }
    }
}