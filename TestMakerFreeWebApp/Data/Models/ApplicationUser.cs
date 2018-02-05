using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestMakerFreeWebApp.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        #region Constructor

        public ApplicationUser()
        {
        }

        #endregion Constructor

        #region Properties

        public string DisplayName { get; set; }

        public string Notes { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        public int Flags { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastModifiedDate { get; set; }

        #endregion Properties

        #region Lazy-Load Properties

        /// <summary>
        /// A list of all the quiz created by this users.
        /// </summary>
        public virtual List<Quiz> Quizzes { get; set; }

        ///<summary>
        /// A list of all the refresh tokens for this user.
        /// </summary>
        public virtual List<Token> Tokens { get; set; }

        #endregion Lazy-Load Properties
    }
}