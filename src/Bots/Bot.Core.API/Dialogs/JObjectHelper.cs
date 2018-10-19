using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public static class JObjectHelper
    {
        public static bool TryParse (string text, out JObject json, ILogger logger = null)
        {
            if (!String.IsNullOrEmpty(text))
            {
                try
                {
                    json = JObject.Parse(text);
                    return true;
                }
                catch (JsonReaderException)
                {
                    if (logger != null)
                        logger.LogError($"invalid json: {text}");
                } catch (Exception exception)
                {
                    if (logger != null)
                        logger.LogError(exception, "JObjectHelper.TryParse error");
                }
            }
            json = null;
            return false;
        }
    }
}