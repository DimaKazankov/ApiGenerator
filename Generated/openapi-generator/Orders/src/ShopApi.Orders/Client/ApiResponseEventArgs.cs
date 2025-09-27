using System;

namespace ShopApi.Orders.Client
{
    public class ApiResponseEventArgs : EventArgs
    {
        public ApiResponse ApiResponse { get; }

        public ApiResponseEventArgs(ApiResponse apiResponse)
        {
            ApiResponse = apiResponse;
        }
    }
}