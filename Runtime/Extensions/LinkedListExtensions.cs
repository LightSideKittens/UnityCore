using System.Collections.Generic;

public static class LinkedListExtensions
{
    public static LinkedListNode<T> GetElementAt<T>(this LinkedList<T> list, int index)
    {
        if (index < 0 || index >= list.Count)
        {
            return null;
        }

        LinkedListNode<T> current;
        if (index < list.Count / 2)
        {
            current = list.First;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }
        }
        else
        {
            current = list.Last;
            for (int i = list.Count - 1; i > index; i--)
            {
                current = current.Previous;
            }
        }
        return current;
    }

    public static void Insert<T>(this LinkedList<T> list, int index, LinkedListNode<T> newNode)
    {
        list.AddBefore(list.GetElementAt(index), newNode);
    }
}