﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codit.LevelTwo.Entities
{
    public class Customization
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Url { get; set; }

        public int NumberSold { get; set; }

        public int InventoryLevel { get; set; }

        [ForeignKey(name: "CarId")]
        public Car Car { get; set; }

        public int CarId { get; set; }

        public void Sell()
        {
            NumberSold += 1;
            InventoryLevel -= 1;
        }
    }
}