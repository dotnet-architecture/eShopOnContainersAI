using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Dialogs
{
    public interface IDialogFactory
    {
        BasketDialog CreateBasketDialog();
        CatalogDialog CreateCatalogDialog();
        CatalogFilterDialog CreateCatalogFilterDialog();
        HelpDialog CreateHelpDialog();
        LoginDialog CreateLoginDialog();
        MyOrdersDialog CreateMyOrdersDialog(bool latestOrder = false);
        OrderDialog CreateOrderDialog();
    }

    public class DialogFactory : IDialogFactory
    {
        private readonly ICatalogService catalogService;
        private readonly IBasketService basketService;
        private readonly ICatalogAIService catalogAIService;
        private readonly IProductSearchImageService productSearchImageService;
        private readonly IOrderingService orderingService;
        private readonly IOIDCClient oidcClient;
        private readonly IIdentityService identityService;
        private readonly BotSettings botSettings;

        public DialogFactory(ICatalogService catalogService, IBasketService basketService,
            ICatalogAIService catalogAIService, IProductSearchImageService productSearchImageService,
            IOrderingService orderingService, IOIDCClient oidcClient, 
            IIdentityService identityService, BotSettings botSettings)
        {
            this.catalogService = catalogService;
            this.basketService = basketService;
            this.catalogAIService = catalogAIService;
            this.productSearchImageService = productSearchImageService;
            this.orderingService = orderingService;
            this.oidcClient = oidcClient;
            this.identityService = identityService;
            this.botSettings = botSettings;
        }

        public BasketDialog CreateBasketDialog()
        {
            return new BasketDialog(this, basketService, identityService);
        }

        public CatalogDialog CreateCatalogDialog()
        {
            return new CatalogDialog(this, basketService, catalogService, catalogAIService, identityService);
        }

        public CatalogFilterDialog CreateCatalogFilterDialog()
        {
            return new CatalogFilterDialog(catalogService, productSearchImageService);
        }

        public HelpDialog CreateHelpDialog()
        {
            return new HelpDialog(identityService);
        }

        public LoginDialog CreateLoginDialog()
        {
            return new LoginDialog(oidcClient, identityService);
        }

        public MyOrdersDialog CreateMyOrdersDialog(bool latestOrder=false)
        {
            return new MyOrdersDialog(this, orderingService, identityService, botSettings, latestOrder);
        }

        public OrderDialog CreateOrderDialog()
        {
            return new OrderDialog(basketService, orderingService, identityService, botSettings);
        }
    }
}