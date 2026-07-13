// using UnityEngine;
//
// public class GridLines : MonoBehaviour
// {
//     [SerializeField] private Material _lineMaterial;
//     [SerializeField] private float _lineWidth = 0.05f;
//     
//     public void Init(int x, int y, float height, int scale=1)
//     {
//         for (var i = 0; i <= x; i += scale)
//         {
//             var gridLine = new GameObject("LineX" + i);
//             var lineRenderer = gridLine.AddComponent<LineRenderer>();
//             gridLine.transform.SetParent(transform);
//             gridLine.transform.rotation = Quaternion.Euler(90, 0, 0);
//             
//             lineRenderer.material = _lineMaterial;
//             lineRenderer.useWorldSpace = false;
//             lineRenderer.alignment = LineAlignment.TransformZ;
//             lineRenderer.startWidth = _lineWidth;
//             
//             var positions = new Vector3[2];
//             positions[0] = new Vector3(i, 0, 0);
//             positions[1] = new Vector3(i, y * scale, 0);
//             lineRenderer.SetPositions(positions);
//         }
//         
//         for (var i = 0; i <= y; i += scale)
//         {
//             var gridLine = new GameObject("LineY" + i);
//             var lineRenderer = gridLine.AddComponent<LineRenderer>();
//             gridLine.transform.SetParent(transform);
//             gridLine.transform.rotation = Quaternion.Euler(90, 90, 0);
//             
//             lineRenderer.material = _lineMaterial;
//             lineRenderer.useWorldSpace = false;
//             lineRenderer.alignment = LineAlignment.TransformZ;
//             lineRenderer.startWidth = _lineWidth;
//             
//             var positions = new Vector3[2];
//             positions[0] = new Vector3(-i, 0, 0);
//             positions[1] = new Vector3(-i, y * scale, 0);
//             lineRenderer.SetPositions(positions);
//         }
//         
//         transform.position = new Vector3(0, height, 0);
//     }
//     
//     public void InitNew(int x, int y, float height, int scale=1)
//     {
//         var lineRenderer = gameObject.AddComponent<LineRenderer>();
//         transform.rotation = Quaternion.Euler(90, 0, 0);
//         lineRenderer.material = _lineMaterial;
//         lineRenderer.useWorldSpace = false;
//         lineRenderer.alignment = LineAlignment.TransformZ;
//         lineRenderer.startWidth = _lineWidth;
//         
//         var pointsCount = ((x + 1) * 2) + ((y + 1) * 2);
//         lineRenderer.positionCount = pointsCount;
//         
//         var index = 0;
//         
//         for (var i = 0; i <= x; i++)
//         {
//             float xPos = i * scale;
//             
//             if (i % 2 == 0)
//             {
//                 lineRenderer.SetPosition(index++, new Vector3(xPos, 0, 0));
//                 lineRenderer.SetPosition(index++, new Vector3(xPos, x * scale, 0));
//             }
//             else
//             {
//                 lineRenderer.SetPosition(index++, new Vector3(xPos, x * scale, 0));
//                 lineRenderer.SetPosition(index++, new Vector3(xPos, 0, 0));
//             }
//         }
//         
//         var lastX = y * scale;
//         var startYOfHorizontal = (y % 2 == 0) ? (x * scale) : 0f;
//         
//         for (var i = 0; i <= y; i++)
//         {
//             float yPos = i * scale;
//             
//             // Adjust the sequence based on the starting position of the line
//             if (startYOfHorizontal > 0)
//             {
//                 float targetY = (x - i) * scale;
//                 if (i % 2 == 0)
//                 {
//                     lineRenderer.SetPosition(index++, new Vector3(lastX, targetY, 0));
//                     lineRenderer.SetPosition(index++, new Vector3(0, targetY, 0));
//                 }
//                 else
//                 {
//                     lineRenderer.SetPosition(index++, new Vector3(0, targetY, 0));
//                     lineRenderer.SetPosition(index++, new Vector3(lastX, targetY, 0));
//                 }
//             }
//             else
//             {
//                 if (i % 2 == 0)
//                 {
//                     lineRenderer.SetPosition(index++, new Vector3(lastX, yPos, 0));
//                     lineRenderer.SetPosition(index++, new Vector3(0, yPos, 0));
//                 }
//                 else
//                 {
//                     lineRenderer.SetPosition(index++, new Vector3(0, yPos, 0));
//                     lineRenderer.SetPosition(index++, new Vector3(lastX, yPos, 0));
//                 }
//             }
//         }
//         
//         transform.position = new Vector3(0, height, 0);
//     }
// }