public class User
{
    public string Id { get; set; }
    public string Name { get; set; }

    public User(string id, string name)
    {
        Id = id;
        Name = name;
    }
}   