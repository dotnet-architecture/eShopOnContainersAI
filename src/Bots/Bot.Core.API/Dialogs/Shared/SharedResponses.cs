using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Shared.Resources;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Shared
{
    public class SharedResponses : TemplateManager
    {
        public const string TypeMore = "SharedResponses." + nameof(TypeMore);
        public const string ChooseOption = "SharedResponses." + nameof(ChooseOption);
        public const string HowCanIHelp = "SharedResponses." + nameof(HowCanIHelp);

        private static LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                { TypeMore, (context, data) => SharedStrings.TypeMore },
                { ChooseOption, (context, data) => SharedStrings.ChooseOption },
                { HowCanIHelp, (context, data) => SharedStrings.HowCanIHelp },
            },
            ["en"] = new TemplateIdMap { },
            ["fr"] = new TemplateIdMap { },
        };

        public SharedResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }
    }
}
