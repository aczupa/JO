using Xunit;
using Bunit;
using Bunit.TestDoubles;
using JO.Shared;
using Microsoft.AspNetCore.Components.Authorization;

public class NavMenuTests : TestContext
{
    [Fact]
    public void NavMenu_Should_Render_All_Common_Links()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        var cut = RenderComponent<CascadingAuthenticationState>(
            ps => ps.AddChildContent<NavMenu>()
        );

        var markup = cut.Markup;
        Assert.Contains("Home", markup);
        Assert.Contains("Login", markup);
        Assert.Contains("Logout", markup);
    }

    [Fact]
    public void NavMenu_Should_Collapse_And_Expand_On_Toggle()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        var cut = RenderComponent<CascadingAuthenticationState>(
            ps => ps.AddChildContent<NavMenu>()
        );

        Assert.Contains("collapse", cut.Markup);

        cut.Find("button.navbar-toggler").Click();
        Assert.DoesNotContain("collapse", cut.Markup);

        cut.Find("button.navbar-toggler").Click();
        Assert.Contains("collapse", cut.Markup);
    }

    [Fact]
    public void NavMenu_Should_Render_Admin_Links_For_Admin_User()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin@test.com");
        authContext.SetRoles("Admin");

        var cut = RenderComponent<CascadingAuthenticationState>(
            ps => ps.AddChildContent<NavMenu>()
        );

        var markup = cut.Markup;
        Assert.Contains("Offers", markup);
        Assert.Contains("Add Offer", markup);
    }

    [Fact]
    public void NavMenu_Should_Not_Render_Admin_Links_For_NonAdmin()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("user@test.com");
        authContext.SetRoles("User");

        var cut = RenderComponent<CascadingAuthenticationState>(
            ps => ps.AddChildContent<NavMenu>()
        );

        var markup = cut.Markup;
        Assert.DoesNotContain("Offers", markup);
        Assert.DoesNotContain("Add Offer", markup);
    }

    [Fact]
    public void NavMenu_Should_Not_Render_Admin_Links_For_Anonymous_User()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        var cut = RenderComponent<CascadingAuthenticationState>(
            ps => ps.AddChildContent<NavMenu>()
        );

        var markup = cut.Markup;
        Assert.DoesNotContain("Offers", markup);
        Assert.DoesNotContain("Add Offer", markup);
    }
}
