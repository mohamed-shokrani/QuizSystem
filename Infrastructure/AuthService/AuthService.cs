﻿using Core.Constants;
using Core.Models;
using Infrastructure.UnitOfWork;
using Infrastructure.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.AuthService;
public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JWT _jwt;
    public AuthService(UserManager<AppUser> userManager, IOptions<JWT> jwt, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _jwt = jwt.Value;
    }

    public async Task<AuthModel> RegisterAsync(RegisterModel model, string role)
    {
        if (await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            return new AuthModel
            {
                Message = "Email is Already Registered"
            };
        }
        if (await _userManager.FindByNameAsync(model.UserName) is not null)
        {
            return new AuthModel
            {
                Message = "UserName is Already Registered"
            };
        }
        var user = new AppUser
        {
            UserName = model.UserName,
            Email = model.Email,

        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = string.Empty;
            foreach (var error in result.Errors)
            {
                errors += $"{error.Description},";
            }
            return new AuthModel { Message = errors };

        }
        await _userManager.AddToRoleAsync(user, role);
        var jwtSecruityToken = await CreateJwtToken(user);
        await CreateNewUser(model, user, role);

        return new AuthModel
        {
            Email = user.Email,
            UserName = user.UserName,
            ExpiresOn = jwtSecruityToken.ValidTo,
            IsAuthenticated = true,
            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecruityToken),
            Roles = new List<string> { role }
        };


    }

    private async Task CreateNewUser(RegisterModel model, AppUser user, string role)
    {
        if (role == UserRole.Student)
        {
            var student = new Student
            {
                Address = model.Address,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedDate = DateTime.UtcNow,
                Mobile = model.Mobile,
                IdentityId = user.Id,
                CreatedBy = user.UserName,
            };
            await _unitOfWork.Students.AddAsync(student);

        }
        else
        {
            var instructor = new Instructor
            {

                Address = model.Address,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IdentityId = user.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = model.UserName,
                Mobile = model.Mobile,
                DateOfBirth = model.DateOfBirth,

            };
            await _unitOfWork.Instructors.AddAsync(instructor);


        }
        await _unitOfWork.Complete();
    }

    public async Task<JwtSecurityToken> CreateJwtToken(AppUser user)
    {
        var userClaim = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var role in roles)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, role));

        }
        var claims = new[]
        {
               new Claim(JwtRegisteredClaimNames.GivenName,$"{ user.UserName} "),
               new Claim(ClaimTypes.NameIdentifier,$"{ user.Id} "),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        }.Union(roleClaims).Union(userClaim);

        var symtricSecruityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var signingCredentials = new SigningCredentials(symtricSecruityKey, SecurityAlgorithms.HmacSha256);

        var symtricSecruityToken = new JwtSecurityToken
        (
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(_jwt.DurationInDays),
            signingCredentials: signingCredentials);

        return symtricSecruityToken;
    }

    public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
    {
        var authModel = new AuthModel();
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            authModel.Message = $"Email or Password is incorrect ";
            return authModel;
        }
        authModel.IsAuthenticated = true;
        var jwtSecurityToken = await CreateJwtToken(user);
        authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        authModel.Email = user.Email;
        authModel.UserName = user.UserName;
        authModel.ExpiresOn = jwtSecurityToken.ValidTo;
        var roleList = await _userManager.GetRolesAsync(user);
        authModel.Roles = roleList.ToList();

        return authModel;
    }
}
