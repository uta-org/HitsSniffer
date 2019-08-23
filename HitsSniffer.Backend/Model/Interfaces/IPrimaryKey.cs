namespace HitsSniffer.Model.Interfaces
{
    public interface IPrimaryKey
    {
        int Id { get; set; }

        string Identifier();
    }
}