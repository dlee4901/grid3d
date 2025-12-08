// using System;
// using System.Collections.Generic;
// using System.Linq;
//
// public enum Direction {N, NE, E, SE, S, SW, W, NW, step, stride, line, diagonal, straight, horizontal, vertical}
// public enum DirectionCardinal {N, NE, E, SE, S, SW, W, NW}
// [Flags] public enum Team {Neutral=1, Ally=2, Enemy=4}
//
// [Serializable]
// public class GridSelection
// {
//     public Direction Direction { get; private set; }
//     public Team Passthrough { get; private set; }
//     public int Distance { get; private set; }
//     public bool Exact { get; private set; }
//     public bool RelativeFacing { get; private set; }
//     public bool Chain { get; private set; }
//
//     public GridSelection(Direction direction, Team passthrough, int distance, bool exact, bool relativeFacing, bool chain)
//     {
//         Direction = direction;
//         Passthrough = passthrough;
//         Distance = distance;
//         Exact = exact;
//         RelativeFacing = relativeFacing;
//         Chain = chain;
//     }
//
//     public HashSet<int> GetSelectableTiles(Grid2D grid, Tuple<int, int> position)
//     {
//         List<Tuple<int, int>> selectableTiles = new();
//         Unit unit = (Unit)grid.GetEntity(position);
//         List<Tuple<int, int>> unitVectors = GetUnitVectors(unit);
//         List<bool> collisions = new List<bool>{false, false, false, false, false, false, false, false};
//         int distance = Distance;
//         int x = grid.X;
//         int y = grid.Y;
//         if (distance == -1) distance = Math.Max(x, y);
//         if (Direction == Direction.step || Direction == Direction.stride)
//         {
//             Dictionary<Tuple<int, int>, bool> visitedTiles = new();
//             for (int i = 0; i < grid.GetSize(); i++) visitedTiles[grid.ToPosition2D(i)] = false;
//             List<Tuple<int, int>> checkTiles = new() {position};
//             for (int i = 0; i <= distance; i++)
//             {
//                 List<Tuple<int, int>> nextTiles = new();
//                 foreach (Tuple<int, int> tilePosition in checkTiles)
//                 {
//                     for (int j = 0; j < 8; j++)
//                     {
//                         if (Direction == Direction.step && j % 2 == 1) continue;
//                         Tuple<int, int> startPosition = tilePosition;
//                         Tuple<int, int> targetPosition = Util.TupleArithmetic(startPosition, unitVectors[j], Util.ArithmeticOperation.Add);
//                         if (visitedTiles[tilePosition] || !grid.IsValidPosition(targetPosition)) continue;
//                         Entity entityColliding = grid.GetEntity(targetPosition);
//                         if (entityColliding != null)
//                         {
//                             if (Passthrough == 0
//                                 || (Passthrough == Team.Ally && !entityColliding.HasSameController(unit))
//                                 || (Passthrough == Team.Enemy && entityColliding.HasSameController(unit)))
//                             {
//                                 continue;
//                             }
//                         }
//                         nextTiles.Add(targetPosition);
//                     }
//                     visitedTiles[tilePosition] = true;
//                 }
//                 selectableTiles.AddRange(checkTiles);
//                 checkTiles = nextTiles;
//             }
//         }
//         else
//         {
//             for (int i = 0; i < distance; i++)
//             {
//                 for (int j = 0; j < 8; j++)
//                 {
//                     Tuple<int, int> startPosition = position;
//                     if (i > 0) startPosition = selectableTiles[8 * (i - 1) + j];
//                     if (collisions[j])
//                     {
//                         selectableTiles.Add(startPosition);
//                         continue;
//                     }
//                     Tuple<int, int> targetPosition = Util.TupleArithmetic(startPosition, unitVectors[j], Util.ArithmeticOperation.Add);
//                     bool targetPositionValid = true;
//                     if (!grid.IsValidPosition(targetPosition)) targetPositionValid = false;
//                     else 
//                     {
//                         Entity entityColliding = grid.GetEntity(targetPosition);
//                         if (entityColliding != null)
//                         {
//                             if (Passthrough == 0
//                                 || (Passthrough == Team.Ally && !entityColliding.HasSameController(unit))
//                                 || (Passthrough == Team.Enemy && entityColliding.HasSameController(unit)))
//                             {
//                                 collisions[j] = true;
//                             }
//                         }
//                     }
//                     if (targetPositionValid) selectableTiles.Add(targetPosition);
//                     else selectableTiles.Add(startPosition);
//                 }
//             }
//         }
//         return new HashSet<int>(grid.ToPosition1DList(selectableTiles));
//     }
//
//     public List<Tuple<int, int>> GetUnitVectors(Unit unit)
//     {
//         List<Tuple<int, int>> unitVectors = new();
//         List<bool> absoluteDirections = GetAbsoluteDirections(unit);
//         for (int i = 0; i < 8; i++)
//         {
//             int xOffset = 0;
//             int yOffset = 0;
//             if (absoluteDirections[i])
//             {
//                 if (i > 4)               xOffset = -1;
//                 else if (i > 0 && i < 4) xOffset = 1;
//                 if (i > 2 && i < 6)      yOffset = 1;
//                 else if (i < 2 || i > 6) yOffset = -1;
//             }
//             unitVectors.Add(new Tuple<int, int>(xOffset, yOffset));
//         }
//         return unitVectors;
//     }
//
//     public List<bool> GetAbsoluteDirections(Unit unit)
//     {
//         List<bool> absoluteDirections = new List<bool>{false, false, false, false, false, false, false, false};
//         switch (Direction)
//         {
//             case Direction.stride: case Direction.line:
//                 for (int i = 0; i < 8; i++) absoluteDirections[i] = true;
//                 break;
//             case Direction.diagonal:
//                 for (int i = 1; i < 8; i += 2) absoluteDirections[i] = true;
//                 break;
//             case Direction.step: case Direction.straight:
//                 for (int i = 0; i < 8; i += 2) absoluteDirections[i] = true;
//                 break;
//             case Direction.horizontal:
//                 absoluteDirections[2] = true;
//                 absoluteDirections[6] = true;
//                 break;
//             case Direction.vertical:
//                 absoluteDirections[0] = true;
//                 absoluteDirections[4] = true;
//                 break;
//             case Direction.N:
//                 absoluteDirections[0] = true;
//                 break;
//             case Direction.NE:
//                 absoluteDirections[1] = true;
//                 break;
//             case Direction.E:
//                 absoluteDirections[2] = true;
//                 break;
//             case Direction.SE:
//                 absoluteDirections[3] = true;
//                 break;
//             case Direction.S:
//                 absoluteDirections[4] = true;
//                 break;
//             case Direction.SW:
//                 absoluteDirections[5] = true;
//                 break;
//             case Direction.W:
//                 absoluteDirections[6] = true;
//                 break;
//             case Direction.NW:
//                 absoluteDirections[7] = true;
//                 break;
//             default:
//                 return absoluteDirections;
//         }
//         if (RelativeFacing && unit != null)
//         {
//             int shift = 0;
//             switch (unit.DirectionFacing)
//             {
//                 case DirectionFacing.E:
//                     shift = 6;
//                     break;
//                 case DirectionFacing.S:
//                     shift = 2;
//                     break;
//                 case DirectionFacing.W:
//                     shift = 4;
//                     break;
//                 default:
//                     return absoluteDirections;
//             }
//             return (List<bool>)absoluteDirections.Skip(shift).Concat(absoluteDirections.Take(shift));
//         }
//         return absoluteDirections;
//     }
// }