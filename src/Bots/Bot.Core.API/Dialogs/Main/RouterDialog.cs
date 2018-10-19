using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Main
{
    public abstract class RouterDialog : ComponentDialog
    {
        public RouterDialog(string dialogId)
            : base(dialogId)
        {
        }

        protected abstract Task<DialogTurnResult> OnMessageAuthentication(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken));

        protected abstract Task<DialogTurnResult> OnMessageInterruptions(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken));

        protected override Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken)) => OnContinueDialogAsync(innerDc, cancellationToken);

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = innerDc.Context.Activity;

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    {
                        var handled = await OnMessageInterruptions(innerDc, cancellationToken);
                        if (handled != null)
                            return handled;

                        var result = await innerDc.ContinueDialogAsync();

                        if (innerDc.Context.Responded) break;

                        switch (result.Status)
                        {
                            case DialogTurnStatus.Empty:
                                {
                                    await RouteAsync(innerDc);
                                    break;
                                }

                            case DialogTurnStatus.Complete:
                                {
                                    await CompleteAsync(innerDc);

                                    // End active dialog.
                                    await innerDc.EndDialogAsync();
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }

                        break;
                    }

                case ActivityTypes.Event:
                case ActivityTypes.Invoke:
                    {
                        if (activity.Name == "tokens/response")
                        {
                            await innerDc.ContinueDialogAsync(cancellationToken);
                            if (!innerDc.Context.Responded)
                            {
                                await OnMessageAuthentication(innerDc, cancellationToken);
                            }
                        }
                        else await OnEventAsync(innerDc);
                        break;
                    }

                case ActivityTypes.ConversationUpdate:
                    {
                        await OnStartAsync(innerDc);
                        break;
                    }

                default:
                    {
                        await OnSystemMessageAsync(innerDc);
                        break;
                    }
            }

            return EndOfTurn;
        }

        protected override Task OnEndDialogAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken)) => base.OnEndDialogAsync(context, instance, reason, cancellationToken);

        protected override Task OnRepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken)) => base.OnRepromptDialogAsync(turnContext, instance, cancellationToken);

        /// <summary>
        /// Called when the inner dialog stack is empty.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task RouteAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Called when the inner dialog stack is complete.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task CompleteAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;

        /// <summary>
        /// Called when an event activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task OnEventAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;

        /// <summary>
        /// Called when a system activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task OnSystemMessageAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;

        /// <summary>
        /// Called when a conversation update activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task OnStartAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
    }

}
