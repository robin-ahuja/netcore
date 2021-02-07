using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace OrgDAL
{
    [Table("Department")]
    public class Department
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int DId { get; set; }

        [Required]
        public string DName { get; set; }

        public string Description { get; set; }

        public IEnumerable<Employee> Employees { get; set; }

        public string CreatedBy { get; set; }
    }
}
