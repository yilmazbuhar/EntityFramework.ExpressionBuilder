public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public Team Team { get; set; }
    public Guid TeamId { get; set; }

    public override string ToString()
    {
        return $"{Name} {Surname} with Id {Id.ToString()} with team {Team.Title}";
    }

}
