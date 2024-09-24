using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Picasso.Palette.Utilities;

public class ObservableStack<T> : ObservableCollection<T>
{
    public ObservableStack() : base() { }

    public ObservableStack(IEnumerable<T> collection) : base(collection) { }

    public ObservableStack(List<T> list) : base(list) { }

    public void Push(T item)
    {
        Insert(0, item);
    }

    public T Pop()
    {
        if (Count == 0)
            throw new InvalidOperationException("The stack is empty.");

        T item = this[0];
        RemoveAt(0);
        return item;
    }

    public T Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("The stack is empty.");

        return this[0];
    }
}
