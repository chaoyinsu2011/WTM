using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Support.FileHandlers;
using WalkingTec.Mvvm.Demo.Models;
using WalkingTec.Mvvm.Mvc;
using WtmBlazorUtils;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddWTMLogger();
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string> { { "HostRoot", builder.Environment.ContentRootPath } });
builder.Services.AddRazorPages();
builder.Services.AddWtmWorkflow(builder.Configuration);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddWtmSession(3600, builder.Configuration);
builder.Services.AddWtmCrossDomain(builder.Configuration);
builder.Services.AddWtmAuthentication(builder.Configuration);
builder.Services.AddWtmHttpClient(builder.Configuration);
builder.Services.AddWtmSwagger();
builder.Services.AddWtmMultiLanguages(builder.Configuration,op => op.LocalizationType = typeof(WalkingTec.Mvvm.BlazorDemo.Shared.Program));
builder.Services.AddMvc(options =>
{
    options.UseWtmMvcOptions();
})
.AddJsonOptions(options => {
    options.UseWtmJsonOptions();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.UseWtmApiOptions();
})
.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddWtmDataAnnotationsLocalization(typeof(Program));
var config = builder.Configuration.Get<Configs>();

if (config.BlazorMode == BlazorModeEnum.Server)
{
    builder.Services.AddServerSideBlazor();
    builder.Services.AddBootstrapBlazor(null, options =>
    {
        options.ResourceManagerStringLocalizerType = typeof(WalkingTec.Mvvm.BlazorDemo.Shared.Program);
    });
    builder.Services.AddWtmBlazor(config);
}


builder.Services.AddWtmContext(builder.Configuration, (options) => {
    options.DataPrivileges = DataPrivilegeSettings();
    options.CsSelector = CSSelector;
    options.FileSubDirSelector = SubDirSelector;
    options.ReloadUserFunc = ReloadUser;
});


var app = builder.Build();

IconFontsHelper.GenerateIconFont("wwwroot/font", "wwwroot/font-awesome");

if (app.Environment.EnvironmentName == "Development")
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler(config.ErrorHandler);
}
app.UseStaticFiles();
app.UseWtmStaticFiles();
app.UseRouting();
app.UseWtmMultiLanguages();
app.UseWtmCrossDomain();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseWtmSwagger(false);
app.UseWtm();
if (config.BlazorMode == BlazorModeEnum.Server)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapBlazorHub();
        endpoints.MapRazorPages();
        endpoints.MapControllerRoute(
           name: "areaRoute",
           pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        endpoints.MapFallbackToPage("/_Host");
    });
}
else
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapRazorPages();
        endpoints.MapControllerRoute(
           name: "areaRoute",
           pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        endpoints.MapFallbackToFile("index.html");
    });
}
app.UseBlazorFrameworkFiles();
app.UseWtmContext(true);

app.Run();

public partial class Program
{
    public static string CSSelector(ActionExecutingContext context)
    {
        //To override the default logic of choosing connection string,
        //change this function to return different connection string key
        //根据context返回不同的连接字符串的名称
        return null;
    }

    /// <summary>
    /// Set data privileges that system supports
    /// 设置系统支持的数据权限
    /// </summary>
    /// <returns>data privileges list</returns>
    public static List<IDataPrivilege> DataPrivilegeSettings()
    {
        List<IDataPrivilege> pris = new List<IDataPrivilege>();
        //Add data privilege to specific type
        //指定哪些模型需要数据权限
        //pris.Add(new DataPrivilegeInfo<City>("城市权限", m => m.Name));
        pris.Add(new DataPrivilegeInfo<School>("学校权限", m => m.SchoolName));
        pris.Add(new DataPrivilegeInfo<Major>("专业权限", m => m.MajorName));
        return pris;
    }

    /// <summary>
    /// Set sub directory of uploaded files
    /// 动态设置上传文件的子目录
    /// </summary>
    /// <param name="fh">IWtmFileHandler</param>
    /// <returns>subdir name</returns>
    public static string SubDirSelector(IWtmFileHandler fh)
    {
        if (fh is WtmLocalFileHandler)
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
        return null;
    }

    /// <summary>
    /// Custom Reload user process when cache is not available
    /// 设置自定义的方法重新读取用户信息，这个方法会在用户缓存失效的时候调用
    /// </summary>
    /// <param name="context"></param>
    /// <param name="account"></param>
    /// <returns></returns>
    public static LoginUserInfo ReloadUser(WTMContext context, string account)
    {
        return null;
    }

}
