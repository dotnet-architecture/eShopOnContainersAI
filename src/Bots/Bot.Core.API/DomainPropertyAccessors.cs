using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Models.User;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API
{
    public class DomainPropertyAccessors
    {
        private readonly UserState userState;
        private readonly ConversationState conversationState;

        public DomainPropertyAccessors(UserState userState, ConversationState conversationState)
        {
            this.userState = userState;
            this.conversationState = conversationState;

            CatalogFilterProperty = conversationState.CreateProperty<CatalogFilterData>(CatalogFilterPropertyName);
            DialogStateProperty = conversationState.CreateProperty<DialogState>(DialogStatePropertyName);
            AuthUserProperty = userState.CreateProperty<AuthUser>(AuthUserPropertyName);
            UserDataProperty = userState.CreateProperty<UserData>(UserDataPropertyName);
        }

        private const string CatalogFilterPropertyName = "eShopData.CatalogFilter";

        public IStatePropertyAccessor<CatalogFilterData> CatalogFilterProperty { get; private set; }

        private const string DialogStatePropertyName = "eShopData.DialogState";

        public IStatePropertyAccessor<DialogState> DialogStateProperty { get; private set; }

        private const string AuthUserPropertyName = "eShopData.AuthUser";

        public IStatePropertyAccessor<AuthUser> AuthUserProperty { get; private set; }

        private const string UserDataPropertyName = "eShopData.UserData";

        public IStatePropertyAccessor<UserData> UserDataProperty { get; set; }

        public Task SaveStatesAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.WhenAll(new[] {
                userState.SaveChangesAsync(turnContext, false, cancellationToken),
                conversationState.SaveChangesAsync(turnContext, false, cancellationToken)
            });
        }
    }
}
