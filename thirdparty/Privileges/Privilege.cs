using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Threading;
using Luid = Privileges.NativeMethods.LUID;
using Win32Exception = System.ComponentModel.Win32Exception;

namespace Privileges
{
    public delegate void PrivilegedCallback(object state);

    public sealed class Privilege : IDisposable
    {
        #region Private static members

        private static readonly LocalDataStoreSlot tlsSlot = Thread.AllocateDataSlot();
        private static readonly HybridDictionary privileges = [];
        private static readonly HybridDictionary luids = [];
        private static readonly ReaderWriterLock privilegeLock = new();

        #endregion Private static members

        #region Private members

        private bool initialState = false;
        private bool stateWasChanged = false;
        private Luid luid;
        private readonly Thread currentThread = Thread.CurrentThread;
        private TlsContents tlsContents = null;

        #endregion Private members

        #region Privilege names

        public const string CreateToken = "SeCreateTokenPrivilege";
        public const string AssignPrimaryToken = "SeAssignPrimaryTokenPrivilege";
        public const string LockMemory = "SeLockMemoryPrivilege";
        public const string IncreaseQuota = "SeIncreaseQuotaPrivilege";
        public const string UnsolicitedInput = "SeUnsolicitedInputPrivilege";
        public const string MachineAccount = "SeMachineAccountPrivilege";
        public const string TrustedComputingBase = "SeTcbPrivilege";
        public const string Security = "SeSecurityPrivilege";
        public const string TakeOwnership = "SeTakeOwnershipPrivilege";
        public const string LoadDriver = "SeLoadDriverPrivilege";
        public const string SystemProfile = "SeSystemProfilePrivilege";
        public const string SystemTime = "SeSystemtimePrivilege";
        public const string ProfileSingleProcess = "SeProfileSingleProcessPrivilege";
        public const string IncreaseBasePriority = "SeIncreaseBasePriorityPrivilege";
        public const string CreatePageFile = "SeCreatePagefilePrivilege";
        public const string CreatePermanent = "SeCreatePermanentPrivilege";
        public const string Backup = "SeBackupPrivilege";
        public const string Restore = "SeRestorePrivilege";
        public const string Shutdown = "SeShutdownPrivilege";
        public const string Debug = "SeDebugPrivilege";
        public const string Audit = "SeAuditPrivilege";
        public const string SystemEnvironment = "SeSystemEnvironmentPrivilege";
        public const string ChangeNotify = "SeChangeNotifyPrivilege";
        public const string RemoteShutdown = "SeRemoteShutdownPrivilege";
        public const string Undock = "SeUndockPrivilege";
        public const string SyncAgent = "SeSyncAgentPrivilege";
        public const string EnableDelegation = "SeEnableDelegationPrivilege";
        public const string ManageVolume = "SeManageVolumePrivilege";
        public const string Impersonate = "SeImpersonatePrivilege";
        public const string CreateGlobal = "SeCreateGlobalPrivilege";
        public const string TrustedCredentialManagerAccess = "SeTrustedCredManAccessPrivilege";
        public const string ReserveProcessor = "SeReserveProcessorPrivilege";

        #endregion Privilege names

        #region LUID caching logic

        //
        // This routine is a wrapper around a hashtable containing mappings
        // of privilege names to luids
        //

