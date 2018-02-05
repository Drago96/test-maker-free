using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestMakerFreeWebApp.Data.Models
{
    public class Token
    {
        #region Constructor

        public Token()
        {
        }

        #endregion Constructor

        #region Properties

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string Value { get; set; }

        public int Type { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        #endregion Properties

        #region Lazy-Load Properties

        ///<summary>
        /// The user related to this token
        /// </summary>
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        #endregion Lazy-Load Properties
    }
}