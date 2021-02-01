using Amazon;
using Amazon.DynamoDBv2;
using ISBNResolver.ISBNDb;
using ISBNResolver.Repository;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ISBNResolver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ISBNResolver", Version = "v1" });
            });

            services.AddMediatR(typeof(Startup));
            services.AddScoped<IApiClient, ApiClient>();
            services.AddScoped<IISBNDb, ISBNDb.ISBNDb>();
            services.AddSingleton<IRepository, Repository.Repository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISBNResolver v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void AddDynamoDb(IServiceCollection services, string envName)
        {
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();

            if (envName.ToLower() == "development")
            {
                // Local DynamoDb Install:
                //   https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/CodeSamples.DotNet.html#CodeSamples.DotNet.RegionAndEndpoint
                clientConfig.ServiceURL = "http://localhost:8000";
            } else {
                clientConfig.RegionEndpoint = RegionEndpoint.APSoutheast2;
            }

            AmazonDynamoDBClient client = new AmazonDynamoDBClient(clientConfig);

            services.AddSingleton<AmazonDynamoDBClient>(client);

        }
    }
}
