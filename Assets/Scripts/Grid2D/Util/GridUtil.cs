using System;
using System.Collections.Generic;
using System.Linq;

public static class GridUtil
{
    public enum ArithmeticOperation { Add, Subtract, Multiply, Divide }

    public static bool BinaryStringContainsEnum(string binaryString, int enumValue)
    {
        return binaryString[enumValue] == '1';
    }

    public static HashSet<T> ListToHashSet<T>(List<T> list)
    {
        return new HashSet<T>(list);
    }

    public static T[] ListToFixedSizeArray<T>(List<T> list, int size)
    {
        var array = new T[size];
        for (var i = 0; i < size; i++)
        {
            if (i > list.Count - 1 || list[i] == null) array[i] = default(T);
            else                                       array[i] = list[i];
        }
        return array;
    }
    
    public static Dictionary<TKey, TValue> ToDictionaryBy<TSource, TKey, TValue>(
        IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector)
    {
        return source.ToDictionary(keySelector, valueSelector);
    }
    
    public static (int, int)? TupleArithmetic((int, int) a, (int, int) b, ArithmeticOperation arithmeticOperation)
    {
        (int, int) result;
        switch (arithmeticOperation)
        {
            case ArithmeticOperation.Add:
                result = (a.Item1 + b.Item1, a.Item2 + b.Item2);
                break;
            case ArithmeticOperation.Subtract:
                result = (a.Item1 - b.Item1, a.Item2 - b.Item2);
                break;
            case ArithmeticOperation.Multiply:
                result = (a.Item1 * b.Item1, a.Item2 * b.Item2);
                break;
            case ArithmeticOperation.Divide:
                result = (a.Item1 / b.Item1, a.Item2 / b.Item2);
                break;
            default:
                return null;
        }
        return result;
    }
    
    public static (int, int)? TupleArithmetic((int, int) a, int b, ArithmeticOperation arithmeticOperation, bool flip=false)
    {
        (int, int) result;
        switch (arithmeticOperation)
        {
            case ArithmeticOperation.Add:
                result = (a.Item1 + b, a.Item2 + b);
                break;
            case ArithmeticOperation.Subtract:
                result = flip ? (b - a.Item1, b - a.Item2) : (a.Item1 - b, a.Item2 - b);
                break;
            case ArithmeticOperation.Multiply:
                result = (a.Item1 * b, a.Item2 * b);
                break;
            case ArithmeticOperation.Divide:
                result = flip ? (b / a.Item1, b / a.Item2) : (a.Item1 / b, a.Item2 / b);
                break;
            default:
                return null;
        }
        return result;
    }

    public static Tuple<int, int> IntToTuple(int index, int x)
    {
        return new Tuple<int, int>(index % x, index / x);
    }

    public static List<TOut> ListAddFunc<TIn, TOut>(List<TIn> list, Func<TIn, TOut> func)
    {
        List<TOut> output = new List<TOut>();
        foreach (TIn element in list)
        {
            output.Add(func(element));
        }
        return output;
    }

    public static List<TOut> ListAddRangeFunc<TIn, TOut>(List<TIn> list, Func<TIn, List<TOut>> func)
    {
        List<TOut> output = new List<TOut>();
        foreach (TIn element in list)
        {
            output.AddRange(func(element));
        }
        return output;
    }
}