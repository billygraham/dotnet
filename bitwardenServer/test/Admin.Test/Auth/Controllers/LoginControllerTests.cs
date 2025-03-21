using Bit.Admin.Auth.Controllers;
using Bit.Admin.Auth.IdentityServer;
using Bit.Admin.Auth.Models;
using Bit.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Admin.Test.Auth.Controllers;

public class LoginControllerTests
{
    private readonly PasswordlessSignInManager<IdentityUser> _signInManager;
    private readonly LoginController _controller;

    public LoginControllerTests()
    {
        _signInManager = Substitute.For<PasswordlessSignInManager<IdentityUser>>(
            Substitute.For<UserManager<IdentityUser>>(
                Substitute.For<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null),
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>(),
            Substitute.For<IOptions<IdentityOptions>>(),
            Substitute.For<ILogger<SignInManager<IdentityUser>>>(),
            Substitute.For<IAuthenticationSchemeProvider>(),
            Substitute.For<IUserConfirmation<IdentityUser>>(),
            Substitute.For<IMailService>()
        );
        _controller = new LoginController(_signInManager);
    }

    [Fact]
    public void Index_Get_ReturnsViewResult_WithLoginModel()
    {
        // Arrange
        string returnUrl = "testUrl";
        int? error = null;
        int? success = null;
        bool accessDenied = false;

        // Act
        var result = _controller.Index(returnUrl, error, success, accessDenied) as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = result.Model as LoginModel;
        Assert.NotNull(model);
        Assert.Equal(returnUrl, model.ReturnUrl);
    }

    [Fact]
    public async Task Index_Post_ValidModel_RedirectsToIndexWithSuccess()
    {
        // Arrange
        var model = new LoginModel { Email = "test@example.com", ReturnUrl = "testUrl" };
        _signInManager.PasswordlessSignInAsync(model.Email, model.ReturnUrl).Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

        // Act
        var result = await _controller.Index(model) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal(3, result.RouteValues["success"]);
    }

    [Fact]
    public async Task Index_Post_InvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var model = new LoginModel { Email = "invalidEmail" };
        _controller.ModelState.AddModelError("Email", "Invalid email");

        // Act
        var result = await _controller.Index(model) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(model, result.Model);
    }

    [Fact]
    public async Task Confirm_ValidToken_RedirectsToReturnUrl()
    {
        // Arrange
        string email = "test@example.com";
        string token = "validToken";
        string returnUrl = "testUrl";
        _signInManager.PasswordlessSignInAsync(email, token, true).Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

        // Act
        var result = await _controller.Confirm(email, token, returnUrl) as RedirectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(returnUrl, result.Url);
    }

    [Fact]
    public async Task Confirm_InvalidToken_RedirectsToIndexWithError()
    {
        // Arrange
        string email = "test@example.com";
        string token = "invalidToken";
        string returnUrl = "testUrl";
        _signInManager.PasswordlessSignInAsync(email, token, true).Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

        // Act
        var result = await _controller.Confirm(email, token, returnUrl) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal(2, result.RouteValues["error"]);
    }

    [Fact]
    public async Task Logout_RedirectsToIndexWithSuccess()
    {
        // Act
        var result = await _controller.Logout() as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal(1, result.RouteValues["success"]);
    }
}
