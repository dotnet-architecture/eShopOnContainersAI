using Microsoft.AspNetCore.Http;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Basket;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Catalog;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Help;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Login;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Main;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Order;
using Microsoft.eShopOnContainers.Bot.API.Services.Basket;
using Microsoft.eShopOnContainers.Bot.API.Services.Catalog;
using Microsoft.eShopOnContainers.Bot.API.Services.LUIS;
using Microsoft.eShopOnContainers.Bot.API.Services.Order;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public class DialogFactory : IDialogFactory
    {
        private readonly DomainPropertyAccessors accessors;
        private readonly IOptions<AppSettings> appSettings;
        private readonly ICatalogService catalogService;
        private readonly ICatalogAIService catalogAIService;
        private readonly ICatalogFilterDialogService catalogFilterDialogService;
        private readonly IBasketService basketService;
        private readonly IOrderingService orderingService;
        private readonly ILuisService luisService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILoggerFactory loggerFactory;
        private CatalogDialog catalogDialog;
        private CatalogFilterDialog catalogFilterDialog;
        private LoginDialog loginDialog;
        private BasketDialog basketDialog;
        private OrderDialog orderDialog;
        private MyOrdersDialog myOrdersDialog;
        private HelpDialog helpDialog;

        public DialogFactory(DomainPropertyAccessors accessors, IOptions<AppSettings> appSettings, ICatalogService catalogService, ICatalogAIService catalogAIService,
            ICatalogFilterDialogService catalogFilterDialogService, IBasketService basketService, IOrderingService orderingService, 
            ILuisService luisService, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
        {
            this.accessors = accessors;
            this.appSettings = appSettings;
            this.catalogService = catalogService;
            this.catalogAIService = catalogAIService;
            this.catalogFilterDialogService = catalogFilterDialogService;
            this.basketService = basketService;
            this.orderingService = orderingService;
            this.luisService = luisService;
            this.httpContextAccessor = httpContextAccessor;
            this.loggerFactory = loggerFactory;
        }

        public MainDialog MainDialog => new MainDialog(accessors, appSettings, luisService, this, catalogFilterDialogService, httpContextAccessor);

        public CatalogDialog CatalogDialog => catalogDialog ?? 
            (catalogDialog = new CatalogDialog(accessors, catalogAIService, catalogService, this, basketService, loggerFactory.CreateLogger<CatalogDialog>()));
        public CatalogFilterDialog CatalogFilterDialog => catalogFilterDialog ?? 
            (catalogFilterDialog = new CatalogFilterDialog(accessors, appSettings, catalogService, catalogFilterDialogService, this)); 
        public LoginDialog LoginDialog => loginDialog ?? 
            (loginDialog = new LoginDialog(accessors, appSettings, loggerFactory.CreateLogger<LoginDialog>()));
        public BasketDialog BasketDialog => basketDialog ?? 
            (basketDialog = new BasketDialog(accessors, this, basketService, loggerFactory.CreateLogger<BasketDialog>()));
        public OrderDialog OrderDialog => orderDialog ?? 
            (orderDialog = new OrderDialog(accessors, basketService, orderingService, appSettings, loggerFactory.CreateLogger<OrderDialog>()));
        public MyOrdersDialog MyOrdersDialog => myOrdersDialog ?? 
            (myOrdersDialog = new MyOrdersDialog(accessors, this, orderingService, appSettings, loggerFactory.CreateLogger<MyOrdersDialog>()));
        public HelpDialog HelpDialog => helpDialog ?? 
            (helpDialog = new HelpDialog(accessors));
    }

    public interface IDialogFactory
    {
        CatalogDialog CatalogDialog { get; }
        CatalogFilterDialog CatalogFilterDialog { get; }
        LoginDialog LoginDialog { get; }
        BasketDialog BasketDialog { get; }
        MyOrdersDialog MyOrdersDialog { get; }
        OrderDialog OrderDialog { get; }
        HelpDialog HelpDialog { get; }
        MainDialog MainDialog { get; }
    }
}