        private static Luid LuidFromPrivilege(string privilege)
        {
            Luid luid;
            luid.LowPart = 0;
            luid.HighPart = 0;

            //
            // Look up the privilege LUID inside the cache
            //

            try
            {
                privilegeLock.AcquireReaderLock(Timeout.Infinite);

                if (luids.Contains(privilege))
                {
                    luid = (Luid)luids[privilege];

                    privilegeLock.ReleaseReaderLock();
                }
                else
                {
                    privilegeLock.ReleaseReaderLock();

                    if (!NativeMethods.LookupPrivilegeValue(null, privilege, ref luid))
                    {
                        int error = Marshal.GetLastWin32Error();

                        if (error == NativeMethods.ERROR_NOT_ENOUGH_MEMORY)
                        {
                            throw new OutOfMemoryException();
                        }
                        else if (error == NativeMethods.ERROR_ACCESS_DENIED)
                        {
                            throw new UnauthorizedAccessException("Caller does not have the rights to look up privilege local unique identifier");
                        }
                        else if (error == NativeMethods.ERROR_NO_SUCH_PRIVILEGE)
                        {
                            throw new ArgumentException(
                                string.Format("{0} is not a valid privilege name", privilege),
                                nameof(privilege));
                        }
                        else
                        {
                            throw new Win32Exception(error);
                        }
                    }

                    privilegeLock.AcquireWriterLock(Timeout.Infinite);
                }
            }
            finally
            {
                if (privilegeLock.IsReaderLockHeld)
                {
                    privilegeLock.ReleaseReaderLock();
                }

                if (privilegeLock.IsWriterLockHeld)
                {
                    if (!luids.Contains(privilege))
                    {
                        luids[privilege] = luid;
                        privileges[luid] = privilege;
                    }

                    privilegeLock.ReleaseWriterLock();
                }
            }

            return luid;
        }

        #endregion LUID caching logic

        #region Nested classes

        private sealed class TlsContents : IDisposable
        {
            private bool disposed = false;
            private IntPtr threadHandle = IntPtr.Zero;

            private static IntPtr processHandle = IntPtr.Zero;
            private static readonly object syncRoot = new();

            #region Constructor and finalizer

            public TlsContents()
            {
                int error = 0;
                int cachingError = 0;
                bool success = true;

                if (processHandle == IntPtr.Zero)
                {
                    lock (syncRoot)
                    {
                        if (processHandle == IntPtr.Zero)
                        {
                            if (!NativeMethods.OpenProcessToken(
                                NativeMethods.GetCurrentProcess(),
                                TokenAccessLevels.Duplicate,
                                ref processHandle))
                            {
                                cachingError = Marshal.GetLastWin32Error();
                                success = false;
                            }
                        }
                    }
                }

                try
                {
                    //
                    // Open the thread token; if there is no thread token,
                    // copy the process token onto the thread
                    //

                    if (!NativeMethods.OpenThreadToken(
                        NativeMethods.GetCurrentThread(),
                        TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges,
                        true,
                        ref threadHandle))
                    {
                        if (success)
                        {
                            error = Marshal.GetLastWin32Error();

                            if (error != NativeMethods.ERROR_NO_TOKEN)
                            {
                                success = false;
                            }

                            if (success)
                            {
                                error = 0;

                                if (!NativeMethods.DuplicateTokenEx(
                                    processHandle,
                                    TokenAccessLevels.Impersonate | TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges,
                                    IntPtr.Zero,
                                    SecurityImpersonationLevel.Impersonation,
                                    TokenType.Impersonation,
                                    ref threadHandle))
                                {
                                    error = Marshal.GetLastWin32Error();
                                    success = false;
                                }
                            }

                            if (success)
                            {
                                if (!NativeMethods.SetThreadToken(
                                    IntPtr.Zero,
                                    threadHandle))
                                {
                                    error = Marshal.GetLastWin32Error();
                                    success = false;
                                }
                            }

                            if (success)
                            {
                                //
                                // This thread is now impersonating; it needs to be reverted to its original state
                                //

                                IsImpersonating = true;
                            }
                        }
                        else
                        {
                            error = cachingError;
                        }
                    }
                    else
                    {
                        success = true;
                    }
                }
                finally
                {
                    if (!success)
                    {
                        Dispose();
                    }
                }

                if (error == NativeMethods.ERROR_NOT_ENOUGH_MEMORY)
                {
                    throw new OutOfMemoryException();
                }
                else if (error is NativeMethods.ERROR_ACCESS_DENIED or
                    NativeMethods.ERROR_CANT_OPEN_ANONYMOUS)
                {
                    throw new UnauthorizedAccessException("The caller does not have the rights to perform the operation");
                }
                else if (error != 0)
                {
                    throw new Win32Exception(error);
                }
            }

