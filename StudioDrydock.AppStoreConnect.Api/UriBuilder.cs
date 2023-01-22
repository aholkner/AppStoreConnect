using System.Text;

namespace StudioDrydock.AppStoreConnect.Api
{

    public struct UriBuilder
    {
        Uri baseUri;
        StringBuilder sb = new StringBuilder();
        bool hasParameter = false;

        public UriBuilder(Uri baseUri, string uri)
        {
            this.baseUri = baseUri;
            sb.Append(uri);
        }

        public Uri uri => new Uri(baseUri, sb.ToString());

        public void AddParameter(string name, string value)
        {
            if (!hasParameter)
                sb.Append('?');
            else
                sb.Append('&');
            sb.Append($"{name}=");
            sb.Append(Uri.EscapeDataString(value));
            hasParameter = true;
        }
    }
}