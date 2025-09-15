using Lab2.Entities;

namespace Lab2.Containers;

public abstract class NodeContainer
{
    public abstract void Enqueue(Node node);
    
    public abstract Node Dequeue();
    
    public abstract int Count { get; }
    
    public abstract string AlgorithmName { get; }
    
    public void SpawnInitNode(Vector state) => Enqueue(CreateInitNode(state));

    protected virtual Node CreateInitNode(Vector state) => new(state);
    
    private const int BytesForOneObj = 100;

    protected NodeContainer(int ramLimitMb) => _allowedObject = ramLimitMb * 1024 * 1024 / BytesForOneObj;

    private readonly int _allowedObject;
    private int _currentObject;
    
    public int StateCount => _currentObject;

    protected void TrackObj()
    {
        _currentObject++;
        if (_currentObject > _allowedObject)
        {
            Console.WriteLine($"Reach {_currentObject} objects, limit is {_allowedObject}");
            throw new OutOfMemoryException("Memory limit exceeded");
        }
    }
}