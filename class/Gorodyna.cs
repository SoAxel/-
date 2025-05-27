public class Gorodyna
{
    public string Name { get; set; }
    public string Country { get; set; }
    public int Season { get; set; }

    public override string ToString() => $"{Name} ({Country}), сезон {Season}";
}
