﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Education.Areas.Admin.Data.DataModel
{
    public class FaultyModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Faulty code is not left blank")]
        [RegularExpression("^[A-Z][A-Z0-9]{2,9}$", ErrorMessage = "Code starts with flower print, including flower printing and numbers. From 3 to 10 characters")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Faulty Name is not left blank")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Faulty Phone is not left blank")]
        [RegularExpression("^[0]([0-9]){9,12}$", ErrorMessage = "Enter the phone number with a length of 10 to 13 characters, start 0 ****!")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Faulty Email is not left blank")]
        [RegularExpression("^[\\w-.]+@([\\w-]+.)+[\\w-]{2,4}$", ErrorMessage = "Email invalidate")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Faulty Address is not left blank")]
        public string Address { get; set; }
        [Display(Name = "Image")]
        public string ImageDisplay { get; set; }
        public HttpPostedFileBase Image { get; set; }
        [Required(ErrorMessage = "Faulty Gender is not left blank")]
        public int Gender { get; set; }
        [Required(ErrorMessage = "Faulty Salary is not left blank")]
        public double Salary { get; set; }
        [Required(ErrorMessage = "Faulty Qualification is not left blank")]
        public string Qualification { get; set; }
        [Required(ErrorMessage = "Faulty Birthday is not left blank")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }
        public string FullName { get; set; }
        [Display(Name = "UserName")]
        [Required(ErrorMessage = "Faulty UserName is not left blank")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Faulty Password is not left blank")]
        [RegularExpression("^([a-zA-Z0-9]){5,15}$", ErrorMessage = "Enter the password that does not contain special characters, lengths from 5 to 15 characters!")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Repeat Password")]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string RepeatPassword { get; set; }
    }
}