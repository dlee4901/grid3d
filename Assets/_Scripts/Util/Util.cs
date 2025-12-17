using System;
using System.Collections.Generic;

public static class Util
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

    public static Tuple<int, int> TupleArithmetic(Tuple<int, int> a, Tuple<int, int> b, ArithmeticOperation arithmeticOperation)
    {
        Tuple<int, int> result;
        switch (arithmeticOperation)
        {
            case ArithmeticOperation.Add:
                result = new Tuple<int, int>(a.Item1 + b.Item1, a.Item2 + b.Item2);
                break;
            case ArithmeticOperation.Subtract:
                result = new Tuple<int,int>(a.Item1 - b.Item1, a.Item2 - b.Item2);
                break;
            case ArithmeticOperation.Multiply:
                result = new Tuple<int,int>(a.Item1 * b.Item1, a.Item2 * b.Item2);
                break;
            case ArithmeticOperation.Divide:
                result = new Tuple<int,int>(a.Item1 / b.Item1, a.Item2 / b.Item2);
                break;
            default:
                result = null;
                break;
        }
        return result;
    }
    
    public static Tuple<int, int> TupleArithmetic(Tuple<int, int> a, int b, ArithmeticOperation arithmeticOperation, bool flip=false)
    {
        Tuple<int, int> result;
        switch (arithmeticOperation)
        {
            case ArithmeticOperation.Add:
                result = new Tuple<int, int>(a.Item1 + b, a.Item2 + b);
                break;
            case ArithmeticOperation.Subtract:
                result = flip ? new Tuple<int,int>(b - a.Item1, b - a.Item2) : new Tuple<int,int>(a.Item1 - b, a.Item2 - b);
                break;
            case ArithmeticOperation.Multiply:
                result = new Tuple<int,int>(a.Item1 * b, a.Item2 * b);
                break;
            case ArithmeticOperation.Divide:
                result = flip ? new Tuple<int,int>(b / a.Item1, b / a.Item2) : new Tuple<int,int>(a.Item1 / b, a.Item2 / b);
                break;
            default:
                result = null;
                break;
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

    // public static IEnumerable<PropertyInfo> GetProperties(object obj, int depth=0)
    // {
    //     if (depth > 4) yield return null;
    //     HashSet<Type> nonRecursiveTypes = new HashSet<Type>{typeof(int), typeof(string), typeof(Array), typeof(Enum)};
    //     foreach (PropertyInfo property in obj.GetType().GetProperties())
    //     {
    //         if (nonRecursiveTypes.Contains(property.GetType()))
    //         {
    //             yield return property;
    //         }
    //         else
    //         {
    //             foreach (PropertyInfo innerProperty in GetProperties(property.GetValue(obj), depth+1))
    //             {
    //                 yield return innerProperty;
    //             }
    //         }
    //     }
    // }

    // public static Dictionary<string, object> GetPropertiesDictionary(object obj)
    // {
    //     if (obj == null) return null;
    //     Dictionary<string, object> dict = new Dictionary<string, object>();
    //     Type type = obj.GetType();
    //     PropertyInfo[] properties = type.GetProperties();
    //     foreach (PropertyInfo property in properties)
    //     {
    //         object value = property.GetValue(obj, new object[]{});
    //         dict.Add(property.Name, value);
    //     }
    //     return dict;
    // }
}