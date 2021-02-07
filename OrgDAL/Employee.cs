using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrgDAL
{
    [Table("Employee")]
    public class Employee
    {
        [Key]
        public int EId { get; set; }

        public string Name { get; set; }

        [ForeignKey("Department")]
        public int DId { get; set; }

        public Department Department { get; set; }

        public string CreatedBy { get; set; }
    }
}