            ~TlsContents()
            {
                if (!disposed)
                {
                    Dispose(false);
                }
            }

            #endregion Constructor and finalizer

            #region IDisposable implementation

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposed)
                {
                    return;
                }

                if (threadHandle != IntPtr.Zero)
                {
                    _ = NativeMethods.CloseHandle(threadHandle);
                    threadHandle = IntPtr.Zero;
                }

                if (IsImpersonating)
                {
                    _ = NativeMethods.RevertToSelf();
                }

                disposed = true;
            }

            #endregion IDisposable implementation

            #region Reference-counting

            public void IncrementReferenceCount()
            {
                ReferenceCountValue++;
            }

            public int DecrementReferenceCount()
            {
                int result = --ReferenceCountValue;

                if (result == 0)
                {
                    Dispose();
                }

                return result;
            }

            public int ReferenceCountValue { get; private set; } = 1;

            #endregion Reference-counting

            #region Properties

            public IntPtr ThreadHandle => threadHandle;

            public bool IsImpersonating { get; } = false;

            #endregion Properties
        }

        #endregion Nested classes

        #region Constructor

        public Privilege(string privilegeName)
        {
            if (privilegeName == null)
            {
                throw new ArgumentNullException(nameof(privilegeName));
            }

            luid = LuidFromPrivilege(privilegeName);
        }

        #endregion Constructor

        #region Public methods and properties

        public void Enable()
        {
            ToggleState(true);
        }

        public void Disable()
        {
            ToggleState(false);
        }

        public void Revert()
        {
            int error = 0;

            //
            // All privilege operations must take place on the same thread
            //

            if (!currentThread.Equals(Thread.CurrentThread))
            {
                throw new InvalidOperationException("Operation must take place on the thread that created the object");
            }

            if (!NeedToRevert)
            {
                return;
            }

            bool success = true;

            try
            {
                //
                // Only call AdjustTokenPrivileges if we're not going to be reverting to self,
                // on this Revert, since doing the latter obliterates the thread token anyway
                //

                if (stateWasChanged &&
                    (tlsContents.ReferenceCountValue > 1 ||
                    !tlsContents.IsImpersonating))
                {
                    NativeMethods.TOKEN_PRIVILEGE newState = new()
                    {
                        PrivilegeCount = 1
                    };
                    newState.Privilege.Luid = luid;
                    newState.Privilege.Attributes = initialState ? NativeMethods.SE_PRIVILEGE_ENABLED : NativeMethods.SE_PRIVILEGE_DISABLED;
                    NativeMethods.TOKEN_PRIVILEGE previousState = new();
                    uint previousSize = 0;

                    if (!NativeMethods.AdjustTokenPrivileges(
                        tlsContents.ThreadHandle,
                        false,
                        ref newState,
                        (uint)Marshal.SizeOf(previousState),
                        ref previousState,
                        ref previousSize))
                    {
                        error = Marshal.GetLastWin32Error();
                        success = false;
                    }
                }
            }
            finally
            {
                if (success)
                {
                    Reset();
                }
            }

            if (error == NativeMethods.ERROR_NOT_ENOUGH_MEMORY)
            {
                throw new OutOfMemoryException();
            }
            else if (error == NativeMethods.ERROR_ACCESS_DENIED)
            {
                throw new UnauthorizedAccessException("Caller does not have the permission to change the privilege");
            }
            else if (error != 0)
            {
                throw new Win32Exception(error);
            }
        }

        public bool NeedToRevert { get; private set; } = false;

        public static void RunWithPrivilege(string privilege, bool enabled, PrivilegedCallback callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            Privilege p = new(privilege);

            try
            {
                if (enabled)
                {
                    p.Enable();
                }
                else
                {
                    p.Disable();
                }

                callback(state);
            }
            catch
            {
                p.Revert();
                throw;
            }
            finally
            {
                p.Revert();
            }
        }

        #endregion Public methods and properties

        #region Private implementation

        private void ToggleState(bool enable)
        {
            int error = 0;

            //
            // All privilege operations must take place on the same thread
            //

            if (!currentThread.Equals(Thread.CurrentThread))
            {
                throw new InvalidOperationException("Operation must take place on the thread that created the object");
            }

            //
            // This privilege was already altered and needs to be reverted before it can be altered again
            //

            if (NeedToRevert)
            {
                throw new InvalidOperationException("Must revert the privilege prior to attempting this operation");
            }

            try
            {
                //
                // Retrieve TLS state
                //

                tlsContents = Thread.GetData(tlsSlot) as TlsContents;

                if (tlsContents == null)
                {
                    tlsContents = new TlsContents();
                    Thread.SetData(tlsSlot, tlsContents);
                }
                else
                {
                    tlsContents.IncrementReferenceCount();
                }

                NativeMethods.TOKEN_PRIVILEGE newState = new()
                {
                    PrivilegeCount = 1
                };
                newState.Privilege.Luid = luid;
                newState.Privilege.Attributes = enable ? NativeMethods.SE_PRIVILEGE_ENABLED : NativeMethods.SE_PRIVILEGE_DISABLED;

                NativeMethods.TOKEN_PRIVILEGE previousState = new();
                uint previousSize = 0;

                //
                // Place the new privilege on the thread token and remember the previous state.
                //

                if (!NativeMethods.AdjustTokenPrivileges(
                    tlsContents.ThreadHandle,
                    false,
                    ref newState,
                    (uint)Marshal.SizeOf(previousState),
                    ref previousState,
                    ref previousSize))
                {
                    error = Marshal.GetLastWin32Error();
                }
                else if (NativeMethods.ERROR_NOT_ALL_ASSIGNED == Marshal.GetLastWin32Error())
                {
                    error = NativeMethods.ERROR_NOT_ALL_ASSIGNED;
                }
                else
                {
                    //
                    // This is the initial state that revert will have to go back to
                    //

                    initialState = (previousState.Privilege.Attributes & NativeMethods.SE_PRIVILEGE_ENABLED) != 0;

                    //
                    // Remember whether state has changed at all
                    //

                    stateWasChanged = initialState != enable;

                    //
                    // If we had to impersonate, or if the privilege state changed we'll need to revert
                    //

                    NeedToRevert = tlsContents.IsImpersonating || stateWasChanged;
                }
            }
            finally
            {
                if (!NeedToRevert)
                {
                    Reset();
                }
            }

            if (error == NativeMethods.ERROR_NOT_ALL_ASSIGNED)
            {
                throw new PrivilegeNotHeldException(privileges[luid] as string);
            }
            if (error == NativeMethods.ERROR_NOT_ENOUGH_MEMORY)
            {
                throw new OutOfMemoryException();
            }
            else if (error is NativeMethods.ERROR_ACCESS_DENIED or
                NativeMethods.ERROR_CANT_OPEN_ANONYMOUS)
            {
                throw new UnauthorizedAccessException("The caller does not have the right to change the privilege");
            }
            else if (error != 0)
            {
                throw new Win32Exception(error);
            }
        }

        private void Reset()
        {
            stateWasChanged = false;
            initialState = false;
            NeedToRevert = false;

            if (tlsContents != null)
            {
                if (tlsContents.DecrementReferenceCount() == 0)
                {
                    tlsContents = null;
                    Thread.SetData(tlsSlot, null);
                }
            }
        }

        public void Dispose()
        {
            tlsContents?.Dispose();
        }

        #endregion Private implementation
    }
}