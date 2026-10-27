public class LRUCache
{
    private readonly int capacity;
    private readonly Dictionary<int, Node> cache;
    private Node? _head;
    private Node? _tail;

    public LRUCache(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than 0");
        }

        this.capacity = capacity;

        cache = new Dictionary<int, Node>();

        _head = new Node(0, 0);
        _tail = new Node(0, 0);

        _head.Next = _tail;
        _tail.Previous = _head;
    }

    public int Capacity => capacity;
    public int Count => cache.Count;

    private void addtoFront(Node node)
    {
        node.Next = _head!.Next;
        node.Previous = _head;
        _head.Next!.Previous = node;
        _head.Next = node;
    }

    private void RemoveNode(Node node)
    {
        node.Previous!.Next = node.Next;
        node.Next!.Previous = node.Previous;

        node.Next = null;
        node.Previous = null;   
    }

    public int Get(int key)
    {
        if (cache.TryGetValue(key, out var node))
        {
            RemoveNode(node);
            addtoFront(node);
            return node.Value;
        }
        return -1;
    }
    
    public void Put(int key, int value)
    {
        if (cache.TryGetValue(key, out var node))
        {
            node.Value = value;
            RemoveNode(node);
            addtoFront(node);
        }
        else
        {
            if (cache.Count >= capacity)
            {
                var lru = _tail!.Previous!;
                RemoveNode(lru);
                cache.Remove(lru.Key);
            }
            var newNode = new Node(key, value);
            addtoFront(newNode);
            cache[key] = newNode;
        }
    }
}
