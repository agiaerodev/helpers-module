
namespace Ihelpers.Helpers
{
    public static class MessageHelper
    {

        public static int GetHttpMessageStatus(int status, object? response)
        {
            var typeResponse = response.GetType();

            if (typeResponse.Name == "HttpResponseMessage")
            {
                HttpResponseMessage responseMessage = (HttpResponseMessage)response;
                status = responseMessage.StatusCode.GetHashCode();
            }
            return status;
        }

    }
}
