using System;

namespace ImgurPortable.Entities
{
    public class ImgurException : Exception
    {
        public ImgurException(ImgurResponse<Error> error)
        {
            StatusCode = error.StatusCode;
            ErrorInfo = error.Response.ErrorData;
        }

        public StatusCode StatusCode { get; set; }
        public ErrorInfo ErrorInfo { get; set; }
    }
}
