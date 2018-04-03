namespace Services.Contracts.Services.Dependency
{
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool? Processed { get; set; }
    }
}
