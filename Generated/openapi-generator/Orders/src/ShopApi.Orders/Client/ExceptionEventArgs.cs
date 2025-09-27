using System;

namespace ShopApi.Orders.Client
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