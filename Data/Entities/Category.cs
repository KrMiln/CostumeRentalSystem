using CostumeRentalSystem.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Costume> Costumes { get; set; } 
            = new List<Costume>();
    }
}