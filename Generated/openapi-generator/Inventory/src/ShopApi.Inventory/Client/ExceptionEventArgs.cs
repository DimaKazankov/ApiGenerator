using System;

namespace ShopApi.Inventory.Client
{
    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}