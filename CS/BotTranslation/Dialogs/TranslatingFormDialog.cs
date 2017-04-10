using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;

namespace BotTranslation.Dialogs
{
    //Use this translating Form Dialog to create a form dialog that will automatically translate any prompts sent to the user using a TranslatingContext
    public class TranslatingFormDialog<T> : IFormDialog<T> where T : class
    {
        private IFormDialog<T> formItem;

        public TranslatingFormDialog(IFormDialog<T> formItem)
        {
            this.formItem = formItem;
        }

        public IForm<T> Form => ((IFormDialog<T>)formItem).Form;

        public Task StartAsync(IDialogContext context)
        {
            return ((IFormDialog<T>)formItem).StartAsync(new TranslatingContext(context));
            
        }

        public static IFormDialog<T> FromForm<T>(BuildFormDelegate<T> buildForm, FormOptions options = FormOptions.None) where T : class, new()
        {
            IFormDialog<T> formDialog = FormDialog.FromForm(buildForm);
            return new TranslatingFormDialog<T>(formDialog);
        }

    }
}
