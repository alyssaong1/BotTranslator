using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace BotTranslation.Dialogs
{
    internal class TranslatingContext : IDialogContext
    {
        private IDialogContext context;

        public TranslatingContext(IDialogContext context)
        {
            this.context = context;
        }

        public IReadOnlyList<Delegate> Frames => context.Frames;

        public CancellationToken CancellationToken => context.CancellationToken;

        public IActivity Activity => context.Activity;

        public IBotDataBag UserData => context.UserData;

        public IBotDataBag ConversationData => context.ConversationData;

        public IBotDataBag PrivateConversationData => context.PrivateConversationData;

        public void Call<R>(IDialog<R> child, ResumeAfter<R> resume)
        {
            context.Call(child, resume);
        }

        public void Done<R>(R value)
        {
            context.Done(value);
        }

        public void Fail(Exception error)
        {
            context.Fail(error);
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            return context.FlushAsync(cancellationToken);
        }

        public Task Forward<R, T>(IDialog<R> child, ResumeAfter<R> resume, T item, CancellationToken token)
        {
            return context.Forward(child, resume, item, token);
        }

        public Task LoadAsync(CancellationToken cancellationToken)
        {
            return context.LoadAsync(cancellationToken);
        }

        public IMessageActivity MakeMessage()
        {
            return context.MakeMessage();
        }

        public void Post<E>(E @event, ResumeAfter<E> resume)
        {
            context.Post(@event, resume);
        }

        public async Task PostAsync(IMessageActivity message, CancellationToken cancellationToken = default(CancellationToken))
        {
            string sourceText = message.Text;
            message.Text = $"In English: {sourceText}";
            await context.PostAsync(message, cancellationToken);
            message.Text = $"In Chinese: { await MessageTranslator.Current.TranslateMessageBack(sourceText)}";
            await context.PostAsync(message, cancellationToken);
        }

        public void Reset()
        {
            context.Reset();
        }

        public void Wait<R>(ResumeAfter<R> resume)
        {
            context.Wait(resume);
        }
    }
}