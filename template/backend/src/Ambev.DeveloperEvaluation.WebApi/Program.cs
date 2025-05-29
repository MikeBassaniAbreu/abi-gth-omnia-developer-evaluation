using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Ambev.DeveloperEvaluation.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();

            builder.Services.AddControllers(options =>
            {
                // Opcional, mas boa prática para consistência:
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                options.SuppressAsyncSuffixInActionNames = false; // Garante que sufixos 'Async' não afetem rotas
            }).ConfigureApiBehaviorOptions(options => // <-- Mudei a configuração para .ConfigureApiBehaviorOptions
            {
                // Esta é a parte mais provável de resolver o seu problema:
                // Desabilita o filtro automático de BadRequest para ModelState inválido.
                // Agora, a propriedade SuppressModelStateInvalidFilter está no local correto.
                options.SuppressModelStateInvalidFilter = true;

                // Aqui estamos customizando a resposta para erros de Model Binding
                // que ocorrem ANTES da sua lógica de controller ou MediatR.
                // Isso garante que mesmo esses erros sejam formatados usando seu ApiResponse.
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Where(m => m.Value != null && m.Value.Errors.Any())
                                                   .SelectMany(m => m.Value.Errors)
                                                   .Select(e => new ValidationErrorDetail // Certifique-se de que ValidationErrorDetail está visível (usings)
                                                   {
                                                       Error = "ModelBinding", // Código de erro customizado
                                                       Detail = e.ErrorMessage
                                                   }).ToList();

                    // Retorna um BadRequestObjectResult que encapsula seu ApiResponse.
                    return new BadRequestObjectResult(new ApiResponse // Certifique-se de que ApiResponse está visível (usings)
                    {
                        Success = false,
                        Message = "Model binding failed.",
                        Errors = errors
                    });
                };
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.AddBasicHealthChecks();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
                )
            );

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.RegisterDependencies();

            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    typeof(ApplicationLayer).Assembly,
                    typeof(Program).Assembly
                );
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            var app = builder.Build();
            app.UseMiddleware<ValidationExceptionMiddleware>();

            //migrations
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    Log.Information("Attempting to apply pending database migrations...");
                    var context = services.GetRequiredService<DefaultContext>();
                    context.Database.Migrate();
                    Log.Information("Database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "An error occurred while applying database migrations. Application will terminate.");
                    throw;
                }
            }


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseBasicHealthChecks(); 

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
