namespace CouchGraphQl.Data.Documents;

using System.ComponentModel.DataAnnotations;

public interface IEntityWithId<T>
    where T : IComparable
{
    [Required]
    T Id { get; set; }
    
    [Required]
    string Type { get; set; }
}