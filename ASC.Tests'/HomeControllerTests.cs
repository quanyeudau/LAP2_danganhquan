using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Xunit;
using ASC.Web.Configuration;
using ASC.Web.Controllers;
using ASC.Tests;
using ASC.Utilities;
using System.Text;
using Newtonsoft.Json;

public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> loggerMock;
    private readonly Mock<IOptions<ApplicationSettings>> optionsMock;
    private readonly Mock<HttpContext> mockHttpContext;
    private readonly Mock<ISession> mockSession;

    public HomeControllerTests()
    {
        // Mock ILogger<HomeController>
        loggerMock = new Mock<ILogger<HomeController>>();

        // Mock IOptions<ApplicationSettings> với giá trị mong đợi
        optionsMock = new Mock<IOptions<ApplicationSettings>>();
        optionsMock.Setup(opt => opt.Value).Returns(new ApplicationSettings
        {
            ApplicationTitle = "Test App" // Sửa từ "ASC" thành "Test App"
        });

        // Mock ISession
        mockSession = new Mock<ISession>();

        // Mock HttpContext
        mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(ctx => ctx.Session).Returns(mockSession.Object);
    }

    [Fact]
    public void HomeController_Index_View_Test()
    {
        var controller = new HomeController(loggerMock.Object, optionsMock.Object);

        // Gán HttpContext giả lập vào Controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        var result = controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void HomeController_Index_NoModel_Test()
    {
        var controller = new HomeController(loggerMock.Object, optionsMock.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        var result = controller.Index() as ViewResult;
        Assert.NotNull(result);
        Assert.Null(result.ViewData.Model);
    }

    [Fact]
    public void HomeController_Index_Validation_Test()
    {
        var controller = new HomeController(loggerMock.Object, optionsMock.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        var result = controller.Index() as ViewResult;
        Assert.NotNull(result);
        Assert.Equal(0, result.ViewData.ModelState.ErrorCount);
    }

    [Fact]
    public void HomeController_Index_Session_Test()
    {
        // Thiết lập dữ liệu session mock
        var testValue = new ApplicationSettings { ApplicationTitle = "Test App" };
        var serializedValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(testValue));

        // Cấu hình ISession giả lập Set() và TryGetValue()
        mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                   .Callback<string, byte[]>((key, value) => serializedValue = value);

        mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                   .Returns((string key, out byte[] value) =>
                   {
                       value = serializedValue;
                       return true;
                   });

        // Tạo controller
        var controller = new HomeController(loggerMock.Object, optionsMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        // Giả lập lưu giá trị vào Session
        controller.HttpContext.Session.SetSession("Test", testValue);

        // Gọi action Index()
        controller.Index();

        // Kiểm tra giá trị trong Session
        var retrievedValue = controller.HttpContext.Session.GetSession<ApplicationSettings>("Test");
        Assert.NotNull(retrievedValue);
        Assert.Equal("Test App", retrievedValue.ApplicationTitle); // So sánh đúng giá trị mong đợi
    }
}
