﻿using EFS_23298_23327.Data;
using EFS_23298_23327.Models;
using EFS_23298_23327.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Data;
using System.Security.Claims;

namespace EFS_23298_23327.API.Gerir
{
    [Route("api/gerir/account")]

    [ApiController]
    public class AccountControllerAPI : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<Utilizadores> _signInManager;
        private readonly UserManager<Utilizadores> _userManager;
        private readonly IUserStore<Utilizadores> _userStore;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserEmailStore<Utilizadores> _emailStore;

        public AccountControllerAPI(UserManager<Utilizadores> userManager,
          IUserStore<Utilizadores> userStore,

          SignInManager<Utilizadores> signInManager,
          ApplicationDbContext context

         )
        {
            _userManager = userManager;
            _context = context;

            _userStore = userStore;

            _signInManager = signInManager;


        }

        // GET: api/AccountController
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            
            if (User.Identity.IsAuthenticated) {
                var u = await _context.Utilizadores.Where(a => !a.Deleted)
                                               .Where(a => a.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
                                               .FirstOrDefaultAsync();
                if (u != null) {
                    return Ok(new { Username = User.FindFirstValue(ClaimTypes.Name), Roles = await _userManager.GetRolesAsync(u), Id = User.FindFirstValue(ClaimTypes.NameIdentifier) });

                }

            }
            return Unauthorized(new { Error = "Not Auth" });
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginVM)
        {
            // Verifica se o modelo de dados enviado é válido
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Error = "Dados inválidos." });
            }

            // Busca o utilizador não excluído
            var u = await _context.Utilizadores.Where(a => !a.Deleted)
                                                .Where(a => a.UserName == loginVM.Username)
                                                .FirstOrDefaultAsync();
            if (u == null)
            {
                return Unauthorized(new { Error = "Utilizador não encontrado." });
            }

            // Verifica se o utilizador está marcado como excluído
            var userDeleted = await _context.Utilizadores.Where(a => a.Deleted)
                                                          .Where(a => a.Id == u.Id)
                                                          .FirstOrDefaultAsync();
            if (userDeleted != null)
            {
                return Unauthorized(new { Error = "Utilizador excluído." });
            }

            // Verifica se a senha é válida
            var isPasswordValid = await _userManager.CheckPasswordAsync(u, loginVM.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Error = "Senha incorreta." });
            }

            // Realiza o login
            var result = await _signInManager.PasswordSignInAsync(u, loginVM.Password, false, false);
            if (result.Succeeded)
            {
                // Verifica e atribui o papel de admin, se necessário
                if (u.UserName == "admin" && !await _userManager.IsInRoleAsync(u, "Admin"))
                {
                    await _userManager.AddToRoleAsync(u, "Admin");
                }

                // Define o papel baseado no utilizador
                var roles = await _userManager.GetRolesAsync(u);


                return Ok(new { Username = User.FindFirstValue(ClaimTypes.Name), Roles = roles,id=User.FindFirstValue(ClaimTypes.NameIdentifier)});
            }

            // Verifica se o email do utilizador foi confirmado
            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(u);
            if (!emailConfirmed)
            {
                return Unauthorized(new { Error = "Por favor, confirme o seu email antes de fazer login." });
            }

            return Unauthorized(new { Error = "Erro na autenticação." });
        }

        [HttpPut]
        public async Task<IActionResult> logout() {
           
            await _signInManager.SignOutAsync();
           
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return Ok();
            }
        }






    }
}
