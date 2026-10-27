public class Node
{
    public int Key { get; set; }
    public int Value { get; set; }
    public Node? Previous { get; set; }
    public Node? Next { get; set; }

    public Node(int key, int value)
    {
        Key = key;
        Value = value;
    }
}