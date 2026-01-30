using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Result
{
  
        public class ApiResult<T>
        {
            public bool Success { get; init; }
            public string? Message { get; init; }
            public T? Data { get; init; }

            public static ApiResult<T> Ok(T data, string? message = null)
                => new() { Success = true, Data = data, Message = message };

            public static ApiResult<T> Fail(string message)
                => new() { Success = false, Message = message };

            public static ApiResult<T> NotFound(string message = "Not found.")
                => new() { Success = false, Message = message };
        }

        public class ApiResult
        {
            public bool Success { get; init; }
            public string? Message { get; init; }

            public static ApiResult Ok(string? message = null)
                => new() { Success = true, Message = message };

            public static ApiResult Fail(string message)
                => new() { Success = false, Message = message };

            public static ApiResult NotFound(string message = "Not found.")
                => new() { Success = false, Message = message };
        }

    }

