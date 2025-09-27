using System;

namespace ShopApi.Users.Client
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