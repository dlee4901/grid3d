using System.Collections.Generic;
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
}