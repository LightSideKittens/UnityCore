public delegate TOut InFunc<T, out TOut>(in T d);
public delegate void InAction<T>(in T value);