using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RwaWebApi.Extenstions
{
    public static class ModelStateDictionaryExtensions
    {
        public static IDictionary<string, IEnumerable<string>> ServerError(this ModelStateDictionary modelStateDictionary, string customErrorMessage)
        {
            var errors = new Dictionary<string, IEnumerable<string>>
            {
                { string.Empty, new List<string> { customErrorMessage } }
            };
            return errors;
        }

        public static IDictionary<string, IEnumerable<string>> ConvertToErrorMessages(this ModelStateDictionary modelStateDictionary)
        {
            var errorMessages = new Dictionary<string, IEnumerable<string>>();
            if (modelStateDictionary == null)
            {
                return errorMessages;
            }

            foreach (var error in modelStateDictionary)
            {
                var errorMessageList = error.Value.Errors.Where(err => !string.IsNullOrEmpty(err.ToString())).Select(err => err.ToString());
                errorMessages.Add(error.Key, errorMessageList!);
            }

            return errorMessages;
        }
    }
}
