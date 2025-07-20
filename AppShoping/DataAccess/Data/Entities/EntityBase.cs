using System.Text.Json.Serialization;

namespace AppShoping.DataAccess.Data.Entities
{
    public abstract class EntityBase : IEntity
    {
        [JsonPropertyOrder(-1)]
        public int Id { get; set; }
    }
}
