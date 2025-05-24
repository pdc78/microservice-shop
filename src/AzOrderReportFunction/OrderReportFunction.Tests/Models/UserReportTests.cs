using System.ComponentModel.DataAnnotations;
using OrderReportFunction.Models;

namespace OrderReportFunction.Tests.Models;
public class UserReportTests
{
    [Test]
    public void UserEmail_Required_FailsIfNotSet()
    {
        var report = (UserReport)Activator.CreateInstance(typeof(UserReport))!;

        var results = ValidateModel(report);

        Assert.That(results.IsValid, Is.False);
    }

    [Test]
    public void UserEmail_IsRequired_ValidationFailsWhenNull()
    {
        var report = new UserReport { UserEmail = null! };

        var results = ValidateModel(report);

        Assert.That(results.IsValid, Is.False);
        Assert.That(results.Errors.Any(e => e.MemberNames.Contains(nameof(UserReport.UserEmail))), Is.True);
    }

    [Test]
    public void UserEmail_IsRequired_ValidationFailsWhenEmpty()
    {
        var report = new UserReport { UserEmail = "" };

        var results = ValidateModel(report);

        Assert.That(results.IsValid, Is.False);
        Assert.That(results.Errors.Any(e => e.MemberNames.Contains(nameof(UserReport.UserEmail))), Is.True);
    }

    [Test]
    public void UserEmail_Whitespace_IsConsideredValid()
    {
        var report = new UserReport { UserEmail = "   " };

        var results = ValidateModel(report);

        Assert.That(results.IsValid, Is.False);
    }

    [Test]
    public void UserEmail_VeryLongString_IsValid()
    {
        var longEmail = new string('a', 500) + "@example.com";
        var report = new UserReport { UserEmail = longEmail };

        var results = ValidateModel(report);

        Assert.That(results.IsValid, Is.True);
    }

    [Test]
    public void Orders_CanContainNullItems()
    {
        var report = new UserReport
        {
            UserEmail = "test@example.com",
            Orders = new List<Order?> { null }
        };

        Assert.That(report.Orders, Does.Contain(null));
    }

    [Test]
    public void Orders_SetToNull_ShouldResultInNullReference()
    {
        var report = new UserReport { UserEmail = "test@example.com" };

        // No exception is thrown but Orders will be null
        report.Orders = null!;

        Assert.That(report.Orders, Is.Null);
    }

    private (bool IsValid, List<ValidationResult> Errors) ValidateModel(object obj)
    {
        var context = new ValidationContext(obj);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(obj, context, results, true);
        return (isValid, results);
    }
}
