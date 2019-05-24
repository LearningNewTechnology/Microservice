﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Test.Xigadee;
using Xigadee;
namespace Tests.Xigadee
{
    /// <summary>
    /// This is the base application class.
    /// </summary>
    /// <seealso cref="Xigadee.ApiMicroserviceStartupBase{Tests.Xigadee.TestStartupContext}" />
    public class TestStartup : JwtApiMicroserviceStartupBase<TestStartupContext>
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TestStartup"/> class.
        /// </summary>
        /// <param name="env">The environment.</param>
        public TestStartup(IHostingEnvironment env) : base(env)
        {
        } 
        #endregion

        /// <summary>
        /// This method configures the repositories.
        /// </summary>
        protected override void MicroserviceConfigure()
        {
            var eOpts = Context.Directives.RepositoryProcessExtract();

            Pipeline.AdjustPolicyTaskManagerForDebug();

            var channelIncoming = Pipeline.AddChannelIncoming("testin");

            eOpts.ForEach(r => channelIncoming.AttachPersistenceRepositoryDirective(r, RepositoryResolve));

            Pipeline.Service.Events.StartCompleted += Service_StartCompleted; 
        }

        private RepositoryBase RepositoryResolve(RepositoryDirective directive)
        {
            if (directive.RepositoryType == typeof(IRepositoryAsync<Guid, MondayMorningBlues>))
            {
                var repo = new RepositoryMemory<Guid, MondayMorningBlues>();
                repo.SearchAdd(new RepositoryMemorySearch<Guid, MondayMorningBlues>("default"), true);
                return repo;
            }

            return directive.RepositoryCreateMemory();
        }

        private void ProcessRepository(RepositoryDirective rd, IPipelineChannelIncoming<MicroservicePipeline> channelIncoming)
        {
            channelIncoming
                .AttachPersistenceManagerHandlerMemory(
                    (MondayMorningBlues e) => e.Id
                    , s => new Guid(s)
                    , versionPolicy: ((e) => $"{e.VersionId:N}", (e) => e.VersionId = Guid.NewGuid(), true)
                    , propertiesMaker: (e) => e.ToReferences2()
                    , searches: new[] { new RepositoryMemorySearch<Guid, MondayMorningBlues>("default") }
                    , searchIdDefault: "default");

            //channelIncoming
            //    .AttachPersistenceClient(out _mmbClient);
        }

        private RepositoryBase ResolveRepository(Type repoType)
        {
            return null;
        }

        private void Service_StartCompleted(object sender, StartEventArgs e)
        {
            //var rs = _mmbClient.Create(new MondayMorningBlues() { Id = new Guid("9A2E3F6D-3B98-4C2C-BD45-74F819B5EDFC") }).Result;
        }

        /// <summary>
        /// Adds the MondayMorningBlues test repo to the collection..
        /// </summary>
        /// <param name="services">The services.</param>
        protected override void ConfigureSingletons(IServiceCollection services)
        {
            base.ConfigureSingletons(services);
            //services.AddSingleton<IRepositoryAsync<Guid, MondayMorningBlues>>(_mmbClient);
        }

        /// <summary>
        /// Configures the authorization policy
        /// </summary>
        /// <param name="services">The services.</param>
        protected override void ConfigureSecurityAuthorization(IServiceCollection services)
        {
            var policy = new AuthorizationPolicyBuilder()
                //.AddAuthenticationSchemes("Cookie, Bearer")
                .RequireAuthenticatedUser()
                .RequireRole("paul")
                //.RequireAssertion(ctx =>
                //{
                //    return ctx.User.HasClaim("editor", "contents") ||
                //            ctx.User.HasClaim("level", "senior");
                //}
                //)
                .Build();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("adminp", policy);
                    //policy =>
                    //{
                    //    policy.RequireAuthenticatedUser();
                    //    policy.RequireRole("admin");
                    //});

            })
            ;
        }

    }


    /// <summary>
    /// This class holds the application settings.
    /// </summary>
    /// <seealso cref="Xigadee.ApiMicroserviceStartUpContext" />
    public class TestStartupContext : TestStartupContextBase
    {

    }

    /// <summary>
    /// This class holds the application settings.
    /// </summary>
    /// <seealso cref="Xigadee.ApiMicroserviceStartUpContext" />
    public class TestStartupContextBase : JwtApiMicroserviceStartUpContext
    {
        [RegisterAsSingleton(typeof((string,string)))]
        public virtual (string, string) GetSomething() => ("hello", DateTime.UtcNow.ToString("o"));

        protected override IApiUserSecurityModule UserSecurityModuleCreate()
        {
            var usm = new UserSecurityModule<TestUser>()
                .SetAsMemoryBased();

            //Add test security accounts here.
            var user = new TestUser() { Username = "paul" };
            var rs = usm.Users.Create(user).Result;

            var rsv = usm.Users.ReadByRef(TestUser.KeyUsername, "paul").Result;

            var uSec = new UserSecurity() { Id = user.Id };
            uSec.AuthenticationSet("","123Enter.");
            var rs2 = usm.UserSecurities.Create(uSec).Result;

            var ur = new UserRoles() { Id = user.Id };
            ur.RoleAdd("paul");
            var rs3 = usm.UserRoles.Create(ur).Result;

            //uSec.

            return usm;
        }

        public override void ModulesCreate(IServiceCollection services)
        {
            base.ModulesCreate(services);
            MondayMorningBlues = new MondayMorningBluesModule();
        }

        public override void Connect(ILoggerFactory lf)
        {
            base.Connect(lf);
            MondayMorningBlues.Logger = lf.CreateLogger<MondayMorningBluesModule>();

        }

        [ModuleStartStop]
        [RepositoriesProcess]
        [RegisterAsSingleton]
        public MondayMorningBluesModule MondayMorningBlues { get; set; }
    }
}
