namespace MailClient.App.Infrastructure.Contracts
{
    namespace MailClient.App.Infrastructure.Contracts
    {
        /// <summary>
        /// Responsible for updating the UI/view based on a given model.
        /// </summary>
        public interface IViewUpdater<TModel>
        {
            /// <summary>
            /// Updates the view to reflect the state of <paramref name="model"/>.
            /// </summary>
            void UpdateView(TModel model);
        }
    }
}
