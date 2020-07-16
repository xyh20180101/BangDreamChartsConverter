using System;

namespace BangDreamChartsConverter.HttpPost
{
    [Serializable]
    public class AjaxResponse : AjaxResponse<object>
    {
        public AjaxResponse()
        {
        }

        public AjaxResponse(bool success)
            : base(success)
        {
        }

        public AjaxResponse(object data)
            : base(data)
        {
        }

        public AjaxResponse(ErrorInfo errorInfo)
            : base(errorInfo)
        {
        }
    }

    [Serializable]
    public class AjaxResponse<T> : AjaxResponseBase
    {
        public AjaxResponse()
        {
        }

        public AjaxResponse(bool success)
        {
            Success = success;
        }

        public AjaxResponse(T data)
        {
            Data = data;
            Success = true;
        }

        public AjaxResponse(ErrorInfo errorInfo)
        {
            Success = false;
            ErrorInfo = errorInfo;
        }

        public T Data { get; set; }
        public ErrorInfo ErrorInfo { get; set; }
    }

    public abstract class AjaxResponseBase
    {
        public string TargetUrl { get; set; }
        public bool Success { get; set; }
    }

    [Serializable]
    public class ErrorInfo
    {
        public ErrorInfo()
        {
        }

        public ErrorInfo(string message)
        {
            Message = message;
        }

        public ErrorInfo(int code)
        {
            Code = code;
        }

        public ErrorInfo(int code, string message)
            : this(message)
        {
            Code = code;
            Message = message;
        }

        public int Code { get; set; }
        public string Message { get; set; }
    }
}