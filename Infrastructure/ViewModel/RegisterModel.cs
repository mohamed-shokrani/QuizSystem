﻿using System.ComponentModel.DataAnnotations;

namespace Infrastructure.ViewModel;
public class RegisterModel
{
    [Required, StringLength(100)]
    public string FirstName { get; set; }
    [Required, StringLength(100)]

    public string LastName { get; set; }
    [Required, StringLength(100)]

    public string UserName { get; set; }
    [Required, StringLength(256)]

    public string Password { get; set; }
    [Required, StringLength(128)]

    public string Email { get; set; }
    [Required, StringLength(50)]

    public string Mobile { get; set; }
    [Required, StringLength(350)]

    public string Address { get; set; }
    public DateTime DateOfBirth { get; set; }
}
