using DreameHouse.Aplication.Services;
using DreameHouse.Domain.Abstarctions;
using DreameHouse.Infrastructure.Repositories;
using DreameHouse.Infrastructure;
using Microsoft.Extensions.Logging;
using DreameHouse.Domain.Entities;

namespace DreameHouse;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
