using System;
using System.Runtime.Serialization;

namespace Privileges
{
    [Serializable]
    public sealed class PrivilegeNotHeldException : UnauthorizedAccessException, ISerializable
    {
        public PrivilegeNotHeldException()
            : base("A privilege necessary for this operation to succeed is not held by the caller.")
        {
        }

        public PrivilegeNotHeldException(string privilege)
            : base(string.Format("The {0} privilege is not held by the caller.", privilege))
        {
            PrivilegeName = privilege;
        }

        public PrivilegeNotHeldException(string privilege, Exception inner)
            : base(string.Format("The {0} privilege is not held by the caller.", privilege), inner)
        {
            PrivilegeName = privilege;
        }

        private PrivilegeNotHeldException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PrivilegeName = info.GetString("PrivilegeName");
        }

        public string PrivilegeName { get; } = null;

        [Obsolete]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);

            info.AddValue("PrivilegeName", PrivilegeName, typeof(string));
        }
    }
}