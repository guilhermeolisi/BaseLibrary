﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public struct GOSResult
{
    private bool _success;

    public bool Success { get => Exception is null ? _success : false; private set => _success = value; }
    public Exception Exception { get; private set; }
    public string Message { get; private set; }
    public GOSResult(bool success) : this()
    {
        Success = success;
        Exception = null;
        Message = null;
    }
    public GOSResult(string message) : this()
    {
        Success = false;
        Exception = null;
        Message = message;
    }
    public GOSResult(Exception exception) : this()
    {
        Success = false;
        Exception = exception;
        Message = null;
    }
    public GOSResult(bool success, string message) : this()
    {
        Success = success;
        Exception = null;
        Message = message;
    }
    public GOSResult(bool success, Exception exception) : this()
    {
        Success = success;
        Exception = exception;
        Message = null;
    }
    public GOSResult(bool success, Exception exception, string message) : this()
    {
        Success = success;
        Exception = exception;
        Message = message;
    }
    public GOSResult(bool success, object _object) : this()
    {
        Success = success;
        Exception = null;
        Message = null;
    }

    public override string ToString()
    {
        string result = (Success ? "Success" : "FAIL") +
            (string.IsNullOrEmpty(Message) ? string.Empty : Environment.NewLine + "MESSAGE" + Environment.NewLine + Message) +
            (Exception is null ? string.Empty : Environment.NewLine + "EXCEPTION" + Environment.NewLine + Exception.Message + (Exception.InnerException is not null ? Environment.NewLine + Exception.InnerException : string.Empty) + (Exception.StackTrace is not null ? Environment.NewLine + Exception.StackTrace : string.Empty));
        ;

        return result;
    }
}
