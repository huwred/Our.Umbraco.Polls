﻿using Microsoft.Extensions.Logging;
using Our.Community.Polls.Models;
using Our.Community.Polls.PollConstants;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Our.Community.Polls.Migrations
{
    public class InitialMigration : MigrationBase
    {
        public InitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists(TableConstants.Questions.TableName))
            {
                Create.Table<Question>().Do();
            }

            if (!TableExists(TableConstants.Answers.TableName))
            {
                Create.Table<Answer>().Do();


                this.Create.Index("QuestionId_Answer")
                    .OnTable(TableConstants.Answers.TableName)
                    .OnColumn("QuestionId").Ascending().WithOptions().NonClustered().Do();
            }

            if (!TableExists(TableConstants.Responses.TableName))
            {
                Create.Table<Response>().Do();


                this.Create.Index("QuestionId_Response")
                    .OnTable(TableConstants.Responses.TableName)
                    .OnColumn("QuestionId").Ascending().WithOptions().NonClustered().Do();

                this.Create.Index("AnswerId_Response")
                    .OnTable(TableConstants.Responses.TableName)
                    .OnColumn("AnswerId").Ascending().WithOptions().NonClustered().Do();
            }

        }
    }

    public class PollsComponent : IComponent
    {
        private readonly ICoreScopeProvider _scopeProvider;
        private readonly IMigrationPlanExecutor  _migrationPlanExecutor;
        private readonly IKeyValueService _keyValueService;
        private readonly ILoggerFactory _logger;
        private readonly IRuntimeState _runtimeState;

        public PollsComponent(ICoreScopeProvider scopeProvider,IMigrationPlanExecutor  migrationPlanExecutor, IKeyValueService keyValueService, ILoggerFactory logger, IRuntimeState runtimeState)
        {
            _scopeProvider = scopeProvider;
            _migrationPlanExecutor = migrationPlanExecutor;
            _keyValueService = keyValueService;
            _logger = logger;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }
            // Create a migration plan for a specific project/feature
            // We can then track that latest migration state/step for this project/feature
            var migrationPlan = new MigrationPlan("Umbraco Polls");

            // This is the steps we need to take
            // Each step in the migration adds a unique value
            migrationPlan.From(string.Empty)
                .To<InitialMigration>("6C94A7D0-C806-430C-BDB9-7B70455012D6");

            // Go and upgrade our site (Will check if it needs to do the work or not)
            // Based on the current/latest step
            var upgrader = new Upgrader(migrationPlan);

            upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
        }

        public void Terminate()
        {
        }
    }
    public class PollsComposer : ComponentComposer<PollsComponent>
    {

    }
}
