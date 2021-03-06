﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams.Middlewares;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TeamsTalentMgmtAppV4.Bot
{
    public class TeamsBotHttpAdapter : BotFrameworkHttpAdapter
    {
        public TeamsBotHttpAdapter(
                    IHostingEnvironment env,
                    ICredentialProvider credentialProvider,
                    IConfiguration configuration,
                    ILogger<TeamsBotHttpAdapter> logger,
                    IBotTelemetryClient botTelemetryClient,
                    ConversationState conversationState = null)
                    : base(configuration, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                logger.LogCritical(exception, exception.Message);
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");

                if (env.IsDevelopment())
                {
                    await turnContext.SendActivityAsync(exception.ToString());
                }

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };

            Use(new TelemetryLoggerMiddleware(botTelemetryClient));
            Use(new ShowTypingMiddleware());
            Use(new TeamsMiddleware(credentialProvider));
        }
    }
}
