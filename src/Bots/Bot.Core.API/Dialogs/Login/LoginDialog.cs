using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Models.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Login
{
    public class LoginDialog : ComponentDialog
    {
        // The connection name here must match the one from
        // your Bot Channels Registration on the settings blade in Azure.

        public const string Name = nameof(LoginDialog) + ".MainDriver";
        private const string PromptOAuth = nameof(LoginDialog) + "." + nameof(PromptOAuth);

        private readonly DomainPropertyAccessors accessors;
        private readonly ILogger<LoginDialog> logger;
        private readonly AppSettings appSettings;
        private readonly string connectionName;

        private string UserInfoEndpoint { get { return $"{appSettings.IdentityUrl}/connect/userinfo"; } }

        public LoginDialog(DomainPropertyAccessors accessors, IOptions<AppSettings> appSettings, ILogger<LoginDialog> logger) : base(Name)
        {
            this.accessors = accessors;
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.connectionName = this.appSettings.AuthenticationConnectionName;
            AddDialog(new WaterfallDialog(Name, new WaterfallStep[] { PromptStepAsync, LoginStepAsync }));
            AddDialog(Prompt(connectionName));

            InitialDialogId = Name;
        }

        /// <summary>
        /// Prompts the user to login using the OAuth provider specified by the connection name.
        /// </summary>
        /// <param name="connectionName"> The name of your connection. It can be found on Azure in
        /// your Bot Channels Registration on the settings blade. </param>
        /// <returns> An <see cref="OAuthPrompt"/> the user may use to log in.</returns>
        private OAuthPrompt Prompt(string connectionName)
        {
            return new OAuthPrompt(
                PromptOAuth,
                new OAuthPromptSettings
                {
                    ConnectionName = connectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                });
        }

        /// <summary>
        /// This <see cref="WaterfallStep"/> prompts the user to log in.
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the operation result of the operation.</returns>
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            logger.LogDebug("LoginDialog.PromptStepAsync");
            return await step.BeginDialogAsync(PromptOAuth, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// In this step we check that a token was received and prompt the user as needed.
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the operation result of the operation.</returns>
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            logger.LogDebug("LoginDialog.PromptStepAsync");
            var tokenResponse = (Microsoft.Bot.Schema.TokenResponse)step.Result;
            if (tokenResponse != null)
            {
                //if (String.IsNullOrEmpty(tokenResponse.Token))
                //{
                //    logger.LogDebug("LoginDialog invalid access token");
                //    await step.Context.SendActivityAsync("Sorry, there was an error. Please, sign in again in a few seconds.");

                //    // Sometimes, the Azure Bot Service still keeps the Access Token although in identity server is experied
                //    // In this case, we need to force the sign out from the Azure Bot Service
                //    var botAdapter = (BotFrameworkAdapter)step.Context.Adapter;
                //    await botAdapter.SignOutUserAsync(step.Context, connectionName, cancellationToken: cancellationToken);
                //    return await step.EndDialogAsync();
                //}

                var authUser = await accessors.AuthUserProperty.GetAsync(step.Context, () => new Models.User.AuthUser());
                authUser.AccessToken = tokenResponse.Token;
                if (DateTime.TryParse(tokenResponse.Expiration, out DateTime expiresAt))
                    authUser.ExpiresAt = expiresAt;
                await accessors.AuthUserProperty.SetAsync(step.Context, authUser);

                var dataUser = await GetUserInfoAsync(UserInfoEndpoint, authUser.AccessToken);
                if (dataUser != null)
                {
                    authUser.UserId = dataUser.UserApplicationId;
                    authUser.UserName = dataUser.Name;
                    await accessors.UserDataProperty.SetAsync(step.Context, dataUser);
                    await accessors.AuthUserProperty.SetAsync(step.Context, authUser);
                    await step.Context.SendActivityAsync($"User {authUser.UserName} is now logged in.", cancellationToken: cancellationToken);
                }
                else
                {
                    logger.LogDebug("LoginDialog invalid access token");
                    await step.Context.SendActivityAsync("Sorry, there was an error. Please, sign in again in a few seconds.");

                    // Sometimes, the Azure Bot Service still keeps the Access Token although in identity server is experied
                    // In this case, we need to force the sign out from the Azure Bot Service
                    var botAdapter = (BotFrameworkAdapter)step.Context.Adapter;
                    await botAdapter.SignOutUserAsync(step.Context, connectionName, cancellationToken: cancellationToken);

                    authUser.Reset();
                    await accessors.AuthUserProperty.SetAsync(step.Context, authUser);
                }
            }
            else
            {
                logger.LogDebug("LoginDialog invalid token response");
                logger.LogDebug($"Token: {step.Result.ToString()}");
                await step.Context.SendActivityAsync("Login was not successful please try again.", cancellationToken: cancellationToken);
            }
            return await step.EndDialogAsync();
        }

        private async Task<UserData> GetUserInfoAsync(string userInfoEndpoint, string accessToken)
        {
            var user = new UserInfoClient(userInfoEndpoint);
            var userInfoResponse = await user.GetAsync(accessToken);
            if (!userInfoResponse.IsError && userInfoResponse.Claims != null)
            {
                var dataUser = new UserData(userInfoResponse.Claims);
                return dataUser;
            }
            return null;
        }
    }
}
