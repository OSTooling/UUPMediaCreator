using System.Runtime.InteropServices;

namespace Windows.Internal.Flighting
{
    public class PlatformCTAC : IEquatable<PlatformCTAC>
    {
        private static ClientAttributes? ClientAttributes;

        public string JSON
        {
            get
            {
                ThrowIfFailed(ClientAttributes.ToJsonString(out string value));
                return value;
            }
        }

        public string UriQuery
        {
            get
            {
                ThrowIfFailed(ClientAttributes.ToUriQueryString(out string value));
                return value;
            }
        }

        public readonly string ApplicationIdentifier;

        public readonly string ApplicationVersion;

        public IReadOnlyDictionary<string, int> AttributeErrors => ClientAttributes.AttributeErrors;

        public PlatformCTAC(string ApplicationIdentifier, string ApplicationVersion)
        {
            this.ApplicationIdentifier = ApplicationIdentifier;
            this.ApplicationVersion = ApplicationVersion;

            ClientAttributes = GetCurrentClientAttributes();
        }

        private ClientAttributes GetCurrentClientAttributes()
        {
            return new ClientAttributes(ApplicationIdentifier, ApplicationVersion);
        }

        private static void ThrowIfFailed(int hResult)
        {
            if (hResult < 0)
            {
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PlatformCTAC);
        }

        public bool Equals(PlatformCTAC other)
        {
            return other is not null &&
                   JSON == other.JSON &&
                   UriQuery == other.UriQuery &&
                   EqualityComparer<IReadOnlyDictionary<string, int>>.Default.Equals(AttributeErrors, other.AttributeErrors);
        }

        public override int GetHashCode()
        {
            int hashCode = 1698843082;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(JSON);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(UriQuery);
            hashCode = (hashCode * -1521134295) + EqualityComparer<IReadOnlyDictionary<string, int>>.Default.GetHashCode(AttributeErrors);
            return hashCode;
        }

        public static bool operator ==(PlatformCTAC left, PlatformCTAC right)
        {
            return EqualityComparer<PlatformCTAC>.Default.Equals(left, right);
        }

        public static bool operator !=(PlatformCTAC left, PlatformCTAC right)
        {
            return !(left == right);
        }
    }
}