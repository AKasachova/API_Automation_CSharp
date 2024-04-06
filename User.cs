public class User : IEquatable<User>
{
    public string Name { get; set; }
    public int? Age { get; set; }
    public string Sex { get; set; }
    public string? ZipCode { get; set; }

    public override bool Equals(object obj)
    {
        return this.Equals(obj as User);
    }

    public bool Equals(User other)
    {
        if (other==null)
            return false;
        return  Name == other.Name && Age == other.Age && Sex == other.Sex && ZipCode == other.ZipCode;
    }
}
