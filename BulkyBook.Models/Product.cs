using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public String Description { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        [Range(1,10000)]
        public double ListPrice { get; set; }
        [Required]
        [Range(1, 10000)]
        [DisplayName("Price for 1:50")]
        public double Price { get; set; }
        [Required]
        [Range(1, 10000)]

        [DisplayName("Price for 50:100")]
        public double Price50 { get; set; }
        [Required]
        [Range(1, 10000)]

        [DisplayName("Price for 100+")]
        public double Price100 { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }
        [Required]

        [DisplayName("Category")]
        public int CategoryId { get; set; }
        // foreign key annotation is not required because 
        // .net frameWork is smart enough to understand the relation implicitly
        // especially because we didn't change the name of CategoryId Property
        // to be different than the CoverTypeId property in the Category class
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        // foreign key annotation is not required because 
        // .net frameWork is smart enough to understand the relation implicitly
        // especially because we didn't change the name of CoverTypeId Property
        // to be different than the CoverTypeId property in the coverType class
        [Required]
        [ForeignKey("CoverTypeId")]
        [DisplayName("Cover Type")]
        public int CoverTypeId { get; set; }
        [ValidateNever]
        public CoverType CoverType { get; set; }


    }
}
